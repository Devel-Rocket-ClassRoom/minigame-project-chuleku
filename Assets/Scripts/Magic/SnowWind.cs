using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class SnowWind : MagicBase, IBeginDragHandler, IDragHandler, IEndDragHandler
{
     private CricleLiner liner;
     private float radius = 0.3f;
     private float alltarget = 300f;
    private TileMap tileMap;
    public GameObject SnowWindObj;
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
        liner.HideCircle();
        if(phase !=DefenceGameManager.Instance.CurrentPhase)return;
        UseEffect();
    }

    protected override void UseEffect()
    {
        Vector3 pos = tileMap.GridToWorld(TileMap.W/2,TileMap.H/2);
        GameObject go = Instantiate(SnowWindObj,pos,Quaternion.identity);
        Destroy(go,10f);
        MagicManager.Instance.UseMagic(instanceId);
    }

}
