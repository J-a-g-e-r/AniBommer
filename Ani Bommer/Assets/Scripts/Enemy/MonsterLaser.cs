using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class MonsterLaser : MonoBehaviour, IMonsterAttack
{
    private enum LaserState { Idle, Tracking, Locked, Firing, Cooldown }

    [Header("Refs")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject rotateRoot;
    [SerializeField] private MonsterTargetSensor sensor;
    [SerializeField] private MonsterController monsterController;

    [Header("Visuals")]
    [SerializeField] private LineRenderer warningLine; // vùng đỏ cảnh báo
    [SerializeField] private LineRenderer laserLine;   // tia laser thật

    [Header("Combat")]
    [SerializeField] private int damagePerTick = 1;
    [SerializeField] private float tickInterval = 0.15f;
    [SerializeField] private float maxRange = 14f;
    [SerializeField] private LayerMask hitMask; // Player + Environment + Shield

    [Header("Timing")]
    [SerializeField] private float trackingTime = 0.8f; // đỏ đuổi player
    [SerializeField] private float lockDelay = 0.5f;    // đứng yên trước khi bắn
    [SerializeField] private float firingTime = 1.0f;   // thời gian bắn
    [SerializeField] private float cooldown = 2.0f;

    [Header("Rotate")]
    [SerializeField] private float rotateSpeed = 8f;

    private LaserState state = LaserState.Idle;
    private GameObject target;
    private float stateTimer;
    private float tickTimer;

    // hướng đã chốt khi vào Locked/Firing
    private Vector3 lockedDirection = Vector3.forward;

    private void Awake()
    {
        if (monsterController == null)
            monsterController = GetComponent<MonsterController>();

        SetLineEnabled(warningLine, false);
        SetLineEnabled(laserLine, false);
    }

    private void OnEnable()
    {
        if (sensor != null)
        {
            sensor.OnTargetEnter += OnTargetEnter;
            sensor.OnTargetExit += OnTargetExit;
        }
    }

    private void OnDisable()
    {
        if (sensor != null)
        {
            sensor.OnTargetEnter -= OnTargetEnter;
            sensor.OnTargetExit -= OnTargetExit;
        }
    }

    private void Update()
    {
        if (target == null)
        {
            ChangeState(LaserState.Idle, 0f);
            return;
        }

        if (stateTimer > 0f) stateTimer -= Time.deltaTime;
        if (tickTimer > 0f) tickTimer -= Time.deltaTime;

        switch (state)
        {
            case LaserState.Idle:
                Attack(target);
                break;

            case LaserState.Tracking:
                UpdateTrackingAndWarning();
                if (stateTimer <= 0f)
                {
                    // chốt hướng tại thời điểm hết tracking
                    lockedDirection = GetAimDirectionToTarget();
                    ChangeState(LaserState.Locked, lockDelay);
                }
                break;

            case LaserState.Locked:
                // warning đứng yên, không đuổi player nữa
                DrawLine(warningLine, firePoint.position, GetEndPoint(firePoint.position, lockedDirection));
                if (stateTimer <= 0f)
                {
                    monsterController?.TriggerAttackAnimation(); // gần bắn mới trigger anim
                    ChangeState(LaserState.Firing, firingTime);
                    tickTimer = 0f;
                    SetLineEnabled(warningLine, false);
                    SetLineEnabled(laserLine, true);
                }
                break;

            case LaserState.Firing:
                FireLockedLaser();
                if (stateTimer <= 0f)
                {
                    SetLineEnabled(laserLine, false);
                    ChangeState(LaserState.Cooldown, cooldown);
                }
                break;

            case LaserState.Cooldown:
                RotateTowardsTarget(); // có thể vẫn quay cho tự nhiên
                if (stateTimer <= 0f)
                {
                    ChangeState(LaserState.Idle, 0f);
                }
                break;
        }
    }

    public void Attack(GameObject targetObj)
    {
        if (targetObj == null || firePoint == null) return;
        ChangeState(LaserState.Tracking, trackingTime);
        SetLineEnabled(warningLine, true);
        SetLineEnabled(laserLine, false);
    }

    private void UpdateTrackingAndWarning()
    {
        RotateTowardsTarget();

        Vector3 liveDir = GetAimDirectionToTarget();
        lockedDirection = liveDir; // trong tracking thì hướng đỏ đang đuổi theo
        DrawLine(warningLine, firePoint.position, GetEndPoint(firePoint.position, liveDir));
    }

    private void FireLockedLaser()
    {
        Vector3 origin = firePoint.position;
        if (Physics.Raycast(origin, lockedDirection, out RaycastHit hit, maxRange, hitMask, QueryTriggerInteraction.Ignore))
        {
            DrawLine(laserLine, origin, hit.point);

            if (hit.collider.CompareTag("Player") && tickTimer <= 0f)
            {
                hit.collider.GetComponent<PlayerStats>()?.TakeDamage(damagePerTick);
                tickTimer = tickInterval;
            }
        }
        else
        {
            DrawLine(laserLine, origin, origin + lockedDirection * maxRange);
        }
    }

    private void RotateTowardsTarget()
    {
        if (rotateRoot == null || target == null) return;

        Vector3 dir = target.transform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        rotateRoot.transform.rotation = Quaternion.Slerp(
            rotateRoot.transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }

    private Vector3 GetAimDirectionToTarget()
    {
        Vector3 dir = target.transform.position - firePoint.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return firePoint.forward;
        return dir.normalized;
    }

    private Vector3 GetEndPoint(Vector3 origin, Vector3 dir)
    {
        if (Physics.Raycast(origin, dir, out RaycastHit hit, maxRange, hitMask, QueryTriggerInteraction.Ignore))
            return hit.point;

        return origin + dir * maxRange;
    }

    private void DrawLine(LineRenderer line, Vector3 a, Vector3 b)
    {
        if (line == null) return;
        line.positionCount = 2;
        Vector3 upOffset = Vector3.up * 1.2f;
        line.SetPosition(0, a + upOffset);
        line.SetPosition(1, b + upOffset);
    }

    private void SetLineEnabled(LineRenderer line, bool enabled)
    {
        if (line != null) line.enabled = enabled;
    }

    private void ChangeState(LaserState newState, float timer)
    {
        if (state == newState) return;
        state = newState;
        stateTimer = timer;

        if (newState == LaserState.Idle)
        {
            SetLineEnabled(warningLine, false);
            SetLineEnabled(laserLine, false);
        }
    }

    private void OnTargetEnter(Transform t)
    {
        target = t != null ? t.gameObject : null;
    }

    private void OnTargetExit()
    {
        target = null;
        ChangeState(LaserState.Idle, 0f);
    }
}