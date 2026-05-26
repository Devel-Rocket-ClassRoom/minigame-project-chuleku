using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Adrenaline : MagicBase, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private float radius = 0.3f;
    private float distance = 500f;
    private CricleLiner liner;
    private TileMap tileMap;
    public GameObject powerUpEffect;
    private Vector3 pos;
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
        // 유닛 프리팹은 콜라이더(Range)와 "Unit" 태그가 서로 다른 GameObject에 있어서
        // CompareTag로는 못 잡는다. 부모 체인을 타고 올라가서 Unit 태그를 가진 루트를 찾고,
        // 같은 유닛의 콜라이더가 여러 개 잡혀도 한 번만 이펙트가 뜨도록 중복 제거.
        var seen = new System.Collections.Generic.HashSet<Transform>();
        foreach(var c in col)
        {
            Transform t = c.transform;
            while (t != null && !t.CompareTag("Unit")) t = t.parent;
            if (t == null || !seen.Add(t)) continue;

            GameObject go = Instantiate(powerUpEffect, t.position, Quaternion.identity);
            Destroy(go, 8f);
        }
        UpgradeManager.Instance.AddAttackBonus(10,Scope.Timed,8f);
        MagicManager.Instance.UseMagic(instanceId);
    }
}
