using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RangeSensor : MonoBehaviour
{
    private readonly List<DamageAble> targets = new();

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
            if (targets[i] == null||targets[i].isDead) { targets.RemoveAt(i); continue; }
            float sq = (targets[i].transform.position - p).sqrMagnitude;
            if (sq < bestSq) { bestSq = sq; best = targets[i]; }
        }
        return best;
    }

    public bool HasTarget(DamageAble d)
    {
        return d != null && !d.isDead && targets.Contains(d);
    }
    public void UnitRange(float range)
    {
          GetComponent<SphereCollider>().radius = range;
    }
}
