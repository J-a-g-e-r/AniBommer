using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameSceneUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    private string lobbyScene = "Lobby";

    public void OnClickStart()
    {
        string playerName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName))
        {
            // TODO: hiện lỗi "vui lòng nhập tên"
            return;
        }

        DataManager.Instance.CreateNewPlayer(playerName);
        SceneManager.LoadScene(lobbyScene);
    }
}