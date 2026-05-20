using UnityEngine;

public abstract class MagicBase : MonoBehaviour
{
    [SerializeField] protected bool useAble;
    protected int instanceId;

    public bool UseAble => useAble;
    public int InstanceId => instanceId;
    public void SetInstanceId(int id) => instanceId = id;

    protected abstract void UseEffect();
}
