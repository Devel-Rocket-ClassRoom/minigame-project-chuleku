using UnityEngine;

public class AttackUpAPT : EffectCardBase
{
    public override bool UseEffect()
    {
        if (!base.UseEffect()) return false;
        UpgradeManager.Instance.AddAttackBonus(2,Scope.Permanent,0);
        CardGameManager.Instance.DiscardFromHand(gameObject);
        return true;
    }
}