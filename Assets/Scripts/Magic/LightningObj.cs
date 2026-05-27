using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class LightningObj : MonoBehaviour
{

    private float lightningdistance;
     public void Setup(Vector3 targetPos,float radius)
    {
        lightningdistance = radius;
        transform.position = targetPos;
        StartCoroutine(CorVolt());
    }
    private IEnumerator CorVolt()
    {
        float t =0;
        while(true)
        {
            t +=Time.deltaTime;
            Collider[] col = Physics.OverlapSphere(transform.position,lightningdistance);
            foreach(var c in col)
            {
                if(c.CompareTag("Enemy"))
                {
                    DamageAble d = c.GetComponent<DamageAble>();
                    if (d.type == EnemyType.Minion)
                    {
                        d.TakeDamage(d.health*0.01f,true);
                    }
                    else if(d.type==EnemyType.Boss)
                    {
                        d.TakeDamage(15,true);
                    }
                }
            }
            yield return new WaitForSeconds(0.25f);
            if (t > 3.5f)
            {
                Destroy(gameObject);
                break;
            }
        }
    }
}
