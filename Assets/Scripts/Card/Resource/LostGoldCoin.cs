using System.Diagnostics.Tracing;
using UnityEngine;

public class LostGoldCoin : ResourceCardBase
{

    protected override void UseResource()
    {
        ResourceManager.Instance.AddGold(1);
    }
}
