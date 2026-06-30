using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using System;
using Firebase;
using Firebase.Auth;
public class LoginManager : MonoBehaviour
{
    [Header("[ Input Fields ]")]
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public FirebaseConfig firebaseConfig;

    [Header("[ Buttons ]")]
    public Button loginButton;
    public Button signUpButton;
    public Button guestButton;

    [Header("[ Status Text ]")]
    public TextMeshProUGUI noticeText;

    void Start()
    {
        loginButton.onClick.AddListener(() => OnClickLogin().Forget());
        signUpButton.onClick.AddListener(() => OnClickSignUp().Forget());
        guestButton.onClick.AddListener(() => OnClickGuest().Forget());

        if (noticeText != null) noticeText.text = "";
        InitFirebase().Forget();
    }
    private void SetButtonsInteractable(bool interactable)
    {
        loginButton.interactable = interactable;
        signUpButton.interactable = interactable;
        guestButton.interactable = interactable;
    }
    private async UniTaskVoid InitFirebase()
    {
        SetNotice("서버 연결 확인 중...");
        try
        {
            DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependencyStatus == DependencyStatus.Available)
            {
                FirebaseApp app;
                if (firebaseConfig != null && firebaseConfig.IsValid)
                {
                    Debug.Log("[Login] FirebaseConfig 에셋을 사용하여 수동 초기화를 시도합니다.");
                    AppOptions options = new AppOptions
                    {
                        ApiKey = firebaseConfig.apiKey,
                        AppId = firebaseConfig.appId,
                        ProjectId = firebaseConfig.projectId,
                        DatabaseUrl = new System.Uri(firebaseConfig.databaseUrl),
                        StorageBucket = firebaseConfig.storageBucket
                    };
                    app = FirebaseApp.Create(options);
                    }
                    else
                    {
                        Debug.Log("[Login] 기본 json 파일 설정을 사용하여 자동 초기화를 시도합니다.");
                        app = FirebaseApp.DefaultInstance;
                    }
    
                    FirebaseAuth auth = FirebaseAuth.GetAuth(app);
                    
                    if (AuthManager.Instance != null)
                    {
                        AuthManager.Instance.Initialize(auth);
                        SetNotice("서버 연결 성공!");
                        SetButtonsInteractable(true);

                        if (AuthManager.Instance.IsLoggedIn)
                        {
                            SetNotice("기존 로그인 세션을 확인했습니다. 이동 중...");
                            await UniTask.Delay(1000);
                            OnLoginSuccess();
                        }
                    }
                }
                else
                {
                    SetNotice($"서버 연결 실패: {dependencyStatus}");
                }
            }
        catch (Exception ex)
        {
            SetNotice($"초기화 중 오류 발생: {ex.Message}");
        }
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }


    private async UniTaskVoid OnClickSignUp()
    {
        try
        {
            string email = emailInputField.text.Trim();
            string password = passwordInputField.text;


            if (!IsValidEmail(email))
            {
                SetNotice("올바른 이메일 형식이 아닙니다. (예: user@email.com)");
                return;
            }
            if (password.Length < 6)
            {
                SetNotice("비밀번호는 최소 6자리 이상이어야 합니다.");
                return;
            }

            SetNotice("회원가입 요청 중...");
            var (success, error) = await AuthManager.Instance.SignUp(email, password);

            SetNotice("회원가입 및 로그인 성공!");
           
            OnLoginSuccess();
        }
        catch(Exception ex)
        {
            SetNotice($"회원가입 실패: {ex.Message}");
        }
    }

    private async UniTaskVoid OnClickLogin()
    {
        try
        {
            string email = emailInputField.text.Trim();
            string password = passwordInputField.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                SetNotice("이메일과 비밀번호를 모두 입력해주세요.");
                return;
            }
            SetNotice("로그인 시도 중...");
            var (success, error) = await AuthManager.Instance.SignIn(email, password);
            SetNotice("로그인 성공!");
            OnLoginSuccess();
        }
        catch (Exception ex)
        {
            SetNotice($"로그인 실패: {ex.Message}");
        }


    }

    private async UniTaskVoid OnClickGuest()
    {
        SetNotice("게스트 로그인 시도 중...");
        var (success, error) = await AuthManager.Instance.SignIn();

        if (success)
        {
            SetNotice("게스트 로그인 성공!");
            OnLoginSuccess();
        }
        else
        {
            SetNotice($"게스트 로그인 실패: {error}");
        }
    }
    private void OnLoginSuccess()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.InitGameScore();
        }

        SceneManager.LoadScene("MainMenu");
    }

    private void SetNotice(string message)
    {
        if (noticeText != null)
        {
            noticeText.text = message;
        }
        Debug.Log($"[AuthUI] {message}");
    }
}
