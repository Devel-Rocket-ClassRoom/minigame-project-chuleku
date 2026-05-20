using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public enum Difficulty
{
    Easy,
    Normal,
    Hard
}
public class DefenceGameManager : MonoBehaviour
{
    public static DefenceGameManager Instance {get; private set;}
     public RectTransform menuPanel;
     public GameObject wallPrefab;
     public GameObject equipButton;
     public GameObject breakButton;
     public TextMeshProUGUI breakText;
     public TextMeshProUGUI createWallText;
     public GameObject summonButton;
     public GameObject summonScrollView;
     
     public GameObject unitPrefab;
     public GameObject bossPrefab;
     public GameObject[] monsterPrefab;
     public int allCount =0;
     public int alivecount = 0;
     public Camera cam;
     public PathPreview pathPreview;
     private TileMap tileMap;
     private Vector2Int tileGrid;
     public int currentStage = 1;
     private const float clickThreshold = 0.25f;
     private float pressStartTime;
     private bool isPressing;
     private bool roundStart;
     private Coroutine cor;

     
    public Difficulty difficulty = Difficulty.Easy;


    void Awake()
    {
        Instance = this;
        GameObject gm = GameObject.FindWithTag("TileMap");
        tileMap = gm.GetComponent<TileMap>();
        cam = Camera.main;
        if (menuPanel != null) menuPanel.gameObject.SetActive(false);
        if(cor!=null) StopCoroutine(cor);
        cor =null;
        roundStart = false;
        equipButton.SetActive(false);
        breakButton.SetActive(false);
        summonScrollView.SetActive(false);
        summonButton.SetActive(false);
        if(!gm.GetComponent<TileMap>())return;
        currentStage = 1;
        alivecount = 0;
        allCount = 0;

    }
    void Start()
    {
        switch(difficulty)
        {
            case Difficulty.Easy:
            DifficultyWallCreate(20);
            break;
            case Difficulty.Normal:
            DifficultyWallCreate(40);
            break;
            case Difficulty.Hard:
            DifficultyWallCreate(60);
            break;
        }

        if (CardGameManager.Instance != null)
            CardGameManager.Instance.UnitSlotClicked += OnUnitSlotClicked;
    }

    void OnDestroy()
    {
        if (CardGameManager.Instance != null)
            CardGameManager.Instance.UnitSlotClicked -= OnUnitSlotClicked;
    }

    // 유닛 패널 버튼 클릭 콜백: 현재 선택된 tileGrid에 슬롯 유닛을 배치
    void OnUnitSlotClicked(CardGameManager.UnitCardSlot slot)
    {
        if (slot.placedUnit != null) return;
        if (tileMap == null) return;
        if (!tileMap.WallCheck(tileGrid.x, tileGrid.y)) { closeButton(); return; }
        if (!tileMap.UnitCheck(tileGrid.x, tileGrid.y)) { closeButton(); return; }

        // UnitTable에서 프리팹/스탯을 cardId로 조회
        var udata = DataTableManager.UnitTable?.Get(slot.cardId);
        if (udata == null) { Debug.LogWarning($"UnitTable에 '{slot.cardId}' 없음"); return; }

        var prefab = LoadUnitPrefab(udata.Prefab);
        if (prefab == null) { Debug.LogWarning($"유닛 프리팹 로드 실패: '{udata.Prefab}'"); return; }

        Vector3 pos = tileMap.GridToWorld(tileGrid.x, tileGrid.y);
        pos.y = 3.5f;
        slot.placedUnit = Instantiate(prefab, pos, Quaternion.identity);

        var unit = slot.placedUnit.GetComponent<UnitBase>();
        if (unit != null) unit.SetupUnitStatus(udata.Attack, udata.AttackSpeed, udata.Range);

        tileMap.CreateUnit(tileGrid.x, tileGrid.y, slot.placedUnit);
        if (slot.buttonGo != null) slot.buttonGo.SetActive(false);
        closeButton();
    }

    private static GameObject LoadUnitPrefab(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        const string prefix = "Resources/";
        if (key.StartsWith(prefix)) key = key.Substring(prefix.Length);
        return Resources.Load<GameObject>(key);
    }

