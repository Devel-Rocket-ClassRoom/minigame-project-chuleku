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
    [SerializeField] private FloatingText floatingText;
    void Awake()
    {
        Instance = this;
    }

    public void ViewInfo(string cardId,int stockmin,int stockmax,int stockslot)
    {
        saveCardId= cardId;
        var data = DataTableManager.CardTable?.Get(cardId);
        if(data==null) return;
        TutorialManager.Instance.NotifyStoreimage();
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
            infoState.text = $"마나 : {data.Mana}\n카드 타입: 효과";
            break;
            case CardType.Resource:
            infoState.text = $"마나 : {data.Mana}\n카드 타입: 자원";
            break;
        }
        infobuyButton.onClick.RemoveAllListeners();
        infobuyButton.onClick.AddListener(()=>OnBuy(cardId,stockslot));
    }
    public void OnBuy(string cardId,int stockslot)
    {
        var data = DataTableManager.CardTable.Get(cardId);
        if (data == null) return;
        if(ResourceManager.Instance.Mana <1)
        {
            var ft = Instantiate(floatingText,infobuyButton.transform);
            ft.transform.position = infobuyButton.transform.position;
            ft.Show("마나 가 부족합니다.",Color.blue);
            SoundManager.Play("Noop");
            return;
        }
        if(ResourceManager.Instance.Gold<data.Cost)
        {
            var ft = Instantiate(floatingText,infobuyButton.transform);
            ft.transform.position = infobuyButton.transform.position;
            ft.Show("골드 가 부족합니다.",Color.red);
            SoundManager.Play("Noop");
            return;   
        }
        if (StoreManager.Instance.BuyOne(stockslot))
        {
            CardGameManager.Instance.BuyCard(cardId);
            TutorialManager.Instance?.NotifyShopBought();
        }
        ViewInfo(cardId,StoreManager.Instance.GetSlot(stockslot).remaining,StoreManager.Instance.perslot,stockslot);
    }
    protected static Sprite LoadSprite(string imageId)
    {
        if (string.IsNullOrEmpty(imageId)) return null;
        var db = DataManager.Instance != null ? DataManager.Instance.SpriteDB : null;
        if (db == null)
        {
            Debug.LogWarning("DataManager 또는 SpriteDB가 씬에 없음");
            return null;
        }
        var sp = db.Get(imageId);
        if (sp == null) Debug.LogWarning($"SpriteDatabase에 '{imageId}' 키 없음");
        return sp;
    }

}
