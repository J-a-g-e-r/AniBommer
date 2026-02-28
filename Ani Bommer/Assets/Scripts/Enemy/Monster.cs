using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour,IHealth
{
    [Header("Monster Stats")]
    [SerializeField] private int maxHP = 3;

    [Header("Collectable Drops")]
    [SerializeField] private List<CollectableDrop> collectableDrops = new List<CollectableDrop>();
    private int currentHP;

    public bool IsDead => currentHP <= 0;

    private HealthBar healthBar;

    private MonsterWaveSpawner waveSpawner;

    public void Init(MonsterWaveSpawner spawner)
    {
        waveSpawner = spawner;
    }

    private void Awake()
    {
        currentHP = maxHP;
        healthBar = GetComponentInChildren<HealthBar>();
        if (healthBar != null)
        {
            healthBar.Init(maxHP);
        }
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        currentHP -= damage;
        if (currentHP <= 0)
        {
            Die();
        }
        Debug.Log($"Monster took {damage} damage, current HP: {currentHP}/{maxHP}");
        healthBar?.UpdateHealth(currentHP);
        MonsterController monsterController = GetComponent<MonsterController>();
        monsterController?.TriggerDamagedAnimation();
    }

    private void Die()
    {
        // Add death logic here (e.g., play animation, drop loot, etc.)
        MonsterController monsterController = GetComponent<MonsterController>();
        monsterController?.TriggerDeathAnimation();
        waveSpawner.OnEnemyDied();
        Destroy(gameObject,0.3f);
        SpawnCollectables();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Explosion"))
        {
            TakeDamage(1);
        }
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
