using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class CardBase : MonoBehaviour
{
    [SerializeField] protected string cardId;
    [SerializeField] protected int cost;
    [SerializeField] protected int mana;
    [SerializeField] protected CardType cardType;

    [SerializeField] protected bool useAble;
    protected int instanceId;
    [Header("UI")]
    [SerializeField] protected TextMeshProUGUI nameText;
    [SerializeField] protected TextMeshProUGUI descText;
    [SerializeField] protected TextMeshProUGUI manaText;
    [SerializeField] protected Image artworkImage;

    public bool UseAble => useAble;
    protected CardTable.Data data;

    public string CardId => cardId;
    public int InstanceId => instanceId;
    public CardType GetCardType() => cardType;
    public int GetMana() => mana;
    public int GetCost() => cost;
    public CardTable.Data Data => data;
    [SerializeField] private GameObject cardEffect;
    [Header("Destroy FX")]
    [SerializeField] private GameObject destroyFx;        // 파괴 연출용 비활성 자식 (UIParticle dissolve). 평소 꺼둘 것
    [SerializeField] private float destroyFxLength = 1f;   // 파티클 재생 길이(초)
    [SerializeField] private float destroyRise = 80f;      // 파괴 시 위로 떠오르는 양(캔버스/anchored 단위)
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
    public void SetCardEffect(bool on)
    {
        if (cardEffect != null) cardEffect.SetActive(on);
        if(cardType == CardType.Unit&& useAble)
        {
            cardEffect.GetComponent<Image>().color = Color.green;
        }
    }

    // 파괴 효과로 카드가 삭제될 때 호출. 즉시 Destroy 대신 dissolve 파티클을 재생하고
    // 파티클 길이 뒤에 GameObject를 제거한다. (자식 destroyFx를 미리 비활성으로 넣어둘 것)
    public void PlayDestroyThenRemove()
    {
        // 연출 중 입력/호버 차단 (hover 비활성화 시 OnDisable에서 테두리/스케일도 정리됨)
        var drag = GetComponent<DragCard>();
        if (drag != null) drag.enabled = false;
        var hover = GetComponent<CardHover>();
        if (hover != null) hover.enabled = false;

        if (destroyFx == null) { Destroy(gameObject); return; }

        // 파티클을 카드 밖(root)으로 빼서, 아래 카드 페이드(CanvasGroup)에 휩쓸리지 않게 함.
        // worldPositionStays=true 라 카드가 있던 자리에서 그대로 재생됨.
        destroyFx.transform.SetParent(transform.root, true);
        destroyFx.SetActive(true);
        Destroy(destroyFx, destroyFxLength);

        // 캔버스 위가 아니라, 이 카드가 회전한 만큼 기울어진 "카드 자신의 위" 방향으로 떠오름.
        // (부채꼴에서 왼쪽 카드는 좌상단, 오른쪽 카드는 우상단으로 각자 벌어지며 상승)
        Vector2 dir = (Vector2)transform.up;   // 카드 로컬 up(월드 방향). Z회전만 있으니 2D로 충분
        var rt = transform as RectTransform;
        if (rt != null)
            rt.DOAnchorPos(rt.anchoredPosition + dir * destroyRise, destroyFxLength).SetEase(Ease.OutQuad);
        var fxRt = destroyFx.transform as RectTransform;
        if (fxRt != null)
            fxRt.DOAnchorPos(fxRt.anchoredPosition + dir * destroyRise, destroyFxLength).SetEase(Ease.OutQuad);

        // 카드 본체만 파티클 길이 동안 서서히 투명화 (CanvasGroup 없으면 추가)
        var cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.DOFade(0f, destroyFxLength).SetEase(Ease.InQuad);

        Destroy(gameObject, destroyFxLength);
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
        cost = data.Cost;
        mana = data.Mana;
        useAble = data.UseAble;
    }

    // 필드 값을 UI 요소에 표시. 서브클래스가 추가 UI 갱신.
    protected virtual void ApplyToUI()
    {
        if (nameText != null) nameText.text = GetName();
        if (descText != null) descText.text = GetDesc();
        if (manaText != null) manaText.text = mana.ToString();
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

    // DataManager의 SpriteDatabase에서 키로 조회
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
