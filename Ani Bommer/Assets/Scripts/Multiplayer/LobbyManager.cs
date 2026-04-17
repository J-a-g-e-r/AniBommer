using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Qos.V2.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    #region Singleton
    public static LobbyManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_PLAYER_CHARACTER = "PlayerCharacter";
    public const string KEY_START_GAME = "StartGame";

    public Action OnLeftLobby;
    public Action<Lobby> OnJoinedLobby;
    public Action<Lobby> OnJoinedLobbyUpdate;
    public Action<Lobby> OnKickedFromLobby;
    public Action<List<Lobby>> OnLobbyListChanged;
    public Action OnGameStarted;

    private float heartbeatTimer;
    private float lobbyPollTimer;
    private Lobby joinedLobby;
    private int PlayerCount;

    private void Start()
    {
        Authenticate();
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += (clientId, sceneName, loadSceneMode) =>
            {
                Debug.Log($"[SceneManager] Client {clientId} loaded scene: {sceneName}");
            };
        }
    }

    private void Update()
    {
        HandleLobbyPolling();
        HandleLobbyHeartbeat();
    }

    public async void Authenticate()
    {
        string playerName = DataManager.Instance.GetPlayerName();

        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            RefreshLobbyList();
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async void HandleLobbyPolling()
    {
        if (joinedLobby == null) return;
        PlayerCount = joinedLobby.Players.Count;

        lobbyPollTimer -= Time.deltaTime;
        if (lobbyPollTimer < 0f)
        {
            lobbyPollTimer = 1.1f;
            joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            OnJoinedLobbyUpdate?.Invoke(joinedLobby);

            if (!IsPlayerInLobby())
            {
                OnKickedFromLobby?.Invoke(joinedLobby);
                joinedLobby = null;
            }
        }
    }

    private async void HandleLobbyHeartbeat()
    {
        if (joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = 15f;
                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }


    public Lobby GetJoinedLobby() => joinedLobby;
    public bool IsLobbyHost() => joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;

    public int GetPlayerCount() => PlayerCount;
    private bool IsPlayerInLobby()
    {
        if (joinedLobby != null && joinedLobby.Players != null)
        {
            foreach (Player player in joinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private Player GetPlayer()
    {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject>
        {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, DataManager.Instance.GetPlayerName()) },
            { KEY_PLAYER_CHARACTER, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, DataManager.Instance.GetPlayerCharacter()) }
        });
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate)
    {
        try
        {
            // 1. Tạo Relay trước
            string relayCode = await RelayManager.Instance.CreateRelay(); // StartHost() bên trong

            // 2. Tạo Lobby, lưu relay code luôn vào Data
            Player player = GetPlayer();
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = player,
                Data = new Dictionary<string, DataObject>
            {
                { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
            }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            joinedLobby = lobby;

            OnJoinedLobby?.Invoke(joinedLobby);

            // 3. Load RoomScene
            NetworkManager.Singleton.SceneManager.LoadScene("RoomScene", LoadSceneMode.Single);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void RefreshLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);
            OnLobbyListChanged?.Invoke(lobbyListQueryResponse.Results);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            Player player = GetPlayer();
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, new JoinLobbyByCodeOptions
            {
                Player = player
            });
            joinedLobby = lobby;

            string relayCode = joinedLobby.Data[KEY_START_GAME].Value;
            await RelayManager.Instance.JoinRelay(relayCode);

            OnJoinedLobby?.Invoke(joinedLobby);
            UnityEngine.SceneManagement.SceneManager.LoadScene("RoomScene");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        try
        {
            Player player = GetPlayer();
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, new JoinLobbyByIdOptions
            {
                Player = player
            });

            // Join Relay luôn
            string relayCode = joinedLobby.Data[KEY_START_GAME].Value;
             await RelayManager.Instance.JoinRelay(relayCode); // StartClient() bên trong

            OnJoinedLobby?.Invoke(joinedLobby);

            // Load RoomScene
            UnityEngine.SceneManagement.SceneManager.LoadScene("RoomScene");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            joinedLobby = lobby;
            OnJoinedLobby?.Invoke(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }


    public async void LeaveLobby()
    {
        if (joinedLobby == null) return;

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby = null;
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }
            OnLeftLobby?.Invoke();
        }
        catch (LobbyServiceException e) { Debug.Log(e); }
    }

    public async void KickPlayer(string playerID)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerID);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }

    public void StartGame()
    {
        LoadClientRpc();
        OnGameStarted?.Invoke();
        NetworkManager.Singleton.SceneManager.LoadScene("MultiplayerScene", LoadSceneMode.Single);
    }


    [ClientRpc]
    private void LoadClientRpc()
    {
        if (IsServer)
            return;

        LoadingFadeEffect.Instance.FadeAll();
    }

}
