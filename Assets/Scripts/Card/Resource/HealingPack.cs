using UnityEngine;

public class HealingPack : ResourceCardBase
{
    public override void UseResource()
    {
        base.UseResource();
        ResourceManager.Instance.AddGold(1);
        ResourceManager.Instance.HealEffect(1);
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DiscardFromHand(gameObject);
    }
}
