using System;
using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    

    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI shardText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI healthText;
    public TextMeshProUGUI enemyCountText;
    

    public static ResourceManager Instance {get; private set;}
    public int Hp { get; private set; }
    public int Gold { get; private set; }
    public int Shard { get; private set; }
    public int Mana { get; private set; }

    public event Action<int> OnGoldChanged;
    public event Action<int> OnShardChanged;
    public event Action<int> OnManaChanged;
    public event Action<int> OnHpChanged;
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
    }

    public void TakeDamage(int amount)
    {
        if(amount<=0)return;
        SetHp(Mathf.Max(0, Hp - amount));
        if(Hp==0) Debug.Log("체력 0이 됨");
    }
    public void HealEffect(int amount)
    {
        if(amount<=0)return;
        SetHp(Mathf.Min(maxHp,Hp+amount));
        if(Hp==0) Debug.Log("체력회복");
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        SetGold(Gold + amount);
    }

    public void AddShard(int amount)
    {
        if (amount <= 0) return;
        SetShard(Shard + amount);
    }

    public void AddMana(int amount)
    {
        if (amount <= 0) return;
        SetMana(Mana + amount);
    }

    public bool TrySpendGold(int amount)
    {
        if (amount < 0 || Gold < amount) return false;
        SetGold(Gold - amount);
        return true;
    }

    public bool TrySpendMana(int amount)
    {
        if (amount < 0 || Mana < amount) return false;
        SetMana(Mana - amount);
        return true;
    }

    public bool TrySpendShard(int amount)
    {
        if (amount < 0 || Shard < amount) return false;
        SetShard(Shard - amount);
        return true;
    }

    // 상점 구매: 카드 가격 골드 + 마나 1 묶음 차감 (원자성 보장)
    public bool TrySpendForShopPurchase(int gold)
    {
        if (Gold < gold || Mana < 1) return false;
        SetGold(Gold - gold);
        SetMana(Mana - 1);
        return true;
    }

    public void OnChangeGoldButton()
    {
        if (Shard < 1) return;
        SetShard(Shard - 1);
        SetGold(Gold + 1);
    }

    public void OnChangeManaButton()
    {
        if (Shard < 2) return;
        SetShard(Shard - 2);
        SetMana(Mana + 1);
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
}
