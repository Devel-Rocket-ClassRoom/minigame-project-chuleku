using UnityEngine;

public abstract class UnitCardBase : CardBase
{
    [SerializeField] protected UnitBase unitPrefab;
    [SerializeField] protected int cardAttack;
    [SerializeField] protected float cardAttackSpeed;
    [SerializeField] protected float cardRange;

    public UnitBase UnitPrefab => unitPrefab;
    public int Attack => cardAttack;
    public float AttackSpeed => cardAttackSpeed;
    public float Range => cardRange;

    public override void Init()
    {
        base.Init();
        if (data != null)
        {
            cardAttack = data.Attack;
            unitPrefab.SetupUnitStatus(Attack,AttackSpeed,Range);
        }
    }
}
