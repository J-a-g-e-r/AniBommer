using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{

    public TMP_InputField lobbyCodeInputField;
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private string playerName;

    private void Awake()
    {
        if (DataManager.Instance == null || DataManager.Instance.PlayerData == null)
        {
            Debug.LogWarning("No player data loaded in Lobby.");
            return;
        }

        var data = DataManager.Instance.PlayerData;
        playerName = data.playerName;
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        Debug.Log(playerName);
    }

    //Nut tao lobby
    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayer = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode",new DataObject(DataObject.VisibilityOptions.Public,"The last survivor")},
                    { "Map",new DataObject(DataObject.VisibilityOptions.Public,"Chirstmas")}
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;


            Debug.Log("Created Lobby: " + lobby.Name + " " + " and max players: " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
            PrintPlayer(hostLobby);
            StartCoroutine(HeartbeatCoroutine());
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Failed to create lobby: " + e.Message);
        }
    }

    //Check danh sach lobby
    public async void ListLobbies()
    {
        try
        {
            //Lọc phòng
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                //Tối đa 25 kq;
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    //Trả về những phòng có slot trống
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found:" + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log("Lobby Name: " + lobby.Name + " | Players: " + lobby.Players.Count + "/" + lobby.MaxPlayers + "|Game Mode:" + lobby.Data["GameMode"].Value);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }

    }

    //Gui test de tranh bi loi lobby bi xoa sau 30s
    private IEnumerator HeartbeatCoroutine()
    {
        while (true)
        {
            if (hostLobby != null)
            {
                Lobbies.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }

            yield return new WaitForSeconds(15f); // < 30s
        }
    }

    private IEnumerator PollLobbyCoroutine()
    {
        while (joinedLobby != null)
        {
            var task = Lobbies.Instance.GetLobbyAsync(joinedLobby.Id);

            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception == null)
            {
                joinedLobby = task.Result;

                Debug.Log("Lobby updated: " + joinedLobby.Players.Count);
            }
            else
            {
                Debug.LogError("Lobby update failed");
            }

            yield return new WaitForSeconds(1.5f); // polling rate
        }
    }


    //Vao lobby nhanh
    //public async void QuickJoinLobby()
    //{
    //    try
    //    {
    //        QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

    //        await Lobbies.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
    //        Lobby lobby = await Lobbies.Instance.GetLobbyAsync(queryResponse.Results[0].Id);
    //        Debug.Log("Lobby Name: " + lobby.Name + " | Players: " + lobby.Players.Count + "/" + lobby.MaxPlayers);

    //    }
    //    catch (LobbyServiceException e)
    //    {
    //        Debug.LogError(e.Message);
    //    }
    //}

    public async void QuickJoinLobby()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            joinedLobby = lobby;
            StartCoroutine(PollLobbyCoroutine());
            Debug.Log("Joined Lobby: " + lobby.Name + " | Players: " + lobby.Players.Count + "/" + lobby.MaxPlayers);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;

            StartCoroutine(PollLobbyCoroutine());
            Debug.Log("Joined Lobby with code: " + lobbyCode);

            PrintPlayer(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void JoinClick()
    {
        string lobbyCode = lobbyCodeInputField.text;
        if (!string.IsNullOrEmpty(lobbyCode))
        {
            JoinLobbyByCode(lobbyCode);
        }
        else
        {
            Debug.LogError("Lobby code is empty!");
        }

    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)}
                    }
        };
    }

    private void PrintPlayer()
    {
        PrintPlayer(joinedLobby);
    }

    private void PrintPlayer(Lobby lobby)
    {
        Debug.Log("Players in Lobby " + lobby.Name + " " + lobby.Data["GameMode"].Value + " " + lobby.Data["Map"].Value);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + "" + player.Data["PlayerName"].Value);
        }
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode",new DataObject(DataObject.VisibilityOptions.Public,gameMode)}
                }
            });
            joinedLobby = hostLobby;
            PrintPlayer(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }


    }


    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName)}
            }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }


    }

    private async void LeaveLobby()
    {
        try
        {
            await Lobbies.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby = null;
            Debug.Log("Left the lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }

    }

    private async void MigrateateHost()
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id // Chuyển host cho player thứ 2 trong danh sách
            });
            joinedLobby = hostLobby;
            Debug.Log("Host migrated to: " + hostLobby.HostId);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }
}
