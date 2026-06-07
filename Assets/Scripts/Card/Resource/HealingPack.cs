using UnityEngine;

public class HealingPack : ResourceCardBase
{
    public override bool UseResource()
    {
        if (!base.UseResource()) return false; // 마나 부족이면 효과/드로우/카드 소멸 없이 중단
        ResourceManager.Instance.AddGold(1);
        ResourceManager.Instance.HealEffect(1);
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DiscardFromHand(gameObject);
        return true;
    }
}
