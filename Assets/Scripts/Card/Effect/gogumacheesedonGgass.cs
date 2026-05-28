using UnityEngine;

public class gogumacheesedonGgass : EffectCardBase
{
    public override void UseEffect()
    {
        if (!ResourceManager.Instance.TrySpendMana(mana)) return;
        base.UseEffect();
        ResourceManager.Instance.AddMana(mana);

        int beforeId = CardGameManager.Instance.LastDrawn?.InstanceId ?? -1;
        CardGameManager.Instance.DrawCard();
        var last = CardGameManager.Instance.LastDrawn;
        if (last == null || last.InstanceId == beforeId) return;   // 드로우 실패

        var d = DataTableManager.CardTable.Get(last.CardId);
        if (d == null) return;

        if (d.Type == CardType.Effect || (d.Type == CardType.Unit && d.UseAble))
        {
            CardGameManager.Instance.DrawCard();
            CardGameManager.Instance.DrawCard();
            ResourceManager.Instance.AddMana(1);
        }
        CardGameManager.Instance.DiscardFromHand(gameObject);
    }
}
