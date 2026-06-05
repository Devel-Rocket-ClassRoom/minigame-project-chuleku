using UnityEngine;

public class CardSpoonKiller : UnitCardBase
{

    public override void UseEffect()
    {
        base.UseEffect();
        UpgradeManager.Instance.AddAttackBonus(1,Scope.Permanent,0);
        CardGameManager.Instance.DiscardFromHand(gameObject);
    }

}
