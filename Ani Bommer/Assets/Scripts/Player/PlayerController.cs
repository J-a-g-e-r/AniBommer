using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Player playerInput;
    private CharacterController controller;
    private PlayerSkills playerSkills;
    private Animator animator;
    private PlayerStats playerStats;
    private BombExplode bombExplode;

    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Characters characterData;


    private void Awake()
    {
        playerInput = new Player();
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        playerSkills = GetComponent<PlayerSkills>();
        bombExplode = bombPrefab.GetComponent<BombExplode>();
    }
    private void Start()
    {
        playerStats.Init(characterData);
        playerSkills.Init(characterData);
        bombExplode.Init(playerStats.BombRange);

    }

    private void OnEnable()
    {
        playerInput.Enable();
        playerInput.PlayerController.Skill1.performed += _ => playerSkills.UseSkill(0);
        playerInput.PlayerController.Skill2.performed += _ => playerSkills.UseSkill(1);
        playerInput.PlayerController.Skill3.performed += _ => playerSkills.UseSkill(2);
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }





    void Update()
    {
        Vector2 movementInput = playerInput.PlayerController.Move.ReadValue<Vector2>();
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
        if (playerInput.PlayerController.PlaceBomb.triggered)
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
        if (other.CompareTag("Explosion"))
        {
            TriggerDeathAnimation();
            
            playerStats.TakeDamage((int)playerStats.GetCurrentHealth());
            Destroy(this.gameObject,0.3f);
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

        GameObject bombObj = Instantiate(bombPrefab, new Vector3(worldPos.x,1f,worldPos.z),bombPrefab.transform.rotation);
        bombObj.GetComponent<BombExplode>().Init(playerStats.BombRange);
        bombObj.GetComponent<BombExplode>().InitPos(grid);
        GridMapSpawner.Instance.PlaceBomb(grid);
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
}