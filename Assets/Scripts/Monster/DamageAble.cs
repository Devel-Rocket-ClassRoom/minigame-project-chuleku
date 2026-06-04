
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    public float maxHealth;
    public float upHealthAmount;
    public int defense;
    public Slider healthSlider;
    public EnemyType type;
    public float sliderSpeed = 5f;
    public bool isDead{get; private set;}

    // 모든 적이 공통으로 쓰는 죽음 가드 + 애니메이터. 풀 재사용 시 한곳에서 리셋하기 위해 base로 통합.
    protected bool dieCheck;
    protected Animator animator;

    public string MonsterId => monsterId;

    // 자식 클래스가 override 하지 않으면 베이스가 테이블에서 스탯을 가져와 세팅.
    // 자식이 override 하더라도 base.OnEnable() 만 호출하면 동일 동작.
    // 풀에서 재사용될 때마다 SetActive(true)로 호출되므로, "갓 스폰된 상태"로 되돌리는 리셋도 여기서 수행.
    protected virtual void OnEnable()
    {
        // 리셋을 빠뜨리면 시각 글리치가 아니라 소프트락이 된다:
        // isDead가 true로 남으면 TakeDamage가 무시되고 콜라이더도 꺼진 채라 끝점 트리거도 안 먹어
        // EnemyDie()가 영영 안 불려 라운드가 끝나지 않는다.
        isDead = false;
        dieCheck = false;

        if (animator == null) animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Rebind();      // 죽기 애니 마지막 프레임(쓰러진 자세)/트리거를 전부 기본값으로 되돌림
            animator.Update(0f);    // 즉시 기본 포즈로 갱신해 1프레임 잔상 방지
        }

        var col = GetComponent<BoxCollider>();
        if (col != null) col.enabled = true;   // 사망 시 꺼둔 콜라이더 복구

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

        // 난이도별 체력 배율
        float difficultyHealth = 1f;
        if (DefenceGameManager.Instance != null)
        {
            switch (DefenceGameManager.Instance.difficulty)
            {
                case Difficulty.Easy:
                    difficultyHealth = 0.8f;
                    break;
                case Difficulty.Normal:
                    difficultyHealth = 1f;
                    break;
                case Difficulty.Hard:
                    difficultyHealth = 1.3f;
                    break;
            }
        }

        // 체력: 매 스테이지마다 HealthScale 만큼 증가 + 난이도 배율
        health = (data.Health + data.HealthScale * stage) * difficultyHealth;
        maxHealth = health;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
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

    public void TakeDamage(float damage,bool ignoreDefense = false)
    {
        if(isDead) return;
        float actualDamage = ignoreDefense ? damage : Mathf.Max(damage - defense, 0);
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
    void Update()
    {
        if (healthSlider == null) return;
        healthSlider.value = Mathf.Lerp(healthSlider.value, health, Time.deltaTime * sliderSpeed);
        
    }

    public virtual void Die()
    {
    }
}
