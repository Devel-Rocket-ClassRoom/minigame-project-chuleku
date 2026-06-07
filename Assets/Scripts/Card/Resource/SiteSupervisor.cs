using UnityEngine;

public class SiteSupervisor : ResourceCardBase
{
    public override bool UseResource()
    {
        if (!base.UseResource()) return false; // 마나 부족이면 보상/카드 소멸 없이 중단
        ResourceManager.Instance.AddFreeCreateWallCoupon(2);
        ResourceManager.Instance.AddGold(3);
        CardGameManager.Instance.DiscardFromHand(gameObject);
        return true;
    }
}
