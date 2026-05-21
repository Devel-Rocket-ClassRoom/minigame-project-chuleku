using UnityEngine;

public class Warrior : UnitBase
{
    protected override void Attack(DamageAble target)
    {
        target.TakeDamage(damage);
        animator?.SetTrigger("Attack");
    }
}
