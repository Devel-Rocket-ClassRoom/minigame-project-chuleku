using UnityEngine;

public abstract class EffectCardBase : CardBase
{
    [SerializeField] protected CardType currentCardType = CardType.Effect;

    void OnEnable()
    {
        useAble = true;
    }
    public abstract void UseEffect();
    
}
