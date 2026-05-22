using UnityEngine;

public class CardWizard : UnitCardBase
{
    public override void UseEffect()
    {
        base.UseEffect();
        if(!ResourceManager.Instance.TrySpendMana(mana))
        {
            return;
        }
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
        UpgradeManager.Instance.AddAttackBonus(1,Scope.Permanent);
    }

}
