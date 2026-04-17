using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameplayManager : NetworkBehaviour
{
    public static GameplayManager Instance { get; private set; }

    [Header("Character Prefabs (khớp với NetworkPrefabList)")]
    [SerializeField] private NetworkCharacterEntry[] characterEntries;

    [Header("Map Bomb")]
    [Tooltip("Prefab bom dùng cho map này (phải có trong NetworkPrefabList)")]
    [SerializeField] private GameObject mapBombPrefab;
    [SerializeField] private GameObject bomb02NetworkPrefab; // này là bom server để kill player

    [Header("Breakable")]
    [SerializeField] private GameObject[] destructiblePrefabs;


    private bool networkMatchEnded = false;
    private Coroutine winCheckCoroutine;
    private bool localResultShown = false;
    private ulong lastAliveSnapshot = ulong.MaxValue;

    //Time battle setting
    private const double MATCH_DURATION_SECONDS = 150.0; // 2 phút 30 giây

    private NetworkVariable<double> matchEndServerTime = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private Coroutine timeoutCoroutine;

    [System.Serializable]
    public class NetworkCharacterEntry
    {
        public string characterId;  // "barbarian", "knight", "rogue_hooded", "mage", "ranger"
                                    // phải khớp với giá trị lưu trong KEY_PLAYER_CHARACTER
        public GameObject prefab;   // Kéo prefab từ NetworkPrefabList vào đây
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (!IsSpawned) return;
        if (matchEndServerTime.Value <= 0) return;

        double remain = matchEndServerTime.Value - NetworkManager.Singleton.ServerTime.Time;
        int seconds = Mathf.CeilToInt((float)remain);

        HUDManager.instance?.UpdateTimerText(seconds);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        StartCoroutine(WaitForMapThenSpawn());
        StartCoroutine(SpawnB02Loop());
    }

    private IEnumerator WaitForMapThenSpawn()
    {
        yield return new WaitUntil(() => GridMapSpawnerNetwork.Instance != null);
        yield return new WaitForSeconds(0.3f);

        SpawnBreakables();
        SpawnAllPlayers();

        matchEndServerTime.Value = NetworkManager.Singleton.ServerTime.Time + MATCH_DURATION_SECONDS; if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);

        if (winCheckCoroutine != null) StopCoroutine(winCheckCoroutine);
        winCheckCoroutine = StartCoroutine(CheckLastSurvivorLoop());

        if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
        timeoutCoroutine = StartCoroutine(CheckTimeoutLoop());
    }

    // ─── SPAWN BREAKABLES ───────────────────────────────────────

    private void SpawnBreakables()
    {
        List<Vector3> positions = GridMapSpawnerNetwork.Instance.GetWorldPositionsOfType(TileType.Destructible);

        foreach (Vector3 pos in positions)
        {
            GameObject prefab = destructiblePrefabs[Random.Range(0, destructiblePrefabs.Length)];
            GameObject obj = Instantiate(prefab, pos + Vector3.up * 0.5f, prefab.transform.rotation);

            NetworkObject netObj = obj.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(true);
                BreakableBlockNetwork bb = obj.GetComponent<BreakableBlockNetwork>();
                bb?.SetGridPosition(GridMapSpawnerNetwork.Instance.WorldToGrid(pos));
            }
            else
            {
                Debug.LogError($"[GameplayManager] {prefab.name} thiếu NetworkObject!");
            }
        }
    }

    private IEnumerator CheckLastSurvivorLoop()
    {
        yield return new WaitForSeconds(0.3f);

        while (!networkMatchEnded)
        {
            ulong currentLastAlive = ulong.MaxValue;
            int aliveCount = 0;

            foreach (var kv in NetworkManager.Singleton.ConnectedClients)
            {
                var playerObj = kv.Value.PlayerObject;
                if (playerObj == null || !playerObj.IsSpawned) continue;

                var stats = playerObj.GetComponent<PlayerStatsNetwork>();
                if (stats == null) continue;
                if (stats.IsDead) continue;

                aliveCount++;
                currentLastAlive = kv.Key;
            }

            if (aliveCount == 1)
                lastAliveSnapshot = currentLastAlive;

            if (aliveCount <= 1)
            {
                networkMatchEnded = true;

                if (aliveCount == 1)
                {
                    AnnounceWinnerClientRpc(currentLastAlive);
                }
                else
                {
                    // Tất cả cùng chết: ưu tiên người sống cuối cùng trước đó
                    if (lastAliveSnapshot != ulong.MaxValue)
                        AnnounceWinnerClientRpc(lastAliveSnapshot);
                }

                yield break;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    [ClientRpc]
    private void AnnounceWinnerClientRpc(ulong winnerClientId)
    {
        if (localResultShown) return;
        localResultShown = true;

        ulong localId = NetworkManager.Singleton.LocalClientId;
        if (localId == winnerClientId)
            GameManager.Instance?.OnGameWin();
        else
            GameManager.Instance?.OnGameLose();
    }

    // ─── SPAWN PLAYERS ──────────────────────────────────────────

    private void SpawnAllPlayers()
    {
        Lobby lobby = LobbyManager.Instance.GetJoinedLobby();
        if (lobby == null) { Debug.LogError("[GameplayManager] Không có lobby!"); return; }

        List<Vector3> spawnPoints = GridMapSpawnerNetwork.Instance.GetWorldPositionsOfType(TileType.PlayerSpawn);
        List<ulong> clients = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);

        if (spawnPoints.Count < lobby.Players.Count)
        {
            Debug.LogError($"[GameplayManager] Thiếu PlayerSpawn tile! Cần {lobby.Players.Count}, có {spawnPoints.Count}");
            return;
        }

        for (int i = 0; i < lobby.Players.Count; i++)
        {
            Player lobbyPlayer = lobby.Players[i];
            string characterId = lobbyPlayer.Data[LobbyManager.KEY_PLAYER_CHARACTER].Value;

            ulong clientId = lobbyPlayer.Id == lobby.HostId
                ? NetworkManager.ServerClientId
                : FindNonHostClientId(clients);

            Vector3 spawnPos = spawnPoints[i] + Vector3.up * 0.5f;
            SpawnPlayerCharacter(clientId, characterId, spawnPos);
        }
    }

    private void SpawnPlayerCharacter(ulong clientId, string characterId, Vector3 spawnPos)
    {
        GameObject prefab = GetPrefabByCharacterId(characterId);
        if (prefab == null)
        {
            Debug.LogError($"[GameplayManager] Không tìm thấy prefab cho: '{characterId}'");
            return;
        }

        GameObject obj = Instantiate(prefab, spawnPos, prefab.transform.rotation);
        NetworkObject netObj = obj.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogError($"[GameplayManager] {prefab.name} thiếu NetworkObject!");
            Destroy(obj);
            return;
        }

        netObj.SpawnAsPlayerObject(clientId, true);
        Debug.Log($"[GameplayManager] Spawned '{characterId}' cho client {clientId} tại {spawnPos}");
    }

    private GameObject GetPrefabByCharacterId(string characterId)
    {
        string idLower = characterId.ToLower();
        foreach (var entry in characterEntries)
        {
            if (entry.characterId.ToLower() == idLower)
                return entry.prefab;
        }
        return null;
    }

    private ulong FindNonHostClientId(List<ulong> clients)
    {
        foreach (ulong id in clients)
            if (id != NetworkManager.ServerClientId)
                return id;

        Debug.LogError("[GameplayManager] Không tìm thấy non-host client!");
        return ulong.MaxValue;
    }
    // ─── MAP BOMB ────────────────────────────────────────────────

    /// <summary>
    /// Trả về bomb prefab được cấu hình cho map hiện tại.
    /// Gọi từ server (PlaceBombServerRpc) để lấy prefab đúng.
    /// </summary>
    public GameObject GetMapBombPrefab()
    {
        if (mapBombPrefab == null)
            Debug.LogError("[GameplayManager] mapBombPrefab chưa được gán trong Inspector!");
        return mapBombPrefab;
    }

    private IEnumerator CheckTimeoutLoop()
    {
        yield return new WaitForSeconds(0.3f);
        while (!networkMatchEnded)
        {
            if (matchEndServerTime.Value > 0 &&
                NetworkManager.Singleton.ServerTime.Time >= matchEndServerTime.Value)
            {
                networkMatchEnded = true;
                List<(ulong clientId, float hp)> alive = new();
                foreach (var kv in NetworkManager.Singleton.ConnectedClients)
                {
                    var playerObj = kv.Value.PlayerObject;
                    if (playerObj == null || !playerObj.IsSpawned) continue;
                    var stats = playerObj.GetComponent<PlayerStatsNetwork>();
                    if (stats == null) continue;
                    if (stats.IsDead) continue;
                    alive.Add((kv.Key, stats.GetCurrentHealth()));
                }
                if (alive.Count == 1)
                {
                    AnnounceWinnerClientRpc(alive[0].clientId);
                    yield break;
                }
                if (alive.Count >= 2)
                {
                    // HP cao nhất thắng; nếu bằng HP -> clientId nhỏ hơn thắng (tie-breaker deterministic)
                    alive.Sort((a, b) =>
                    {
                        int hpCompare = b.hp.CompareTo(a.hp);
                        if (hpCompare != 0) return hpCompare;
                        return a.clientId.CompareTo(b.clientId);
                    });
                    AnnounceWinnerClientRpc(alive[0].clientId);
                    yield break;
                }
                // Không còn ai sống -> fallback snapshot
                if (lastAliveSnapshot != ulong.MaxValue)
                    AnnounceWinnerClientRpc(lastAliveSnapshot);
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator SpawnB02Loop()
    {
        if (!IsServer) yield break;

        yield return new WaitForSeconds(30f);
        while (IsServer && !networkMatchEnded) { 
            Spawn3B02Once();
            yield return new WaitForSeconds(5f);
        }
    }
    private void Spawn3B02Once()
    {
        var gridMgr = GridMapSpawnerNetwork.Instance;
        if (gridMgr == null || bomb02NetworkPrefab == null) return;
        var empty = gridMgr.GetEmptyCells();
        int count = Mathf.Min(3, empty.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, empty.Count);
            Vector2Int grid = empty[idx];
            empty.RemoveAt(idx);
            if (!gridMgr.CanPlaceBomb(grid)) { i--; continue; }
            Vector3 worldPos = gridMgr.GridToWorld(grid);
            Vector3 spawnPos = new Vector3(worldPos.x, 1f, worldPos.z);
            var go = Instantiate(bomb02NetworkPrefab, spawnPos, bomb02NetworkPrefab.transform.rotation);
            var netObj = go.GetComponent<NetworkObject>();
            var nt = go.GetComponent<NetworkTransform>();
            var bomb = go.GetComponent<BombExplodeNetwork>();
            if (netObj == null || nt == null || bomb == null) { Destroy(go); return; }
            // Spawn trước
            // Mark trên map data TRƯỚC khi spawn tiếp — tránh 2 bom cùng 1 ô
            gridMgr.PlaceBomb(grid);

            // Đặt transform TRƯỚC khi Spawn — NetworkTransform sẽ sync
            // vị trí ban đầu này xuống client, không cần Teleport riêng.
            // (Teleport() ngay sau Spawn() bị interpolation nuốt mất ở client)
            go.transform.SetPositionAndRotation(spawnPos, bomb02NetworkPrefab.transform.rotation);
            netObj.Spawn(true);

            bomb.Init(1);
            bomb.InitPos(grid);
            bomb.SetOwnerClientId(ulong.MaxValue);
            Debug.Log($"SpawnB02 grid={grid} world={worldPos}");

        }
    }
}
