using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyJoinIDPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyCodeInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button openIDInputFieldButton;

    private void Awake()
    {
        joinButton.onClick.AddListener(OnJoinClicked);
        backButton.onClick.AddListener(Hide);
        openIDInputFieldButton.onClick.AddListener(Show);
        Hide();
    }

    private void OnJoinClicked()
    {
        string code = lobbyCodeInput.text.Trim();

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("Lobby Code is empty!");
            return;
        }

        LobbyManager.Instance.JoinLobbyByCode(code);
        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        lobbyCodeInput.Select();
        lobbyCodeInput.ActivateInputField();
    }

}
