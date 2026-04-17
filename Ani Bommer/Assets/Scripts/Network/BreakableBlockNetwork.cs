using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
[RequireComponent(typeof(NetworkObject))]
public class BreakableBlockNetwork : NetworkBehaviour
{
    private bool destroyed;
    private Vector2Int gridPos;
    [Header("Collectable Drops")]
    [SerializeField] private List<CollectableDrop> collectableDrops = new();
    [SerializeField] private AudioClip breakSound;
    public void SetGridPosition(Vector2Int grid)
    {
        gridPos = grid;
    }
    private void OnTriggerEnter(Collider other)
    {
        // Chỉ SERVER được phá + quyết định drop (tránh mỗi máy roll khác nhau)
        if (!IsServer) return;
        if (destroyed) return;
        if (other.CompareTag("Explosion"))
        {
            destroyed = true;
            DestroyBlockServer();
        }
    }
    private void DestroyBlockServer()
    {
        SpawnCollectableServer();
        if (GridMapSpawnerNetwork.Instance != null)
            GridMapSpawnerNetwork.Instance.RemoveDestructible(gridPos);
        RemoveDestructibleClientRpc(gridPos.x, gridPos.y);
        PlayBreakSfxClientRpc();
        // Despawn network object để tất cả client đều mất block
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn(true);
        else
            Destroy(gameObject);
    }
    private void SpawnCollectableServer()
    {
        if (collectableDrops == null || collectableDrops.Count == 0)
            return;
        float totalChance = 0f;
        foreach (var drop in collectableDrops)
        {
            if (drop != null && drop.collectablePrefab != null)
                totalChance += drop.spawnChance;
        }
        float roll = Random.Range(0f, 100f);
        if (roll > totalChance)
            return;
        float current = 0f;
        foreach (var drop in collectableDrops)
        {
            if (drop == null || drop.collectablePrefab == null)
                continue;
            current += drop.spawnChance;
            if (roll <= current)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
                var go = Instantiate(drop.collectablePrefab, spawnPosition, Quaternion.identity);
                // Collectable muốn sync multiplayer thì prefab PHẢI có NetworkObject
                var netObj = go.GetComponent<NetworkObject>();
                if (netObj != null)
                    netObj.Spawn(true);
                return;
            }
        }
    }
    [ClientRpc]
    private void PlayBreakSfxClientRpc()
    {
        if (AudioManager.Instance != null && breakSound != null)
            AudioManager.Instance.PlaySound(breakSound);
    }

    [ClientRpc]
    private void RemoveDestructibleClientRpc(int x, int y)
    {
    if (GridMapSpawnerNetwork.Instance != null)
        GridMapSpawnerNetwork.Instance.RemoveDestructible(new Vector2Int(x, y));
    }
    
}