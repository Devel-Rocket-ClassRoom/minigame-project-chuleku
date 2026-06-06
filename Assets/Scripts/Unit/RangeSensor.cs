using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RangeSensor : MonoBehaviour
{
    private readonly List<DamageAble> targets = new();
    public List<DamageAble> Targets =>targets;

    void OnEnable()
    {
        GetComponent<SphereCollider>().isTrigger = true;
      
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out DamageAble d) && !targets.Contains(d))
            targets.Add(d);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out DamageAble d))
            targets.Remove(d);
    }

    public DamageAble GetNearest()
    {
        Vector3 p = transform.position;
        DamageAble best = null;
        float bestSq = float.MaxValue;
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            // 풀링으로 SetActive(false) 된 적은 OnTriggerExit가 항상 호출되진 않아 목록에 남는다.
            // 끝점으로 빠져나간 적은 isDead=false인 채 비활성화되므로 activeInHierarchy로 함께 거른다.
            if (targets[i] == null || targets[i].isDead || !targets[i].gameObject.activeInHierarchy) { targets.RemoveAt(i); continue; }
            float sq = (targets[i].transform.position - p).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = targets[i]; }
        }
        return best;
    }

    public bool HasTarget(DamageAble d)
    {
        return d != null && !d.isDead && d.gameObject.activeInHierarchy && targets.Contains(d);
    }
    public void UnitRange(float range)
    {
        GetComponent<SphereCollider>().radius = range;
    }
}
