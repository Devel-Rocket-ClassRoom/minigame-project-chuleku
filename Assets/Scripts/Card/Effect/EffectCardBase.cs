using UnityEngine;

// 프리팹에 박아두는 "껍데기" 역할. 실제 효과는 DrawCard에서 AddComponent된 자식 클래스가
// UseEffect를 override해서 처리한다. 자식이 없는 카드는 빈 효과로 동작.
public class EffectCardBase : CardBase
{

    public override void OnEnable()
    {
        if (!string.IsNullOrEmpty(cardId)) Init();
        cardType = CardType.Effect;
        useAble = true;
    }

    public virtual void UseEffect() { }
}