    private void Update()
    {
        TileInput();
    }
    void TileInput()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                isPressing = false;
                return;
            }
            isPressing = true;
            pressStartTime = Time.unscaledTime;
            return;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isPressing)
        {
            isPressing = false;
            if (Time.unscaledTime - pressStartTime <= clickThreshold)
            {
                HandleTileClick(Mouse.current.position.ReadValue());
            }
        }
    }

    void HandleTileClick(Vector2 screenPos)
    {
        var c = cam != null ? cam : Camera.main;
        if (c == null) return;

        var ray = c.ScreenPointToRay(screenPos);
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
                int amount = 0;
                switch (difficulty)
                {
                    case Difficulty.Easy:
                    amount = 1;
                    break;
                    case Difficulty.Normal:
                    amount = 2;
                    break;
                    case Difficulty.Hard:
                    amount = 3;
                    break;
                    
                }
                if(tileMap.TilesView[gx,gz].wallStageID==currentStage)
                {
                    breakText.text = $"벽 부수기(+3)";
                }
                else
                {
                    breakText.text = $"벽 부수기(-{amount})";
                }
                
            }
            else
            {
                breakText.text = "유닛 해제";
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
    public void OnCreateWall()
    {
        if (roundStart)
        {
            Debug.Log("게임중에는 벽을 설치할수 없습니다.");
            closeButton();
            return;
        }
        if(ResourceManager.Instance.TrySpendGold(3))
        {
            tileMap.CreateWall(tileGrid.x,tileGrid.y,Instantiate(wallPrefab,tileMap.GridToWorld(tileGrid.x,tileGrid.y),Quaternion.identity),currentStage);
        }
        else
        {
            Debug.Log("골드가 부족합니다.");
        }
        closeButton();
    }
    public void OnBreakButton()
    {
        if(roundStart)
        {
            Debug.Log("게임 중에는 벽을 부술수없습니다.");
            closeButton();
            return;
        }
        int v = 0;
        switch (difficulty)
        {
            case Difficulty.Easy:
            v = 1;
            break;
            case Difficulty.Normal:
            v = 2;
            break;
            case Difficulty.Hard:
            v = 3;
            break;      
        }

        if(tileMap.UnitCheck(tileGrid.x,tileGrid.y))
        {

            tileMap.BreakWall(tileGrid.x,tileGrid.y,currentStage,v); 
 
        }
        else
        {
            tileMap.BreakUnit(tileGrid.x,tileGrid.y);
        }
        closeButton();
      
    }
    public void OnSummon()
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
        summonScrollView.SetActive(false);
    }
    public void OnSummonButton()
    {
        summonScrollView.SetActive(true);
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
        if (roundStart)
        {
            Debug.Log("게임중에는 시작을 누를수없습니다");
            return;
        }
        List<Vector2Int> path = Pathfinder.FindPath(tileMap,TileMap.Start,TileMap.Goal);
        if(path == null)
        {
            Debug.Log("길을 찾을수없습니다.");
            return;
        }
  
        roundStart = true;
        // if(stage%5==0)
        // {
        //     GameObject gm = Instantiate(bossPrefab,tileMap.GridToWorld(0,0),Quaternion.identity);
        //     gm.GetComponent<MoveEnemy>().SetPath(path);
        //     currentStage++;
        //     return;
        // }
        // if(stage%2==0)
        // {
            
        //     currentStage++;
        //     return;
        // }
        // else
        alivecount = 2*stage;
        allCount = 2*stage;
        ResourceManager.Instance.enemyCountText.text = $"{alivecount}/{allCount}";
        cor = StartCoroutine(SpawnMonsterCort(alivecount,monsterPrefab[0],path));
        CardGameManager.Instance.EndRound();
        currentStage++;
        return;
    }
    IEnumerator SpawnMonsterCort(int count,GameObject prefab,List<Vector2Int> path)
    {
        float spawndelay = 0.5f;
        float c = 0;
        while(c<count)
        {
            
            Vector3 pos = tileMap.GridToWorld(TileMap.Start);
            GameObject go =Instantiate(prefab,pos,Quaternion.identity);
            go.GetComponent<MoveEnemy>().SetPath(path);
            c++;
            yield return new WaitForSeconds(spawndelay);
        }
        cor = null;
    }

    public void EnemyDie()
    {
        alivecount--;
        ResourceManager.Instance.enemyCountText.text = $"{alivecount}/{allCount}";
        if(alivecount<=0)
        {
            RoundEnd();
        }
    }
    private void RoundEnd()
    {
        roundStart = false;
        CardGameManager.Instance.StartRound();
        Debug.Log("라운드 종료 준비라운드!");
    }


    private void DifficultyWallCreate(int wallCount)
    {
        if (tileMap == null || wallPrefab == null) return;

        int createwall = 0;
        int maxattemps = 200;
        int attemp = 0;
        while(createwall<wallCount&&attemp<maxattemps)
        {
            attemp++;
            int rx = UnityEngine.Random.Range(0, TileMap.W);
            int rz = UnityEngine.Random.Range(0, TileMap.H);
            if(tileMap.IsInBounds(rx,rz)&&tileMap.IsWalkable(rx,rz))
            {
                Vector2Int randomPos = new Vector2Int(rx,rz);
                if(randomPos==TileMap.Start||randomPos == TileMap.Goal)
                continue;
                if(tileMap.WallCheck(rx,rz))
                continue;

                Vector3 worldPos = tileMap.GridToWorld(rx,rz);
                GameObject wallgo = Instantiate(wallPrefab,worldPos,Quaternion.identity);
                tileMap.CreateWall(rx,rz,wallgo,-1);
                createwall++;
            }
        }
        if (tileMap == null) return;
        List<Vector2Int> path = Pathfinder.FindPath(tileMap, TileMap.Start, TileMap.Goal);
        if(path==null)
        {
            Debug.Log("길 없음 다시생성");
            tileMap.AllWallBreak();
            DifficultyWallCreate(wallCount);
        }
    }

}
