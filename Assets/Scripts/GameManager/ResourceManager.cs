using System;
using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    

    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI shardText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI freeCouponText;
    [SerializeField] private FloatingText prefab;
    [SerializeField] private RectTransform canvasParent;

    public TextMeshProUGUI enemyCountText;
    

    public static ResourceManager Instance {get; private set;}
    public int Hp { get; private set; }
    public int Gold { get; private set; }
    public int Shard { get; private set; }
    public int Mana { get; private set; }
    public int FreeCreateWallCoupon { get; private set; }

    public event Action<int> OnGoldChanged;
    public event Action<int> OnShardChanged;
    public event Action<int> OnManaChanged;
    public event Action<int> OnHpChanged;
    public event Action<int> OnFreeCouponChanged;
    private int maxMana = 50;
    private int maxHp = 50;


    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        SetGold(0);
        SetShard(0);
        SetMana(2);
        SetHp(20);
        var d = DefenceGameManager.Instance.difficulty;
        switch(d)
        {
            case Difficulty.Easy:
            SetCreateWallCoupon(10);
            break;
            case Difficulty.Normal:
            SetCreateWallCoupon(3);
            break;
            case Difficulty.Hard:
            SetCreateWallCoupon(0);
            break;
        }
        
    }

    public void TakeDamage(int amount)
    {
        if(amount<=0)return;
        SetHp(Mathf.Max(0, Hp - amount));
        if(Hp==0)
        {
            UiManager.Instance.gameoverText.text = "게임 오버!";
            UiManager.Instance.GameEnd();
        }
    }
    public void HealEffect(int amount)
    {
        if(amount<=0)return;
        SetHp(Mathf.Min(maxHp,Hp+amount));

        if(Hp==0) Debug.Log("체력회복");
        Spawn($"+{amount}",Color.red,healthText.transform.position);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        SetGold(Gold + amount);
        Spawn($"+{amount}",Color.gold,goldText.transform.position);
        SoundManager.Play("CoinGet");
    }

    public void AddShard(int amount)
    {
        if (amount <= 0) return;
        SetShard(Shard + amount);
        Spawn($"+{amount}",Color.purple,shardText.transform.position);
    }

    public void AddMana(int amount)
    {
        if (amount <= 0) return;
        SetMana(Mana + amount);
        Spawn($"+{amount}",Color.blue,manaText.transform.position);
        SoundManager.Play("ManaGet");
    }
    public void AddFreeCreateWallCoupon(int amount)
    {
        if(amount <=0)return;
        SetCreateWallCoupon(FreeCreateWallCoupon+amount);
        Spawn($"+{amount}",Color.gray,freeCouponText.transform.position);
    }

    public bool TrySpendGold(int amount)
    {
        if (amount < 0 || Gold < amount) return false;
        SetGold(Gold - amount);
        if(amount ==0)return true;
        SpawnDown($"-{amount}",Color.gold,goldText.transform.position);
        return true;
    }

    public bool TrySpendMana(int amount)
    {
        if (amount < 0 || Mana < amount) return false;
        SetMana(Mana - amount);
        if(amount ==0)return true;
        SpawnDown($"-{amount}",Color.blue,manaText.transform.position);
        return true;
    }

    public bool TrySpendShard(int amount)
    {
        if (amount < 0 || Shard < amount) return false;
        SetShard(Shard - amount);
        SpawnDown($"-{amount}",Color.purple,shardText.transform.position);
        return true;
    }
    public bool TrySpendFreeCreateWallCoupon(int amount)
    {
        if(amount<0|| FreeCreateWallCoupon<amount) return false;
        SetCreateWallCoupon(FreeCreateWallCoupon-amount);
        SpawnDown($"-{amount}",Color.gray,freeCouponText.transform.position);
        return true;
    }

    // 상점 구매: 카드 가격 골드 + 마나 1 묶음 차감 (원자성 보장)
    public bool TrySpendForShopPurchase(int gold)
    {
        if (Gold < gold || Mana < 1) return false;
        SetGold(Gold - gold);
        SetMana(Mana - 1);
        SpawnDown($"-{gold}",Color.gold,goldText.transform.position);
        SpawnDown($"-{1}",Color.blue,manaText.transform.position);
        return true;
    }

    public void OnChangeGoldButton()
    {
        if (Shard < 1) return;
        SetShard(Shard - 1);
        SetGold(Gold + 1);
        Spawn($"+1",Color.gold,goldText.transform.position);
        SpawnDown($"-1",Color.purple,shardText.transform.position);
        SoundManager.Play("CoinGet");
    }

    public void OnChangeManaButton()
    {
        if (Shard < 2) return;
        SetShard(Shard - 2);
        SetMana(Mana + 1);
        SpawnDown($"-2",Color.purple,shardText.transform.position);
        Spawn($"+1",Color.blue,manaText.transform.position);
        SoundManager.Play("ManaGet");
    }

    // 라운드 전환: 골드 초기화 + 마나 2로 회복. 샤드는 유지.
    public void StartRound()
    {
        SetGold(0);
        SetMana(2);
    }
    void SetHp(int value)
    {
        Hp = value;
        if (healthText != null) healthText.text = $"{value}/{maxHp}";
        OnHpChanged?.Invoke(value);
    }

    void SetGold(int value)
    {
        Gold = value;
        if (goldText != null) goldText.text = value.ToString();
        OnGoldChanged?.Invoke(value);
    }

    void SetShard(int value)
    {
        Shard = value;
        if (shardText != null) shardText.text = value.ToString();
        OnShardChanged?.Invoke(value);
    }
    void SetCreateWallCoupon(int value)
    {
        FreeCreateWallCoupon = value;
        if(freeCouponText !=null) freeCouponText.text = value.ToString();
        OnFreeCouponChanged?.Invoke(value);
    }

    void SetMana(int value)
    {
        Mana = value;
        if(Mana >maxMana)
        {
            Mana = maxMana;
        }
        if (manaText != null) manaText.text = Mana.ToString();
        OnManaChanged?.Invoke(value);
    }
    public void Spawn(string msg, Color color, Vector3 worldOrScreenPos)
    {
        var ft = Instantiate(prefab, canvasParent);
        ft.transform.position = worldOrScreenPos;
        ft.Show(msg, color);
    }
    public void SpawnDown(string msg, Color color, Vector3 worldOrScreenPos)
    {
        var ft = Instantiate(prefab, canvasParent);
        ft.transform.position = worldOrScreenPos;
        ft.Show(msg, color);
    }
}
