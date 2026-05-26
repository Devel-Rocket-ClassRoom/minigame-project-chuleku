using UnityEngine;

public class SpoonKiller : UnitBase
{
    protected override void Attack(DamageAble target)
    {
        animator.SetTrigger("Attack");
        target.TakeDamage(damage);
    }
}
