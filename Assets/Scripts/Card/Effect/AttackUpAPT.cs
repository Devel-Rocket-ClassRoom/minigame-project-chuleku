using UnityEngine;

public class AttackUpAPT : EffectCardBase
{
    public override void UseEffect()
    {
        if(!ResourceManager.Instance.TrySpendMana(mana))return;
        base.UseEffect();
        UpgradeManager.Instance.AddAttackBonus(2,Scope.Permanent,0);
        CardGameManager.Instance.DiscardFromHand(gameObject);
    }
}