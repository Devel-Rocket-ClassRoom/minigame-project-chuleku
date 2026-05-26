using UnityEngine;

public class DarkHands : MagicBase
{
    
    protected override void UseEffect()
    {
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
    }
}
