using UnityEngine;

public enum EnemyType
{
    Boss,
    Minion,
}
public abstract class DamageAble : MonoBehaviour
{
    public float health;
    public int defense;
    public EnemyType type;
    public bool isDead{get; private set;}

    public void TakeDamage(float damage)
    {
        if(isDead) return;
        float actualDamage = Mathf.Max(damage - defense, 0);
        health -= actualDamage;
        if (health <= 0)
        {
            isDead = true;
            gameObject.GetComponent<BoxCollider>().enabled = false;
            gameObject.GetComponent<MoveEnemy>().Die();
            Die();
        }
    }   

    public virtual void Die()
    {
    }
}
