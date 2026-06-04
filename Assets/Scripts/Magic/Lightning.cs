using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Lightning : MagicBase, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CricleLiner liner;
    private float radius = 10f;
    private TileMap tileMap;
    public GameObject lightningobj;
    public float speed = 50f;
    private Vector3 spawnPointOffset = new Vector3(80f, 15f, 0f);
    void OnEnable()
    {
        GameObject go = GameObject.FindWithTag("Liner");
        liner = go.GetComponent<CricleLiner>();
        GameObject gm = GameObject.FindWithTag("TileMap");
        tileMap = gm.GetComponent<TileMap>();
    }

    protected override void UseEffect(){}
    public void OnBeginDrag(PointerEventData eventData)
    {
        if(phase !=DefenceGameManager.Instance.CurrentPhase)return;
        liner.ShowCircle(GetMouseWorldPosition(), radius);
    }

    public void OnDrag(PointerEventData eventData)
    {
        liner.ShowCircle(GetMouseWorldPosition(), radius);
        UiManager.Instance.CloseInfo();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        liner.HideCircle(); // 원 숨기기
        if(phase !=DefenceGameManager.Instance.CurrentPhase)return;

        Vector3 worldMousePos = GetMouseWorldPosition();
        var (gx, gz) = tileMap.WorldToGrid(worldMousePos);
        
        if (tileMap.IsInBounds(gx, gz))
        {
            Vector3 tileWorldPos = tileMap.GridToWorld(gx, gz);

            GameObject lightning = PoolManager.Instance.Spawn(lightningobj, tileWorldPos, Quaternion.identity);
            LightningObj obj = lightning.GetComponent<LightningObj>();
            obj.Setup(tileWorldPos,radius);
            MagicManager.Instance.UseMagic(instanceId);
            // LightningObj가 코루틴 종료 시 스스로 풀로 회수하므로 여기서 별도 처리 안 함
            // (기존 Destroy(obj,3.5f)는 컴포넌트만 파괴하던 무의미 코드라 제거)
        }
            
    }
    private Vector3 GetMouseWorldPosition()
    {
        var c = Camera.main;
        if (c == null) return Vector3.zero;
        
        Ray ray = c.ScreenPointToRay(Mouse.current.position.ReadValue());
        var plane = new Plane(Vector3.up, new Vector3(0f, tileMap.Origin.y, 0f));
        
        if (plane.Raycast(ray, out float dist))
        {
            return ray.GetPoint(dist);
        }
        return Vector3.zero;
    }
}
