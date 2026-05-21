using System;
using TMPro;
using UnityEngine;

public class UnitCardBase : CardBase
{
    [SerializeField] protected UnitBase unitPrefab;
    protected int cardAttack;
    protected int cardUpgradeAttack;
    protected float cardAttackSpeed;
    protected float cardRange;
    [Header("UI")]
    [SerializeField] protected TextMeshProUGUI atkText;
    [SerializeField] protected int upgradeAttack;
    [SerializeField] protected int upgradeAmount;
    protected UnitTable.Data unitdata;


    public int UpgradeAttack => upgradeAttack;
    public UnitBase UnitPrefab => unitPrefab;
    public int Attack => cardAttack+upgradeAttack;
    public float AttackSpeed => cardAttackSpeed;
    public float Range => cardRange;
    public UnitTable.Data UnitData =>unitdata;

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
            upgradeAmount = unitdata.UpgradeAmount;
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
    public void UpGradeDamage(int amount)
    {
        upgradeAttack = upgradeAmount*amount;
    }
}
