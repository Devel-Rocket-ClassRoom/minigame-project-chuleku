using UnityEngine;

public class SnowWindObj : MonoBehaviour
{
    private float distance = 500f;
    void Update()
    {
        SlowMoveEnemy();
    }
    void OnDisable()
    {
        Collider[] col = Physics.OverlapSphere(transform.position,distance);
        foreach(var c in col)
        {
            if(c.CompareTag("Enemy"))
            {
                c.GetComponent<MoveEnemy>().currentMoveSpeed = c.GetComponent<MoveEnemy>().moveSpeed;
            }
        }
    }
    void SlowMoveEnemy()
    {
        Collider[] col = Physics.OverlapSphere(transform.position,distance);
        foreach(var c in col)
        {
            if(c.CompareTag("Enemy"))
            {
                var enemy = c.GetComponent<MoveEnemy>();
                float slowed = enemy.moveSpeed * 0.8f;
                if (enemy.currentMoveSpeed > slowed)
                    enemy.currentMoveSpeed = slowed;
                // if(c.GetComponent<MoveEnemy>().currentMoveSpeed <= c.GetComponent<MoveEnemy>().moveSpeed*0.8f)continue;
                // c.GetComponent<MoveEnemy>().currentMoveSpeed = c.GetComponent<MoveEnemy>().moveSpeed*0.8f;
            }
        }
    }
}
