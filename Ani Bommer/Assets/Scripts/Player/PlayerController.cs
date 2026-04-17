using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerInputAction playerInputAction;
    private CharacterController controller;
    private PlayerSkills playerSkills;
    private Animator animator;
    private PlayerStats playerStats;
    private BombExplode bombExplode;
    private PlayerEffects playerEffects;

    [Header("Data Source")]
    [SerializeField] private GameDatabase gameDatabase;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Characters characterData;

    [Header("Pool Data")]
    [SerializeField] private int bombPoolSize = 10;
    [SerializeField] private int explosionPoolSize = 50;
    [SerializeField] private bool prewarmOnStart = true;

    private void Awake()
    {
        playerInputAction = new PlayerInputAction();
        Application.targetFrameRate = 150;
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        playerSkills = GetComponent<PlayerSkills>();
        playerEffects = GetComponent<PlayerEffects>();
        bombExplode = bombPrefab.GetComponent<BombExplode>();
        GetEquippedBombPrefab();
    }
    private void Start()
    {
        playerStats.Init(characterData);
        playerSkills.Init(characterData);
        bombExplode.Init(playerStats.BombRange);

    }

    private void OnEnable()
    {
        playerInputAction.Enable();
        playerInputAction.PlayerController.Skill1.performed += _ => playerSkills.UseSkill(0);
        playerInputAction.PlayerController.Skill2.performed += _ => playerSkills.UseSkill(1);
        playerInputAction.PlayerController.Skill3.performed += _ => playerSkills.UseSkill(2);
    }

    private void OnDisable()
    {
        playerInputAction.Disable();
    }





    void Update()
    {
        Vector2 movementInput = playerInputAction.PlayerController.Move.ReadValue<Vector2>();
        //Debug.Log(playerInput.PlayerController.Move.ReadValue<Vector2>());
        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
        controller.Move(move * Time.deltaTime * playerStats.MoveSpeed);

        // Rotate player towards movement direction
        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        // Animation
        bool isRunning = move.magnitude > 0.01f;
        animator.SetBool("IsRunning", isRunning);

        // MPlace bomb
        if (playerInputAction.PlayerController.PlaceBomb.triggered)
        {
            PlaceBomb();
        }


    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.y = 0.5f;
        pos.x = Mathf.Clamp(pos.x, -18f, 18f);
        pos.z = Mathf.Clamp(pos.z, -12f, 12f);
        transform.position = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerStats.IsInvincible)
            return;

        if (other.CompareTag("Explosion"))
        {
            TriggerDeathAnimation();

            playerStats.TakeDamage((int)playerStats.GetCurrentHealth());
            Destroy(this.gameObject, 0.3f);
        }
    }

    private void PlaceBomb()
    {

        //float snapX = Mathf.Round(transform.position.x / tileSize) * tileSize;
        //float snapZ = Mathf.Round(transform.position.z / tileSize) * tileSize;

        Vector2Int grid = GridMapSpawner.Instance.WorldToGrid(transform.position);

        if (!playerStats.CanPlaceBomb() || !GridMapSpawner.Instance.CanPlaceBomb(grid))
        {
            return;
        }
        Vector3 worldPos = GridMapSpawner.Instance.GridToWorld(grid);

        GameObject bombObj = ObjectPoolingManager.Instance.Spawn(bombPrefab, new Vector3(worldPos.x, 1f, worldPos.z), bombPrefab.transform.rotation);
        //GameObject bombObj = Instantiate(bombPrefab, new Vector3(worldPos.x,1f,worldPos.z),bombPrefab.transform.rotation);
        var bomb = bombObj.GetComponent<BombExplode>();
        bomb.Init(playerStats.BombRange);
        bomb.InitPos(grid);
        //bomb.StartCountdown();
        //bombObj.GetComponent<BombExplode>().Init(playerStats.BombRange);
        //bombObj.GetComponent<BombExplode>().InitPos(grid);
        GridMapSpawner.Instance.PlaceBomb(grid);
        playerEffects.PlayPutBombSound();
        GameEvents.OnBombPlaced?.Invoke();

    }

    public void TriggerDamagedAnimation()
    {
        animator.SetTrigger("IsDamaged");
    }

    public void TriggerDeathAnimation()
    {
        animator.SetTrigger("IsDead");
    }

    public void TriggerThrowAnimation()
    {
        animator.SetTrigger("IsThrowing");
    }

    private void GetEquippedBombPrefab()
    {
        // Nếu thiếu data thì giữ nguyên bombPrefab fallback trong Inspector
        if (gameDatabase == null || DataManager.Instance == null || DataManager.Instance.PlayerData == null)
        {
            return;
        }

        string equippedBombId = DataManager.Instance.PlayerData.equippedBombId;
        if (string.IsNullOrEmpty(equippedBombId))
        {
            return;
        }

        BombConfig bombConfig = gameDatabase.GetBomb(equippedBombId);
        if (bombConfig == null)
        {
            Debug.LogWarning($"PlayerController: cannot find BombConfig with id '{equippedBombId}'.");
            return;
        }

        if (bombConfig.prefab == null)
        {
            Debug.LogWarning($"PlayerController: BombConfig '{equippedBombId}' has null prefab.");
            return;
        }

        bombPrefab = bombConfig.prefab;
        PrewarmEquippedBombAndExplosion(bombPrefab);
    }

    private void PrewarmEquippedBombAndExplosion(GameObject equippedBombPrefab)
    {
        if (!prewarmOnStart) return;
        if (equippedBombPrefab == null) return;
        if (ObjectPoolingManager.Instance == null) return;

        // pool bomb đang equip
        ObjectPoolingManager.Instance.Warm(equippedBombPrefab, bombPoolSize);

        // lấy explosionEffect tương ứng từ BombExplode trên bomb prefab
        var bombExplode = equippedBombPrefab.GetComponent<BombExplode>();
        if (bombExplode == null) return;

        var explosionPrefab = bombExplode.ExplosionEffectPrefab;
        if (explosionPrefab == null) return;

        ObjectPoolingManager.Instance.Warm(explosionPrefab, explosionPoolSize);
    }
}