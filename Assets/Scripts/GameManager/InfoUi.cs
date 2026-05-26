using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class InfoUi : MonoBehaviour
{
    public static InfoUi Instance {get; private set;}
    public string saveCardId;
    public TextMeshProUGUI infoName;
    public TextMeshProUGUI infoDesc;
    public TextMeshProUGUI infoCost;
    public TextMeshProUGUI infoState;
    public TextMeshProUGUI infoStock;
    public Button infobuyButton;
    public Image infoImage;
    void Awake()
    {
        Instance = this;
    }

    public void ViewInfo(string cardId,int stockmin,int stockmax,int stockslot)
    {
        saveCardId= cardId;
        var data = DataTableManager.CardTable?.Get(cardId);
        if(data==null) return;
        
        infoName.text = DataTableManager.StringTable?.Get(data.Name);
        infoCost.text = $"가격 : {data.Cost}";
        infoDesc.text = DataTableManager.StringTable?.Get(data.Desc);
        infoStock.text = $"매물 : {stockmin}/{stockmax}";
        var sp = LoadSprite(data.Image);
        infoImage.sprite = sp;
        switch(data.Type)
        {
            case CardType.Unit:
            var unit = DataTableManager.UnitTable.Get(data.Id);
            if (data.UseAble)
            {
                infoState.text = $"공격력 : {unit.Attack}\n사거리 : {unit.Range}\n유닛 타입 : 유닛,효과\n마나 : {data.Mana}";
            }
            else
            {
                infoState.text = $"공격력 : {unit.Attack}\n사거리 : {unit.Range}\n유닛 타입 : 유닛";
            }
            break;
            case CardType.Effect:
            infoState.text = $"마나 : {data.Mana}";
            break;
            case CardType.Resource:
            infoState.text = $"마나 : {data.Mana}";
            break;
        }
        infobuyButton.onClick.RemoveAllListeners();
        infobuyButton.onClick.AddListener(()=>OnBuy(cardId,stockslot));
    }
    public void OnBuy(string cardId,int stockslot)
    {
        var data = DataTableManager.CardTable.Get(cardId);
        if (data == null) return;
        if(ResourceManager.Instance.Gold<data.Cost)return;
        if (StoreManager.Instance.BuyOne(stockslot))
        {
            CardGameManager.Instance.BuyCard(cardId);
        }
        ViewInfo(cardId,StoreManager.Instance.GetSlot(stockslot).remaining,StoreManager.Instance.perslot,stockslot);
    }
    protected static Sprite LoadSprite(string imageId)
    {
        if (string.IsNullOrEmpty(imageId)) return null;
        return Resources.Load<Sprite>($"Sprites/Cards/Art/{imageId}");
    }

}
