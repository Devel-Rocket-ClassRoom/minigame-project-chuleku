using System;
using System.Collections;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public RectTransform difficultyPanal;
    public RectTransform settingPanal;
    public GameObject guardPanal;
    public RectTransform loadingPanal;
    private Vector3 viewPanal = Vector3.zero;
    private Vector3 hidePanal = new Vector3(0,1000,0);
    private Vector3 hideLoadingPanal = new Vector3(-2500,0,0);
    private Vector3 hideSettingPanal = new Vector3(2500,0,0);
    private bool clickCheck;
    public Slider loadingBar;
    private Coroutine cor;
    private Coroutine loadcor;
    private Coroutine loadingTextCor;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI tipText;
    private bool Loading;
    private Difficulty difficulty;
    public Button logoutButton;
    public Button profileButton;
    public GameObject profilePanal;
    private bool clickProfile;
    void Awake()
    {
        Time.timeScale = 1f; // 인게임 일시정지/게임오버(timeScale=0) 상태로 나와도 메뉴는 항상 정상 진행되게 복구
        clickCheck = false;
        if(cor!=null)StopCoroutine(cor);
        cor = null;
        if(loadcor !=null)StopCoroutine(loadcor);
        loadcor=null;
        if(loadingTextCor !=null)StopCoroutine(loadingTextCor);
        loadingTextCor=null;
        guardPanal.SetActive(false);
        // 게임에서 돌아온 경우엔 덮은 상태(viewPanal)로 시작 → Start에서 걷어낸다.
        // 첫 실행(부팅)이면 안 덮인 상태(hideLoadingPanal)로 바로 보여준다.
        loadingPanal.anchoredPosition = GameSession.ReturnedFromGame ? viewPanal : hideLoadingPanal;
        settingPanal.anchoredPosition = hideSettingPanal;
        difficultyPanal.anchoredPosition = hidePanal;
        Loading = false;
        loadingBar.value = 1;
        tipText.text = "Tip!";
        profilePanal.SetActive(false);
        clickProfile = false;
        tipText.text = GameSession.tipText;
    }
    void Start()
    {
        SoundManager.PlayBgm("MainMenuBGM");
        // 게임에서 돌아온 경우에만 로딩패널을 걷어내는 연출 실행 (InGame의 reveal과 대칭).
        if (GameSession.ReturnedFromGame)
        {
            GameSession.ReturnedFromGame = false; // 1회성: 다음 첫 진입에선 안 덮이게
            if (loadcor != null) StopCoroutine(loadcor);
            loadcor = StartCoroutine(RevealLoadingPanal());
        }
        logoutButton.onClick.AddListener(()=>SignOut().Forget());
        profileButton.onClick.AddListener(OnclickProfile);
    }

    // 덮인 로딩패널을 옆으로 밀어 걷어낸다. (InGame UiManager.HideLoadingPanalCor와 동일한 연출)
    IEnumerator RevealLoadingPanal()
    {
        loadingPanal.anchoredPosition = viewPanal; // 덮은 상태 보장
        float t = 0;
        float speed = 15f;
        Vector2 startPos = loadingPanal.anchoredPosition;
        Vector2 targetPos = hideLoadingPanal;
        loadingText.text = "Loading!";
        yield return new WaitForSecondsRealtime(1f); // timeScale=0 상태로 진입해도 멈추지 않게 언스케일드
        SoundManager.Play("OutLoading");
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * speed; // timeScale=0 중에도 패널이 정상적으로 걷히도록
            loadingPanal.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        loadingPanal.anchoredPosition = targetPos;
        loadcor = null;
    }

    public void OnEasy()
    {
        difficulty = Difficulty.Easy;
    }
    public void OnNormal()
    {
        difficulty = Difficulty.Normal;
    }
    public void OnHard()
    {
        difficulty = Difficulty.Hard;
    }

    public void OnStartClick()
    {
        GameSession.SelectedDifficulty = difficulty;
        if(loadcor!=null) StopCoroutine(loadcor);
        loadcor = StartCoroutine(LoadInGame());
        SoundManager.StopBgm();
    }
    public void OnClickQuit()
    {
        if(cor !=null)StopCoroutine(cor);
        cor = null;
        clickCheck = false;
        cor = StartCoroutine(MoveSelectPanal());
        SoundManager.Play("CloseSetting");
    }

    public void OnSelectDifficulty()
    {
        if(cor !=null)StopCoroutine(cor);
        cor = null;
        clickCheck = true;
        cor = StartCoroutine(MoveSelectPanal());
        SoundManager.Play("OpenSetting");
    }

    public void OnClickSetting()
    {
        if(cor!=null)StopCoroutine(cor);
        cor = null;
        clickCheck = true;
        cor = StartCoroutine(MoveSettingPanalCor());
        SoundManager.Play("OpenSetting");
    }
    public void ExitSetting()
    {
        if(cor!=null)StopCoroutine(cor);
        cor = null;
        clickCheck = false;
        cor = StartCoroutine(MoveSettingPanalCor());
        SoundManager.Play("CloseSetting");
    }
    public void OnExit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public IEnumerator MoveSelectPanal()
    {
        guardPanal.SetActive(true);
        float t = 0;
        float speed = 30f;
        Vector2 startPos = difficultyPanal.anchoredPosition;
        Vector2 targetPos = clickCheck ? viewPanal : hidePanal;
   
        while (t<1f)
        {
            t += Time.deltaTime*speed;
            difficultyPanal.anchoredPosition = Vector2.Lerp(startPos,targetPos,t);
            yield return null;
        }
        difficultyPanal.anchoredPosition = targetPos;
        cor = null;
        guardPanal.SetActive(false);
    }
    IEnumerator MoveSettingPanalCor()
    {
        guardPanal.SetActive(true);
        float t = 0;
        float speed = 15f;
        Vector2 startPos = settingPanal.anchoredPosition;
        Vector2 targetPos = clickCheck ? viewPanal : hideSettingPanal;

        while(t<1f)
        {
            t += Time.deltaTime*speed;
            settingPanal.anchoredPosition = Vector2.Lerp(startPos,targetPos,t);
            yield return null;
        }
        settingPanal.anchoredPosition = targetPos;
        cor = null;
        guardPanal.SetActive(false);
    }
    IEnumerator LoadInGame()
    {
        guardPanal.SetActive(true);
        float t =0;
        float speed = 15f;
        loadingBar.value = 0;
        Vector2 startPos = loadingPanal.anchoredPosition;
        Vector2 targetPos = viewPanal;
        SoundManager.Play("InLoading");
        tipText.text = $"Tip!\n{DataTableManager.TipTable.GetRandom()}";
        GameSession.tipText = tipText.text;
        while(t<1f)
        {
            t+=Time.deltaTime*speed;
            loadingPanal.anchoredPosition = Vector2.Lerp(startPos,targetPos,t);
            yield return null;
        }
        loadingPanal.anchoredPosition = targetPos;
        guardPanal.SetActive(false);
        loadcor = null;
        Loading = true;
        loadingTextCor = StartCoroutine(LoadTextCor());
        var op = SceneManager.LoadSceneAsync("InGame");
        op.allowSceneActivation = false;

        float minLoadTime = 2f;
        float loadStartTime = Time.unscaledTime;

        while (true)
        {
            float elapsed = Time.unscaledTime - loadStartTime;
            float timeT = elapsed / minLoadTime;
            float progT = op.progress / 0.9f;
            loadingBar.value = Mathf.Clamp01(Mathf.Min(timeT, progT));

            if (timeT >= 1f && op.progress >= 0.9f) break;
            yield return null;
        }
        loadingBar.value = 1f;
        op.allowSceneActivation = true;
        Loading=false;
        if(loadingTextCor!=null) StopCoroutine(loadingTextCor);
        loadingTextCor = null;
    }
    IEnumerator LoadTextCor()
    {
        while(Loading)
        {
            loadingText.text = "Loading.";
            yield return new WaitForSeconds(0.25f);
            loadingText.text = "Loading..";
            yield return new WaitForSeconds(0.25f);
            loadingText.text = "Loading...";
            yield return new WaitForSeconds(0.25f);
        }
    }
    private async UniTaskVoid SignOut()
    {
        try
        {
            var(succese,error) = await AuthManager.Instance.SignOut();
            SceneManager.LoadScene("LoginScene");
        }
        catch(Exception ex)
        {
            Debug.Log($"로그아웃 실패 {ex.Message}");
        }
    }
    private void OnclickProfile()
    {
        clickProfile = !clickProfile;
        profilePanal.SetActive(clickProfile);
    }
}
