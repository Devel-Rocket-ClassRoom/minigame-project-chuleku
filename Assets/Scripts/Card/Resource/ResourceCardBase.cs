using TMPro;
using UnityEngine;

public class ResourceCardBase : CardBase
{
    [SerializeField] protected CardType currentCardType = CardType.Resource;
    [SerializeField] protected TextMeshProUGUI ValueText;
    public virtual void UseResource(){}
}
