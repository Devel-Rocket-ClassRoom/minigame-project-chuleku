using TMPro;
using UnityEngine;

public class ResourceCardBase : CardBase
{
    [SerializeField] protected CardType currentCardType = CardType.Resource;
    [SerializeField] protected TextMeshProUGUI ValueText;
    public virtual void UseResource()
    {
        if (!ResourceManager.Instance.TrySpendMana(mana))
        {
            CenterToast.Show("마나가 부족합니다.");
            return;
        }
        cardType = CardType.Resource;
        SoundManager.Play("UseCard");
        TutorialManager.Instance?.NotifyCardUsed();
    }
}