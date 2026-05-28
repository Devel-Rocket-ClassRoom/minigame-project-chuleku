using UnityEngine;

public class BreakShard : ResourceCardBase
{
    public override void UseResource()
    {
        base.UseResource();
        if(ResourceManager.Instance.Mana<mana)return;

        if(ResourceManager.Instance.TrySpendMana(mana))
        {
            ResourceManager.Instance.AddShard(3);
        }
        CardGameManager.Instance.DiscardFromHand(gameObject);
    }
}
