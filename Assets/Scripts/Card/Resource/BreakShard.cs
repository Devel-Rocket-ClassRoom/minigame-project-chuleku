using UnityEngine;

public class BreakShard : ResourceCardBase
{
    public override void UseResource()
    {
        base.UseResource();
        ResourceManager.Instance.AddShard(3);
        CardGameManager.Instance.DiscardFromHand(gameObject);
    }
}
