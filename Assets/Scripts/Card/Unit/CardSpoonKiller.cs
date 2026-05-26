using UnityEngine;

public class CardSpoonKiller : UnitCardBase
{

    public override void UseEffect()
    {
        base.UseEffect();
        if(!ResourceManager.Instance.TrySpendMana(mana))return;
        base.UseEffect();
        UpgradeManager.Instance.AddAttackBonus(1,Scope.Permanent,0);
    }
}
