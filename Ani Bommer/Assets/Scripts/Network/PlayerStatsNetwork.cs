using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class PlayerStatsNetwork : NetworkBehaviour, IHealth
{
    // ─────────────────────────────────────────────
    //  Inspector
    // ─────────────────────────────────────────────
    [Header("Base Stats (filled by Init)")]
    public float PlayerHealth;
    public float MoveSpeed;
    public int BombRange;
    public int MaxBomb;
    private NetworkVariable<float> netMaxHealth = new NetworkVariable<float>(0f,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Stat Caps")]
    [SerializeField] private float limitedMoveSpeed = 15f;
    [SerializeField] private int limitedBombRange = 6;
    [SerializeField] private int limitedBomb = 8;

    [Header("Damage Popup")]
    [SerializeField] private DamageNumberPopup damageNumberPrefab;
    [SerializeField] private Vector3 damagePopupOffset = new Vector3(0f, 4f, 0f);

    // ─────────────────────────────────────────────
    //  NetworkVariables  (Server writes – All read)
    // ─────────────────────────────────────────────
    // Lưu ý: NetworkVariable chỉ đồng bộ sau khi object được Spawn trên network.
    private NetworkVariable<float> netHealth = new NetworkVariable<float>(0f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<int> netCurrentBomb = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<int> netMaxBomb = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<float> netMoveSpeed = new NetworkVariable<float>(0f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<int> netBombRange = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<bool> netIsInvincible = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // ─────────────────────────────────────────────
    //  Public accessors
    // ─────────────────────────────────────────────
    public bool IsDead => netHealth.Value <= 0f;
    public bool IsInvincible
    {
        get => netIsInvincible.Value;
        set { if (IsServer) netIsInvincible.Value = value; }
    }

    // ─────────────────────────────────────────────
    //  Private state (local-only, không cần sync)
    // ─────────────────────────────────────────────
    private bool isPoisoned;
    private Coroutine poisonCoroutine;

    private bool isSlowed;
    private Coroutine slowCoroutine;
    private float currentSlowAmount = 0f;

    private PlayerControllerNetwork playerControllerNetwork;
    private HealthBar healthBar;
    private PlayerEffects playerEffects;

    // ─────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────
    private void Awake()
    {
        playerControllerNetwork = GetComponent<PlayerControllerNetwork>();
        healthBar = GetComponentInChildren<HealthBar>();
        playerEffects = GetComponent<PlayerEffects>();
    }

    /// <summary>
    /// Gọi sau khi NetworkObject được Spawn. Đây là điểm khởi đầu an toàn để đọc IsOwner.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // Đăng ký callback để cập nhật UI / local state mỗi khi NetworkVariable thay đổi.
        netHealth.OnValueChanged += OnHealthChanged;
        netCurrentBomb.OnValueChanged += OnCurrentBombChanged;
        netMaxBomb.OnValueChanged += OnMaxBombChanged;
        netMoveSpeed.OnValueChanged += OnMoveSpeedChanged;
        netBombRange.OnValueChanged += OnBombRangeChanged;
        netMaxHealth.OnValueChanged += OnMaxHealthChanged;


        // Chỉ owner mới subscribe GameEvents (bom của mình đặt thôi).
        //if (IsOwner)
        //{
        //    GameEventNetwork.OnBombPlaced += OnBombPlaced;
        //    GameEventNetwork.OnBombExploded += OnBombExploded;
        //}
    }

    public override void OnNetworkDespawn()
    {
        netHealth.OnValueChanged -= OnHealthChanged;
        netCurrentBomb.OnValueChanged -= OnCurrentBombChanged;
        netMaxBomb.OnValueChanged -= OnMaxBombChanged;
        netMoveSpeed.OnValueChanged -= OnMoveSpeedChanged;
        netBombRange.OnValueChanged -= OnBombRangeChanged;
        netMaxHealth.OnValueChanged -= OnMaxHealthChanged;

        //if (IsOwner)
        //{
        //    GameEventNetwork.OnBombPlaced -= OnBombPlaced;
        //    GameEventNetwork.OnBombExploded -= OnBombExploded;
        //}
    }

    // ─────────────────────────────────────────────
    //  Init  (Server calls this after spawning player)
    // ─────────────────────────────────────────────
    /// <summary>
    /// Chỉ gọi từ Server sau khi Spawn xong.
    /// Server ghi vào NetworkVariable → tất cả client tự nhận.
    /// </summary>
    public void Init(Characters data)
    {
        if (!IsServer) return;

        // Cache base stats (dùng để tính cap)
        PlayerHealth = data.playerHealth;
        MoveSpeed = data.moveSpeed;
        BombRange = data.bombRange;
        MaxBomb = data.maxBombs;

        // Ghi vào NetworkVariable → tự broadcast
        netMaxHealth.Value = PlayerHealth;
        netHealth.Value = PlayerHealth;
        netCurrentBomb.Value = MaxBomb;
        netMaxBomb.Value = MaxBomb;
        netMoveSpeed.Value = MoveSpeed;
        netBombRange.Value = BombRange;
        netIsInvincible.Value = false;
        //healthBar?.Init(PlayerHealth); 
    }

    // ─────────────────────────────────────────────
    //  NetworkVariable Callbacks  (chạy trên mọi client)
    // ─────────────────────────────────────────────
    private void OnHealthChanged(float prev, float next)
    {
        healthBar?.UpdateHealth(next);

        // Chỉ hiện HUD của chính mình
        if (IsOwner && HUDManager.instance != null)
            HUDManager.instance.InitPlayerStats(PlayerHealth, netMaxBomb.Value, netBombRange.Value, netMoveSpeed.Value);
    }

    private void OnMaxHealthChanged(float prev, float next)
    {
        PlayerHealth = next;
        healthBar?.Init(next);

        if (IsOwner && HUDManager.instance != null)
            HUDManager.instance.InitPlayerStats(next, netMaxBomb.Value, netBombRange.Value, netMoveSpeed.Value);
    }

    private void OnCurrentBombChanged(int prev, int next)
    {
        if (IsOwner && HUDManager.instance != null)
            HUDManager.instance.UpdateMaxBombText(next);
    }

    private void OnMaxBombChanged(int prev, int next)
    {
        if (IsOwner && HUDManager.instance != null)
            HUDManager.instance.UpdateMaxBombText(netCurrentBomb.Value);
    }

    private void OnMoveSpeedChanged(float prev, float next)
    {
        MoveSpeed = next; // cập nhật local field để PlayerController dùng
        if (IsOwner && HUDManager.instance != null)
            HUDManager.instance.UpdateSpeedText(next);
    }

    private void OnBombRangeChanged(int prev, int next)
    {
        BombRange = next;
        if (IsOwner && HUDManager.instance != null)
            HUDManager.instance.UpdateBombRangeText(next);
    }

    // ─────────────────────────────────────────────
    //  Bomb Events  (chỉ owner subscribe)
    // ─────────────────────────────────────────────
    //private void OnBombPlaced(ulong ownerClientId)
    //{
    //    if (!IsOwner) return;
    //    if (OwnerClientId != ownerClientId) return;
    //    PlaceBombServerRpc();
    //}

    //private void OnBombExploded(ulong ownerClientId)
    //{
    //    if (!IsOwner) return;
    //    if (OwnerClientId != ownerClientId) return;
    //    BombExplodedServerRpc();
    //}

    //[ServerRpc]
    //private void PlaceBombServerRpc()
    //{
    //    if (netCurrentBomb.Value <= 0) return;
    //    netCurrentBomb.Value--;
    //}

    //[ServerRpc]
    //private void BombExplodedServerRpc()
    //{
    //    netCurrentBomb.Value = Mathf.Min(netCurrentBomb.Value + 1, netMaxBomb.Value);
    //}

    public bool CanPlaceBomb() => netCurrentBomb.Value > 0;

    // ─────────────────────────────────────────────
    //  Stat Upgrades
    // ─────────────────────────────────────────────
    public void IncreaseMaxBomb(int amount)
    {
        if (!IsServer) return;
        if (netMaxBomb.Value >= limitedBomb) return;

        netMaxBomb.Value = Mathf.Min(netMaxBomb.Value + amount, limitedBomb);
        netCurrentBomb.Value = Mathf.Min(netCurrentBomb.Value + amount, netMaxBomb.Value);
    }

    public void IncreaseBombRange(int amount)
    {
        if (!IsServer) return;
        if (netBombRange.Value >= limitedBombRange) return;
        netBombRange.Value = Mathf.Min(netBombRange.Value + amount, limitedBombRange);
    }

    public void IncreaseMoveSpeed(float amount)
    {
        if (!IsServer) return;
        if (netMoveSpeed.Value >= limitedMoveSpeed) return;
        netMoveSpeed.Value = Mathf.Min(netMoveSpeed.Value + amount, limitedMoveSpeed);
    }

    // ─────────────────────────────────────────────
    //  Damage / Heal
    // ─────────────────────────────────────────────

    /// <summary>
    /// Bất kỳ client nào cũng có thể yêu cầu gây damage (ví dụ: bom nổ).
    /// Server là người thực sự xử lý logic.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (IsDead || netIsInvincible.Value) return;

        float oldHealth = netHealth.Value;
        netHealth.Value = Mathf.Max(0f, oldHealth - damage);

        int deducted = Mathf.RoundToInt(oldHealth - netHealth.Value);

        // Yêu cầu tất cả client hiện popup số damage
        if (deducted > 0)
            ShowDamagePopupClientRpc(deducted, false);

        if (netHealth.Value <= 0f)
            DieClientRpc();
        else
            TriggerDamageEffectsClientRpc();
    }

    // Giữ lại để tương thích với code cũ gọi TakeDamage() trực tiếp (ví dụ: PoisonRoutine trên server)
    public void TakeDamage(int damage)
    {
        if (IsServer)
        {
            TakeDamageServerRpc(damage);
        }
        else
        {
            // Client không phải server thì phải đi qua RPC
            TakeDamageServerRpc(damage);
        }
    }

    public void Heal(int amount)
    {
        if (IsServer)
            HealInternal(amount);
        else
            HealServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void HealServerRpc(int amount) => HealInternal(amount);

    private void HealInternal(int amount)
    {
        if (amount <= 0 || IsDead) return;

        float oldHealth = netHealth.Value;
        netHealth.Value = Mathf.Min(oldHealth + amount, PlayerHealth);

        int healed = Mathf.RoundToInt(netHealth.Value - oldHealth);
        if (healed > 0)
            ShowDamagePopupClientRpc(healed, true);
    }

    public float GetCurrentHealth() => netHealth.Value;

    // ─────────────────────────────────────────────
    //  ClientRpcs  (broadcast hiệu ứng)
    // ─────────────────────────────────────────────
    [ClientRpc]
    private void ShowDamagePopupClientRpc(int amount, bool isHeal)
    {
        if (damageNumberPrefab == null || amount <= 0) return;

        Vector3 pos = transform.position + damagePopupOffset;
        var popupGo = ObjectPoolingManager.Instance.Spawn(damageNumberPrefab.gameObject, pos, Quaternion.identity);
        var popup = popupGo.GetComponent<DamageNumberPopup>();

        if (isHeal) popup.InitHeal(amount);
        else popup.InitDamage(amount);
    }

    [ClientRpc]
    private void TriggerDamageEffectsClientRpc()
    {
        playerControllerNetwork?.TriggerDamagedAnimation();
        playerEffects?.PlayGetHitSound();
        playerEffects?.PlayDamageEffect();
        Debug.Log($"[{OwnerClientId}] HP: {netHealth.Value}/{PlayerHealth}");
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        Debug.Log($"[{OwnerClientId}] Player has died!");
        playerControllerNetwork?.TriggerDeathAnimation();

        //if (IsOwner)
            //GameManager.Instance?.OnGameLose();

        // Server chịu trách nhiệm Destroy object
        if (IsServer)
        {
            var no = GetComponent<NetworkObject>();
            StartCoroutine(DespawnAfterDelay(no, 1f));
        }
    }

    private IEnumerator DespawnAfterDelay(NetworkObject no, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (no != null && no.IsSpawned)
            no.Despawn(true); // true = destroy gameObject sau khi despawn
    }

    // ─────────────────────────────────────────────
    //  Poison
    // ─────────────────────────────────────────────
    /// <summary>
    /// Chỉ chạy trên Server để tránh damage bị tính nhiều lần.
    /// </summary>
    public void ApplyPoison(int damagePerSecond, float duration)
    {
        if (!IsServer) return;
        if (IsDead || netIsInvincible.Value) return;

        if (!isPoisoned)
            PlayPoisonEffectClientRpc();

        if (isPoisoned && poisonCoroutine != null)
            StopCoroutine(poisonCoroutine);

        poisonCoroutine = StartCoroutine(PoisonRoutine(damagePerSecond, duration));
    }

    private IEnumerator PoisonRoutine(int damagePerSecond, float duration)
    {
        isPoisoned = true;
        float elapsed = 0f;
        Debug.Log($"[Server] Player {OwnerClientId} bị trúng độc!");

        while (elapsed < duration)
        {
            if (IsDead) yield break;
            TakeDamageServerRpc(damagePerSecond);
            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }

        isPoisoned = false;
        poisonCoroutine = null;
        Debug.Log($"[Server] Player {OwnerClientId} hết trúng độc.");
    }

    [ClientRpc]
    private void PlayPoisonEffectClientRpc() => playerEffects?.PlayPoisonedEffect();

    // ─────────────────────────────────────────────
    //  Slow
    // ─────────────────────────────────────────────
    /// <summary>
    /// Chỉ chạy trên Server. MoveSpeed được sync qua NetworkVariable.
    /// </summary>
    public void ApplySlow(float slowPercent, float duration)
    {
        if (!IsServer) return;
        if (IsDead || netIsInvincible.Value) return;

        if (!isSlowed)
            PlaySlowEffectClientRpc();

        if (isSlowed && slowCoroutine != null)
            StopCoroutine(slowCoroutine);

        slowCoroutine = StartCoroutine(SlowRoutine(slowPercent, duration));
    }

    private IEnumerator SlowRoutine(float slowPercent, float duration)
    {
        isSlowed = true;

        // Hoàn trả slow cũ nếu bị stack
        netMoveSpeed.Value += currentSlowAmount;
        // Áp dụng slow mới
        currentSlowAmount = slowPercent;
        netMoveSpeed.Value -= currentSlowAmount;

        yield return new WaitForSeconds(duration);

        netMoveSpeed.Value += currentSlowAmount;
        currentSlowAmount = 0f;
        isSlowed = false;
        slowCoroutine = null;
    }

    [ClientRpc]
    private void PlaySlowEffectClientRpc() => playerEffects?.PlaySlowEffect();

    public bool TryConsumeBombOnServer()
    {
        if (!IsServer) return false;
        if (netCurrentBomb.Value <= 0) return false;

        netCurrentBomb.Value--;
        return true;
    }

    public void RestoreBombOnServer()
    {
        if (!IsServer) return;
        netCurrentBomb.Value = Mathf.Min(netCurrentBomb.Value + 1, netMaxBomb.Value);
    }
}