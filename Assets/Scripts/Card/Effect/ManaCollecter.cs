using System.Linq;
using UnityEngine;

public class ManaCollecter : EffectCardBase
{
    public override bool UseEffect()
    {

        if (!base.UseEffect()) return false;
        ResourceManager.Instance.AddMana(1);
        var c = CardGameManager.Instance.HandObjs;
        foreach(var co in c)
        {
            if (co.Value == gameObject) continue;
            if(co.Value.gameObject.GetComponent<CardBase>().GetCardType()==CardType.Effect)
            {
                CardGameManager.Instance.DiscardFromHand(gameObject);
                return true;
            }
            if(co.Value.gameObject.GetComponent<CardBase>().GetCardType()==CardType.Unit&&co.Value.gameObject.GetComponent<CardBase>().UseAble)
            {

                CardGameManager.Instance.DiscardFromHand(gameObject);
                return true;
            }
        }
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DiscardFromHand(gameObject);

        return true;
    }
}
