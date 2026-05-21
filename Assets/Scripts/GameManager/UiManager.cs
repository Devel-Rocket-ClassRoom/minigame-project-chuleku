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
    public GameObject gameEnd;
    public TextMeshProUGUI gameoverText;
    void Awake()
    {
        Instance = this;
        escKey = InputSystem.actions.FindAction("Player/Exit");
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


}
