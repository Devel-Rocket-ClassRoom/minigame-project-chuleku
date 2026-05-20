using UnityEngine;

public abstract class MagicBase : MonoBehaviour
{
    [SerializeField] protected bool useAble;

    public bool UseAble => useAble;

    protected abstract void UseEffect();
}
