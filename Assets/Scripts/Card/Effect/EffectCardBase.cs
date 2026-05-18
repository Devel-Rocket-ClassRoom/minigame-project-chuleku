using UnityEngine;

public abstract class EffectCardBase : CardBase
{
    [SerializeField] protected CardType currentCardType = CardType.Effect;
    protected abstract void UseEffect();
    
}
