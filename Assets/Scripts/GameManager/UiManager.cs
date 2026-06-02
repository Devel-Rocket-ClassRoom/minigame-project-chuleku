using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance {get; private set;}
    private InputAction escKey;
    public GameObject escPanal;
    public GameObject storeButton;
    public RectTransform storePanal;
    public GameObject gameEnd;
    public GameObject BossBonusPanal;
    public TextMeshProUGUI gameoverText;
    private bool clickCheck;
    private Coroutine storecor;
    private Coroutine escCor;
    private Vector2 hideStore = new Vector2(-40,1000);
    private Vector2 viewStore = new Vector2(-40,-100);
    private Vector2 hideLoading = new Vector2(-2500,0);
    private bool menuLoading;
    private Coroutine menuLoadingTextCor;

    public GameObject info;
    public Image infoImage;
    public TextMeshProUGUI infoName;
    public TextMeshProUGUI infoCost;
    public TextMeshProUGUI infoMana;
    public TextMeshProUGUI infoState;
    public TextMeshProUGUI infoDesc;
    public GameObject useButton;
    public RectTransform loadingPanal;
    public TextMeshProUGUI lodingText;
    public TextMeshProUGUI tipText;
    public Slider loadingbar;
    public GameObject guardPanal;

    void Awake()
    {
        Instance = this;
        escKey = InputSystem.actions.FindAction("Player/Exit");
        if(storecor!=null) StopCoroutine(storecor);
        storecor=null;
        if(escCor != null) StopCoroutine(escCor);
        escCor = null;
        escPanal.SetActive(false);
        gameEnd.SetActive(false);
        info.SetActive(false);
        BossBonusPanal.SetActive(false);
        guardPanal.SetActive(false);
        loadingPanal.anchoredPosition = Vector2.zero;
        lodingText.text = "Loding!";
        loadingbar.value = 1f;
        tipText.text = GameSession.tipText;
        
    }
    void Start()
    {
        StartCoroutine(HideLoadingPanalCor());
        
    }

    void Update()
    {
        Pause();
    }

    void Pause()
    {
        if(escKey.WasPerformedThisFrame())
        {
            bool escCheck = !escPanal.activeSelf;
            if(escCheck)
            {
                escPanal.SetActive(true);
                escPanal.transform.localScale = new Vector3(0.1f,0.1f,0.1f);
                if(escCor != null) StopCoroutine(escCor);
                escCor = null;
                DefenceGameManager.Instance.closeButton();
                CloseInfo();
                StartGameUiHide();
                Time.timeScale = 0;
                escCor = StartCoroutine(OpenEscPanal());
            }
            else
            {
                if(escCor != null) StopCoroutine(escCor);
                escCor = null;
                escCor = StartCoroutine(CloseEscPanal());
                Time.timeScale = 1;
            }
            
        }
    }

    public void ResumeButton()
    {
        if(escCor != null) StopCoroutine(escCor);
        escCor = null;
        escCor = StartCoroutine(CloseEscPanal());
        Time.timeScale = 1;
    }
    public void RestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        ScoreManager.Instance.GameEnd();

    }

    // 메인메뉴 버튼 OnClick에 연결. 로딩패널로 화면을 덮은(hide→view) 뒤 메인메뉴 씬으로 이동.
    // 메뉴는 GameSession.ReturnedFromGame 플래그를 보고 덮은 상태로 시작 → 걷어내기 한다.
    public void GoToMainMenuButton()
    {
        Time.timeScale = 1f; // ESC 패널 등에서 0이 됐을 수 있으니 복구
        StartCoroutine(ToMainMenuCor());
    }

    IEnumerator ToMainMenuCor()
    {
        // 1) 로딩패널로 화면 덮기 (hide → view) — MenuManager.LoadInGame과 동일
        guardPanal.SetActive(true);
        loadingPanal.gameObject.SetActive(true);     // reveal 때 꺼졌으므로 다시 켠다
        loadingPanal.anchoredPosition = hideLoading; // 화면 밖(hide)에서 시작
        loadingbar.value = 0f;
        SoundManager.Play("InLoading");
        tipText.text = $"Tip!\n{DataTableManager.TipTable.GetRandom()}";
        GameSession.tipText = tipText.text;
        float t = 0;
        float speed = 15f;
        Vector2 startPos = hideLoading;
        Vector2 targetPos = Vector2.zero;            // 덮은 위치(view)
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * speed;
            loadingPanal.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        loadingPanal.anchoredPosition = targetPos;
        guardPanal.SetActive(false);

        // 2) 비동기 로드 + 진행바 채우기 (최소 2초 보장)
        menuLoading = true;
        menuLoadingTextCor = StartCoroutine(MenuLoadTextCor());

        var op = SceneManager.LoadSceneAsync("MainMenu");
        op.allowSceneActivation = false;

        float minLoadTime = 2f;
        float loadStartTime = Time.unscaledTime;
        while (true)
        {
            float elapsed = Time.unscaledTime - loadStartTime;
            float timeT = elapsed / minLoadTime;
            float progT = op.progress / 0.9f;
            loadingbar.value = Mathf.Clamp01(Mathf.Min(timeT, progT));

            if (timeT >= 1f && op.progress >= 0.9f) break;
            yield return null;
        }
        loadingbar.value = 1f;

        // 3) 씬 전환 직전에 메뉴가 덮은 상태로 시작하도록 신호 + 정리
        menuLoading = false;
        if (menuLoadingTextCor != null) StopCoroutine(menuLoadingTextCor);
        menuLoadingTextCor = null;
        GameSession.ReturnedFromGame = true;
        ScoreManager.Instance.GameEnd();
        op.allowSceneActivation = true;
    }

    // 로딩 중 "Loading." → ".." → "..." 텍스트 애니메이션
    IEnumerator MenuLoadTextCor()
    {
        while (menuLoading)
        {
            lodingText.text = "Loading.";
            yield return new WaitForSecondsRealtime(0.25f);
            lodingText.text = "Loading..";
            yield return new WaitForSecondsRealtime(0.25f);
            lodingText.text = "Loading...";
            yield return new WaitForSecondsRealtime(0.25f);
        }
    }
    public void ExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void GameEnd()
    {
        Time.timeScale =0;
        guardPanal.SetActive(true);
        gameEnd.SetActive(true);
        ScoreManager.Instance.EndScore();
        DefenceGameManager.Instance.diecheck = true;
    }

    public void OnclickStore()
    {
        if(storecor != null) StopCoroutine(storecor);
        storecor = null;
        if(DefenceGameManager.Instance.Round)return;
        StoreManager.Instance.SetMoveHide();
        clickCheck = !clickCheck;
        storecor = StartCoroutine(MoveStore());
    }
    private IEnumerator MoveStore()
    {
        float t = 0;
        float speed = 30f;
        Vector2 startPos = storePanal.anchoredPosition;
        Vector2 targetPos = clickCheck ? viewStore : hideStore;
        string s = clickCheck ? "OpenSetting" : "CloseSetting";
        SoundManager.Play(s);
   
        while (t<1f)
        {
            t += Time.deltaTime*speed;
            storePanal.anchoredPosition = Vector2.Lerp(startPos,targetPos,t);
            yield return null;
        }
        storePanal.anchoredPosition = targetPos;
        storecor = null;
    }
    public void StartGameUiHide()
    {
        if(storecor !=null) StopCoroutine(storecor);
        storecor = null;
        StoreManager.Instance.SetMoveHide();
        DefenceGameManager.Instance.closeButton();
        storePanal.anchoredPosition = hideStore;
        clickCheck = false;
    }
    public void HideStoreButton()
    {
        storeButton.SetActive(false);
    }
    public void ViewStoreButton()
    {
        storeButton.SetActive(true);
    }

    public void ViewInfo(string cardId)
    {
        var card = DataTableManager.CardTable.Get(cardId);
        if (card == null) return;
        info.SetActive(true);
        useButton.SetActive(false);

        infoImage.sprite = LoadSprite(card.Image);
        infoName.text = DataTableManager.StringTable?.Get(card.Name);
        infoCost.text = $"가격 : {card.Cost}";
        infoMana.text = card.Mana.ToString();
        infoDesc.text = DataTableManager.StringTable?.Get(card.Desc);
        switch(card.Type)
        {
            case CardType.Unit:
            var unitcard = DataTableManager.UnitTable.Get(cardId);
            infoState.text = $"공격력 : {unitcard.Attack}\n공격속도 : {unitcard.AttackSpeed}\n사거리 : {unitcard.Range}";
            break;
            case CardType.Effect:
            infoState.text = $"";
            break;
            case CardType.Resource:
            infoState.text = $"";
            break;
            case CardType.Magic:
            infoState.text = "";
            break;
        }

    }

    // 마법탭 슬롯 클릭 시 호출. 메인 페이즈 마법만 사용 버튼을 띄우고,
    // 배틀 페이즈 마법은 드래그로 발동하므로 버튼은 숨긴다.
    public void ViewMagicInfo(MagicBase magic)
    {
        if (magic == null) return;
        var data = DataTableManager.MagicTable?.Get(magic.MagicId);
        if (data == null) return;

        info.SetActive(true);

        infoImage.sprite = LoadSprite(data.Image);
        infoName.text = DataTableManager.StringTable?.Get(data.Name);
        infoDesc.text = DataTableManager.StringTable?.Get(data.Desc);
        infoCost.text = "";
        infoMana.text = "";
        infoState.text = data.Phase == Phase.Main ? "메인 페이즈 전용" : "배틀 페이즈 전용";
        if (data.Phase == Phase.Main)
        {
            useButton.SetActive(true);
            var btn = useButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    magic.TryActivate();
                    info.SetActive(false);
                    useButton.SetActive(false);
                });
            }
        }
        else
        {
            useButton.SetActive(false);
        }
    }
    public void CloseInfo()
    {
        info.SetActive(false);
    }
    protected static Sprite LoadSprite(string imageId)
    {
        if (string.IsNullOrEmpty(imageId)) return null;
        var db = DataManager.Instance != null ? DataManager.Instance.SpriteDB : null;
        if (db == null)
        {
            Debug.LogWarning("DataManager 또는 SpriteDB가 씬에 없음");
            return null;
        }
        var sp = db.Get(imageId);
        if (sp == null) Debug.LogWarning($"SpriteDatabase에 '{imageId}' 키 없음");
        return sp;
    }

    public void KillBoss()
    {
        BossBonusPanal.SetActive(true);
    } 
    public void AddRandomCard()
    {
        var c = DataTableManager.CardTable.GetAllIds().ToList();
        int i = Random.Range(0,c.Count);
        CardGameManager.Instance.AddDeckCard(c[i]);
        BossBonusPanal.SetActive(false);
    }
    public void AddReroll()
    {
        StoreManager.Instance.AddRerollCount(3);
        BossBonusPanal.SetActive(false);
    }
    public void AddUpgrade()
    {
        UpgradeManager.Instance.AddAttackBonus(1,Scope.Permanent);
        BossBonusPanal.SetActive(false);
    }
    public void AddDrawCard()
    {
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
        CardGameManager.Instance.DrawCard();
        BossBonusPanal.SetActive(false);
    }
    IEnumerator HideLoadingPanalCor()
    {
        yield return new WaitForSeconds(1f);
        float t = 0;
        float speed = 15f;
        Vector2 startPos = loadingPanal.anchoredPosition;
        Vector2 targetPos = hideLoading;
        SoundManager.Play("OutLoading");
        while (t<1f)
        {
            t += Time.deltaTime*speed;
            loadingPanal.anchoredPosition = Vector2.Lerp(startPos,targetPos,t);
            yield return null;
        }
        loadingPanal.anchoredPosition = targetPos;
        loadingPanal.gameObject.SetActive(false);
    }
    IEnumerator OpenEscPanal()
    {
        guardPanal.SetActive(true);
        float t =0;
        float speed = 15f;
        Vector3 start = new Vector3(0.1f,0.1f,0.1f);
        Vector3 end = Vector3.one;
        while(t<1f)
        {
            t +=Time.fixedDeltaTime*speed;
            escPanal.transform.localScale = Vector3.Lerp(start,end,t);
            yield return null;
        }
        escPanal.transform.localScale = end;
        escCor =null;
    }
    IEnumerator CloseEscPanal()
    {
        float t =0;
        float speed = 15f;
        Vector3 start = escPanal.transform.localScale;
        Vector3 end = new Vector3(0.1f,0.1f,0.1f);
        while(t<1f)
        {
            t +=Time.fixedDeltaTime*speed;
            escPanal.transform.localScale = Vector3.Lerp(start,end,t);
            yield return null;
        }
        escPanal.transform.localScale = end;
        escCor =null;
        guardPanal.SetActive(false);
        escPanal.SetActive(false);
    }
}
