using UnityEngine;

public enum EnemyType
{
    Boss,
    Minion,
}
public abstract class DamageAble : MonoBehaviour
{
    // 프리팹 인스펙터에서 CSV의 Id 값을 그대로 입력 (예: "Goblin", "Skeleton", "GoblinWarChief")
    [SerializeField] protected string monsterId;

    public float health;
    public float upHealthAmount;
    public int defense;
    public EnemyType type;
    public bool isDead{get; private set;}

    public string MonsterId => monsterId;

    // 자식 클래스가 override 하지 않으면 베이스가 테이블에서 스탯을 가져와 세팅.
    // 자식이 override 하더라도 base.OnEnable() 만 호출하면 동일 동작.
    protected virtual void OnEnable()
    {
        ApplyStatsFromTable();
    }

    protected void ApplyStatsFromTable()
    {
        var data = DataTableManager.MonsterTable?.Get(monsterId);
        if (data == null)
        {
            Debug.LogWarning($"MonsterTable에 '{monsterId}' 없음 (DamageAble.ApplyStatsFromTable)");
            return;
        }

        int stage = DefenceGameManager.Instance != null
            ? DefenceGameManager.Instance.currentStage
            : 1;

        // 체력: 매 스테이지마다 HealthScale 만큼 증가
        health = data.Health + data.HealthScale * stage;

        // 방어력: 5스테이지마다 DefenceScale 만큼 증가
        // stage 1~4 → +0, 5~9 → +1*scale, 10~14 → +2*scale ...
        defense = data.Defence + data.DefenceScale * (stage / 5);

        type = data.Type;

        // MoveEnemy의 이동 속도도 같이 세팅 (있는 경우만)
        var move = GetComponent<MoveEnemy>();
        if (move != null)
        {
            move.moveSpeed = data.MoveSpeed;
            move.currentMoveSpeed = data.MoveSpeed;
        }
    }

    public void TakeDamage(float damage)
    {
        if(isDead) return;
        float actualDamage = Mathf.Max(damage - defense, 0);
        if(actualDamage<1) actualDamage = 1;
        health -= actualDamage;
        if (health <= 0)
        {
            isDead = true;
            gameObject.GetComponent<BoxCollider>().enabled = false;
            gameObject.GetComponent<MoveEnemy>().Die();
            Die();
        }
    }

    public virtual void Die()
    {
    }
}
