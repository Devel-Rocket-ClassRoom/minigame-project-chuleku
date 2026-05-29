using UnityEngine;

public class BreakShard : ResourceCardBase
{
    public override void UseResource()
    {
        if(ResourceManager.Instance.Mana<mana)return;
        base.UseResource();

        if(ResourceManager.Instance.TrySpendMana(mana))
        {
            ResourceManager.Instance.AddShard(3);
        }
        CardGameManager.Instance.DiscardFromHand(gameObject);
    }
}
