using UnityEngine;

public enum MonsterType
{
    Normal,
    Poison,
    Ice,
}

public class MeleeMonster : MonoBehaviour, IMonsterAttack
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private MonsterType monsterType = MonsterType.Normal;
    private float lastAttackTime;
    private BoxCollider attackCollider;

    private void Start()
    {
        // Lấy BoxCollider gắn trên cùng GameObject này
        attackCollider = FinderHelper.GetComponentOnObject<BoxCollider>(gameObject);
    }

    public void Attack(GameObject target)
    {
        if(Time.time - lastAttackTime < cooldown) return;

        PlayerStats player = FinderHelper.GetComponentOnObject<PlayerStats>(target);
        if (player == null) return;

        if(monsterType == MonsterType.Normal)
        {
            player.TakeDamage(damage);
        }
        else if (monsterType == MonsterType.Poison)
        {
            player.ApplyPoison(damage,4f);
        }
        else if (monsterType == MonsterType.Ice)
        {
            player.ApplySlow(4f, 4f);
            player.TakeDamage(damage);
        }
        lastAttackTime = Time.time;
        MonsterController controller = GetComponent<MonsterController>();
        controller?.TriggerAttackAnimation();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && other.bounds.Intersects(attackCollider.bounds))
        {
            Attack(other.gameObject);
        }
    }


    public int GetDamage()
    {
        return damage;
    }
}
