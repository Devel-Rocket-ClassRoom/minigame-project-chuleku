using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainWizard : UnitBase
{
    [SerializeField] private float jumpRadius = 8f;
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private float falloff = 0.7f;
    [SerializeField] private float hopDelay = 0.08f;
    private float yOffset = 1.2f;
    [SerializeField] private Material boltMaterial;
    [SerializeField] private Transform muzzle;
    protected override void Attack(DamageAble target)
    {
        animator?.SetTrigger("Attack");
        if (target == null || target.isDead) return;
        StartCoroutine(ChainRoutine(target));
    }
    private IEnumerator ChainRoutine(DamageAble firstTarget)
    {
        var hit = new HashSet<DamageAble> { firstTarget };
        Vector3 prevPos = muzzle != null ? muzzle.position : transform.position; 
        float dmg = damage;
        DamageAble current = firstTarget;

        // 첫 타격: 유닛 → 락온 타겟
        Vector3 hitPos = current.transform.position+Vector3.up*yOffset;
        SpawnBolt(prevPos, hitPos);
        current.TakeDamage(dmg);
        prevPos = hitPos;
        dmg *= falloff;

        // 이후 점프: 마법판과 동일
        for (int i = 0; i < maxBounces; i++)
        {
            yield return new WaitForSeconds(hopDelay);
            if (current == null || current.isDead)
            {
                // 현재 타겟이 죽었지만 마지막 위치 기준으로 다음 적 탐색은 가능
                // prevPos는 이미 갱신돼 있으니 그대로 사용
            }

            var next = FindNearest(prevPos, jumpRadius, hit);
            if (next == null) break;

            hitPos = next.transform.position+Vector3.up*yOffset;
            SpawnBolt(prevPos, hitPos);
            next.TakeDamage(dmg);
            hit.Add(next);
            prevPos = hitPos;
            current = next;
            dmg *= falloff;
        }
    }

    private DamageAble FindNearest(Vector3 from, float radius, HashSet<DamageAble> exclude)
    {
        var col = Physics.OverlapSphere(from, radius);
        DamageAble best = null;
        float bestSqr = float.MaxValue;
        foreach (var c in col)
        {
            if (!c.CompareTag("Enemy")) continue;
            var d = c.GetComponentInParent<DamageAble>();
            if (d == null || d.isDead || exclude.Contains(d)) continue;
            float sqr = (d.transform.position - from).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = d; }
        }
        return best;
    }

    private void SpawnBolt(Vector3 from, Vector3 to) 
    {
        var go = new GameObject("Bolt");
        var lr = go.AddComponent<LineRenderer>();
        lr.material = boltMaterial;
        lr.startWidth = 0.15f; lr.endWidth = 0.05f;
        lr.startColor = Color.cyan; lr.endColor = Color.white;

        // 지그재그 노이즈 추가
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
