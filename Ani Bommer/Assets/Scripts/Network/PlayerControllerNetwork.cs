using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerControllerNetwork : NetworkBehaviour
{
    private PlayerInputAction playerInput;
    private CharacterController controller;
    private PlayerSkills playerSkills;
    private Animator animator;
    private PlayerStatsNetwork playerStatsNetwork;
    private PlayerEffects playerEffects;

    [Header("Data Source")]
    [SerializeField] private GameDatabase gameDatabase;
    [SerializeField] private Characters characterData;
    [SerializeField] private float explosionHitCooldown = 0.15f;



    private void Awake()
    {
        playerInput = new PlayerInputAction();
        Application.targetFrameRate = 150;
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerStatsNetwork = GetComponent<PlayerStatsNetwork>();
        playerSkills = GetComponent<PlayerSkills>();
        playerEffects = GetComponent<PlayerEffects>();
    }
    private void Start()
    {
        playerStatsNetwork.Init(characterData);
        playerSkills.Init(characterData);
    }

    private void OnEnable()
    {
        //playerInput.Enable();
        //playerInput.PlayerController.Skill1.performed += _ => playerSkills.UseSkill(0);
        //playerInput.PlayerController.Skill2.performed += _ => playerSkills.UseSkill(1);
        //playerInput.PlayerController.Skill3.performed += _ => playerSkills.UseSkill(2);
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }



    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        BindOwnerInput();
        var camSetter = FindFirstObjectByType<CinemachineTargetSetter>();
        if (camSetter != null)
        {
            camSetter.SetTarget(transform);
        }

        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.RegisterLocalPlayer(gameObject);
        }
    }

    public override void OnNetworkDespawn()
    {
        UnbindOwnerInput();
        base.OnNetworkDespawn();
    }

    private void BindOwnerInput()
    {
        playerInput.Enable();
        playerInput.PlayerController.Skill1.performed += OnSkill1Performed;
        playerInput.PlayerController.Skill2.performed += OnSkill2Performed;
        playerInput.PlayerController.Skill3.performed += OnSkill3Performed;
    }

    private void UnbindOwnerInput()
    {
        playerInput.PlayerController.Skill1.performed -= OnSkill1Performed;
        playerInput.PlayerController.Skill2.performed -= OnSkill2Performed;
        playerInput.PlayerController.Skill3.performed -= OnSkill3Performed;
        playerInput.Disable();
    }

    private void OnSkill1Performed(InputAction.CallbackContext _)
    {
        if (!IsOwner) return;
        RequestUseSkillServerRpc(0);
    }

    private void OnSkill2Performed(InputAction.CallbackContext _)
    {
        if (!IsOwner) return;
        RequestUseSkillServerRpc(1);
    }

    private void OnSkill3Performed(InputAction.CallbackContext _)
    {
        if (!IsOwner) return;
        RequestUseSkillServerRpc(2);
    }

    [ServerRpc]
    private void RequestUseSkillServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId) return;
        if (index < 0 || index >= 3) return;

        // Server thực thi logic/cooldown thật
        playerSkills.UseSkill(index);

        // Broadcast cho mọi client thấy hiệu ứng skill
        UseSkillClientRpc(index);
    }

    [ClientRpc]
    private void UseSkillClientRpc(int index)
    {
        if (index < 0 || index >= 3) return;

        // Host đã cast ở server rồi -> tránh cast double
        if (IsServer) return;

        playerSkills.UseSkill(index);
    }


    void Update()
    {
        if (!IsOwner) return;
        Vector2 movementInput = playerInput.PlayerController.Move.ReadValue<Vector2>();
        //Debug.Log(playerInput.PlayerController.Move.ReadValue<Vector2>());
        Vector3 move = new Vector3(movementInput.x, 0, movementInput.y);
        controller.Move(move * Time.deltaTime * playerStatsNetwork.MoveSpeed);

        // Rotate player towards movement direction
        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        // Animation
        bool isRunning = move.magnitude > 0.01f;
        animator.SetBool("IsRunning", isRunning);

        // MPlace bomb
        if (playerInput.PlayerController.PlaceBomb.triggered)
        {
            TryPlaceBomb();
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

    private float lastExplosionHitTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return; // chỉ owner của player này mới gửi damage request
        if (playerStatsNetwork.IsInvincible) return;
        if (!other.CompareTag("Explosion")) return;

        // Chặn ăn nhiều hit trong cùng 1 cụm nổ
        if (Time.time - lastExplosionHitTime < explosionHitCooldown) return;
        lastExplosionHitTime = Time.time;

        playerStatsNetwork.TakeDamage(100);
    }

    private void TryPlaceBomb()
    {
        if (!IsOwner) return;

        Vector2Int grid = GridMapSpawnerNetwork.Instance.WorldToGrid(transform.position);

        // Validate trước ở client để tránh spam RPC không cần thiết
        if (!playerStatsNetwork.CanPlaceBomb() || !GridMapSpawnerNetwork.Instance.CanPlaceBomb(grid))
        {
            return;
        }

        PlaceBombServerRpc(grid.x, grid.y);
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



    [ServerRpc]
    private void PlaceBombServerRpc(int gridX, int gridY, ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != OwnerClientId) return;

        Vector2Int grid = new Vector2Int(gridX, gridY);

        // Validate server
        if (!GridMapSpawnerNetwork.Instance.CanPlaceBomb(grid))
            return;

        // Trừ bomb tại server (nguồn sự thật)
        if (!playerStatsNetwork.TryConsumeBombOnServer())
            return;

        Vector3 worldPos = GridMapSpawnerNetwork.Instance.GridToWorld(grid);

        GameObject bombPrefab = GameplayManager.Instance?.GetMapBombPrefab();
        if (bombPrefab == null)
        {
            Debug.LogError("[PlayerControllerNetwork] mapBombPrefab is null.");
            playerStatsNetwork.RestoreBombOnServer();
            return;
        }

        GameObject bombObj = Instantiate(
            bombPrefab,
            new Vector3(worldPos.x, 1f, worldPos.z),
            bombPrefab.transform.rotation
        );

        var netObj = bombObj.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("Bomb prefab missing NetworkObject.");
            Destroy(bombObj);
            playerStatsNetwork.RestoreBombOnServer();
            return;
        }

        var bomb = bombObj.GetComponent<BombExplodeNetwork>();
        if (bomb == null)
        {
            Debug.LogError("Bomb prefab missing BombExplodeNetwork.");
            Destroy(bombObj);
            playerStatsNetwork.RestoreBombOnServer();
            return;
        }

        netObj.Spawn(true);
        bomb.Init(playerStatsNetwork.BombRange);
        bomb.InitPos(grid);
        bomb.SetOwnerClientId(OwnerClientId);

        GridMapSpawnerNetwork.Instance.PlaceBomb(grid);

        // Chỉ để âm thanh / local event phụ trợ
        OnBombPlacedClientRpc(OwnerClientId);
    }

    [ClientRpc]
    private void OnBombPlacedClientRpc(ulong ownerClientId)
    {
        // Local audio/effect
        if (playerEffects != null)
        {
            playerEffects.PlayPutBombSound();
        }

        // Broadcast local event (mọi máy đều nhận cùng ownerId)
        GameEventNetwork.RaiseBombPlaced(ownerClientId);
    }
}