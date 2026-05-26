using UnityEngine;

public class PotofGreed : EffectCardBase
{
    public override void UseEffect()
    {
        if(!ResourceManager.Instance.TrySpendMana(mana))return;
        base.UseEffect();
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DiscardFromHand(gameObject);
    }
}
