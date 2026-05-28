using System;
using TMPro;
using UnityEngine;

public class UnitCardBase : CardBase
{
    [SerializeField] protected UnitBase unitPrefab;
    protected int cardAttack;
    protected float cardAttackSpeed;
    protected float cardRange;
    [Header("UI")]
    [SerializeField] protected TextMeshProUGUI atkText;
    protected UnitTable.Data unitdata;


    public UnitBase UnitPrefab => unitPrefab;
    public int Attack => cardAttack + (UpgradeManager.Instance != null ? UpgradeManager.Instance.GlobalAttackBonus : 0);
    public float AttackSpeed => cardAttackSpeed;
    public float Range => cardRange;
    public UnitTable.Data UnitData => unitdata;
    public virtual void UseEffect() { SoundManager.Play("UseCard");}

    public override void OnEnable()
    {
        base.OnEnable();
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgradeChanged += RefreshAtkText;
    }

    void OnDisable()
    {
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.OnUpgradeChanged -= RefreshAtkText;
    }

    private void RefreshAtkText()
    {
        if (atkText != null) atkText.text = Attack.ToString();
    }

    public override void Init()
    {
        base.Init();
        cardType = CardType.Unit;
        // 유닛 전용 스탯/프리팹은 UnitTable에서 로드
        unitdata = DataTableManager.UnitTable?.Get(cardId);
        if (unitdata != null)
        {
            cardAttack = unitdata.Attack;
            cardAttackSpeed = unitdata.AttackSpeed;
            cardRange = unitdata.Range;
            if (!string.IsNullOrEmpty(unitdata.Prefab))
            {
                var go = LoadUnitPrefab(unitdata.Prefab);
                if (go != null) unitPrefab = go.GetComponent<UnitBase>();
            }
        }

        if (atkText != null) atkText.text = Attack.ToString();
    }

    // CSV의 Prefab 컬럼이 "Resources/UnitPrefab/Archer" 또는 "UnitPrefab/Archer" 둘 다 허용
    protected static GameObject LoadUnitPrefab(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        const string prefix = "Resources/";
        if (key.StartsWith(prefix)) key = key.Substring(prefix.Length);
        return Resources.Load<GameObject>(key);
    }
}
