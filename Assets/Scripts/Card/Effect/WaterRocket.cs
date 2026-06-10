using UnityEngine;

public class WaterRocket : EffectCardBase
{
    public override bool UseEffect()
    {
        if (!base.UseEffect()) return false;
        CardGameManager.Instance.DiscardFromHand(gameObject);
        int count = 0;
        var h = CardGameManager.Instance.HandObjs;
        foreach(var c in h)
        {
            if(c.Value.gameObject.GetComponent<CardBase>().GetCardType() == CardType.Unit)
            {
                count++;
            }
        }
        if(count == 0)
        {
            return true;
        }

        for(int i = 0;i<count;i++)
        {
            CardGameManager.Instance.DrawCard();
        }

        return true;
    }

}
