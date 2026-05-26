using UnityEngine;

// "손패 카드 1장을 파괴하고 3장 드로우". 타겟팅 인프라(CardGameManager)에 위임.
public class EffectDestroyDraw : EffectCardBase
{
    public override void UseEffect()
    {
        if (CardGameManager.Instance == null) return;
        if(ResourceManager.Instance.Mana<mana)
        {
            return;
        }

        // 마나 체크는 타겟팅 시작 전에. 부족하면 카드는 손패에 그대로 둔다.

        CardGameManager.Instance.BeginTargetHandCard(gameObject, target =>
        {
            CardGameManager.Instance.RemoveCardByInstanceId(target.InstanceId);
            CardGameManager.Instance.DrawCard();
            CardGameManager.Instance.DrawCard();
            CardGameManager.Instance.DrawCard();
            ResourceManager.Instance.TrySpendMana(GetMana());
            CardGameManager.Instance.DiscardFromHand(gameObject);
        });
    }
}
