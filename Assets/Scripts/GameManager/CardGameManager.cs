using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Linq;
public class CardGameManager : MonoBehaviour
{
    public TextMeshProUGUI hideText;
    public GameObject hand;
    public GameObject unitMainCard;
    public GameObject resourceMainCard;
    public GameObject effectMainCard;
    public Dictionary<CardType,GameObject> cardPrefabs = new();
    [Header("Unit Panel")]
    [SerializeField] private GameObject unitButtonPrefab;   // 스크롤뷰에 들어갈 버튼 프리팹
    [SerializeField] private Transform unitScrollContent;   // ScrollView/Viewport/Content

   
     public bool IsTargeting { get; private set; }
    private Action<CardBase> targetCallback;
    private GameObject targetingSource; // 효과 카드 자기 자신. 자기 선택 방지용.
    private Transform sourceOriginalParent; // 타겟팅 종료 시 손패로 복귀하기 위해 저장
    private int sourceOriginalSiblingIndex;
    private bool targetingCancelable = true; // false면 ESC 취소 차단 (효과가 이미 일부 발동된 상태에서 사용)

    [Header("Targeting Line")]
    [SerializeField] private RectTransform targetingLine; // 가는 막대 UI Image. 비활성 상태로 씬에 두고 인스펙터에서 연결.
     [Serializable]
    public class UnitCardSlot
    {
        public int instanceId;
        public string cardId;
        public UnitCardBase cardPrefab;   // 마스터 프리팹 컴포넌트 (UnitPrefab 조회용)
        public GameObject buttonGo;
        public GameObject placedUnit;
    }
    public static CardGameManager Instance {get; private set;}

    private readonly List<UnitCardSlot> unitSlots = new();
    private readonly List<CardInstance> deck = new();
    private readonly List<CardInstance> grave = new();
    private readonly Dictionary<int, GameObject> handObjs = new();
    private int nextInstanceId = 1;
    private CardInstance lastDrawn;
    public CardInstance LastDrawn => lastDrawn;
    private bool hideCheck;
    public Dictionary<int,GameObject>HandObjs =>handObjs;

    public IReadOnlyList<UnitCardSlot> UnitSlots => unitSlots;
    public IEnumerable<CardBase> HandCards =>
    handObjs.Values.Select(go => go != null ? go.GetComponent<CardBase>() : null)
                   .Where(cb => cb != null);

    public bool CanCancelCurrentTargeting() => IsTargeting && targetingCancelable;

    // 유닛 패널 버튼 클릭 시 발생. 실제 배치는 구독자(DefenceGameManager)가 처리.
    public event Action<UnitCardSlot> UnitSlotClicked;

    void Awake()
    {
        Instance = this;
        hideCheck = false;
        cardPrefabs[CardType.Unit] = unitMainCard;
        cardPrefabs[CardType.Effect] = effectMainCard;
        cardPrefabs[CardType.Resource] = resourceMainCard;
    }
    void Start()
    {
        for(int i = 0;i<4;i++)
        {
            AddUnitCard("Archer");
        }
        for(int i = 0;i<2;i++)
        {
            AddUnitCard("Warrior");
        }
        for(int i =0;i<8;i++)
        {
            AddResourceCard("LostGold");
        }
        AddEffectCard("IllegalMagic");
        AddEffectCard("IllegalMagic");
        AddEffectCard("DestroyDraw");
    
        Shuffle(deck);
        StartRound();

    }

    // === 카드 획득 ===

    private CardInstance NewInstance(string cardId) => new CardInstance(nextInstanceId++, cardId);

    public CardInstance AddDeckCard(string cardId)
    {
        var inst = NewInstance(cardId);
        deck.Add(inst);
        return inst;
    }
    // 상점 구매: CardTable의 Cost를 골드로 차감하고 타입에 맞게 등록.
    // 골드 부족 시 null 반환, 아무것도 변경하지 않음.
    public CardInstance BuyCard(string cardId)
    {
        var data = DataTableManager.CardTable.Get(cardId);
        if (data == null) return null;
        if (!ResourceManager.Instance.TrySpendGold(data.Cost)){
            Debug.Log("골드 가 부족");
            return null;
        }

        switch (data.Type)
        {
            case CardType.Unit:     return BuyUnitCard(cardId);
            case CardType.Effect:   return BuyEffectCard(cardId);
            case CardType.Resource: return BuyResourceCard(cardId);
            default:                return AddDeckCard(cardId);
        }
    }
    public CardInstance BuyUnitCard(string cardId)
    {
        var inst = NewInstance(cardId);
        grave.Add(inst);
        RegisterUnitSlot(inst);
        return inst;
    }
    public CardInstance BuyResourceCard(string cardId)
    {
        var inst = NewInstance(cardId);
        grave.Add(inst);
        return inst;   
    }
    public CardInstance BuyEffectCard(string cardId)
    {
        var inst = NewInstance(cardId);
        grave.Add(inst);
        return inst;   
    }

