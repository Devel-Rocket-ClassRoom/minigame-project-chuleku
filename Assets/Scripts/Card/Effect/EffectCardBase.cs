using UnityEngine;

public abstract class EffectCardBase : CardBase
{
    [SerializeField] protected CardType currentCardType = CardType.Effect;

    public override void OnEnable()
    {
        useAble = true;
    }
    public abstract void UseEffect();
    
}
