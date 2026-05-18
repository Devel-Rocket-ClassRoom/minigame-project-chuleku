using System.Collections;
using UnityEngine;

public class Archer : UnitBase
{
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform firePoint;
    private DamageAble pendingTarget;
    public float arrowspeed = 15f;
    protected override void Attack(DamageAble target)
    {
        pendingTarget = target;
        animator?.SetTrigger("Attack");
    }
    public void ThrowArrow()
    {
        if (pendingTarget == null || pendingTarget.isDead) return;
        var go = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        var arrow = go.GetComponent<Arrow>();
        arrow.ArrowDamage((int)damage);
        arrow.Launch(pendingTarget, arrowspeed);
    }
}
