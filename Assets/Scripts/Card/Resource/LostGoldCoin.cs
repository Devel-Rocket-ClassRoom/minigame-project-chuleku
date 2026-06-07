using System.Diagnostics.Tracing;
using UnityEngine;

public class LostGoldCoin : ResourceCardBase
{

    public override bool UseResource()
    {
        if (!base.UseResource()) return false; // 마나 부족이면 골드 획득/카드 소멸 없이 중단
        ResourceManager.Instance.AddGold(2);
        CardGameManager.Instance.DiscardFromHand(gameObject);
        return true;
    }
}
