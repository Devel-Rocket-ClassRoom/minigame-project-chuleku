using System.Collections;
using UnityEngine;

public class Archer : UnitBase
{
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform firePoint;
    private DamageAble pendingTarget;
    private float arrowspeed = 30f;
    private Coroutine castCo;
    protected override void Attack(DamageAble target)
    {
        pendingTarget = target;
        animator?.SetTrigger("Attack");
        if (castCo != null) castCo = null;
        castCo = StartCoroutine(AnimatorAttak());
    }
    public void ThrowArrow()
    {
        if (pendingTarget == null || pendingTarget.isDead) return;
        var go = PoolManager.Instance.Spawn(arrowPrefab, firePoint.position, firePoint.rotation);
        var arrow = go.GetComponent<Arrow>();
        arrow.ArrowDamage((int)damage);
        arrow.Launch(pendingTarget, arrowspeed);
    }
    private IEnumerator AnimatorAttak()
    {
        yield return new WaitForSeconds(0.25f);
        ThrowArrow();
        SoundManager.Play("ArcherAttack");
        castCo = null;
    }
}
