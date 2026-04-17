using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        if (PlayerData.unlockedLevels == null)
        {
            PlayerData.unlockedLevels = new List<string>();
        }

        if (PlayerData.levelStars == null)
        {
            PlayerData.levelStars = new List<PlayerData.LevelStarRecord>();
        }

        if (PlayerData.unlockedLevels.Count == 0)
        {
            PlayerData.unlockedLevels.Add("GamePlay");
        }
        else if (PlayerData.unlockedLevels.Contains("Level1") && !PlayerData.unlockedLevels.Contains("GamePlay"))
        {
            // Migrate save cũ: Level1 -> GamePlay
            PlayerData.unlockedLevels.Add("GamePlay");
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

    public string GetPlayerName() => PlayerData != null ? PlayerData.playerName : "Unknown";
    public string GetPlayerCharacter() => PlayerData != null ? PlayerData.equippedCharacterId : "DefaultCharacter";

    public void SetCurrentLevel(string levelId)
    {
        if (PlayerData == null || string.IsNullOrWhiteSpace(levelId)) return;
        PlayerData.currentLevelId = levelId;
        SavePlayerData();
    }

    public bool IsLevelUnlocked(string levelId)
    {
        if (PlayerData == null || string.IsNullOrWhiteSpace(levelId)) return false;
        return PlayerData.unlockedLevels.Contains(levelId);
    }

    public void UnlockLevel(string levelId)
    {
        if (PlayerData == null || string.IsNullOrWhiteSpace(levelId)) return;
        if (PlayerData.unlockedLevels.Contains(levelId)) return;

        PlayerData.unlockedLevels.Add(levelId);
        SavePlayerData();
    }

    public int GetLevelStars(string levelId)
    {
        if (PlayerData == null || string.IsNullOrWhiteSpace(levelId)) return 0;
        foreach (var record in PlayerData.levelStars)
        {
            if (record != null && record.levelId == levelId)
            {
                return Mathf.Clamp(record.stars, 0, 3);
            }
        }

        return 0;
    }

    public void CompleteLevel(string levelId, int starsEarned, string nextLevelId = null)
    {
        if (PlayerData == null) return;

        if (string.IsNullOrWhiteSpace(levelId))
        {
            levelId = SceneManager.GetActiveScene().name;
        }

        starsEarned = Mathf.Clamp(starsEarned, 0, 3);
        UnlockLevel(levelId);

        var existingRecord = PlayerData.levelStars.Find(x => x != null && x.levelId == levelId);
        if (existingRecord == null)
        {
            PlayerData.levelStars.Add(new PlayerData.LevelStarRecord
            {
                levelId = levelId,
                stars = starsEarned
            });
        }
        else
        {
            existingRecord.stars = Mathf.Max(existingRecord.stars, starsEarned);
        }

        if (!string.IsNullOrWhiteSpace(nextLevelId))
        {
            UnlockLevel(nextLevelId);
        }

        PlayerData.currentLevelId = levelId;
        PlayerData.crowns +=100;
        SavePlayerData();
    }
}
