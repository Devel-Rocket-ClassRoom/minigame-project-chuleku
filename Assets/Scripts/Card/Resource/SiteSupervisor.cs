using UnityEngine;

public class SiteSupervisor : ResourceCardBase
{
    public override void UseResource()
    {
        base.UseResource();
        ResourceManager.Instance.AddFreeCreateWallCoupon(2);
        ResourceManager.Instance.AddGold(3);
        CardGameManager.Instance.DiscardFromHand(gameObject);
        
    }
}
