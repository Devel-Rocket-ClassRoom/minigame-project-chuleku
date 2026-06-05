using UnityEngine;

public class StoreOner : EffectCardBase
{
    public override void UseEffect()
    {
        base.UseEffect();
        for(int i =0;i<6;i++)
        {
            StoreManager.Instance.AddStock(i,1);
        }
        StoreManager.Instance.AddRerollCount(1);
    }
}
