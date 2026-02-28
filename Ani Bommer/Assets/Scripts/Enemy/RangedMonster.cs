using UnityEngine;

public class RangedMonster : MonoBehaviour, IMonsterAttack
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float rotateSpeed = 8f;
    [SerializeField] private GameObject rotateRoot;

    private float attackTimer;
    private GameObject target;

    private void Awake()
    {
        MonsterBullet bullet = projectilePrefab.GetComponent<MonsterBullet>();
        if (bullet != null)
        {
            bullet.GetDamage(damage);
        }
    }

    private void Update()
    {
        if (target == null) return;

        RotateTowardsTarget();
        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            Attack(target);
            attackTimer = attackCooldown;
        }
    }

    public void Attack(GameObject target)
    {
        Vector3 dir = (target.transform.position - firePoint.position);
        dir.y = 0f;
        MonsterController controller = GetComponent<MonsterController>();
        controller?.TriggerAttackAnimation();
        Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(dir)
        );

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            target = other.gameObject;
         
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            target = null;
        }
    }

    private void RotateTowardsTarget()
    {
        Vector3 dir = target.transform.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        rotateRoot.transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }
}