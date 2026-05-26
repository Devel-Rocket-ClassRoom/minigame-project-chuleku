using UnityEngine;

public class ForgeMaster : MagicBase
{
    protected override void UseEffect()
    {
        UpgradeManager.Instance.AddAttackBonus(3,Scope.ThisRound,0);
    }
}
