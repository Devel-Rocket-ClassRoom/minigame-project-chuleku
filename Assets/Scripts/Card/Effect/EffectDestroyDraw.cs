using UnityEngine;

// "손패 카드 1장을 파괴하고 3장 드로우". 타겟팅 인프라(CardGameManager)에 위임.
public class EffectDestroyDraw : EffectCardBase
{
    public override bool UseEffect()
    {
        if (CardGameManager.Instance == null) return false;
        if(ResourceManager.Instance.Mana <= mana)
        {
            CenterToast.Show("마나가 부족합니다.");
            return false;
        }

        CardGameManager.Instance.BeginTargetHandCard(gameObject, target =>
        {
            CardGameManager.Instance.RemoveCardByInstanceId(target.InstanceId);
            SoundManager.Play("BreakCard");
            CardGameManager.Instance.DrawCard();
            CardGameManager.Instance.DrawCard();
            CardGameManager.Instance.DrawCard();
            ResourceManager.Instance.TrySpendMana(mana);
            SoundManager.Play("UseCard");
            CardGameManager.Instance.DiscardFromHand(gameObject);
        });
        return true;
    }
}
