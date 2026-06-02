using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using System.Text.RegularExpressions;
using System.Linq;

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
     public TextMeshProUGUI currenStageText;
     public TextMeshProUGUI phaseText;
     public GameObject summonButton;
     public GameObject pathbutton1;
     public GameObject pathbutton2;
     public GameObject summonScrollView;
    public Phase CurrentPhase => phase;
     public int allCount =0;
     public int alivecount = 0;
     private int createWallCost = 3;
     public Camera cam;
     public PathPreview pathPreview;
     private TileMap tileMap;
     private Vector2Int tileGrid;
     public int currentStage = 1;
     private const float clickThreshold = 0.25f;
     private float pressStartTime;
     private bool isPressing;
     private bool roundStart;
     private Phase phase;
     private Coroutine spawncor;
     private Coroutine phasecor;
     private bool bossKillCheck;
     public bool Round => roundStart;
    public bool diecheck;
    private bool pathOnOff;
    public Difficulty difficulty = Difficulty.Easy;


    void Awake()
    {
        Instance = this;
        GameObject gm = GameObject.FindWithTag("TileMap");
        tileMap = gm.GetComponent<TileMap>();
        cam = Camera.main;
        if (menuPanel != null) menuPanel.gameObject.SetActive(false);
        if(spawncor!=null) StopCoroutine(spawncor);
        spawncor =null;
        if(phasecor!=null) StopCoroutine(phasecor);
        phasecor =null;
        roundStart = false;
        equipButton.SetActive(false);
        breakButton.SetActive(false);
        summonScrollView.SetActive(false);
        summonButton.SetActive(false);
        if(!gm.GetComponent<TileMap>())return;
        currentStage = 1;
        currenStageText.text = $"스테이지 {currentStage}";
        phaseText.text = "메인 페이즈";
        phaseText.color = Color.blue; // 메인 페이즈 = 파랑
        phase = Phase.Main;
        alivecount = 0;
        allCount = 0;
        difficulty = GameSession.SelectedDifficulty;
        diecheck = false;
        pathOnOff = true;
        pathbutton1.SetActive(false);
        pathbutton2.SetActive(true);
    }
    void Start()
    {
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
        if (unit != null) unit.SetupUnitStatus(udata.Attack, udata.AttackSpeed, udata.Range,udata.UpgradeAmount);

        tileMap.CreateUnit(tileGrid.x, tileGrid.y, slot.placedUnit);
        TutorialManager.Instance?.NotifyUnitPlaced();
        if (slot.buttonGo != null) slot.buttonGo.SetActive(false);
        closeButton();
    }
    private static GameObject LoadMonsterPrefab(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        const string prefix = "Resources/";
        if (key.StartsWith(prefix)) key = key.Substring(prefix.Length);
        return Resources.Load<GameObject>(key);
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
        if(Time.timeScale==0)return;

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
                UiManager.Instance.CloseInfo();
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
            if(roundStart)return;
            tileGrid = new Vector2Int(gx,gz);
            equipButton.SetActive(true);
            if(ResourceManager.Instance.FreeCreateWallCoupon>=1)
            createWallText.text = "벽 생성";
            else createWallText.text = $"벽 생성(-{createWallCost})";
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
                    if(tileMap.TilesView[gx,gz].Coupon==1)
                    {
                        breakText.text = $"벽 부수기";
                    }
                    else
                    {
                        breakText.text = $"벽 부수기(+{tileMap.TilesView[gx,gz].installCost})";
                    }
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
        if(ResourceManager.Instance.FreeCreateWallCoupon>=1)
        {
            ResourceManager.Instance.TrySpendFreeCreateWallCoupon(1);
            tileMap.CreateWallCoupon(tileGrid.x,tileGrid.y,Instantiate(wallPrefab,tileMap.GridToWorld(tileGrid.x,tileGrid.y),Quaternion.identity),currentStage,1);
            TutorialManager.Instance?.NotifyWallPlaced();
            closeButton();
            return;
        }
        if(ResourceManager.Instance.TrySpendGold(createWallCost))
        {
            tileMap.CreateWall(tileGrid.x,tileGrid.y,Instantiate(wallPrefab,tileMap.GridToWorld(tileGrid.x,tileGrid.y),Quaternion.identity),currentStage,createWallCost);
            TutorialManager.Instance?.NotifyWallPlaced();
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
            TutorialManager.Instance?.NotifyWallBroken();

        }
        else
        {
            tileMap.BreakUnit(tileGrid.x,tileGrid.y);
        }
        closeButton();
    }
    public void GameStartButton()
    {
        // 튜토리얼 StartBattle 단계에서는 전용 작은 웨이브(TutorialStage)를 쓴다.
        // currentStage 필드는 그대로 1이므로 라운드 종료 후 정상(2)으로 진행된다.
        bool tut = TutorialManager.Instance != null
                   && TutorialManager.Instance.Current == TutorialManager.Step.StartBattle;
        GameStartButton(tut ? TutorialStage : currentStage);
    }
    public void closeButton()
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
            tileMap.WarningWallColor(currentStage);
            return;
        }
        pathOnOff = !pathOnOff;
        if(pathOnOff)
        {
            pathbutton1.SetActive(false);
            pathbutton2.SetActive(true);
            
        }
        else
        {
            pathbutton1.SetActive(true);
            pathbutton2.SetActive(false);
        }
        tileMap.PathOnOff(pathOnOff);
        TutorialManager.Instance?.NotifyPathPreviewed();
    }
    public void GameStartButton(int stage)
    {
        if(!TutorialManager.Instance.IsRunning||!TutorialManager.Instance.IsTutorial)return;
        if (roundStart)
        {
            Debug.Log("게임중에는 시작을 누를수없습니다");
            return;
        }
        if(spawncor !=null)
        {
            StopCoroutine(spawncor);
            spawncor = null;
        }
        List<Vector2Int> path = Pathfinder.FindPath(tileMap,TileMap.Start,TileMap.Goal);
        if(path == null)
        {
            Debug.Log("길을 찾을수없습니다.");
             tileMap.WarningWallColor(currentStage);
            return;
        }
        if(stage !=1111)
        {
            if(TutorialManager.Instance.IsRunning)
            {
                Debug.Log("튜토리얼중엔 시작 누를수없습니다.");
                return;
            }
        }
        var Groups = DataTableManager.StageTable.Get(GetStageLookupId(stage));
        if( Groups ==null){
            UiManager.Instance.gameoverText.text = "라운드 미구현";
            UiManager.Instance.GameEnd();
            return;
        }
        roundStart = true;
        TutorialManager.Instance?.NotifyBattleStarted();
        UiManager.Instance.HideStoreButton();
        phase = Phase.Battle;
        if(phasecor !=null)StopCoroutine(phasecor);
        phasecor = StartCoroutine(BattlePhaseCor());
        alivecount = 0;
        allCount = 0;
        foreach(var g in Groups)allCount += GetScaleCount(g.Count,stage);
        alivecount = allCount;
        ResourceManager.Instance.enemyCountText.text = $"{alivecount}/{allCount}";
        foreach(var g in Groups)
        {
            var prefab = LoadMonsterPrefab(g.Prefab);
            StartCoroutine(SpawnMonsterCort(g.SpawnTime,GetScaleCount(g.Count,stage),g.Delay,path,prefab));
        }
        
        CardGameManager.Instance.EndRound();
        UiManager.Instance.StartGameUiHide();
        return;
    }
    private IEnumerator SpawnMonsterCort(float spawntime,int count,float delay,List<Vector2Int> path,GameObject prefab)
    {
        yield return new WaitForSeconds(spawntime);
        float c = 0;
        while(c<count)
        {
            Vector3 pos = tileMap.GridToWorld(TileMap.Start);
            GameObject go =Instantiate(prefab,pos,Quaternion.identity);
            go.GetComponent<MoveEnemy>().SetPath(path);
            c++;
            yield return new WaitForSeconds(delay);
        }
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
        if(phasecor != null)
        {
            StopCoroutine(phasecor);
            phasecor = null;
        }
        if(spawncor != null)
        {
            StopCoroutine(spawncor);
            spawncor = null;
        }
        if(diecheck)return;

        TutorialManager.Instance?.NotifyRoundEnded();

        if (UpgradeManager.Instance != null)
        UpgradeManager.Instance.OnRoundEnded();
        CardGameManager.Instance.StartRound();
        ResourceManager.Instance.StartRound();
        Debug.Log("라운드 종료 준비라운드!");
        phaseText.text = "메인 페이즈";
        phaseText.color = Color.blue; // 메인 페이즈 = 파랑
        phase = Phase.Main;
        currentStage++;
        currenStageText.text = $"스테이지 {currentStage}";
        StageCountSet(currentStage);
        UiManager.Instance.ViewStoreButton();
        if(bossKillCheck)
        {
            UiManager.Instance.KillBoss();
            bossKillCheck=false;
        }
    }


    private void DifficultyWallCreate(int wallCount)
    {
        if (tileMap == null || wallPrefab == null) return;

        int createwall = 0;
        int maxattemps = 200;
        int attemp = 0;
        tileMap.FirstCreateWall(true);
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
                tileMap.CreateWall(rx,rz,wallgo,-1,0);
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
          tileMap.FirstCreateWall(false);
    }
    IEnumerator BattlePhaseCor()
    {
        float delay = 0.5f;
        phaseText.color = Color.red; // 배틀 페이즈 = 빨강 (한 번만 설정, 점 애니메이션은 색 유지)
        while(roundStart)
        {
            phaseText.text = "배틀 페이즈.";
            yield return new WaitForSeconds(delay);
            phaseText.text = "배틀 페이즈..";
            yield return new WaitForSeconds(delay);
            phaseText.text = "배틀 페이즈...";
            yield return new WaitForSeconds(delay);
        }
    }
    public void StageCountSet(int stage)
    {
        var Groups = DataTableManager.StageTable.Get(GetStageLookupId(stage));
        if(Groups == null)
        {
            Debug.Log("스테이지 미구현");
            return;
        }
        int monsterCount =0;
        Dictionary<string,int> m = new();
        foreach(var c in Groups)
        {
            monsterCount++;
            if(monsterCount >1)
            {
                ResourceManager.Instance.enemyCountText.text += $"\n{DataTableManager.StringTable?.Get(c.MonsterName)} : {GetScaleCount(c.Count,stage)}";
            }
            else
            {
                ResourceManager.Instance.enemyCountText.text =$"{DataTableManager.StringTable?.Get(c.MonsterName)} : {GetScaleCount(c.Count,stage)}";
            }
        }
    }
    // 튜토리얼 전용 스테이지 ID. StageTable에 이 ID로 작은 웨이브(예: 기본몬스터 1마리)를 둔다.
    public const int TutorialStage = 1111;

    public static int GetStageLookupId(int stage)
    {
        if (stage == TutorialStage) return TutorialStage; // 튜토리얼은 무한모드 매핑/스케일을 타지 않음
        return stage > 10 ? ((stage-6)%5)+1001 : stage;
    }
    public static int GetScaleCount(int baseCount,int stage)
    {
        if (stage == TutorialStage) return baseCount; // 튜토리얼은 마릿수 스케일 안 함
        if(stage<=10)return baseCount;
        int loops = (stage-6)/5;
        return Mathf.RoundToInt(baseCount*(1f+loops*0.3f));
    }

    public void BossKill()
    {
        bossKillCheck = true;
    }

    // 튜토리얼 전용: 웨이브 없이 페이즈만 임시로 바꾼다(마법 시연용).
    // roundStart는 건드리지 않으므로 벽 설치 등 준비페이즈 동작은 그대로 가능.
    public void SetPhase(Phase p)
    {
        phase = p;
        phaseText.color = p == Phase.Battle ? Color.red : Color.blue;
        phaseText.text = p == Phase.Battle ? "배틀 페이즈" : "메인 페이즈";
    }
    public void StartGame()
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
        StageCountSet(currentStage);
        SoundManager.PlayBgm("InGameBGM");
        currentStage = 1;
        currenStageText.text = $"스테이지 {currentStage}";
        phaseText.text = "메인 페이즈";
        phaseText.color = Color.blue; // 메인 페이즈 = 파랑
    }
}
