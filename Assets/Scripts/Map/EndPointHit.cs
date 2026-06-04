using UnityEngine;
using UnityEngine.EventSystems;

public class EndPointHit : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        switch(other.GetComponent<DamageAble>().type)
        {
            case EnemyType.Minion:
            ResourceManager.Instance.TakeDamage(1);
            DefenceGameManager.Instance.EnemyDie();
            SoundManager.Play("EndPosHit");
            PoolManager.Instance.Despawn(other.gameObject);
            break;
            case EnemyType.Boss:
            ResourceManager.Instance.TakeDamage(10);
            DefenceGameManager.Instance.EnemyDie();
            SoundManager.Play("EndPosHit");
            PoolManager.Instance.Despawn(other.gameObject);
            break;
        }
    }
}
