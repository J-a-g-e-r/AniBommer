using UnityEngine;

public class PlayerStats : MonoBehaviour, IHealth
{
    [Header("Player Stats")]
    public float PlayerHealth;
    public float MoveSpeed;
    public int BombRange;
    public int MaxBomb;

    private float limitedMoveSpeed = 15f;
    private int limitedBombRange = 6;
    private int limitedBomb = 8;

    
    private float currentHealth;
    private int currentBomb;

    public bool IsDead => currentHealth <= 0;


    private PlayerController playerController;
    private HealthBar healthBar;
    private PlayerEffects playerEffects;
    public void Init(Characters data)
    {
        PlayerHealth = data.playerHealth;
        MoveSpeed = data.moveSpeed;
        BombRange = data.bombRange;
        MaxBomb = data.maxBombs;

        currentBomb = MaxBomb;
        currentHealth = PlayerHealth;

        playerController = GetComponent<PlayerController>();
        healthBar = GetComponentInChildren<HealthBar>();
        playerEffects = GetComponent<PlayerEffects>();
        healthBar.Init(PlayerHealth);
        HUDManager.instance.InitPlayerStats(PlayerHealth, MaxBomb, BombRange, MoveSpeed);

    }

    private void OnEnable()
    {
        GameEvents.OnBombPlaced += OnBombPlaced;
        GameEvents.OnBombExploded += OnBombExploded;
    }

    private void OnDisable()
    {
        GameEvents.OnBombPlaced -= OnBombPlaced;
        GameEvents.OnBombExploded -= OnBombExploded;
    }

    private void OnBombPlaced()
    {
        if (currentBomb <= 0) return;


        currentBomb--;
        HUDManager.instance.UpdateMaxBombText(currentBomb);
    }

    private void OnBombExploded()
    {
        currentBomb = Mathf.Min(currentBomb + 1, MaxBomb);
        HUDManager.instance.UpdateMaxBombText(currentBomb);
    }

    public bool CanPlaceBomb()
    {
        return currentBomb > 0;
    }

    public void IncreaseMaxBomb(int amount)
    {
        if(MaxBomb >= limitedBomb) return;
        MaxBomb += amount;
        // Tăng currentBomb tương ứng để player có thể đặt thêm bom ngay
        currentBomb += amount;
        HUDManager.instance.UpdateMaxBombText(currentBomb);
    }

    public void IncreaseBombRange(int amount)
    {
        if (BombRange >= limitedBombRange) return;
        BombRange += amount;
        HUDManager.instance.UpdateBombRangeText(BombRange);
    }

    public void IncreaseMoveSpeed(float amount)
    {
        if (MoveSpeed >= limitedMoveSpeed) return;
        MoveSpeed += amount;
        HUDManager.instance.UpdateSpeedText(MoveSpeed);
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        currentHealth -= damage;
        healthBar.UpdateHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        Debug.Log($"Player took {damage} damage, current HP: {currentHealth}/{PlayerHealth}");
        playerController?.TriggerDamagedAnimation();
        playerEffects?.PlayGetHitSound();
        playerEffects?.PlayDamageEffect();
    }

    private void Die()
    {
        // Add death logic here (e.g., play animation, show game over screen, etc.)
        Debug.Log("Player has died!");
        // For now, we just destroy the player object
        playerController?.TriggerDeathAnimation();
        Destroy(gameObject,1f);
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}