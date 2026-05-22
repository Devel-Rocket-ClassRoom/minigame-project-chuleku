using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance {get; private set;}
    private InputAction escKey;
    public GameObject escPanal;
    public GameObject storeButton;
    public RectTransform storePanal;
    public GameObject gameEnd;
    public TextMeshProUGUI gameoverText;
    private bool clickCheck;
    private Coroutine storecor;
    private Vector2 hideStore = new Vector2(-40,1000);
    private Vector2 viewStore = new Vector2(-40,-100);
    void Awake()
    {
        Instance = this;
        escKey = InputSystem.actions.FindAction("Player/Exit");
        if(storecor!=null) StopCoroutine(storecor);
        storecor=null;
        escPanal.SetActive(false);
        gameEnd.SetActive(false);
        
    }

    void Update()
    {
        Pause();
    }

    void Pause()
    {
        if(escKey.WasPerformedThisFrame())
        {
            escPanal.SetActive(true);
            DefenceGameManager.Instance.closeButton();
            Time.timeScale = 0;
        }
    }

    public void ResumeButton()
    {
        escPanal.SetActive(false);
        
        Time.timeScale = 1;
    }
    public void RestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        gameEnd.SetActive(true);
    }

    public void OnclickStore()
    {
        if(storecor != null) StopCoroutine(storecor);
        storecor = null;
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
   
        while (t<1f)
        {
            t += Time.deltaTime*speed;
            storePanal.anchoredPosition = Vector2.Lerp(startPos,targetPos,t);
            yield return null;
        }
        storePanal.anchoredPosition = targetPos;
        storecor = null;
    }
    
}
