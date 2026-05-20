using UnityEngine;
using UnityEngine.EventSystems;

// 손패 카드 프리팹에 부착. 평소엔 클릭이 무시되고, CardGameManager가
// 타겟팅 중일 때만 OnHandCardClicked로 전달된다.
[RequireComponent(typeof(CardBase))]
public class HandCardClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"HandCardClick.OnPointerClick on {gameObject.name}");
        if (CardGameManager.Instance == null) return;
        if (!CardGameManager.Instance.IsTargeting) { Debug.Log("타겟팅 중 아님 → 무시"); return; }
        CardGameManager.Instance.OnHandCardClicked(GetComponent<CardBase>());
    }
}
