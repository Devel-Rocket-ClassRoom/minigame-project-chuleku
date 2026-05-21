using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    private InputAction escKey;
    public GameObject escPanal;
    void Awake()
    {
        escKey = InputSystem.actions.FindAction("Player/Exit");
        escPanal.SetActive(false);
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
}
