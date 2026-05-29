using UnityEngine;

public class Skeleton : DamageAble
{
    private Animator animator;
    private bool dieCheck;
    void Awake()
    {
        dieCheck = false;
        animator = GetComponent<Animator>();
    }
    public override void Die()
    {
        if (dieCheck) return;
        dieCheck = true;
        animator.SetTrigger("Die"); 
        ScoreManager.Instance.SetScore(10);
    }
    public void AnimationDestroy()
    {
        DefenceGameManager.Instance.EnemyDie();
        Destroy(gameObject);
    }

}
