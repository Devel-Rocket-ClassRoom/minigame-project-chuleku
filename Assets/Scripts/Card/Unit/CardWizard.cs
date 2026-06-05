using UnityEngine;

public class CardWizard : UnitCardBase
{
    public override void UseEffect()
    {
        base.UseEffect();
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
        UpgradeManager.Instance.AddAttackBonus(1,Scope.Permanent);
        CardGameManager.Instance.DiscardFromHand(gameObject);
    }

}
