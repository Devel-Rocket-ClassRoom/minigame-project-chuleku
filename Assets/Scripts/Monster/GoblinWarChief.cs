using UnityEngine;

public class GoblinWarChief : DamageAble
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
    }
    public void AnimationDestroy()
    {
        DefenceGameManager.Instance.EnemyDie();
        DefenceGameManager.Instance.BossKill();
        ResourceManager.Instance.AddShard(2);
        Destroy(gameObject);
    }

}
