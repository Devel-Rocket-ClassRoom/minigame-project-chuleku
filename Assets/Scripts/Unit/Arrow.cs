using UnityEngine;

public class Arrow : MonoBehaviour
{
    public int damage;
    public float speed = 15f;
    public float maxLifetime = 5f;

    private DamageAble target;
    private float lifetime;

    public void Launch(DamageAble t, float s)
    {
        target = t;
        speed = s;
        lifetime = 0f; // 풀에서 재사용될 때 이전 수명이 남아 즉시 소멸하지 않도록 초기화
    }

    public void ArrowDamage(int dm)
    {
        damage = dm;
    }

    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime > maxLifetime) { PoolManager.Instance.Despawn(gameObject); return; }

        if (target == null || target.isDead)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            PoolManager.Instance.Despawn(gameObject);
            return;
        }

        Vector3 to = target.transform.position+Vector3.up*1.2f;
        transform.position = Vector3.MoveTowards(transform.position, to, speed * Time.deltaTime);
        transform.LookAt(to);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out DamageAble d))
        {
            d.TakeDamage(damage);
            PoolManager.Instance.Despawn(gameObject);
        }
    }
}
