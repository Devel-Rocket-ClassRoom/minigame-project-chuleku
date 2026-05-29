using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

// 손패 카드에 부착. 마우스를 올리면 테두리 이펙트를 켜고 살짝 확대한다.
// HandLayout은 위치/회전만 건드리고 스케일은 안 건드리므로 hover 스케일이 안전하게 유지된다.
[RequireComponent(typeof(CardBase))]
public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.2f;   // 확대 배율
    [SerializeField] private float duration = 0.15f;    // 확대/복귀 시간

    private CardBase card;
    private Vector3 baseScale;
    private Tweener scaleTween;

    void Awake()
    {
        card = GetComponent<CardBase>();
        baseScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 드래그 중에는 hover 무시 (카드를 끌고 다닐 때 확대 안 되게)
        if (eventData.dragging) return;

        card.SetCardEffect(true);
        ScaleTo(baseScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        card.SetCardEffect(false);
        ScaleTo(baseScale);
    }

    private void ScaleTo(Vector3 target)
    {
        // 위치 트윈(DragCard의 DOLocalMove)은 안 건드리고 스케일 트윈만 교체
        scaleTween?.Kill();
        scaleTween = transform.DOScale(target, duration).SetEase(Ease.OutCubic);
    }

    void OnDisable()
    {
        // 카드가 묘지로 가거나 손패가 꺼질 때 트윈/상태 정리
        scaleTween?.Kill();
        transform.localScale = baseScale;
        card.SetCardEffect(false);
    }
}
