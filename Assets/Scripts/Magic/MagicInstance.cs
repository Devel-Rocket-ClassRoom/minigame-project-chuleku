using UnityEngine;

// 마법 1장의 인스턴스. 마법탭 슬롯과 발동용 GameObject(View)를 함께 보관한다.
// 같은 종류(MagicId) 마법이 여러 장 들어와도 InstanceId로 구별.
public class MagicInstance
{
    public int InstanceId;
    public string MagicId;
    public Phase phase;
    public GameObject View;


    public MagicInstance(int instanceId, string magicId, GameObject view,Phase phase)
    {
        InstanceId = instanceId;
        MagicId = magicId;
        View = view;
        this.phase = phase;
    }
}
