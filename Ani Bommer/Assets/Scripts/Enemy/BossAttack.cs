using UnityEngine;

public class BossAttack : MonoBehaviour, IMonsterAttack
{
    [Header("Ranged Attack")]
    [SerializeField] private float cooldown = 1.5f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private MonsterTargetSensor sensor;

    [Header("Pattern Settings")]
    [SerializeField] private int radialBulletCount = 8;
    [SerializeField] private float fanAngle = 60f;
    [SerializeField] private int fanBulletCount = 5;

    [Header("Melee Contact Attack")]
    [SerializeField] private int meleeDamage = 1;
    [SerializeField] private float meleeCooldown = 1f;
    [SerializeField] private Collider attackCollider; // hitbox gần 

    private float nextRangedAttackTime;
    private float lastMeleeAttackTime;
    private int attackIndex = 0;

    private GameObject currentTarget;

    private void Awake()
    {
        if (attackCollider == null)
            attackCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        if (sensor != null)
        {
            sensor.OnTargetEnter += HandleTargetEnter;
            sensor.OnTargetExit += HandleTargetExit;
        }
    }

    private void OnDisable()
    {
        if (sensor != null)
        {
            sensor.OnTargetEnter -= HandleTargetEnter;
            sensor.OnTargetExit -= HandleTargetExit;
        }
    }

    private void Update()
    {
        // Đánh xa khi có target trong vùng sensor
        if (currentTarget != null)
            Attack(currentTarget);
    }

    private void HandleTargetEnter(Transform targetTransform)
    {
        currentTarget = targetTransform != null ? targetTransform.gameObject : null;
    }

    private void HandleTargetExit()
    {
        currentTarget = null;
    }

    // IMonsterAttack: pattern luân phiên
    public void Attack(GameObject target)
    {
        if (target == null) return;
        if (Time.time < nextRangedAttackTime) return;

        nextRangedAttackTime = Time.time + cooldown;

        switch (attackIndex)
        {
            case 0:
                AttackSingle(target);
                break;
            case 1:
                AttackRadial();
                break;
            case 2:
                AttackFan(target, fanBulletCount);
                break;
        }

        attackIndex = (attackIndex + 1) % 3;

        var controller = GetComponent<MonsterController>();
        controller?.TriggerAttackAnimation();
    }

    private void AttackSingle(GameObject target)
    {
        if (firePoint == null || bulletPrefab == null) return;

        Vector3 dir = target.transform.position - firePoint.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) dir = transform.forward;

        Instantiate(bulletPrefab, firePoint.position + new Vector3(0,1f,0), Quaternion.LookRotation(dir));
    }

    private void AttackRadial()
    {
        if (firePoint == null || bulletPrefab == null || radialBulletCount <= 0) return;

        float step = 360f / radialBulletCount;
        for (int i = 0; i < radialBulletCount; i++)
        {
            float angle = i * step;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Instantiate(bulletPrefab, firePoint.position + new Vector3(0, 1f, 0), Quaternion.LookRotation(dir));
        }
    }

    private void AttackFan(GameObject target, int bulletCount)
    {
        if (firePoint == null || bulletPrefab == null || target == null || bulletCount <= 0) return;

        Vector3 baseDir = (target.transform.position - firePoint.position).normalized;
        baseDir.y = 0f;
        if (baseDir.sqrMagnitude < 0.001f) baseDir = transform.forward;

        float start = -fanAngle * 0.5f;
        float step = bulletCount == 1 ? 0f : fanAngle / (bulletCount - 1);

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = start + step * i;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * baseDir;
            Instantiate(bulletPrefab, firePoint.position + new Vector3(0, 1f, 0), Quaternion.LookRotation(dir));
        }
    }

    // Đánh gần khi player chạm vào melee hitbox
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (attackCollider == null) return;
        if (!other.bounds.Intersects(attackCollider.bounds)) return;

        AttackMelee(other.gameObject);
    }

    private void AttackMelee(GameObject target)
    {
        if (Time.time - lastMeleeAttackTime < meleeCooldown) return;

        PlayerStats player = FinderHelper.GetComponentOnObject<PlayerStats>(target);
        if (player == null) return;

        player.TakeDamage(meleeDamage);
        lastMeleeAttackTime = Time.time;

        var controller = GetComponent<MonsterController>();
        controller?.TriggerAttackAnimation();
    }
}