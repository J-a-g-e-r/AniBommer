using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour,IHealth
{
    [Header("Monster Stats")]
    [SerializeField] private int maxHP = 3;

    [Header("Collectable Drops")]
    [SerializeField] private List<CollectableDrop> collectableDrops = new List<CollectableDrop>();
    private int currentHP;

    [Header("Damage Popup")]
    [SerializeField] private DamageNumberPopup damageNumberPrefab;
    [SerializeField] private Vector3 damagePopupOffset = new Vector3(0f, 5f, 0f);
    
    
    public bool IsDead => currentHP <= 0;

    private HealthBar healthBar;

    private MonsterWaveSpawner waveSpawner;

    private MonsterEffect monsterEffect;



    public void Init(MonsterWaveSpawner spawner)
    {
        waveSpawner = spawner;
    }

    private void Awake()
    {

        currentHP = maxHP;
        healthBar = GetComponentInChildren<HealthBar>();
        monsterEffect = GetComponent<MonsterEffect>();
        if (healthBar != null)
        {
            healthBar.Init(maxHP);
        }
    }
    private void SpawnDamageNumber(int amount)
    {
        //if (damageNumberPrefab == null || amount <= 0) return;

        Vector3 pos = transform.position + damagePopupOffset;

        var popupGo = ObjectPoolingManager.Instance.Spawn(damageNumberPrefab.gameObject,pos,Quaternion.identity * Quaternion.Euler(65, 0, 0));
        var popup = popupGo.GetComponent<DamageNumberPopup>();
        //var popup = Instantiate(damageNumberPrefab, pos, Quaternion.identity * Quaternion.Euler(65,0,0));
        popup.InitDamage(amount);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        int oldHP = currentHP;
        currentHP = Mathf.Max(0, currentHP - damage);

        int deducted = oldHP - currentHP;
        //currentHP -= damage;
        if (deducted > 0)
            SpawnDamageNumber(deducted);

        if (GetComponent<BossAttack>() != null)
        {
            BossHealthUI bossUI = FindObjectOfType<BossHealthUI>();
            if (bossUI != null)
                bossUI.UpdateBossHP(currentHP);
        }

        if (currentHP <= 0)
        {
            Die();
        }
        Debug.Log($"Monster took {damage} damage, current HP: {currentHP}/{maxHP}");
        healthBar?.UpdateHealth(currentHP);
        MonsterController monsterController = GetComponent<MonsterController>();
        monsterController?.TriggerDamagedAnimation();
        monsterEffect?.PlayBloodEffect();
    }

    private void Die()
    {
        // Add death logic here (e.g., play animation, drop loot, etc.)
        MonsterController monsterController = GetComponent<MonsterController>();
        monsterController?.TriggerDeathAnimation();
        waveSpawner.OnEnemyDied();
        monsterEffect?.PlayDeathEffect();
        MonsterChaseMovement chaseMovement = FinderHelper.GetComponentOnObject<MonsterChaseMovement>(gameObject);
        if (chaseMovement != null)
            chaseMovement.speed = 0;
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

    public int GetMaxHP() => maxHP;
}
