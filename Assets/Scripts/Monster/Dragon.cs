using UnityEngine;

public class Dragon : DamageAble
{
    // dieCheck / animator 는 DamageAble base로 이동(풀 재사용 시 OnEnable에서 일괄 리셋)
    public override void Die()
    {
        if (dieCheck) return;
        dieCheck = true;
        animator.SetTrigger("Die"); 
    }
    public void AnimationDestroy()
    {
        DefenceGameManager.Instance.EnemyDie();
        PoolManager.Instance.Despawn(gameObject);
    }
}
