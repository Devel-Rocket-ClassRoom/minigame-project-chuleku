using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class CardBase : MonoBehaviour
{
    [SerializeField] protected string cardId;
    [SerializeField] protected int price;
    [SerializeField] protected int cost;
    [SerializeField] protected CardType cardType;

    [Header("UI")]
    [SerializeField] protected TextMeshProUGUI nameText;
    [SerializeField] protected TextMeshProUGUI descText;
    [SerializeField] protected TextMeshProUGUI costText;
    [SerializeField] protected Image artworkImage;

    protected CardTable.Data data;

    public string CardId => cardId;
    public CardType GetCardType() => cardType;
    public int GetCost() => cost;
    public CardTable.Data Data => data;

    protected virtual void Awake() => Init();

    public virtual void Init()
    {
        data = DataTableManager.CardTable?.Get(cardId);
        if (data == null)
        {
            Debug.LogWarning($"CardTable에 Id 없음: '{cardId}' ({GetType().Name})");
            return;
        }

        ApplyData();
        ApplyToUI();
    }

    // 런타임에 카드 ID를 갈아끼울 때 호출 (예: 손패 드로우)
    public void SetCardId(string id)
    {
        cardId = id;
        Init();
    }

    // 데이터테이블 값을 컴포넌트 필드로 복사. 서브클래스가 자기 필드도 채움.
    protected virtual void ApplyData()
    {
        cardType = data.Type;
        cost = data.Mana;
    }

    // 필드 값을 UI 요소에 표시. 서브클래스가 추가 UI 갱신.
    protected virtual void ApplyToUI()
    {
        if (nameText != null) nameText.text = GetName();
        if (descText != null) descText.text = GetDesc();
        if (costText != null) costText.text = cost.ToString();
        if (artworkImage != null)
        {
            var sprite = LoadSprite(data.Image);
            if (sprite != null) artworkImage.sprite = sprite;
        }
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

    // Resources/CardArt/<imageId>.png (또는 .sprite) 에서 로드
    protected static Sprite LoadSprite(string imageId)
    {
        if (string.IsNullOrEmpty(imageId)) return null;
        return Resources.Load<Sprite>($"CardArt/{imageId}");
    }
}
