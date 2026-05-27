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
                // Instantiate(effectprefab,c.gameObject.transform.position,Quaternion.identity);
            }
        }
        MagicManager.Instance.UseMagic(instanceId);
    }
}
