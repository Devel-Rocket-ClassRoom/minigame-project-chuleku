using UnityEngine;

public class WaterRocket : EffectCardBase
{
    public override void UseEffect()
    {
        if(!ResourceManager.Instance.TrySpendMana(mana))return;
        base.UseEffect();
        int count = 0;
        var h = CardGameManager.Instance.HandObjs;
        foreach(var c in h)
        {
            if(c.Value.gameObject.GetComponent<CardBase>().GetCardType() == CardType.Unit)
            {
                count++;
            }
        }
        if(count == 0)return;

        for(int i = 0;i<count;i++)
        {
            CardGameManager.Instance.DrawCard();
        }
        CardGameManager.Instance.DiscardFromHand(gameObject);
    }

}
