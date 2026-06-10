using UnityEngine;

public class PotofGreed : EffectCardBase
{
    public override bool UseEffect()
    {
        if (!base.UseEffect()) return false;
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DiscardFromHand(gameObject);
        return true;
    }
}
