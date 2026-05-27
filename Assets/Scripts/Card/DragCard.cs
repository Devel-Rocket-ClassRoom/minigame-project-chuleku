using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
public class DragCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform parentAfterDrag;
    private int siblingIndexBeforeDrag;
    private Vector3 localPositionBeforeDrag;
    private float clickTime;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CardGameManager.Instance != null && CardGameManager.Instance.IsTargeting)
        {
            if (!CardGameManager.Instance.CanCancelCurrentTargeting())
            {
                eventData.pointerDrag = null;
                return;
            }
            CardGameManager.Instance.CancelTargeting();
        }
        clickTime = Time.time;
        parentAfterDrag = transform.parent;
        siblingIndexBeforeDrag = transform.GetSiblingIndex();
        localPositionBeforeDrag = transform.localPosition;
        transform.SetParent(transform.root);
    }
    public void RestoreToOriginalSlot()
    {
        if (parentAfterDrag == null) return;
        transform.SetParent(parentAfterDrag, true);   // worldPositionStays=true (DOTween 트윈용)
        transform.SetSiblingIndex(siblingIndexBeforeDrag);
        transform.DOLocalMove(localPositionBeforeDrag, 0.2f).SetEase(Ease.OutCubic);
    }

    public void OnDrag(PointerEventData eventData)
    {
       transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag,true);
        transform.SetSiblingIndex(siblingIndexBeforeDrag);
        transform.DOLocalMove(localPositionBeforeDrag, 0.2f).SetEase(Ease.OutCubic);
        // 중앙 영역 체크
        float distanceFromCenter = Vector2.Distance(eventData.position, new Vector2(Screen.width / 2, Screen.height / 2));
        float effectRadius = 200f; 

        if (distanceFromCenter < effectRadius)
        {
        
           if(transform.GetComponent<CardBase>().UseAble)
            {
                Debug.Log("효과 발동!");
                switch(transform.GetComponent<CardBase>().GetCardType())
                {
                    case CardType.Unit:
                    // 프리팹의 베이스 UnitCardBase와 AddComponent로 붙은 자식이 공존하므로
                    // 자식 타입(가장 구체적인 효과)을 우선 호출.
                    var units = transform.GetComponents<UnitCardBase>();
                    UnitCardBase unitTarget = null;
                    foreach (var u in units)
                    {
                        if (u.GetType() != typeof(UnitCardBase)) { unitTarget = u; break; }
                    }
                    if (unitTarget == null && units.Length > 0) unitTarget = units[0];
                    if (unitTarget == null) break;

                    if (unitTarget.UseAble)
                    {
                        unitTarget.UseEffect();
                        CardGameManager.Instance.DiscardFromHand(transform.gameObject);
                    }
                    else
                    {
                        Debug.Log("사용 불가능한 카드");
                    }
                    break;
                    case CardType.Effect:
                    // 효과 카드는 즉시 묘지로 보내지 않고 자기가 끝나는 시점에 Discard.
                    // (타겟팅 같은 다단계 효과를 지원하기 위함)
                    // 프리팹의 EffectCardBase 껍데기와 AddComponent로 붙은 자식이 공존하므로
                    // 자식 타입(가장 구체적인 효과)을 우선 호출.
                    var effects = transform.GetComponents<EffectCardBase>();
                    EffectCardBase target = null;
                    foreach (var e in effects)
                    {
                        if (e.GetType() != typeof(EffectCardBase)) { target = e; break; }
                    }
                    if (target == null && effects.Length > 0) target = effects[0];
                    if (target != null) target.UseEffect();
                    break;
                    case CardType.Resource:
                    // 프리팹의 베이스 ResourceCardBase와 AddComponent로 붙은 자식이 공존하므로
                    // 자식 타입(가장 구체적인 효과)을 우선 호출.
                    var resources = transform.GetComponents<ResourceCardBase>();
                    ResourceCardBase resourceTarget = null;
                    foreach (var r in resources)
                    {
                        if (r.GetType() != typeof(ResourceCardBase)) { resourceTarget = r; break; }
                    }
                    if (resourceTarget == null && resources.Length > 0) resourceTarget = resources[0];
                    if (resourceTarget == null) break;

                    resourceTarget.UseResource();
                    CardGameManager.Instance.DiscardFromHand(transform.gameObject);
                    break;
                }
            }
            else
            {
                Debug.Log("효과 사용 불가능한 카드");
            }
        }
    }
}
