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
    void OnEnable()
    {
        health = 500;
        defense = 10;
        type = EnemyType.Boss;
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
        ResourceManager.Instance.AddShard(2);
        Destroy(gameObject);
    }

}
