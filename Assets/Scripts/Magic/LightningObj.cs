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
                    float pierce = c.GetComponent<DamageAble>().defense;
                    c.GetComponent<DamageAble>().TakeDamage(10+pierce);
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
