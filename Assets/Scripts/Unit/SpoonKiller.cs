using UnityEngine;

public class SpoonKiller : UnitBase
{
    [SerializeField] private Material boltMaterial;
    [SerializeField] private float muzzleHeight = 0.9f;
    protected override void Attack(DamageAble target)
    {
        animator.SetTrigger("Attack");

        // 조준점은 TakeDamage 호출 "전"에 캡처한다.
        // TakeDamage가 킬 처리 시 BoxCollider를 꺼버리면(DamageAble.cs) 바운드가 무효해질 수 있다.
        // 적 피벗은 발밑이라 transform.position을 쓰면 땅바닥에만 닿으므로, 콜라이더 바운드 중심(몸통)을 겨냥한다.
        Vector3 aim;
        var col = target.GetComponent<BoxCollider>();
        if (col != null) aim = col.bounds.center;
        else aim = target.transform.position + Vector3.up * muzzleHeight;

        Vector3 muzzle = transform.position + Vector3.up * muzzleHeight;

        target.TakeDamage(damage);
        LineAttack(muzzle, aim);
        SoundManager.Play("SpoonKillerAttack");
    }
    private void LineAttack(Vector3 from, Vector3 to)
    {
        var go = new GameObject("Bolt");
        var lr = go.AddComponent<LineRenderer>();
        lr.material = boltMaterial;
        lr.startWidth = 0.15f; lr.endWidth = 0.05f;
        lr.startColor = Color.cyan; lr.endColor = Color.white;
        const int segments = 8;
        lr.positionCount = segments;
        Vector3 dir = (to - from).normalized;
        Vector3 perp = Vector3.Cross(dir, Vector3.up);
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1);
            Vector3 p = Vector3.Lerp(from, to, t);
            if (i > 0 && i < segments - 1)
                p += perp * Random.Range(-0.2f, 0.2f);
            lr.SetPosition(i, p);
        }
        Destroy(go, 0.12f);
    }
}
