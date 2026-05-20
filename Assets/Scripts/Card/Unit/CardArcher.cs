using UnityEngine;

public class CardArcher : UnitCardBase
{
    public override void OnEnable()
    {
        upgradeAmount = 2;
        cardRange = 10f;
        cardAttackSpeed = 1f;
        base.OnEnable();
    }
}
