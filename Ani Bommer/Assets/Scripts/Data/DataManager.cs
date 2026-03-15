using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }
    public PlayerData PlayerData { get; private set; }
    private string SavePath => Path.Combine(Application.persistentDataPath, "playerdata.json");


    public bool HasSave() => File.Exists(SavePath);
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

    public void LoadPlayerData()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            PlayerData = JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            PlayerData = PlayerData.CreateDefault();
            SavePlayerData();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume01(PlayerData.bgmVolume);
            AudioManager.Instance.SetSFXVolume01(PlayerData.sfxVolume);
        }

    }

    public void SavePlayerData()
    {
        if(PlayerData == null) return;  

        if(AudioManager.Instance != null)
        {
            PlayerData.bgmVolume = AudioManager.Instance.GetSavedBGM01();
            PlayerData.sfxVolume = AudioManager.Instance.GetSavedSFX01();
        }
        string json = JsonUtility.ToJson(PlayerData, true);
        File.WriteAllText(SavePath, json);
    }

    public void ResetData()
    {
        PlayerData = PlayerData.CreateDefault();
        SavePlayerData();
    }

    public void CreateNewPlayer(string playerName)
    {
        PlayerData = PlayerData.CreateDefault();
        PlayerData.playerName = playerName;
        SavePlayerData();
    }


}
