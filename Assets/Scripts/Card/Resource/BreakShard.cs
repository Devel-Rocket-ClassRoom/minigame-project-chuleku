using UnityEngine;

public class BreakShard : ResourceCardBase
{
    public override bool UseResource()
    {
        if (!base.UseResource()) return false; // 마나 부족이면 샤드 획득/카드 소멸 없이 중단
        ResourceManager.Instance.AddShard(3);
        CardGameManager.Instance.DiscardFromHand(gameObject);
        return true;
    }
}
