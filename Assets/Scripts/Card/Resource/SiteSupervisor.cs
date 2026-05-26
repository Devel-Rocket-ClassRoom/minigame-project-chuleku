using UnityEngine;

public class SiteSupervisor : ResourceCardBase
{
    public override void UseResource()
    {
        if(!ResourceManager.Instance.TrySpendMana(mana))return;
        base.UseResource();
        ResourceManager.Instance.AddFreeCreateWallCoupon(2);
        ResourceManager.Instance.AddGold(3);

    }
}
