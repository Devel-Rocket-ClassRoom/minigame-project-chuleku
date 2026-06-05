using UnityEngine;

public class PotofGreed : EffectCardBase
{
    public override void UseEffect()
    {
        base.UseEffect();
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DiscardFromHand(gameObject);
    }
}
