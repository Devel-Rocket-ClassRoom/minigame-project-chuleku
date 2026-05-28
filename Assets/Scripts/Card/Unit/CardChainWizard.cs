using UnityEngine;

public class CardChainWizard : UnitCardBase
{
    public override void UseEffect()
    {
        if(!ResourceManager.Instance.TrySpendMana(mana))return;
        base.UseEffect();
        string id = DataTableManager.MagicTable?.GetRandomId();
        if (string.IsNullOrEmpty(id)) return;

        MagicManager.Instance.AddMagic(id);
        int beforeId = CardGameManager.Instance.LastDrawn?.InstanceId ?? -1;
        CardGameManager.Instance.DrawCard();
        var last = CardGameManager.Instance.LastDrawn;
        if (last == null || last.InstanceId == beforeId) return; 
        var d = DataTableManager.CardTable.Get(last.CardId);
        if (d == null) return;

        if (d.Type == CardType.Unit)
        {
            ResourceManager.Instance.AddMana(1);
        }
        CardGameManager.Instance.DiscardFromHand(gameObject);
    }
}
