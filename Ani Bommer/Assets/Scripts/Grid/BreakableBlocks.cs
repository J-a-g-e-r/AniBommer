using System.Collections.Generic;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    private bool destroyed = false;

    private Vector2Int gridPos;

    [Header("Collectable Drops")]
    [SerializeField] private List<CollectableDrop> collectableDrops = new List<CollectableDrop>();

    public void SetGridPosition(Vector2Int grid)
    {
        gridPos = grid;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (destroyed) return;

        if (other.CompareTag("Explosion"))
        {
            destroyed = true;
            DestroyBlock();
        }
    }

    private void DestroyBlock()
    {
        // TODO: spawn break VFX
        SpawnCollectables();
        GridMapSpawner.Instance.RemoveDestructible(gridPos);
        Destroy(gameObject);
    }

    private void SpawnCollectables()
    {
        if (collectableDrops == null || collectableDrops.Count == 0)
            return;

        // 🔹 Tổng % của tất cả item
        float totalChance = 0f;
        foreach (var drop in collectableDrops)
        {
            if (drop.collectablePrefab != null)
                totalChance += drop.spawnChance;
        }

        // 🔹 Nếu totalChance < 100 → có khả năng KHÔNG RƠI GÌ
        float roll = Random.Range(0f, 100f);
        if (roll > totalChance)
            return; // ❌ không spawn gì

        // 🔹 Chọn item
        float current = 0f;
        foreach (var drop in collectableDrops)
        {
            if (drop.collectablePrefab == null)
                continue;

            current += drop.spawnChance;
            if (roll <= current)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
                Instantiate(drop.collectablePrefab, spawnPosition, Quaternion.identity);
                return; // ✅ spawn 1 item là dừng
            }
        }
    }
}
