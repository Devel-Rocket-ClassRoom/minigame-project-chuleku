using UnityEngine;
using UnityEngine.EventSystems;
public class DragCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform parentAfterDrag;
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
       transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);

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
                    var unit = transform.GetComponent<UnitCardBase>();
                    if(unit.UseAble)
                    {
                        unit.UseEffect();        
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
                    transform.GetComponent<ResourceCardBase>().UseResource();
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
