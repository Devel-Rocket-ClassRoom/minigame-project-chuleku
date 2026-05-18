using UnityEngine;

public abstract class CardBase : MonoBehaviour
{
    [SerializeField] protected string cardId;
    [SerializeField] protected int price;
    [SerializeField] protected int cost;
    [SerializeField] protected CardType cardType;

    protected CardTable.Data data;

    public string CardId => cardId;
    public CardType GetCardType() => cardType;
    public int GetCost() => cost;
    public CardTable.Data Data => data;

    protected virtual void Awake()
    {
        Init();
    }

    public virtual void Init()
    {
        data = DataTableManager.CardTable?.Get(cardId);
        if (data == null)
        {
            Debug.LogWarning($"CardTable에 Id 없음: '{cardId}' ({GetType().Name})");
            return;
        }

        cardType = data.Type;
        cost = data.Mana;
    }

    public string GetName()
    {
        if (data == null) return StringTable.UnKnown;
        return DataTableManager.StringTable?.Get(data.Name) ?? StringTable.UnKnown;
    }

    public string GetDesc()
    {
        if (data == null) return StringTable.UnKnown;
        return DataTableManager.StringTable?.Get(data.Desc) ?? StringTable.UnKnown;
    }
}
