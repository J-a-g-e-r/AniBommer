using UnityEngine;

public class MonsterController : MonoBehaviour
{
    private IMonsterAttack attackBehaviour;
    private Animator animator;
    private void Awake()
    {
        attackBehaviour = GetComponent<IMonsterAttack>();
        animator = GetComponentInParent<Animator>();
    }

    private void Attack(GameObject target)
    {
        attackBehaviour?.Attack(target);
    }

    public void TriggerAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("IsAttacking");
        }
    }

    public void TriggerDeathAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("IsDead");
        }
    }

    public void TriggerDamagedAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("IsDamaged");
        }
    }
}
