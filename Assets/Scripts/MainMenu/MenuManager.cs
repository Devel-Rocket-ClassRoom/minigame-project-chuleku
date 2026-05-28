using System.Collections;
using System.Runtime.InteropServices;
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
    private bool Loading;

    private Difficulty difficulty;
    void Awake()
    {
        clickCheck = false;
        if(cor!=null)StopCoroutine(cor);
        cor = null;
        if(loadcor !=null)StopCoroutine(loadcor);
        loadcor=null;
        if(loadingTextCor !=null)StopCoroutine(loadingTextCor);
        loadingTextCor=null;
        guardPanal.SetActive(false);
        loadingPanal.anchoredPosition = hideLoadingPanal;
        settingPanal.anchoredPosition = hideSettingPanal;
        difficultyPanal.anchoredPosition = hidePanal;
        Loading = false;
        loadingBar.value = 0;
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
        Vector2 startPos = loadingPanal.anchoredPosition;
        Vector2 targetPos = viewPanal;
        SoundManager.Play("InLoading");
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
}