    // 유닛 카드: 덱 + 유닛 패널에 같은 InstanceId로 동시 등록 (GDD 4.3)
    public CardInstance AddUnitCard(string cardId)
    {
        var inst = NewInstance(cardId);
        deck.Add(inst);
        RegisterUnitSlot(inst);
        return inst;
    }

    public CardInstance AddResourceCard(string cardId) => AddDeckCard(cardId);
    public CardInstance AddEffectCard(string cardId) => AddDeckCard(cardId);

    // === 드로우 ===

    public void DrawCard()
    {
        if (deck.Count <= 0) return;
        if(handObjs.Count>=20)return;
        var inst = deck[0];
        deck.RemoveAt(0);

        var data = DataTableManager.CardTable.Get(inst.CardId);
        if (data == null) return;
        if (!cardPrefabs.TryGetValue(data.Type, out var prefab) || prefab == null) return;

        var go = Instantiate(prefab, hand.transform);

        // 효과/자원 카드는 CSV의 Behavior 컬럼에 적힌 클래스를 런타임에 부착.
        // 같은 프리팹으로 여러 종류의 효과/자원 카드를 처리하기 위함.
        if ((data.Type == CardType.Effect || data.Type == CardType.Resource||data.Type == CardType.Unit)
            && !string.IsNullOrEmpty(data.Behavior))
        {
            var behaviorType = System.Type.GetType(data.Behavior);
            if (behaviorType != null) go.AddComponent(behaviorType);
            else Debug.LogWarning($"Behavior 타입을 찾을 수 없음: '{data.Behavior}' (카드 Id: {inst.CardId})");
        }

        foreach (var cb in go.GetComponents<CardBase>())
        {
            cb.SetCardId(inst.CardId);
            cb.SetInstanceId(inst.InstanceId);
        }
        handObjs[inst.InstanceId] = go;
        lastDrawn = inst;
    }

    // === 손패 → 묘지 ===
    public void DiscardFromHand(GameObject cardGo)
    {
        if (cardGo == null) return;
        var cb = cardGo.GetComponent<CardBase>();
        if (cb == null) { Destroy(cardGo); return; }

        int id = cb.InstanceId;
        grave.Add(new CardInstance(id, cb.CardId));
        handObjs.Remove(id);
        Destroy(cardGo);
    }

    // === 라운드 종료: 손패→묘지, 묘지→덱, 셔플 ===

