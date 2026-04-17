using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerLoader : MonoBehaviour
{
    [SerializeField] GameObject loadingPanel;
    private int expectedPlayerCount;

    private void Awake()
    {
        expectedPlayerCount = LobbyManager.Instance.GetPlayerCount();
    }

    private void Start()
    {
        loadingPanel.SetActive(true);
        if (NetworkManager.Singleton.IsHost)
        {
            // Host đã StartHost từ trước
            CheckPlayersReady();
        }
        else
        {
            // Client sẽ StartClient trong JoinRelay
            StartCoroutine(WaitForClientConnected());
        }
    }

    private IEnumerator WaitForClientConnected()
    {
        while (!NetworkManager.Singleton.IsConnectedClient)
        {
            yield return null;
        }

        loadingPanel.SetActive(false);
    }

    private void CheckPlayersReady()
    {
        StartCoroutine(WaitForAllPlayers());
    }

    private IEnumerator WaitForAllPlayers()
    {
        while (NetworkManager.Singleton.ConnectedClients.Count < expectedPlayerCount)
        {
            yield return null;
        }

        // 🔥 Tắt loading cho tất cả client
        HideLoadingClientRpc();
    }

    [ClientRpc]
    void HideLoadingClientRpc()
    {
        loadingPanel.SetActive(false);
    }
}