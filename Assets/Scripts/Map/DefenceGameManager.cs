using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class DefenceGameManager : MonoBehaviour
{
     public GameObject wallPrefab;
     public GameObject equipButton;
     public Camera cam;
     private TileMap tileMap;
     private Vector2Int tileGrid;


    void Awake()
    {
        GameObject gm = GameObject.FindWithTag("TileMap");
        cam = Camera.main;
        equipButton.SetActive(false);
        if(!gm.GetComponent<TileMap>())return;
        tileMap = gm.GetComponent<TileMap>();
    }
    private void Update()
    {
        InputTest();
    }
        void InputTest()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        var c = cam != null ? cam : Camera.main;
        if (c == null) return;

        var ray = c.ScreenPointToRay(Mouse.current.position.ReadValue());
        var plane = new Plane(Vector3.up, new Vector3(0f, tileMap.Origin.y, 0f));
        if (!plane.Raycast(ray, out float dist)) return;

        var world = ray.GetPoint(dist);
        var (gx, gz) = tileMap.WorldToGrid(world);
        if(tileMap.IsInBounds(gx,gz)&&tileMap.IsWalkable(gx,gz))
        {
            if(tileMap.DonCreateCheck(gx,gz))return;
            tileGrid = new Vector2Int(gx,gz);
            equipButton.SetActive(true);
            
        }
       
    }
    public void OnEquip()
    {
        tileMap.CreateWall(tileGrid.x,tileGrid.y);
        Instantiate(wallPrefab,tileMap.GridToWorld(tileGrid.x,tileGrid.y),Quaternion.identity);
        equipButton.SetActive(false);
    }
}
