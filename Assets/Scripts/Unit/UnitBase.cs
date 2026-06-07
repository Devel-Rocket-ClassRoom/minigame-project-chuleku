using UnityEngine;

public abstract class UnitBase : MonoBehaviour
{
    [SerializeField] protected RangeSensor sensor;
    [SerializeField] protected float attackCooldown = 1f;
    [SerializeField] protected int damage = 10;

    protected int baseDamage;
    protected int ungradeAmount;
    protected Animator animator;
    protected DamageAble currentTarget;
    private float cooldownTimer;
    private bool subscribed;
    private bool isIdle;
    private bool hadTarget;

    // true면 새 타겟을 포착했을 때 첫 공격도 공격 간격(쿨다운)만큼 기다린다(즉시 발사 방지).
    // 기본 false라 일반 타워는 기존처럼 발견 즉시 공격. 마법사 등에서만 override.
    protected virtual bool DelayFirstAttack => false;

    protected virtual void OnEnable()
    {
        animator = GetComponentInChildren<Animator>();
        sensor = transform.GetComponentInChildren<RangeSensor>();
        hadTarget = false; // 풀 재사용 시 첫 포착 딜레이를 다시 적용

        if (UpgradeManager.Instance != null && !subscribed)
        {
            UpgradeManager.Instance.OnUpgradeChanged += RefreshDamage;
            subscribed = true;
        }
    }

    protected virtual void OnDisable()
    {
        if (UpgradeManager.Instance != null && subscribed)
        {
            UpgradeManager.Instance.OnUpgradeChanged -= RefreshDamage;
            subscribed = false;
        }
    }

    public void SetupUnitStatus(int attack, float attackSpeed, float range,int upgradeamount)
    {
        baseDamage = attack;
        ungradeAmount = upgradeamount;
        RefreshDamage();
        // 공격 속도 역수를 취해 쿨타임으로 적용 (공속이 2면 쿨타임은 0.5초)
        this.attackCooldown = attackSpeed > 0 ? 1f / attackSpeed : 1f;

        // 중요! 가지고 계신 RangeSensor의 사거리 조절 함수 호출
        if (sensor != null)
        {
            sensor.UnitRange(range);
        }
    }

    private void RefreshDamage()
    {
        int bonus = UpgradeManager.Instance != null ? UpgradeManager.Instance.GlobalAttackBonus : 0;
        damage = baseDamage +(ungradeAmount*bonus);
    }

    protected virtual void Update()
    {
        cooldownTimer -= Time.deltaTime;

        // 현재 타겟이 죽거나 사거리를 벗어났을 때만 새 타겟 탐색 (sticky targeting)
        if (!sensor.HasTarget(currentTarget))
            currentTarget = sensor.GetNearest();

        if (currentTarget == null)
        {
            // 타겟이 사라지면 Idle로 복귀. 매 프레임 쏘면 Idle 전이(CanTransitionToSelf)가
            // 계속 리스타트되므로 1회만 트리거한다.
            if (!isIdle && animator != null)
            {
                // 공속이 빠르면 아직 소비되지 않은 Attack 트리거가 버퍼에 남아 있다가
                // Idle보다 높은 우선순위로 다시 Attack 상태에 진입시킨다. 먼저 비워준다.
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Idle");
                isIdle = true;
            }
            hadTarget = false;
            return;
        }

        // 직전 프레임에 타겟이 없다가 방금 새로 포착한 순간
        if (!hadTarget && DelayFirstAttack)
            cooldownTimer = attackCooldown; // 첫 공격을 한 박자(공격 간격) 늦춘다
        hadTarget = true;

        isIdle = false;
        FaceTarget(currentTarget.transform.position);

        if (cooldownTimer > 0f) return;

        // 공격 진입 전, 직전에 남아 있을 수 있는 Idle 트리거를 비워 둔다.
        if (animator != null) animator.ResetTrigger("Idle");
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
