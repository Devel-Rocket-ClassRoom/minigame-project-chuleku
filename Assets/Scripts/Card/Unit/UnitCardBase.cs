using UnityEngine;

public abstract class UnitCardBase : CardBase
{
    [SerializeField] protected UnitBase unitPrefab; 
    [SerializeField] protected CardType currentCardType = CardType.Unit;
    [SerializeField] protected int cardAttack;
    [SerializeField] protected float cardAttackSpeed;
    [SerializeField] protected float cardRange;

    // 프로퍼티로 외부(매니저)에 데이터 제공
    public UnitBase UnitPrefab => unitPrefab;
    public CardType CurrentCardType => currentCardType;
    public int Attack => cardAttack;
    public float AttackSpeed => cardAttackSpeed;
    public float Range => cardRange;
}
