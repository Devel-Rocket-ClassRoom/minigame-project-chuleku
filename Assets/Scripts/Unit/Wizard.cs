using UnityEngine;

public class Wizard : UnitBase
{
    private DamageAble pendingTarget;
    public ParticleSystem particleSystem;
    protected override void Attack(DamageAble target)
    {
        pendingTarget = target;
        animator?.SetTrigger("Attack");
    }

    public void TakeDamageOn()
    {
       if (particleSystem != null)
        {
            particleSystem.Stop();
            particleSystem.Play();
        }
        var list = sensor.Targets;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var d = list[i];
            if (d == null || d.isDead) { list.RemoveAt(i); continue; }
            d.TakeDamage(damage);
        }
    }
}
