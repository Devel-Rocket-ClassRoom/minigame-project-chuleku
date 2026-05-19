using TMPro;
using UnityEngine;

public abstract class ResourceCardBase : CardBase
{
    [SerializeField] protected CardType currentCardType = CardType.Resource;
    [SerializeField] protected TextMeshProUGUI ValueText;
    void OnEnable()
    {
        useAble = true;
    }
    public abstract void UseResource();
}
