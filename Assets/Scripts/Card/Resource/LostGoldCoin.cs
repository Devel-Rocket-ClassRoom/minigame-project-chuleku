using System.Diagnostics.Tracing;
using UnityEngine;

public class LostGoldCoin : ResourceCardBase
{

    public override void UseResource()
    {
        base.UseResource();
        ResourceManager.Instance.AddGold(2);
    }
}
