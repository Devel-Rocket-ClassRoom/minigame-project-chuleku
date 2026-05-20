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
            Destroy(other.gameObject);
            break;
            case EnemyType.Boss:
            ResourceManager.Instance.TakeDamage(10);
            DefenceGameManager.Instance.EnemyDie();
            Destroy(other.gameObject);
            break;
        }
    }
}
