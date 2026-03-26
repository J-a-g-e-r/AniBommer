using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public string playerName;
    // Thông tin nhân vật
    public int characterLevel;
    public int currentExp;

    // Tiền tệ
    public int crowns;   // kim cương / vương miện
    public int gold;

    // Sở hữu
    public List<string> ownedCharacters = new List<string>(); // id/tên nhân vật
    public List<string> ownedBombs = new List<string>();      // id/tên bomb
    public string equippedCharacterId;
    public string equippedBombId;

    // Âm thanh (0..1)
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;

    // Tiến độ màn chơi
    public string currentLevelId;          // id/scene name màn đang chơi
    public List<string> unlockedLevels = new List<string>();  // các màn đã mở

    // Khởi tạo mặc định cho người chơi mới
    public static PlayerData CreateDefault()
    {
        var data = new PlayerData();
        data.characterLevel = 1;
        data.currentExp = 0;
        data.crowns = 0;
        data.gold = 0;

        // ví dụ: cho sẵn 1 nhân vật + 1 bomb + 1 màn đầu
        data.ownedCharacters.Add("Char_Default");
        data.ownedCharacters.Add("Barbarian");
        data.ownedCharacters.Add("Mage");
        data.ownedCharacters.Add("Rogue");
        data.ownedCharacters.Add("Ranger");

        data.ownedBombs.Add("Bomb_Default");
        data.ownedBombs.Add("B02");
        data.ownedBombs.Add("B03");
        data.ownedBombs.Add("B04");

        data.equippedCharacterId = "Char_Default";
        data.equippedBombId = "Bomb_Default";

        data.currentLevelId = "Level1";
        data.unlockedLevels.Add("Level1");

        data.bgmVolume = 1f;
        data.sfxVolume = 1f;

        return data;
    }
}