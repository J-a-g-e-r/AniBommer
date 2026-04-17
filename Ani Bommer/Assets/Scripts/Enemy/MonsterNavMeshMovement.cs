using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MonsterNavMeshMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float repathInterval = 0.15f;
    [SerializeField] private float stopDistance = 0.6f;

    private Transform player;
    private NavMeshAgent agent;
    private float repathTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = stopDistance;
        agent.autoBraking = true;
        agent.updateRotation = true;
        agent.updateUpAxis = true;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerSpawned += SetTarget;
        GameEvents.OnMapReady += TryWarpToNavMesh;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerSpawned -= SetTarget;
        GameEvents.OnMapReady -= TryWarpToNavMesh;
    }

    private void Start()
    {
        SetTarget();
        TryWarpToNavMesh();
    }

    private void Update()
    {
        if (player == null)
        {
            SetTarget();
            return;
        }

        repathTimer -= Time.deltaTime;
        if (repathTimer > 0f) return;
        repathTimer = repathInterval;

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }

    public void SetTarget()
    {
        player = FindAnyObjectByType<PlayerController>()?.transform;
    }

    private void TryWarpToNavMesh()
    {
        if (agent == null) return;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            if (!agent.isOnNavMesh)
            {
                agent.Warp(hit.position);
            }
        }
        else
        {
            Debug.LogWarning($"MonsterNavMeshMovement: No NavMesh near {name}");
        }
    }
}