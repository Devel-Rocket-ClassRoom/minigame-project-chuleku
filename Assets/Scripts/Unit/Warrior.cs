using UnityEngine;

public class Warrior : UnitBase
{
    public ParticleSystem particle;
    protected override void Attack(DamageAble target)
    {
        target.TakeDamage(damage);
        animator?.SetTrigger("Attack");
        particle.transform.LookAt(new Vector3(target.transform.position.x,transform.position.y,target.transform.position.z));
        particle.Stop();
        particle.Play();
    }
}
