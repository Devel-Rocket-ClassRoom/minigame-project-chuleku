using UnityEngine;

public class EffectCreateMagic : EffectCardBase
{
    public GameObject magicDeck;
    public GameObject magic;
    protected override void UseEffect()
    {
        Instantiate(magic,Vector3.zero,Quaternion.identity,magicDeck.transform);
    }
}
