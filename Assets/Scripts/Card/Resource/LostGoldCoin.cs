using System.Diagnostics.Tracing;
using UnityEngine;

public class LostGoldCoin : ResourceCardBase
{

    public override void UseResource()
    {
        ResourceManager.Instance.AddGold(2);
    }
}
