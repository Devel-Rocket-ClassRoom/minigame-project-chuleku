using UnityEngine;

public enum Type
{
    Boss,
    Minion,
}
public abstract class DamageAble : MonoBehaviour
{
    public float health;
    public int defense;
    public Type type;

    public void TakeDamage(float damage)
    {
        float actualDamage = Mathf.Max(damage - defense, 0);
        health -= actualDamage;
        if (health <= 0)
        {
            Die();
        }
    }   
    public virtual void Die()
    {
    }
}
