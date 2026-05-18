using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class DefenceGameManager : MonoBehaviour
{
     public GameObject wallPrefab;
     public GameObject equipButton;
     public GameObject breakButton;
     public GameObject summonButton;
     public GameObject unitPrefab;
     public GameObject bossPrefab;
     public GameObject[] monsterPrefab;
     public Camera cam;
     private TileMap tileMap;
     private Vector2Int tileGrid;
     public int currentStage = 1;


    void Awake()
    {
        GameObject gm = GameObject.FindWithTag("TileMap");
        cam = Camera.main;
        equipButton.SetActive(false);
        breakButton.SetActive(false);
        summonButton.SetActive(false);
        if(!gm.GetComponent<TileMap>())return;
        tileMap = gm.GetComponent<TileMap>();
        currentStage = 1;
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
            breakButton.SetActive(false);
            summonButton.SetActive(false);
            equipButton.SetActive(false);
            if(tileMap.DonCreateCheck(gx,gz))return;
            tileGrid = new Vector2Int(gx,gz);     
            equipButton.SetActive(true);        
            
        }
        else if(tileMap.IsInBounds(gx,gz)&&!tileMap.IsWalkable(gx,gz))
        {
            breakButton.SetActive(false);
            equipButton.SetActive(false);
            summonButton.SetActive(false);
            tileGrid = new Vector2Int(gx,gz);
            summonButton.SetActive(true);
            breakButton.SetActive(true);
        }
    }
    public void OnEquip()
    {
        tileMap.CreateWall(tileGrid.x,tileGrid.y,Instantiate(wallPrefab,tileMap.GridToWorld(tileGrid.x,tileGrid.y),Quaternion.identity));
        breakButton.SetActive(false);
        equipButton.SetActive(false);
        summonButton.SetActive(false);
    }
    public void OnBreakButton()
    {
        equipButton.SetActive(false);
        summonButton.SetActive(false);
        if(tileMap.UnitCheck(tileGrid.x,tileGrid.y))
        {
            tileMap.BreakWall(tileGrid.x,tileGrid.y);
            breakButton.SetActive(false);
        }
        else
        {
            tileMap.BreakUnit(tileGrid.x,tileGrid.y);
            breakButton.SetActive(false);
        }
      
    }
    public void OnSummonButton()
    {
        equipButton.SetActive(false);
        breakButton.SetActive(false);
        if(!tileMap.UnitCheck(tileGrid.x,tileGrid.y))
        {
            summonButton.SetActive(false);
            return;
        }
        Vector3 pos = tileMap.GridToWorld(tileGrid.x,tileGrid.y);
        pos.y = 3.5f;
     
        tileMap.CreateUnit(tileGrid.x,tileGrid.y,Instantiate(unitPrefab,pos,quaternion.identity));
        summonButton.SetActive(false);
    }
    public void GameStartButton()
    {
        GameStartButton(currentStage);
    }
    public void GameStartButton(int stage)
    {
        
        if(stage%5==0)
        {
            GameObject gm = Instantiate(bossPrefab,tileMap.GridToWorld(0,0),Quaternion.identity);
            List<Vector2Int> path = Pathfinder.FindPath(tileMap,TileMap.Start,TileMap.Goal);
            gm.GetComponent<MoveEnemy>().SetPath(path);
            currentStage++;
            return;
        }
        if(stage%2==0)
        {
            GameObject gm = Instantiate(monsterPrefab[0],tileMap.GridToWorld(0,0),Quaternion.identity);
            List<Vector2Int> path = Pathfinder.FindPath(tileMap,TileMap.Start,TileMap.Goal);
            gm.GetComponent<MoveEnemy>().SetPath(path);
            currentStage++;
            return;
        }
        else
        {
            GameObject gm = Instantiate(monsterPrefab[0],tileMap.GridToWorld(0,0),Quaternion.identity);
            List<Vector2Int> path = Pathfinder.FindPath(tileMap,TileMap.Start,TileMap.Goal);
            gm.GetComponent<MoveEnemy>().SetPath(path);
            currentStage++;
            return;
        }
    }
}
