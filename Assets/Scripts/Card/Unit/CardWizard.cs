using UnityEngine;

public class CardWizard : UnitCardBase
{
    public override void UseEffect()
    {
        if(!ResourceManager.Instance.TrySpendMana(mana))
        {
            return;
        }
        base.UseEffect();
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
        UpgradeManager.Instance.AddAttackBonus(1,Scope.Permanent);
    }

}
