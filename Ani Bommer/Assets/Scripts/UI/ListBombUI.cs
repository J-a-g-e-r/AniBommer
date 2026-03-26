using System.Collections.Generic;
using UnityEngine;

public class ListBombUI : MonoBehaviour
{
    [Header("Data")]
    public GameDatabase gameDatabase;
    private PlayerData playerData;

    [Header("UI")]
    public Transform contentParent;
    public BombInforUI itemPrefab;
    //public Transform previewBombPos;

    private readonly List<BombInforUI> _spawnedItems = new List<BombInforUI>();

    private void Awake()
    {
        playerData = DataManager.Instance.PlayerData;
        //PreviewBomb(playerData.equippedBombId);
    }

    private void OnEnable()
    {
        RefreshList();
    }

    public void RefreshList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        _spawnedItems.Clear();

        if (playerData == null || gameDatabase == null)
        {
            Debug.LogError("ListBombUI: missing playerData or gameDatabase");
            return;
        }

        string equippedId = playerData.equippedBombId;

        foreach (string bombId in playerData.ownedBombs)
        {
            BombConfig config = gameDatabase.GetBomb(bombId);
            if (config == null)
            {
                Debug.LogWarning($"Cannot find BombConfig for id: {bombId}");
                continue;
            }

            bool isEquipped = (bombId == equippedId);

            BombInforUI item = Instantiate(itemPrefab, contentParent);
            item.Setup(config, isEquipped, OnSelectBomb);

            _spawnedItems.Add(item);
        }
    }

    private void OnSelectBomb(string bombId)
    {
        playerData.equippedBombId = bombId;
        //PreviewBomb(bombId);
        DataManager.Instance.SavePlayerData();
        RefreshList();
    }

    //private void PreviewBomb(string bombId)
    //{
    //    foreach (Transform child in previewBombPos)
    //    {
    //        Destroy(child.gameObject);
    //    }

    //    BombConfig bomb = gameDatabase.GetBomb(bombId);
    //    if (bomb == null || bomb.prefab == null)
    //    {
    //        Debug.LogWarning($"Cannot preview bomb: {bombId}");
    //        return;
    //    }

    //    Instantiate(bomb.prefab, previewBombPos.position, previewBombPos.rotation, previewBombPos);
    //}
}