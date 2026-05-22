using TMPro;
using UnityEngine;

public class ResourceCardBase : CardBase
{
    [SerializeField] protected CardType currentCardType = CardType.Resource;
    [SerializeField] protected TextMeshProUGUI ValueText;
    public override void OnEnable()
    {
        useAble = true;
    }
    public virtual void UseResource(){}
}
