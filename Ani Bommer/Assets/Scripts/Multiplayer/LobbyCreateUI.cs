using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{


    public static LobbyCreateUI Instance { get; private set; }


    [SerializeField] private Button createButton;
    [SerializeField] private TMP_InputField lobbyNameInputField;
    [SerializeField] private Button publicPrivateButton;
    [SerializeField] private TMP_InputField maxPlayersInputField;

    [SerializeField] private TextMeshProUGUI publicPrivateText;


    private string lobbyName;
    private bool isPrivate;
    private int maxPlayers;

    private void Awake()
    {
        Instance = this;

        createButton.onClick.AddListener(() => {

            // Validate maxPlayers
            int parsedMaxPlayers = 4;
            if (!int.TryParse(maxPlayersInputField.text, out parsedMaxPlayers))
            {
                parsedMaxPlayers = 4;
            }

            // Clamp từ 1 -> 4
            parsedMaxPlayers = Mathf.Clamp(parsedMaxPlayers, 1, 4);

            LobbyManager.Instance.CreateLobby(
                lobbyNameInputField.text,
                parsedMaxPlayers,
                isPrivate
            );

            Hide();
        });

        publicPrivateButton.onClick.AddListener(() => {
            isPrivate = !isPrivate;
            UpdateText();
        });

        // Optional: realtime validate input
        maxPlayersInputField.onValueChanged.AddListener(ValidateMaxPlayersInput);

        Hide();
    }

    private void ValidateMaxPlayersInput(string value)
    {
        if (int.TryParse(value, out int result))
        {
            result = Mathf.Clamp(result, 1, 4);
            maxPlayersInputField.text = result.ToString();
        }
        else if (!string.IsNullOrEmpty(value))
        {
            maxPlayersInputField.text = "1";
        }
    }

    private void UpdateText()
    {
        publicPrivateText.text = isPrivate ? "Private" : "Public";
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        lobbyNameInputField.text = "MyLobby";
        maxPlayersInputField.text = "4";
        isPrivate = false;

        UpdateText();
    }

}