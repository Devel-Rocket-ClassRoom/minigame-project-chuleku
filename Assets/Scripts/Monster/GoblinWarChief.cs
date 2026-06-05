using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GoblinWarChief : DamageAble
{
    // dieCheck / animator 는 DamageAble base로 이동(OnEnable에서 일괄 리셋)
    public GameObject shieldPrefab;
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
        if (healthSlider == null) return;
        healthSlider.value = Mathf.Lerp(healthSlider.value, health, Time.deltaTime * sliderSpeed);
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
        if(scor != null)
        {
            StopCoroutine(scor);
            scor=null;
        }
        animator.SetTrigger("Die");
        ScoreManager.Instance.SetScore(100);
        SoundManager.Play("EnemyDie");
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
        float t = 0;
        GameObject go =Instantiate(shieldPrefab,transform.position,Quaternion.identity);
        SoundManager.Play("WarChiefSkillStart");
        float skillhealing = 10f;
        float stagehealing = 1f;
        switch(DefenceGameManager.Instance.difficulty)
        {
            case Difficulty.Easy:
                skillhealing = 10f;
                stagehealing = 1f;
                break;
            case Difficulty.Normal:
                skillhealing = 15f;
                stagehealing = 2f;
                break;
            case Difficulty.Hard:
                skillhealing = 20f;
                stagehealing = 3f;
                break;
        }
        while(t<3f)
        {
            t += 0.25f;
            health +=skillhealing+(stagehealing*DefenceGameManager.Instance.currentStage);
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
        SoundManager.Play("WarChiefSkillEnd");
    }

}
