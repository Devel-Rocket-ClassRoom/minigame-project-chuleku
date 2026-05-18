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
    }

    public void ArrowDamage(int dm)
    {
        damage = dm;
    }

    void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime > maxLifetime) { Destroy(gameObject); return; }

        if (target == null || target.isDead)
        {
            // 타겟이 사라지면 그냥 직진 (또는 Destroy)
            transform.position += transform.forward * speed * Time.deltaTime;
            return;
        }

        Vector3 to = target.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, to, speed * Time.deltaTime);
        transform.LookAt(to);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out DamageAble d))
        {
            d.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
