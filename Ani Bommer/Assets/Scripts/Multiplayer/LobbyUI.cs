using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI lobbyIDText;
    [SerializeField] private TextMeshProUGUI lobbyStateText;

    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private Button startGameButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        playerSingleTemplate.gameObject.SetActive(false);

        leaveLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
        });

        startGameButton.onClick.AddListener(() => {
            LobbyManager.Instance.StartGame();
        });
    }

    private void Start()
    {
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += OnKickedFromLobby;

        // Nếu đã có lobby (vừa tạo/join rồi load scene), render luôn
        if (LobbyManager.Instance.GetJoinedLobby() != null)
        {
            UpdateLobby(LobbyManager.Instance.GetJoinedLobby());
        }
    }

    private void Update()
    {
        startGameButton.interactable = LobbyManager.Instance.IsLobbyHost()
                                       && LobbyManager.Instance.GetPlayerCount() >= 2;
    }

    private void OnDestroy()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.OnJoinedLobby -= UpdateLobby_Event;
            LobbyManager.Instance.OnJoinedLobbyUpdate -= UpdateLobby_Event;
            LobbyManager.Instance.OnLeftLobby -= LobbyManager_OnLeftLobby;
            LobbyManager.Instance.OnKickedFromLobby -= OnKickedFromLobby;
        }
    }

    private void LobbyManager_OnLeftLobby()
    {
        ReturnToLobbyList();
    }

    private void UpdateLobby_Event(Lobby lobby)
    {
        UpdateLobby(lobby);
    }

    private void OnKickedFromLobby(Lobby lobby)
    {
        ReturnToLobbyList();
    }

    private void UpdateLobby(Lobby lobby)
    {
        // ✅ Clear trước khi render lại
        ClearLobby();

        foreach (Player player in lobby.Players)
        {
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
            playerSingleTransform.gameObject.SetActive(true);
            LobbyPlayerSingleUI lobbyPlayerSingleUI = playerSingleTransform.GetComponent<LobbyPlayerSingleUI>();

            lobbyPlayerSingleUI.SetKickPlayerButtonVisible(
                LobbyManager.Instance.IsLobbyHost() &&
                player.Id != AuthenticationService.Instance.PlayerId
            );

            lobbyPlayerSingleUI.UpdatePlayer(player);
        }

        lobbyNameText.text = lobby.Name;
        playerCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        lobbyIDText.text = lobby.LobbyCode;
        lobbyStateText.text = lobby.IsPrivate ? "Private" : "Public";

        // ✅ Ẩn nút Start nếu không phải host
        startGameButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());
    }

    private void ClearLobby()
    {
        foreach (Transform child in container)
        {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
    }

    private void ReturnToLobbyList()
    {
        SceneManager.LoadScene("LobbyScene");
    }
}