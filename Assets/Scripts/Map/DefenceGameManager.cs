using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using TMPro;

public class DefenceGameManager : MonoBehaviour
{
     public RectTransform menuPanel;
     public GameObject wallPrefab;
     public GameObject equipButton;
     public GameObject breakButton;
     public TextMeshProUGUI breakText;
     public GameObject summonButton;
     public GameObject unitPrefab;
     public GameObject bossPrefab;
     public GameObject[] monsterPrefab;
     public Camera cam;
     public PathPreview pathPreview;
     private TileMap tileMap;
     private Vector2Int tileGrid;
     public int currentStage = 1;


    void Awake()
    {
        GameObject gm = GameObject.FindWithTag("TileMap");
        cam = Camera.main;
        if (menuPanel != null) menuPanel.gameObject.SetActive(false);
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
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) 
        {
            return; 
        }

        var c = cam != null ? cam : Camera.main;
        if (c == null) return;

        var ray = c.ScreenPointToRay(Mouse.current.position.ReadValue());
        var plane = new Plane(Vector3.up, new Vector3(0f, tileMap.Origin.y, 0f));
        if (!plane.Raycast(ray, out float dist)) return;

        var world = ray.GetPoint(dist);
        var (gx, gz) = tileMap.WorldToGrid(world);
        Vector3 tileWorldPos = tileMap.GridToWorld(gx, gz);
        if(tileMap.IsInBounds(gx,gz)&&tileMap.IsWalkable(gx,gz))
        {
            closeButton();
            if(tileMap.DonCreateCheck(gx,gz))return;
            tileGrid = new Vector2Int(gx,gz);     
            equipButton.SetActive(true);   
            MoveMenuToTile(tileWorldPos);     
        }
        else if(tileMap.IsInBounds(gx,gz)&&!tileMap.IsWalkable(gx,gz))
        {
            closeButton();
            tileGrid = new Vector2Int(gx,gz);
            summonButton.SetActive(true);
            breakButton.SetActive(true);
            if(tileMap.UnitCheck(gx,gz))
            {
                breakText.text = "벽 부수기";
            }
            else
            {
                breakText.text = "유닛 삭제";
            }
            MoveMenuToTile(tileWorldPos);
        }
        else
        {
            if (menuPanel != null) menuPanel.gameObject.SetActive(false);
        }
    }
    private void MoveMenuToTile(Vector3 tileWorlposition)
    {
        if (menuPanel == null || cam == null) return;
        Vector3 targetWorldPos = tileWorlposition + new Vector3(0f, 0.5f, 0f);

        // 카메라이 각도를 계산해 3D 좌표를 2D 화면상의 좌표로 변환합니다.
        Vector3 screenPos = cam.WorldToScreenPoint(targetWorldPos);

        // 만약 카메라 뒤쪽에 있는 좌표라면 UI를 그리지 않습니다.
        if (screenPos.z < 0) return;

        // UI 패널을 켜고 위치를 대입합니다.
        menuPanel.gameObject.SetActive(true);
        menuPanel.position = screenPos;
    }
    public void OnEquip()
    {
        tileMap.CreateWall(tileGrid.x,tileGrid.y,Instantiate(wallPrefab,tileMap.GridToWorld(tileGrid.x,tileGrid.y),Quaternion.identity));
        closeButton();
    }
    public void OnBreakButton()
    {

        if(tileMap.UnitCheck(tileGrid.x,tileGrid.y))
        {
            tileMap.BreakWall(tileGrid.x,tileGrid.y);
        }
        else
        {
            tileMap.BreakUnit(tileGrid.x,tileGrid.y);
        }
        closeButton();
      
    }
    public void OnSummonButton()
    {

        if(!tileMap.UnitCheck(tileGrid.x,tileGrid.y))
        {
            closeButton();
            return;
        }
        Vector3 pos = tileMap.GridToWorld(tileGrid.x,tileGrid.y);
        pos.y = 3.5f;
     
        tileMap.CreateUnit(tileGrid.x,tileGrid.y,Instantiate(unitPrefab,pos,quaternion.identity));
         closeButton();
    }
    public void GameStartButton()
    {
        GameStartButton(currentStage);
    }
    private void closeButton()
    {
        breakButton.SetActive(false);
        equipButton.SetActive(false);
        summonButton.SetActive(false);
    }
    public void PathButton()
    {
        if (tileMap == null || pathPreview == null) return;
        List<Vector2Int> path = Pathfinder.FindPath(tileMap, TileMap.Start, TileMap.Goal);
        if (path == null)
        {
            Debug.Log("경로를 찾을 수 없습니다.");
            return;
        }
        pathPreview.Show(tileMap, path);
    }
    public void GameStartButton(int stage)
    {
        List<Vector2Int> path = Pathfinder.FindPath(tileMap,TileMap.Start,TileMap.Goal);
        if(path == null)
        {
            Debug.Log("길을 찾을수없습니다.");
            return;
        }
        if(stage%5==0)
        {
            GameObject gm = Instantiate(bossPrefab,tileMap.GridToWorld(0,0),Quaternion.identity);
            gm.GetComponent<MoveEnemy>().SetPath(path);
            currentStage++;
            return;
        }
        if(stage%2==0)
        {
            GameObject gm = Instantiate(monsterPrefab[0],tileMap.GridToWorld(0,0),Quaternion.identity);
            gm.GetComponent<MoveEnemy>().SetPath(path);
            currentStage++;
            return;
        }
        else
        {
            GameObject gm = Instantiate(monsterPrefab[0],tileMap.GridToWorld(0,0),Quaternion.identity);
            gm.GetComponent<MoveEnemy>().SetPath(path);
            currentStage++;
            return;
        }
    }
}
