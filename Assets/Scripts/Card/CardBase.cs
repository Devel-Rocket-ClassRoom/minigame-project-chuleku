using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class CardBase : MonoBehaviour
{
    [SerializeField] protected string cardId;
    [SerializeField] protected int price;
    [SerializeField] protected int cost;
    [SerializeField] protected CardType cardType;

    [SerializeField] protected bool useAble;
    protected int instanceId;
    [Header("UI")]
    [SerializeField] protected TextMeshProUGUI nameText;
    [SerializeField] protected TextMeshProUGUI descText;
    [SerializeField] protected TextMeshProUGUI costText;
    [SerializeField] protected Image artworkImage;

    public bool UseAble => useAble;
    protected CardTable.Data data;

    public string CardId => cardId;
    public int InstanceId => instanceId;
    public CardType GetCardType() => cardType;
    public int GetCost() => cost;
    public CardTable.Data Data => data;

    // 같은 종류 카드 여러 장을 구별하기 위해 드로우/생성 시점에 호출.
    public void SetInstanceId(int id) => instanceId = id;
    // AddComponent로 부착된 직후엔 cardId가 비어있을 수 있어 SetCardId가 먼저 호출되길 기다린다.
    public virtual void OnEnable()
    {
        if (!string.IsNullOrEmpty(cardId)) Init();
    }
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
        useAble = data.UseAble;
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
        return Resources.Load<Sprite>($"Sprites/Cards/Art/{imageId}");
    }

}
