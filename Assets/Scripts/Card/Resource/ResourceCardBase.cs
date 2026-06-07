using TMPro;
using UnityEngine;

public class ResourceCardBase : CardBase
{
    [SerializeField] protected CardType currentCardType = CardType.Resource;
    [SerializeField] protected TextMeshProUGUI ValueText;
    // 마나 차감에 성공하면 true, 부족해서 사용하지 못하면 false.
    // 서브클래스는 base.UseResource()가 false면 효과/소멸을 진행하지 말고 즉시 return해야 한다.
    public virtual bool UseResource()
    {
        if (!ResourceManager.Instance.TrySpendMana(mana))
        {
            CenterToast.Show("마나가 부족합니다.");
            return false;
        }
        cardType = CardType.Resource;
        SoundManager.Play("UseCard");
        TutorialManager.Instance?.NotifyCardUsed();
        return true;
    }
}