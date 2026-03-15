using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingToLobby : MonoBehaviour
{
    [SerializeField] private string lobbyScene = "Lobby";

    private void Start()
    {
        // Có thể làm loading bar ở đây
        SceneManager.LoadScene(lobbyScene);
    }
}