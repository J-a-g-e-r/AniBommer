using UnityEngine;

public class MeleeMonster : MonoBehaviour, IMonsterAttack
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float cooldown = 1f;
    private float lastAttackTime;

    public void Attack(GameObject target)
    {
        if(Time.time - lastAttackTime < cooldown) return;

        PlayerStats player = FinderHelper.GetComponentOnObject<PlayerStats>(target);
        if (player == null) return;

        player.TakeDamage(damage);
        lastAttackTime = Time.time;
        MonsterController controller = GetComponent<MonsterController>();
        controller?.TriggerAttackAnimation();
    }

    private void OnTriggerStay(Collider other)
    {
        Attack(other.gameObject);

    }


    public int GetDamage()
    {
        return damage;
    }
}
