using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance {get;private set;}
    public Button[] rotateStoreSlot;
    public GameObject storeInfo;
    private int storeRerollCount;
    public TextMeshProUGUI RerollCountText;

    public class Slot
    {
        public string cardId;   // 이 슬롯에서 파는 카드
        public int remaining;   // 남은 재고
    }

    // 남아있는 카드 풀. 한 번 진열된 카드는 여기서 영구 제거된다.
    private List<string> allstock = new();
    // 슬롯 인덱스 → 슬롯 정보 (카드ID + 재고)
    private readonly List<Slot> slots = new();

    private const int StockPerSlot = 6;
    public int perslot => StockPerSlot;
    public int SlotCount => slots.Count;

    public Slot GetSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return null;
        return slots[slotIndex];
    }

    void Awake()
    {
        Instance = this;
        storeInfo.SetActive(false);
    }
    void Start()
    {
        storeRerollCount = 0;
        allstock = DataTableManager.CardTable.GetAllIds().ToList();
        for(int i =0;i<6;i++)
        {
            RollStock(i);
        }
        RerollCountText.text = $"남은 리롤 : {storeRerollCount}";
    }

    // slotIndex 자리에 카드 1종을 뽑아 재고 6으로 채운다. 풀에서 영구 제거.
    public void RollStock(int slotIndex)
    {
        while (slots.Count <= slotIndex) slots.Add(new Slot());
        if (allstock.Count == 0)
        {
            // slots[slotIndex] = new Slot { cardId = null, remaining = 0 };
            return;
        }
        

        int idx = Random.Range(0, allstock.Count);
        string picked = allstock[idx];
        if(picked == null) return;
        // swap-and-pop으로 풀에서 영구 제거
        int last = allstock.Count - 1;
        allstock[idx] = allstock[last];
        allstock.RemoveAt(last);

        slots[slotIndex] = new Slot { cardId = picked, remaining = StockPerSlot };
        rotateStoreSlot[slotIndex].onClick.RemoveAllListeners();
        rotateStoreSlot[slotIndex].onClick.AddListener(() =>StoreClickedInfo(picked,slotIndex));
        var data = DataTableManager.CardTable.Get(picked);
        rotateStoreSlot[slotIndex].GetComponent<Image>().sprite = LoadSprite(data.Image);
    }
    public void AddRerollCount(int amount)
    {
        storeRerollCount +=amount;
    }
    public void AddStock(int slotIndex,int amount)
    {
        if(slotIndex<0||slotIndex>=slots.Count)return;
        slots[slotIndex].remaining +=amount;
    }

    // 리롤: 지정된 슬롯 한 칸만 새 카드로 교체.
    // 기존 카드는 풀로 되돌리지 않음 → "한 번 뜬 매물은 다시 안 뜸" 규칙 유지.
    public void RerollStock(int slotIndex)
    {
        if(storeRerollCount<1)return;
        if (slotIndex < 0 || slotIndex >= slots.Count) return;
        storeRerollCount --;
        RerollCountText.text = $"남은 리롤 : {storeRerollCount}";
        RollStock(slotIndex);
    }

    // 한 장 구매 처리. true 반환 시 구매 성공.
    public bool BuyOne(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return false;
        if(!ResourceManager.Instance.TrySpendMana(1)) return false;
        var s = slots[slotIndex];
        if (s.cardId == null || s.remaining <= 0) return false;
        s.remaining--;
        return true;
    }

    public void StoreClickedInfo(string cardId,int stockslot)
    {
        storeInfo.SetActive(true);
        InfoUi.Instance.ViewInfo(cardId,GetSlot(stockslot).remaining,StockPerSlot,stockslot);
    }

    public void SetMoveHide()
    {
        storeInfo.SetActive(false);
    }
    protected static Sprite LoadSprite(string imageId)
    {
        if (string.IsNullOrEmpty(imageId)) return null;
        return Resources.Load<Sprite>($"Sprites/Cards/Art/{imageId}");
    }
}
