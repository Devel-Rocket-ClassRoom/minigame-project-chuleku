using System.Collections;
using UnityEngine;

public class GoblinShaman : DamageAble
{
    // dieCheck / animator 는 DamageAble base로 이동(OnEnable에서 일괄 리셋)
    private float cooltime = 5f;
    private float cool =0;
    private bool isCasting;
    private Coroutine scor;
    protected override void OnEnable()
    {
        base.OnEnable();
        // 이 적 고유 상태만 추가 리셋 (isDead/dieCheck/animator/콜라이더는 base가 처리)
        isCasting = false;
        cool = 0;
    }
    void Update()
    {
        if(dieCheck) return;
        cool +=Time.deltaTime;
        if(cool>cooltime&&!isCasting)
        {
            MonsterSkill();
        }   
    }
     public override void Die()
    {
        if (dieCheck) return;
        dieCheck = true;
        DefenceGameManager.Instance.BossKill();
        animator.SetTrigger("Die"); 
    }
    public void AnimationDestroy()
    {
        DefenceGameManager.Instance.EnemyDie();
        ResourceManager.Instance.AddShard(2);
        PoolManager.Instance.Despawn(gameObject);
    }
    private void MonsterSkill()
    {
        transform.GetComponent<MoveEnemy>().currentMoveSpeed = 0;
        isCasting = true;
        if(scor != null)
        {
            StopCoroutine(scor);
            scor=null;
        }
        animator.SetTrigger("Idle");
        scor=StartCoroutine(SkillCor());
    }
    private IEnumerator SkillCor()
    {
        yield return null;
    }
}
