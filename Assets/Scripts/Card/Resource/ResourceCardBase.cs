using UnityEngine;

public abstract class ResourceCardBase : CardBase
{
    [SerializeField] protected CardType currentCardType = CardType.Resource;
    protected abstract void UseResource();
}
