using UnityEngine;

public abstract class UnitBase : MonoBehaviour
{
    [SerializeField] protected RangeSensor sensor;
    [SerializeField] protected float attackCooldown = 1f;
    [SerializeField] protected int damage = 10;

    protected Animator animator;
    protected DamageAble currentTarget;
    private float cooldownTimer;

    protected virtual void OnEnable()
    {
        animator = GetComponentInChildren<Animator>();
        sensor = transform.GetComponentInChildren<RangeSensor>();
    }
    public void SetupUnitStatus(int attack, float attackSpeed, float range)
    {
        this.damage = attack;
        // 공격 속도 역수를 취해 쿨타임으로 적용 (공속이 2면 쿨타임은 0.5초)
        this.attackCooldown = attackSpeed > 0 ? 1f / attackSpeed : 1f; 
        
        // 중요! 가지고 계신 RangeSensor의 사거리 조절 함수 호출
        if (sensor != null)
        {
            sensor.UnitRange(range);
        }
    }

    protected virtual void Update()
    {
        cooldownTimer -= Time.deltaTime;

        // 현재 타겟이 죽거나 사거리를 벗어났을 때만 새 타겟 탐색 (sticky targeting)
        if (!sensor.HasTarget(currentTarget))
            currentTarget = sensor.GetNearest();
        if (currentTarget == null) return;

        FaceTarget(currentTarget.transform.position);

        if (cooldownTimer > 0f) return;

        Attack(currentTarget);
        cooldownTimer = attackCooldown;
    }

    protected void FaceTarget(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    protected abstract void Attack(DamageAble target);
}
