using UnityEngine;

public class EffectCreateMagic : EffectCardBase
{
    public GameObject magicDeck;
    public MagicBase magic;

    public override void OnEnable()
    {
        base.OnEnable();
        GameObject go = GameObject.FindWithTag("MagicDeck");
    }

   
    public override void UseEffect()
    {
        Instantiate(magic,Vector3.zero,Quaternion.identity,magicDeck.transform);
    }
}
