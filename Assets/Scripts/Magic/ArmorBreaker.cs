using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ArmorBreaker : MagicBase, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private float radius = 0.3f;
    private float distance = 500f;
    private CricleLiner liner;
    private TileMap tileMap;
    private Vector3 pos;
    public GameObject effectprefab;
    void OnEnable()
    {
        GameObject go = GameObject.FindWithTag("Liner");
        liner = go.GetComponent<CricleLiner>();
        GameObject gm = GameObject.FindWithTag("TileMap");
        tileMap = gm.GetComponent<TileMap>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
         if(phase !=DefenceGameManager.Instance.CurrentPhase)return;
        liner.ShowCircle(GetMouseWorldPosition(), radius);
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
            pos = tileMap.GridToWorld(gx,gz);
            UseEffect();
        }
    }

    protected override void UseEffect()
    {
        Collider[] col = Physics.OverlapSphere(pos,distance);
        foreach(var c in col)
        {
            if(c.CompareTag("Enemy"))
            {
                c.GetComponent<DamageAble>().defense -=1;

                // 적의 자식으로 붙이지 않는다: 적이 3초 안에 죽으면 자식 이펙트(풀 객체)도
                // 함께 파괴되어 풀에서 누수되기 때문. 시전 시점 위치에 고정 표시 후 회수.
                GameObject go = PoolManager.Instance.Spawn(effectprefab, c.transform.position, Quaternion.identity);
                PoolManager.Instance.Despawn(go, 3f);
            }
        }
        MagicManager.Instance.UseMagic(instanceId);
        SoundManager.Play("ArmorBreak");
    }
}
