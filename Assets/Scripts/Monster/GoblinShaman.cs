using System.Collections;
using UnityEngine;

public class GoblinShaman : DamageAble
{
    private Animator animator;
    private bool dieCheck;
    private float cooltime = 5f;
    private float cool =0;
    private bool isCasting;
    private Coroutine scor;
     void Awake()
    {
        dieCheck = false;
        animator = GetComponent<Animator>();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        dieCheck = false;
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
        Destroy(gameObject);
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
