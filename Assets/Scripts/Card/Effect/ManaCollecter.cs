using System.Linq;
using UnityEngine;

public class ManaCollecter : EffectCardBase
{
    public override void UseEffect()
    {
        if(ResourceManager.Instance.TrySpendMana(mana))
        {
            base.UseEffect();
            ResourceManager.Instance.AddMana(1);
            var c = CardGameManager.Instance.HandObjs;
            foreach(var co in c)
            {
                if (co.Value == gameObject) continue;
                if(co.Value.gameObject.GetComponent<CardBase>().GetCardType()==CardType.Effect)
                {
                    CardGameManager.Instance.DiscardFromHand(gameObject);
                    return;
                }
            }
            CardGameManager.Instance.DrawCard();
            CardGameManager.Instance.DrawCard();
            CardGameManager.Instance.DiscardFromHand(gameObject);

        }
    }
}
