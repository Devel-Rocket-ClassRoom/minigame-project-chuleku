using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        BuildWall,   // 벽 설치 유도
        PreviewPath, // 경로 미리보기 버튼 유도
        PlaceUnit,   // 유닛 패널에서 벽 위로 배치 유도
        StartBattle, // 게임 시작 버튼 유도
        Battle,      // 전투 관전 (라운드 종료 대기)
        UseCard,     // 자원/효과 카드 사용 유도
        Shop,        // 상점 안내
        Done         // 종료
    }

    [Header("가이드 UI (인스펙터에서 연결, 없으면 null 허용)")]
    [SerializeField] private GameObject guidePanel;     // 단계 설명 말풍선 패널
    [SerializeField] private TextMeshProUGUI guideText; // 설명 텍스트
    [SerializeField] private GameObject nextButton;     // 수동 진행용 "다음" 버튼
    [SerializeField] private GameObject skipButton;     // 튜토리얼 건너뛰기

    private Step current = Step.None;
    public Step Current => current;
    public bool IsRunning => current != Step.None && current != Step.Done;

    private const string TutorialDoneKey = "tutorial_done";

    void Awake()
    {
        Instance = this;
        tutorialPanal.SetActive(true);

        if (guidePanel != null) guidePanel.SetActive(false);
        if(PlayerPrefs.GetInt(TutorialDoneKey)==1)
        {
            OnClickNo();
        }
    }

    public void OnClickNo()
    {
        tutorialPanal.SetActive(false);
        CardGameManager.Instance.StartGame();
        DefenceGameManager.Instance.StartGame();
        ResourceManager.Instance.StartGame();
        StoreManager.Instance.StartGame();
        ResourceManager.Instance.AddGold(50);
        ResourceManager.Instance.AddMana(20);
    }

    public void OnClickYes()
    {
        tutorialPanal.SetActive(false);
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
        PlayerPrefs.SetInt(TutorialDoneKey, 1);
    }

    public void OnClickSkip() => EndTutorial();

    // 다음 단계로 넘긴다(수동/자동 공통). 단계 데이터 표시는 ShowStep에서.
    private void GoToStep(Step step)
    {
        current = step;
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

        bool manualNext = false; // 플레이어가 "다음"을 눌러야 넘어가는 단계인지
        string msg = "";

        switch (step)
        {
            case Step.Intro:
                msg = "적은 시작점에서 끝점까지 이동합니다.";
                manualNext = true;
                break;
            case Step.BuildWall:
                msg = "빈 타일을 눌러 벽을 세워 길을 막아보세요.";
                break;
            case Step.PreviewPath:
                msg = "경로 미리보기로 적이 갈 길을 확인하세요.";
                break;
            case Step.PlaceUnit:
                msg = "벽을 누르고 유닛 패널에서 유닛을 벽 위에 배치하세요.";
                break;
            case Step.StartBattle:
                msg = "준비가 끝났으면 게임 시작을 누르세요.";
                break;
            case Step.Battle:
                msg = "유닛이 자동으로 적을 공격합니다. 라운드가 끝날 때까지 지켜보세요.";
                break;
            case Step.UseCard:
                msg = "손패의 자원 카드를 사용해 골드를 얻어보세요.";
                break;
            case Step.Shop:
                msg = "상점에서 골드와 마나로 카드를 살 수 있습니다.";
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
        if (current == Step.BuildWall) Advance();
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

    // 자원/효과 카드 사용 성공 후
    public void NotifyCardUsed()
    {
        if (current == Step.UseCard) Advance();
    }

    // ───────── (선택) 행동 게이팅용 헬퍼 ─────────
    // 특정 단계에서 다른 동작을 막고 싶으면 DefenceGameManager 쪽에서 이걸 참조해 제한.
    // TODO: 필요해지면 단계별 허용 동작 표를 채운다.
    public bool IsActionAllowed(Step requiredStep)
    {
        if (!IsRunning) return true; // 튜토리얼 중이 아니면 전부 허용
        return current == requiredStep;
    }
}
