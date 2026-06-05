using UnityEngine;

public class AttackUpAPT : EffectCardBase
{
    public override void UseEffect()
    {
        base.UseEffect();
        UpgradeManager.Instance.AddAttackBonus(2,Scope.Permanent,0);
        CardGameManager.Instance.DiscardFromHand(gameObject);
    }
}