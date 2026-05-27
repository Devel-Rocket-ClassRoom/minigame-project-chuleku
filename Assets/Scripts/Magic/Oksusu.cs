using UnityEngine;

public class Oksusu : MagicBase
{
    protected override void UseEffect()
    {
        ResourceManager.Instance.AddGold(3);
        ResourceManager.Instance.AddMana(1);
    }
}
