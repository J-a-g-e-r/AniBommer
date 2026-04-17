using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Cysharp.Threading.Tasks;

/// <summary>
/// Network version của BombExplode.
/// - Server: đếm giờ, quyết định nổ, destroy object
/// - Client: nhận ClientRpc để spawn effect + sound local
/// </summary>
/// 
public class BombExplodeNetwork : NetworkBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private LayerMask levelMask;
    [SerializeField] private float bombCountdownTime = 3f;

    private Vector2Int serverGridPos;

    // Sync range từ server xuống client (để CreateExplosions dùng đúng range)
    private NetworkVariable<int> bombRange = new NetworkVariable<int>(
        3,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<Vector2Int> gridPos = new NetworkVariable<Vector2Int>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool isExploded = false;
    private Collider col;
    private SpriteRenderer spriteRenderer;

    private NetworkVariable<ulong> ownerClientId = new NetworkVariable<ulong>(
    0,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
);

    public void SetOwnerClientId(ulong ownerId)
    {
        // Hàm này chỉ được gọi từ server trong PlaceBombServerRpc
        ownerClientId.Value = ownerId;
    }

    // ─── Init (gọi từ PlaceBombServerRpc trên server) ───────────

    public void Init(int range)
    {
        // Hàm này chỉ được gọi từ server trong PlaceBombServerRpc
        bombRange.Value = range;
    }

    public void InitPos(Vector2Int grid)
    {
        // Hàm này chỉ được gọi từ server trong PlaceBombServerRpc
        serverGridPos = grid;
    }

    // ─── Lifecycle ───────────────────────────────────────────────

    private void Awake()
    {
        col = GetComponent<Collider>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isExploded = false;

        if (IsServer)
        {
            // Chỉ server chạy đồng hồ đếm ngược
            StartCoroutine(CountdownExplode());
        }
        Debug.Log($"Bomb spawned IsServer={IsServer} pos={transform.position}");
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        StopAllCoroutines();
    }

    // ─── Countdown (Server only) ─────────────────────────────────

    private IEnumerator CountdownExplode()
    {
        yield return new WaitForSeconds(bombCountdownTime);
        if (!isExploded)
            TriggerExplosionServer();
    }

    // ─── Chain explosion: bom khác nổ chạm vào (physics local, nhưng chỉ server xử lý) ──

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (!isExploded && other.CompareTag("Explosion"))
        {
            TriggerExplosionServer();
        }
    }

    // ─── Server: thực hiện nổ ────────────────────────────────────

    private void TriggerExplosionServer()
    {
        if (isExploded) return;
        isExploded = true;

        StopAllCoroutines();

        // Clear đúng ô bomb trên map data
        Debug.Log($"[BombExplodeNetwork] RemoveBomb at {serverGridPos}");
        GridMapSpawnerNetwork.Instance.RemoveBomb(serverGridPos);

        // Hoàn bomb trực tiếp cho đúng owner
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(ownerClientId.Value, out var client))
        {
            var ownerPlayerObj = client.PlayerObject;
            var ownerStats = ownerPlayerObj != null ? ownerPlayerObj.GetComponent<PlayerStatsNetwork>() : null;
            ownerStats?.RestoreBombOnServer();
        }

        // Event local nếu cần listener khác
        OnBombExplodedClientRpc(ownerClientId.Value);

        // VFX/SFX
        ExplodeClientRpc(transform.position, bombRange.Value);

        DespawnAfterDelay().Forget();
    }

    private async UniTaskVoid DespawnAfterDelay()
    {
        await UniTask.Delay(300);
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn(true); // true = Destroy luôn
    }

    // ─── ClientRpc: chạy effect + âm thanh trên mọi client ──────

    [ClientRpc]
    private void OnBombExplodedClientRpc(ulong bombOwnerClientId)
    {
        GameEventNetwork.RaiseBombExploded(bombOwnerClientId);
    }

    [ClientRpc]
    private void ExplodeClientRpc(Vector3 position, int range)
    {
        // Ẩn sprite
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        // Spawn effect tại tâm
        SpawnExplosionEffect(position + Vector3.up * 0.5f);

        // Âm thanh
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(explosionSound);

        // Lan nổ 4 hướng
        StartCoroutine(CreateExplosions(Vector3.forward, position, range));
        StartCoroutine(CreateExplosions(Vector3.right, position, range));
        StartCoroutine(CreateExplosions(Vector3.back, position, range));
        StartCoroutine(CreateExplosions(Vector3.left, position, range));
    }

    // ─── Spawn effect (Instantiate/Destroy) ──────────────────────

    private void SpawnExplosionEffect(Vector3 pos)
    {
        if (explosionEffect == null) return;

        GameObject vfx = Instantiate(explosionEffect, pos, explosionEffect.transform.rotation);

        float despawnDelay = 1f;
        var ps = vfx.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            despawnDelay = main.duration + main.startLifetime.constantMax + 0.05f;
            ps.Play(true);
        }

        Destroy(vfx, despawnDelay);
    }

    // ─── Lan nổ theo hướng (chạy trên mọi client) ───────────────

    private IEnumerator CreateExplosions(Vector3 direction, Vector3 origin, int range)
    {
        Vector3 rayOrigin = origin + Vector3.up * 0.5f;

        for (int i = 1; i <= range; i++)
        {
            float distance = i * 2f;

            bool hasHit = Physics.Raycast(
                rayOrigin,
                direction,
                out RaycastHit hit,
                distance,
                levelMask
            );

            if (hasHit)
            {
                Vector3 hitPos = hit.point;
                hitPos.y = origin.y + 0.5f;
                SpawnExplosionEffect(hitPos);
                break; // Gặp block → dừng
            }
            else
            {
                Vector3 spawnPos = origin + direction * distance;
                spawnPos.y = origin.y + 0.5f;
                SpawnExplosionEffect(spawnPos);
            }

            yield return new WaitForSeconds(0.02f);
        }
    }
}