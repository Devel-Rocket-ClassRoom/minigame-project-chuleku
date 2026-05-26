using System.Linq;
using UnityEngine;

public class GracefulCharity : EffectCardBase
{
    public override void UseEffect()
    {
        if (!ResourceManager.Instance.TrySpendMana(mana)) return;
        base.UseEffect();
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
        PickAndDiscard(2);
    }

    // BeginTargetHandCard는 비동기(콜백 기반)라 연속 호출 시 두 번째가 IsTargeting 가드에 막힌다.
    // 콜백 안에서 다음 타겟팅을 거는 식으로 체이닝해야 함.
    private void PickAndDiscard(int remaining)
    {
        int targetable = CardGameManager.Instance.HandObjs.Count(kv => kv.Value != gameObject);
        if (remaining <= 0 || targetable <= 0)
        {
            CardGameManager.Instance.DiscardFromHand(gameObject);
            return;
        }

        CardGameManager.Instance.BeginTargetHandCard(gameObject, target =>
        {
            CardGameManager.Instance.DiscardFromHand(target.gameObject);
            PickAndDiscard(remaining - 1);
        }, false);
    }
}
