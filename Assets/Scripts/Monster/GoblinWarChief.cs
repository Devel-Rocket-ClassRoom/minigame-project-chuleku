using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GoblinWarChief : DamageAble
{
    private Animator animator;
    public GameObject shieldPrefab;
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
        float t = 0;
        GameObject go =Instantiate(shieldPrefab,transform.position,Quaternion.identity);
        while(t<3f)
        {
             t += 0.25f;
            health +=20+(2*DefenceGameManager.Instance.currentStage);
            if(health>maxHealth)
            {
                health = maxHealth;
            }
            yield return new WaitForSeconds(0.25f);
        }
        Destroy(go);
        cool =0;
        isCasting = false;
        scor = null;
        animator.SetTrigger("Walk");
        transform.GetComponent<MoveEnemy>().currentMoveSpeed =   transform.GetComponent<MoveEnemy>().moveSpeed;
    }

}
