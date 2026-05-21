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
    void OnEnable()
    {
        health = 100f;
        defense = 2;
        type = EnemyType.Minion;
    }
    public override void Die()
    {
        if (dieCheck) return;
        dieCheck = true;
        animator.SetTrigger("Die"); 
    }
    public void AnimationDestroy()
    {
        DefenceGameManager.Instance.EnemyDie();
        Destroy(gameObject);
    }

}
