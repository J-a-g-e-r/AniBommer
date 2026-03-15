using System.Collections.Generic;
using UnityEngine;

public class LobbyCharacterList : MonoBehaviour
{
    [Header("Data")]
    public GameDatabase gameDatabase;
    private PlayerData playerData; // có thể gán từ GameManager hoặc ScriptableObject chứa PlayerData

    [Header("UI")]
    public Transform contentParent;                // Content có Vertical Layout Group
    public CharacterInforUI itemPrefab;        // prefab ở trên

    private readonly List<CharacterInforUI> _spawnedItems = new List<CharacterInforUI>();

    private void Awake()
    {
        playerData = DataManager.Instance.PlayerData;
    }

    private void OnEnable()
    {
        RefreshList();
    }

    public void RefreshList()
    {
        // Xoá cũ
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        _spawnedItems.Clear();

        if (playerData == null || gameDatabase == null)
        {
            Debug.LogError("LobbyCharacterList: thiếu playerData hoặc gameDatabase");
            return;
        }

        string equippedId = playerData.equippedCharacterId;

        // Duyệt các nhân vật mà player sở hữu
        foreach (string charId in playerData.ownedCharacters)
        {
            CharacterConfig config = gameDatabase.GetCharacter(charId);
            if (config == null)
            {
                Debug.LogWarning($"Không tìm thấy CharacterConfig cho id: {charId}");
                continue;
            }

            bool isEquipped = (charId == equippedId);

            CharacterInforUI item = Instantiate(itemPrefab, contentParent);
            item.Setup(config, isEquipped, OnSelectCharacter);

            _spawnedItems.Add(item);
        }
    }

    private void OnSelectCharacter(string characterId)
    {
        // Cập nhật nhân vật đang sử dụng
        playerData.equippedCharacterId = characterId;

        // TODO: nếu bạn có hệ thống save, gọi Save ở đây
        DataManager.Instance.SavePlayerData();

        // Refresh lại UI để nút đổi thành "In Used"
        RefreshList();
    }
}