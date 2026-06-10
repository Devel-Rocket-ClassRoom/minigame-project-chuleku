using UnityEngine;

public class StoreOner : EffectCardBase
{
    public override bool UseEffect()
    {
        if (!base.UseEffect()) return false;
        for(int i =0;i<6;i++)
        {
            StoreManager.Instance.AddStock(i,1);
        }
        StoreManager.Instance.AddRerollCount(1);
        CardGameManager.Instance.DiscardFromHand(gameObject);
        return true;
    }
}
