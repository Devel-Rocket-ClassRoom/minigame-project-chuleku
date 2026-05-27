using System;
using System.Collections;
using UnityEngine;

public class FireBallobj : MonoBehaviour
{
    public float speed = 15f;
    private float fireballdistance;
    private Vector3 targetPosition;
    // 카드가 생성 직후 호출해 줄 초기화 함수
    public void Setup(Vector3 targetPos,float radius)
    {
        targetPosition = targetPos;
        transform.LookAt(targetPosition);
        fireballdistance = radius;

        // 구체 스스로 코루틴을 시작합니다!
        StartCoroutine(FlyRoutine());
    }

    private IEnumerator FlyRoutine()
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                targetPosition, 
                speed * Time.deltaTime
            );
            yield return null; 
        }

        transform.position = targetPosition; 

        // 데미지 처리 및 폭발 이펙트 생성 등을 여기서 처리합니다.
        Explode();
    }

    private void Explode()
    {
        Collider[] col = Physics.OverlapSphere(transform.position,fireballdistance);
        foreach(var c in col)
        {
          
            if(c.CompareTag("Enemy"))
            {
                DamageAble d = c.GetComponent<DamageAble>();
                if (d.type == EnemyType.Minion)
                {
                    d.TakeDamage(d.health*0.1f,true);
                }
                else if(d.type==EnemyType.Boss)
                {
                    d.TakeDamage(100,true);
                }
                
            }
        }
        Destroy(gameObject);
    }
}
