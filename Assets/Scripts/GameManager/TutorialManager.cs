using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public GameObject tutorialPanal;

    // ───────── 아래부터 추가된 튜토리얼 뼈대 ─────────

    // 튜토리얼 단계. 게임 의존성 순서대로 진행한다.
    // (벽으로 길 만들기 → 경로 확인 → 유닛 배치 → 전투 → 자원/상점)
    public enum Step
    {
        None,        // 비활성
        Intro,       // "적은 시작점→끝점으로 간다" 안내 (다음 버튼)
        ViewResource,
        BuildWall,   // 벽 설치 유도
        BuildWallSecond,
        BreakWall,   // 벽 파괴 유도
        PreviewPath, // 경로 미리보기 버튼 유도
        PlaceUnit,   // 유닛 패널에서 벽 위로 배치 유도
        StartBattle, // 게임 시작 버튼 유도
        Battle,      // 전투 관전 (라운드 종료 대기)
        UseCard,     // 자원/효과 카드 사용 유도
        UseCardTest,
        Shop,        // 상점 안내
        ShopTest,
        BreakCard,   // 유닛 카드파괴 (설명)
        BreakCardTest, // 실제로 유닛카드 파괴 실습
        UseMagic,    // 마법 사용
        UseMainMagic,
        Last,
        Done         // 종료
    }

    [Header("가이드 UI (인스펙터에서 연결, 없으면 null 허용)")]
    [SerializeField] private GameObject guidePanel;     // 단계 설명 말풍선 패널
    [SerializeField] private TextMeshProUGUI guideText; // 설명 텍스트
    [SerializeField] private GameObject nextButton;     // 수동 진행용 "다음" 버튼
    [SerializeField] private GameObject skipButton;     // 튜토리얼 건너뛰기
    
    [SerializeField] private GameObject tutorialUiPanal;
    [SerializeField] private GameObject tutorialUiSpawn;
    [SerializeField] private GameObject tutorialUiEnd;
    [SerializeField] private GameObject tutorialUiResource;
    [SerializeField] private GameObject tutorialUiPath;
    [SerializeField] private GameObject tutorialUiGameStartButton;
    [SerializeField] private GameObject uiStartButton;
    [SerializeField] private GameObject uiPath;
    [SerializeField] private GameObject uiStore;
    [SerializeField] private GameObject uiResourceSpotlight;

    [Header("스포트라이트(구멍 강조)")]
    [SerializeField] private TutorialSpotlight spotlight;


    private Step current = Step.None;
    public Step Current => current;
    public bool IsRunning => current != Step.None && current != Step.Done;
    private bool first;
    private bool path;
    private bool startbutton;
    private Coroutine cor;

    private const string TutorialDoneKey = "tutorial_done";

    void Awake()
    {
        Instance = this;
        tutorialPanal.SetActive(true);
        if (guidePanel != null) guidePanel.SetActive(false);
        // if(PlayerPrefs.GetInt(TutorialDoneKey)==1)
        // {
        //     OnClickNo();
        // }
        if(cor != null) StopCoroutine(cor);
        cor =null;
        tutorialUiPanal.SetActive(false);
        first = false;
        path = false;
        startbutton = false;
        skipButton.SetActive(false);
        AllFalse();
    }
    public void AllFalse()
    {
        tutorialUiPanal.SetActive(false);
        tutorialUiSpawn.SetActive(false);
        tutorialUiEnd.SetActive(false);
        tutorialUiPath.SetActive(false);
        tutorialUiGameStartButton.SetActive(false);
        uiResourceSpotlight.SetActive(false);
        if (spotlight != null) spotlight.Hide(); // 단계 전환 시 이전 구멍 강조 끄기
        first = false;
        path = false;
        startbutton = false;
    }

    public void OnClickNo()
    {
        tutorialPanal.SetActive(false);
        AllFalse();
        CardGameManager.Instance.StartGame();
        DefenceGameManager.Instance.StartGame();
        ResourceManager.Instance.StartGame();
        StoreManager.Instance.StartGame();
        ScoreManager.Instance.StartGame();
        uiPath.GetComponent<Canvas>().sortingOrder = 0;
        uiStartButton.GetComponent<Canvas>().sortingOrder = 0;  
        uiStore.GetComponent<Canvas>().sortingOrder = 0;
        skipButton.SetActive(false);
        
    }

    public void OnClickYes()
    {
        tutorialPanal.SetActive(false);
        skipButton.SetActive(true);
        StartTutorial(); // ← 추가된 한 줄: 튜토리얼 오버레이 시작
    }

    // ───────── 진행 제어 ─────────

    public void StartTutorial()
    {
        // 게임은 매니저 Start()에서 이미 시작된 상태라 여기선 단계만 띄운다.
        GoToStep(Step.Intro);
    }

    public void EndTutorial()
    {
        current = Step.Done;
        if (guidePanel != null) guidePanel.SetActive(false);
        if (DefenceGameManager.Instance != null)
            DefenceGameManager.Instance.SetPhase(Phase.Main); // 임시 전투 페이즈 원복
        PlayerPrefs.SetInt(TutorialDoneKey, 1);
        OnClickNo();
    }

    public void OnClickSkip() => EndTutorial();

    // 다음 단계로 넘긴다(수동/자동 공통). 단계 데이터 표시는 ShowStep에서.
    private void GoToStep(Step step)
    {
        current = step;
        AllFalse();
        ShowStep(step);
    }

    private void Advance()
    {
        // enum 순서대로 다음 단계로. Done이면 종료.
        if (current == Step.Done) return;
        Step next = current + 1;
        if (next == Step.Done) { EndTutorial(); return; }
        GoToStep(next);
    }

    // 현재 단계의 안내문/허용 UI를 세팅한다.
    // TODO: 단계별 하이라이트(화살표/구멍 뚫기)·허용 영역 제한은 여기에 추가
    private void ShowStep(Step step)
    {
        if (guidePanel != null) guidePanel.SetActive(true);
        if (guideText != null) guideText.gameObject.SetActive(true); // BreakCardTest에서 꺼둔 걸 되살림

        bool manualNext = false; // 플레이어가 "다음"을 눌러야 넘어가는 단계인지
        string msg = "";
        GameObject go = GameObject.FindWithTag("TileMap");
        var t = go.GetComponent<TileMap>();
        switch (step)
        {
            case Step.Intro:
                msg = "적은 왼쪽위 시작점에서부터 오른쪽아래 끝점으로 이동합니다.";
                manualNext = true;
                first = true;
                tutorialUiPanal.SetActive(true);
                tutorialUiSpawn.SetActive(true);
                tutorialUiEnd.SetActive(true);
                spotlight.Show(t.Origin, t.GridToWorld(19,9));
                if(cor!=null) StopCoroutine(cor);
                cor = StartCoroutine(SizeEffect());
                break;
            case Step.ViewResource:
                msg = "골드,마나,샤드,쿠폰,체력,업그레이드 순으로 확인할수있습니다.";
                uiResourceSpotlight.SetActive(true);
                manualNext = true;
                break;
            case Step.BuildWall:
                ResourceManager.Instance.AddGold(6);
                spotlight.Show(t.GridToWorld(1,0));
                msg = "빈 타일을 눌러 벽을 세워 길을 막아보세요.\n벽 생성은 3골드를 필요로 합니다.";
                break;
            case Step.BuildWallSecond:
                msg = "빈 타일을 눌러 벽을 세워 길을 막아보세요.\n벽 생성은 3골드를 필요로 합니다.";
                spotlight.Show(t.GridToWorld(1,1));
                break;
            case Step.BreakWall:
                msg = "벽을 설치한 라운드에는 100%환불이 되며\n그 이외 라운드에는 난이도에 따라 1, 2, 3 골드의 비용이 지불됩니다.";
                spotlight.Show(t.GridToWorld(1,1));
                break;
            case Step.PreviewPath:
                msg = "경로 미리보기로 적이 갈 길을 확인하세요.";
                tutorialUiPanal.SetActive(true);
                path = true;
                tutorialUiPath.SetActive(true);
                if(cor !=null)StopCoroutine(cor);
                cor = StartCoroutine(SizeEffect());
                uiPath.GetComponent<Canvas>().sortingOrder = 180;
                manualNext = false;
                break;
            case Step.PlaceUnit:
                msg = "벽을 누르고 유닛 패널에서 유닛을 벽 위에 배치하세요.";

                CardGameManager.Instance.AddResourceCard("LostGold");
                CardGameManager.Instance.AddUnitCard("Archer");
                manualNext = false;
                break;
            case Step.StartBattle:
                msg = "준비가 끝났으면 게임 시작을 누르세요.";
                tutorialUiPanal.SetActive(true);
                tutorialUiGameStartButton.SetActive(true);
                startbutton = true;
                if(cor !=null)StopCoroutine(cor);
                cor = StartCoroutine(SizeEffect());
                uiStartButton.GetComponent<Canvas>().sortingOrder = 180;
                manualNext = false;
                break;
            case Step.Battle:
                msg = "유닛이 자동으로 적을 공격합니다.\n라운드가 끝날 때까지 지켜보세요.";
                manualNext = false;
                break;
            case Step.UseCard:
                msg = "손패의 자원 카드를 사용해 골드를 얻어보세요.";
                tutorialUiPanal.SetActive(true);
                manualNext = true;
                break;
            case Step.UseCardTest:
                guidePanel.SetActive(false);
                CardGameManager.Instance.DrawCard();
                manualNext = false;
                break;
            case Step.Shop:
                msg = "상점에서 골드와 마나로 카드를 살 수 있습니다.";
                guidePanel.SetActive(true);
                tutorialUiPanal.SetActive(true);
                ResourceManager.Instance.AddMana(1);
                StoreManager.Instance.TutorialRollStock();
                uiStore.GetComponent<Canvas>().sortingOrder = 180;
                manualNext = true;
                break;
            case Step.ShopTest:
                guidePanel.SetActive(false);
                manualNext = false;
                break;
            case Step.BreakCard:
                msg = "패에 있는 유닛카드를 파괴하면 벽에 배치한 유닛도 파괴됩니다.\n효과카드를 이용해 유닛카드를 파괴해봅시다.";
                UiManager.Instance.StartGameUiHide();
                tutorialUiPanal.SetActive(true);
                guidePanel.SetActive(true);
                CardGameManager.Instance.AddEffectCard("DestroyDraw");
                CardGameManager.Instance.DrawCard();
                CardGameManager.Instance.DrawCard();
                manualNext = true; // 설명 단계 → "다음" 누르면 실습(BreakCardTest)으로
                break;
            case Step.BreakCardTest:
                guidePanel.SetActive(false);
                guideText.gameObject.SetActive(false);
                manualNext = false; // 실제로 유닛카드를 파괴하면 NotifyCardDestroyed로 진행
                break;
            case Step.UseMagic:
                msg = "마법을 클릭시 왼쪽에서 정보를 확인할수 있습니다.\n마법을 맵 위로 드래그해 사용해보세요.";
                guidePanel.SetActive(true);
                DefenceGameManager.Instance.SetPhase(Phase.Battle); // 임시 전투 페이즈
                MagicManager.Instance.AddMagic("FireBall");
                MagicManager.Instance.TutorialUiOn();               // 마법 패널 켜기
                manualNext = false;
                break;
            case Step.UseMainMagic:
                msg = "메인 페이즈 마법은 정보창에서 사용하기 버튼을 눌러 사용합니다.";
                DefenceGameManager.Instance.SetPhase(Phase.Main);
                MagicManager.Instance.AddMagic("DarkHands");
                MagicManager.Instance.TutorialUiOn(); 
                manualNext = false;
                break;
            case Step.Last:
                msg = "이제 시작하겠습니다!";
                manualNext = true;
                break;
        }

        if (guideText != null) guideText.text = msg;
        if (nextButton != null) nextButton.SetActive(manualNext);
        if (skipButton != null) skipButton.SetActive(true);
    }

    // "다음" 버튼 OnClick에 연결. 수동 진행 단계에서만 의미 있음.
    public void OnClickNext()
    {
        if (!IsRunning) return;
        Advance();
    }

    // ───────── 게임 시스템이 호출하는 진행 훅 ─────────
    // 각 매니저의 해당 동작 성공 지점에서 TutorialManager.Instance?.Notify...() 를 호출하면
    // 현재 단계와 일치할 때만 다음 단계로 넘어간다. (불일치 시 무시 → 평소엔 안전)

    // DefenceGameManager.OnCreateWall 성공 후
    public void NotifyWallPlaced()
    {
        if (current == Step.BuildWall||current == Step.BuildWallSecond) Advance();
    }

    // DefenceGameManager.PathButton (경로 표시 성공) 후
    public void NotifyPathPreviewed()
    {
        if (current == Step.PreviewPath) Advance();
    }

    // DefenceGameManager.OnUnitSlotClicked (유닛 배치 성공) 후
    public void NotifyUnitPlaced()
    {
        if (current == Step.PlaceUnit) Advance();
    }

    // DefenceGameManager.GameStartButton (전투 진입 성공) 후
    public void NotifyBattleStarted()
    {
        if (current == Step.StartBattle) Advance();
    }

    // DefenceGameManager.RoundEnd 에서
    public void NotifyRoundEnded()
    {
        if (current == Step.Battle) Advance();
    }

    // 자원/효과 카드 사용 성공 후 → 실습 단계(UseCardTest) 완료
    public void NotifyCardUsed()
    {
        if (current == Step.UseCardTest) Advance();
    }

    // DefenceGameManager.OnBreakButton (벽 파괴 성공) 후
    public void NotifyWallBroken()
    {
        if (current == Step.BreakWall) Advance();
    }

    // InfoUi.OnBuy (상점 구매 성공) 후 → 실습 단계(ShopTest) 완료
    public void NotifyShopBought()
    {
        if (current == Step.ShopTest) Advance();
    }

    // CardGameManager.RemoveCardByInstanceId (카드 파괴) 후 → 실습 단계 완료
    public void NotifyCardDestroyed()
    {
        if (current == Step.BreakCardTest) Advance();
    }

    // MagicManager.UseMagic (마법 사용 성공) 후 → 전투마법(UseMagic) / 메인마법(UseMainMagic) 둘 다
    public void NotifyMagicUsed()
    {
        if (current == Step.UseMagic || current == Step.UseMainMagic) Advance();
    }

    public void CheckPanal()
    {
        tutorialPanal.SetActive(false);
    }

    // ───────── (선택) 행동 게이팅용 헬퍼 ─────────
    // 특정 단계에서 다른 동작을 막고 싶으면 DefenceGameManager 쪽에서 이걸 참조해 제한.
    // TODO: 필요해지면 단계별 허용 동작 표를 채운다.
    public bool IsActionAllowed(Step requiredStep)
    {
        if (!IsRunning) return true; // 튜토리얼 중이 아니면 전부 허용
        return current == requiredStep;
    }
    IEnumerator SizeEffect()
    {
        float t= 0f;
        bool change = true;
        Vector3 one = new Vector3(1f,1f,1f);
        Vector3 twe = new Vector3(2f,2f,2f);
        while(first)
        {
            t += Time.fixedDeltaTime;
            if(change&&t<1f)
            {
                tutorialUiSpawn.transform.localScale = Vector3.Lerp(twe,one,t);
                tutorialUiEnd.transform.localScale = Vector3.Lerp(twe,one,t);
            }
            else if(!change&&t<1f)
            {
                tutorialUiSpawn.transform.localScale = Vector3.Lerp(one,twe,t);
                tutorialUiEnd.transform.localScale = Vector3.Lerp(one,twe,t);
            }
            if(t>1f)
            {
                change = !change;
                t =0f;
            }
            yield return null;
        }
        while(path)
        {
            t += Time.fixedDeltaTime;
            if(change&&t<1f)
            {
                tutorialUiPath.transform.localScale = Vector3.Lerp(twe,one,t);
            }
            else if(!change&&t<1f)
            {
                tutorialUiPath.transform.localScale = Vector3.Lerp(one,twe,t);
            }
            if(t>1f)
            {
                change = !change;
                t =0f;
            }
            yield return null;
        }
        while(startbutton)
        {
            t += Time.fixedDeltaTime;
            if(change&&t<1f)
            {
                tutorialUiGameStartButton.transform.localScale = Vector3.Lerp(twe,one,t);
            }
            else if(!change&&t<1f)
            {
                tutorialUiGameStartButton.transform.localScale = Vector3.Lerp(one,twe,t);
            }
            if(t>1f)
            {
                change = !change;
                t =0f;
            }
            yield return null;
        }
    }
}
