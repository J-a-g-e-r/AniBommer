using UnityEngine;
using System.Collections;
public enum PLayerState
{
    Slow,
    Normal,
    Poisoned,
    Freezing,
}

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
    public bool IsInvincible { get; set; }

    private bool isPoisoned;
    private Coroutine poisonCoroutine;

    private bool isSlowed;
    private Coroutine slowCoroutine;
    private float currentSlowAmount = 0f;

    private PlayerController playerController;
    private HealthBar healthBar;
    private PlayerEffects playerEffects;


    [Header("Damage Popup")]
    [SerializeField] private DamageNumberPopup damageNumberPrefab;
    [SerializeField] private Vector3 damagePopupOffset = new Vector3(0f, 4f, 0f);

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


    private void SpawnDamageNumber(int amount)
    {
        if (damageNumberPrefab == null || amount <= 0) return;

        Vector3 pos = transform.position + damagePopupOffset;
        var popupGo = ObjectPoolingManager.Instance.Spawn(damageNumberPrefab.gameObject, pos, Quaternion.identity);
        var popup = popupGo.GetComponent<DamageNumberPopup>();
        //var popup = Instantiate(damageNumberPrefab, pos, Quaternion.identity);
        popup.InitDamage(amount);
    }
    public void TakeDamage(int damage)
    {
        if (IsDead || IsInvincible) return;

        float oldHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);

        int deducted = Mathf.RoundToInt(oldHealth - currentHealth);

        healthBar.UpdateHealth(currentHealth);

        if (deducted > 0)
            SpawnDamageNumber(deducted);
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
        GameManager.Instance?.OnGameLose();

    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, PlayerHealth);

        int healed = Mathf.RoundToInt(currentHealth - oldHealth);

        healthBar.UpdateHealth(currentHealth);

        if (healed > 0)
        {
            if (damageNumberPrefab != null)
            {
                Vector3 pos = transform.position + damagePopupOffset;
                var popupGo = ObjectPoolingManager.Instance.Spawn(damageNumberPrefab.gameObject, pos, Quaternion.identity);
                var popup = popupGo.GetComponent<DamageNumberPopup>();
                //var popup = Instantiate(damageNumberPrefab, pos, Quaternion.identity);
                popup.InitHeal(healed);
            }
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void ApplyPoison(int damagePerSecond, float duration)
    {
        if(IsDead || IsInvincible) return;
        // Nếu đang trúng độc rồi thì dừng cái cũ để reset lại thời gian trúng độc mới
        if (!isPoisoned)
        {
            playerEffects.PlayPoisonedEffect();
        }
        if (isPoisoned && poisonCoroutine != null)
        {
            StopCoroutine(poisonCoroutine);
        }

        poisonCoroutine = StartCoroutine(PoisonRoutine(damagePerSecond, duration));
    }

    //Trungs ddoojc
    private IEnumerator PoisonRoutine(int damagePerSecond, float duration)
    {
        isPoisoned = true;
        float elapsed = 0f;

        // Có thể thêm hiệu ứng màu sắc ở đây nếu muốn
        Debug.Log("Player bị trúng độc!");

        while (elapsed < duration)
        {
            // Kiểm tra nếu player chết trong lúc đang dính độc thì thoát
            if (IsDead) yield break;

            // Gọi hàm TakeDamage có sẵn của bạn
            TakeDamage(damagePerSecond);

            // Chờ 1 giây trước khi trừ máu tiếp
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        isPoisoned = false;
        poisonCoroutine = null;
        Debug.Log("Player đã hết trúng độc.");
    }


    public void ApplySlow(float slowPercent, float duration)
    {
        if (IsDead || IsInvincible) return;

        if (!isSlowed)
        {
            playerEffects.PlaySlowEffect(); // Gọi effect slow 1 lần
        }

        if (isSlowed && slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }
        slowCoroutine = StartCoroutine(SlowRoutine(slowPercent, duration));
    }

    private IEnumerator SlowRoutine(float slowPercent, float duration)
    {
        isSlowed = true;

        // Hoàn trả slow cũ (nếu bị cắn lại trước khi hết duration)
        MoveSpeed += currentSlowAmount;
        // Áp dụng slow mới
        currentSlowAmount = slowPercent;
        MoveSpeed -= currentSlowAmount;

        yield return new WaitForSeconds(duration);

        // Chỉ hoàn trả đúng lượng đã trừ
        MoveSpeed += currentSlowAmount;
        currentSlowAmount = 0f;

        isSlowed = false;
        slowCoroutine = null;
    }
}