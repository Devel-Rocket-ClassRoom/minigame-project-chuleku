using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardGameManager : MonoBehaviour
{
    public TextMeshProUGUI hideText;
    public GameObject hand;
    public GameObject unitMainCard;
    public GameObject resourceMainCard;
    public Dictionary<CardType,GameObject> cardPrefabs = new();
    [Header("Unit Panel")]
    [SerializeField] private GameObject unitButtonPrefab;   // 스크롤뷰에 들어갈 버튼 프리팹
    [SerializeField] private Transform unitScrollContent;   // ScrollView/Viewport/Content
    [SerializeField] private GameObject unitPanel;          // 선택용 스크롤뷰 패널(켜고 끔)
    [SerializeField] private TileMap tileMap;               // 배치 위치 계산용

    [Serializable]
    public class UnitCardSlot
    {
        public UnitCardBase card;
        public GameObject buttonGo;
        public GameObject placedUnit;
    }

    private readonly List<UnitCardSlot> unitSlots = new();
    private List<CardBase> deck = new();
    private bool hideCheck;
    private Vector2Int? pendingTile;   // 배치 대기 중인 타일 (없으면 null)

    void Awake()
    {
        hideCheck = false;
        if (unitPanel != null) unitPanel.SetActive(false);
        if (tileMap == null)
        {
            var tm = GameObject.FindWithTag("TileMap");
            if (tm != null) tileMap = tm.GetComponent<TileMap>();
        }
        cardPrefabs[CardType.Unit] = unitMainCard;
        cardPrefabs[CardType.Resource] = resourceMainCard;
    }
    void Start()
    {
        for(int i = 0;i<4;i++)
        {
            AddUnitCard("Archer");
        }
        for(int i =0;i<6;i++)
        {
            AddResourceCard("LostGold");
        }
    }

    public void AddDeckCard(CardBase cb)
    {
        deck.Add(cb);
    }
    public void AddUnitCard(string cardId)
    {
        CardBase cb = unitMainCard.GetComponent<CardBase>();
        cb.SetCardId(cardId);
        AddDeckCard(cb);
    }
    public void AddResourceCard(string cardId)
    {
        CardBase cb = resourceMainCard.GetComponent<CardBase>();
        cb.SetCardId(cardId);
        AddDeckCard(cb);
    }
    public void DrawCard()
    {
        if(deck.Count<=0)
        return;
        var type = deck[0].GetCardType();
        GameObject go = cardPrefabs[type];
        deck.RemoveAt(0);
        Instantiate(go,hand.transform);
    }
    // 유닛 카드 획득 시 호출 → 스크롤뷰에 버튼 추가
    public void AddUnitCard(UnitCardBase card)
    {
        if (card == null || unitButtonPrefab == null || unitScrollContent == null) return;

        var btnGo = Instantiate(unitButtonPrefab, unitScrollContent);
        var slot = new UnitCardSlot { card = card, buttonGo = btnGo };
        unitSlots.Add(slot);

        // 버튼 비주얼이 카드 정보를 자동으로 들고 오게 (CardBase.SetCardId가 모든 UI 채움)
        var btnCard = btnGo.GetComponent<CardBase>();
        if (btnCard != null) btnCard.SetCardId(card.CardId);

        var btn = btnGo.GetComponent<Button>() ?? btnGo.GetComponentInChildren<Button>();
        if (btn != null) btn.onClick.AddListener(() => OnSlotClicked(slot));
    }

    // 타일에서 소환 버튼 눌렀을 때 → 패널 열고 배치 위치 기억
    public void OpenSelectionForTile(Vector2Int tile)
    {
        pendingTile = tile;
        if (unitPanel != null) unitPanel.SetActive(true);
    }

    // 스크롤뷰 닫기 (취소)
    public void CloseSelection()
    {
        pendingTile = null;
        if (unitPanel != null) unitPanel.SetActive(false);
    }

    // 카드 파괴 시 호출 → 버튼/배치된 유닛까지 같이 정리
    public void RemoveCard(UnitCardBase card)
    {
        var slot = unitSlots.Find(s => s.card == card);
        if (slot == null) return;
        if (slot.placedUnit != null) Destroy(slot.placedUnit);
        if (slot.buttonGo != null) Destroy(slot.buttonGo);
        unitSlots.Remove(slot);
        deck.Remove(card);
    }

    void OnSlotClicked(UnitCardSlot slot)
    {
        if (slot.placedUnit != null) return;       // 이미 배치된 카드
        if (pendingTile == null) return;           // 타일 선택 없이 눌렀음
        if (tileMap == null || slot.card == null || slot.card.UnitPrefab == null) return;

        var t = pendingTile.Value;
        if (!tileMap.WallCheck(t.x, t.y)) { CloseSelection(); return; }   // 벽 없으면 배치 불가
        if (!tileMap.UnitCheck(t.x, t.y)) { CloseSelection(); return; }   // 이미 유닛 있음

        Vector3 pos = tileMap.GridToWorld(t.x, t.y);
        pos.y = 3.5f;
        slot.placedUnit = Instantiate(slot.card.UnitPrefab.gameObject, pos, Quaternion.identity);
        tileMap.CreateUnit(t.x, t.y, slot.placedUnit);

        slot.buttonGo.SetActive(false);
        CloseSelection();
    }
    

    public void HandHide()
    {
        hideCheck = !hideCheck;
        if(hideCheck)
        {
            hideText.text = "손패 켜기";
        }
        else
        {
            hideText.text = "손패 가리기";
        }
        hand.SetActive(!hideCheck);
    }
}
