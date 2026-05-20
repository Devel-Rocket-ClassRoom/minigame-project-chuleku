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
                    case CardType.Effect:
                    transform.GetComponent<EffectCardBase>().UseEffect();
                    CardGameManager.Instance.DiscardFromHand(transform.gameObject);
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