    public void EndRound()
    {
        foreach (var kv in handObjs)
        {
            var go = kv.Value;
            if (go == null) continue;
            var cb = go.GetComponent<CardBase>();
            if (cb != null) grave.Add(new CardInstance(cb.InstanceId, cb.CardId));
            Destroy(go);
        }
        handObjs.Clear();

        deck.AddRange(grave);
        grave.Clear();
        Shuffle(deck);
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    public void StartRound()
    {
        for(int i =0;i<5;i++)
        {
            DrawCard();
        }
        SoundManager.Play("SuffleCard");
    }


    // === 유닛 패널 ===

    private void RegisterUnitSlot(CardInstance inst)
    {
        if (unitButtonPrefab == null || unitScrollContent == null) return;

        var btnGo = Instantiate(unitButtonPrefab, unitScrollContent);
        var btnCard = btnGo.GetComponent<CardBase>();
        if (btnCard != null)
        {
            btnCard.SetCardId(inst.CardId);
            btnCard.SetInstanceId(inst.InstanceId);
        }

        // unitMainCard 프리팹 컴포넌트를 스탯/UnitPrefab 소스로 사용
        // (버튼 프리팹은 UnitCardBase가 없을 수 있으니 마스터 카드 프리팹을 참조)
        var slot = new UnitCardSlot
        {
            instanceId = inst.InstanceId,
            cardId = inst.CardId,
            cardPrefab = unitMainCard != null ? unitMainCard.GetComponent<UnitCardBase>() : null,
            buttonGo = btnGo,
        };
        unitSlots.Add(slot);

        // 런타임 생성된 버튼이라 인스펙터 onClick이 비어 있으므로 여기서 묶어줌
        var btn = btnGo.GetComponent<Button>() ?? btnGo.GetComponentInChildren<Button>();
        if (btn != null) btn.onClick.AddListener(() => UnitSlotClicked?.Invoke(slot));
    }

    // 외부(TileMap.BreakUnit 등)에서 배치 유닛이 파괴되었을 때 호출.
    // 슬롯의 placedUnit 참조를 끊고 패널 버튼을 다시 활성화시켜 재배치 가능하게 함.
    public void NotifyPlacedUnitRemoved(GameObject placedUnit)
    {
        if (placedUnit == null) return;
        var slot = unitSlots.Find(s => s.placedUnit == placedUnit);
        if (slot == null) return;
        slot.placedUnit = null;
        if (slot.buttonGo != null) slot.buttonGo.SetActive(true);
    }

    // === 손패 타겟팅 ===
    // "손패 카드 1장을 골라야 하는" 효과에서 사용. 효과 카드가 BeginTargetHandCard로
    // 콜백 등록 → HandCardClick 컴포넌트가 카드 클릭 시 OnHandCardClicked를 부름 →
    // 매니저가 콜백 발화하고 타겟팅 종료. ESC로 취소 가능.

   

    public void BeginTargetHandCard(GameObject source, Action<CardBase> onPicked, bool cancelable = true)
    {
        if (IsTargeting)
        {
            Debug.LogWarning("이미 타겟팅 중");
            return;
        }
        IsTargeting = true;
        targetingSource = source;
        targetCallback = onPicked;
        targetingCancelable = cancelable;

        // 효과 카드를 손패 레이아웃에서 빼고 화면 중앙으로 옮김.
        // 취소 시 복귀를 위해 원래 부모/순서 저장.
        if (source != null)
        {
            sourceOriginalParent = source.transform.parent;
            sourceOriginalSiblingIndex = source.transform.GetSiblingIndex();

            var canvas = source.GetComponentInParent<Canvas>();
            if (canvas != null) source.transform.SetParent(canvas.transform, false);
            var rt = source.transform as RectTransform;
            if (rt != null) rt.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        }
    }

    public void OnHandCardClicked(CardBase card)
    {
        Debug.Log($"OnHandCardClicked: card={(card != null ? card.name : "null")}, IsTargeting={IsTargeting}");
        if (!IsTargeting || card == null) return;
        if (card.gameObject == targetingSource) 
        {
            Debug.Log("효과 카드 자신은 선택 불가");
            if (targetingCancelable)
            CancelTargeting();
            return; 
        }

        var cb = targetCallback;
        EndTargeting();
        cb?.Invoke(card);
    }

    public void CancelTargeting()
    {
        if (!IsTargeting) return;
        if (!targetingCancelable) { Debug.Log("취소 불가 타겟팅"); return; }
        // 효과 카드를 손패의 원래 자리로 복귀
        if (targetingSource != null )
        {
            var dc = targetingSource.GetComponent<DragCard>();
            if (dc != null)
            {
                dc.RestoreToOriginalSlot();
            }
        }
        else if (sourceOriginalParent != null)
        {
            targetingSource.transform.SetParent(sourceOriginalParent, false);
            targetingSource.transform.SetSiblingIndex(sourceOriginalSiblingIndex);
        }
        EndTargeting();
        Debug.Log("타겟팅 취소");
    }

    private void EndTargeting()
    {
        IsTargeting = false;
        targetCallback = null;
        targetingSource = null;
        sourceOriginalParent = null;
        targetingCancelable = true;
    }

    void Update()
    {
        if (IsTargeting && targetingCancelable && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            CancelTargeting();

        UpdateTargetingLine();
    }

    private void UpdateTargetingLine()
    {
        if (targetingLine == null) return;

        if (!IsTargeting || targetingSource == null)
        {
            if (targetingLine.gameObject.activeSelf) targetingLine.gameObject.SetActive(false);
            return;
        }
        if (!targetingLine.gameObject.activeSelf) targetingLine.gameObject.SetActive(true);

        Vector2 a = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 b = Mouse.current != null ? Mouse.current.position.ReadValue() : a;

        Vector2 dir = b-a;
        float len = Vector2.Distance(a, b);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        targetingLine.position = a;
        targetingLine.sizeDelta = new Vector2(len, targetingLine.sizeDelta.y); // 두께는 인스펙터 값 유지
        targetingLine.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // 카드 파괴: instanceId 기준으로 덱/묘지/손패/패널/배치유닛 모두 정리
    public void RemoveCardByInstanceId(int instanceId)
    {
        var slot = unitSlots.Find(s => s.instanceId == instanceId);
        if (slot != null)
        {
            if (slot.placedUnit != null) Destroy(slot.placedUnit);
            if (slot.buttonGo != null) Destroy(slot.buttonGo);
            unitSlots.Remove(slot);
        }

        if (handObjs.TryGetValue(instanceId, out var handGo) && handGo != null)
        {
            // 즉시 Destroy 대신 dissolve 파티클 재생 후 제거 (CardBase가 지연 처리)
            var cb = handGo.GetComponent<CardBase>();
            if (cb != null) cb.PlayDestroyThenRemove();
            else Destroy(handGo);
        }
        handObjs.Remove(instanceId);
        deck.RemoveAll(c => c.InstanceId == instanceId);
        grave.RemoveAll(c => c.InstanceId == instanceId);
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
