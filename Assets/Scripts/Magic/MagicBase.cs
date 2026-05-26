using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MagicBase : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] protected bool useAble;
    protected int instanceId;
    protected string magicId;
    protected Phase phase;

    public Phase Phase => phase;
    public bool UseAble => useAble;
    public int InstanceId => instanceId;
    public string MagicId => magicId;
    public void SetInstanceId(int id) => instanceId = id;
    public void SetMagicId(string id) => magicId = id;
    public void SetPhase(Phase p) => phase = p;

    protected abstract void UseEffect();

    // 메인 페이즈용 마법: 인포 패널의 사용 버튼에서 호출.
    // 페이즈 가드 + 발동 + 인스턴스 정리까지 한 번에.
    public void TryActivate()
    {
        if (phase != DefenceGameManager.Instance.CurrentPhase) return;
        UseEffect();
        MagicManager.Instance.UseMagic(instanceId);
    }

    // 마법탭 슬롯 클릭 시 인포 패널 표시.
    // Unity가 드래그 후 같은 오브젝트 위에서 릴리즈하면 OnPointerClick도 같이 발화시키는 경우가 있어
    // dragging 플래그 + 눌렀던 위치와의 거리(드래그 임계치)로 더블 가드.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.dragging) return;
        float threshold = EventSystem.current != null ? EventSystem.current.pixelDragThreshold : 10f;
        if ((eventData.position - eventData.pressPosition).sqrMagnitude > threshold * threshold) return;

        if (UiManager.Instance != null) UiManager.Instance.ViewMagicInfo(this);
    }
}
