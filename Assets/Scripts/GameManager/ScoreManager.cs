using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public static ScoreManager Instance=>instance;
    public TextMeshProUGUI recordScoreText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameEndscoreText;
    public TextMeshProUGUI newRecordText;
    private DatabaseReference m_DatabaseRef;
    [SerializeField] private int recordScore;
    private int currentScore;
    public bool gameEnd;

    private const string PrefRecordScore = "Record_Score";

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        InitGameScore();
    }
    // public void StartGame()
    // {
    //     scoreText.gameObject.SetActive(true);
    // }

    public void SetScore(int score)
    {
        currentScore+=score;
        scoreText.text = $"현재 점수 : {currentScore}";
    }
    public async void InitGameScore()
    {
        currentScore = 0;
        scoreText.text = $"현재 점수 : {currentScore}";
        newRecordText.gameObject.SetActive(false);
        gameEnd = false;
        scoreText.gameObject.SetActive(true);
        recordScore = PlayerPrefs.GetInt(PrefRecordScore,0);
        // 2. 만약 로그인이 되어 있다면 Firebase 서버에서 최신 최고 점수를 동기화
        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            Debug.Log("[Score] Firebase 서버에서 최고 점수 동기화 시도...");
            int serverScore = await LoadUserScore();
            
            // 서버 점수가 더 높거나 로컬과 다르면 서버 기준으로 갱신
            if (serverScore > recordScore)
            {
                recordScore = serverScore;
                PlayerPrefs.SetInt(PrefRecordScore, recordScore);
                PlayerPrefs.Save();
            }
        }
    }
    public async UniTask<bool> SaveUserScore(int score)
    {
        // 1. 먼저 인증된 유저인지 체크
        if (!AuthManager.Instance.IsLoggedIn)
        {
            Debug.LogError("[Database] 로그인 정보가 없어 점수를 저장할 수 없습니다.");
            return false;
        }

        string uid = AuthManager.Instance.CurrentUserUid;

        try
        {
            Debug.Log($"[Database] 점수 저장 시도 (UID: {uid}, Score: {score})");

            // 데이터베이스 구조: users / {uid} / score = score_value
            // SetValueAsync를 통해 해당 경로에 값을 덮어씁니다.
            await m_DatabaseRef.Child("users").Child(uid).Child("score").SetValueAsync(score);

            Debug.Log("[Database] 점수 저장 성공!");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Database] 점수 저장 실패: {ex.Message}");
            return false;
        }
    }
    public async UniTask<int> LoadUserScore()
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            Debug.LogError("[Database] 로그인 정보가 없어 점수를 불러올 수 없습니다.");
            return 0;
        }

        string uid = AuthManager.Instance.CurrentUserUid;

        try
        {
            // 해당 경로의 데이터를 한 번만 가져옵니다 (GetValueAsync)
            DataSnapshot snapshot = await m_DatabaseRef.Child("users").Child(uid).Child("score").GetValueAsync();

            if (snapshot.Exists && snapshot.Value != null)
            {
                int score = Convert.ToInt32(snapshot.Value);
                Debug.Log($"[Database] 점수 불러오기 성공: {score}");
                return score;
            }
            else
            {
                Debug.Log("[Database] 저장된 점수 데이터가 없습니다. 0점으로 시작합니다.");
                return 0;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Database] 점수 불러오기 실패: {ex.Message}");
            return 0;
        }
    }
    public async UniTaskVoid EndScore()
    {
        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            int currentServerRecord = await LoadUserScore();
            if (currentServerRecord > recordScore)
            {
                recordScore = currentServerRecord;
            }
        }
        recordScoreText.text = recordScore.ToString();
        bool isNewRecord = currentScore > recordScore;
        if(isNewRecord)
        {
            recordScore = currentScore;
            PlayerPrefs.SetInt(PrefRecordScore,recordScore);
            PlayerPrefs.Save();
            if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
            {
                bool saveSuccess = await SaveUserScore(recordScore);
                if (saveSuccess)
                {
                    Debug.Log("[Score] 서버에 최고 기록 저장 완료!");
                }
                else
                {
                    Debug.LogWarning("[Score] 서버 최고 기록 저장 실패 (네트워크 확인 필요)");
                }
            }
        }
        StartCoroutine(ScoreCor(isNewRecord));
    }

    IEnumerator ScoreCor(bool isNewRecord)
    {
        float t = 0;
        float speed = 0.5f; // 2초 동안 0 → 1
        gameEndscoreText.text = "0";
        while(t<1f)
        {
            t += Time.unscaledDeltaTime*speed; // 결과/일시정지(timeScale=0) 중에도 카운트업 동작 + 프레임레이트 독립
            gameEndscoreText.text = Mathf.RoundToInt(Mathf.Lerp(0,currentScore,t)).ToString();
            yield return null;
        }
        gameEndscoreText.text = currentScore.ToString();

        gameEnd = true;

        if(isNewRecord)
        {
            newRecordText.gameObject.SetActive(true);
            SoundManager.Play("NewRecord");
            StartCoroutine(randomColor());
        }
    }
    IEnumerator randomColor()
    {
        while(gameEnd)
        {
            if(Time.frameCount %5==0)
            {
                newRecordText.color = UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.7f, 1f);
            }
            yield return null;
        }
    }
    public void GameEnd()
    {
        gameEnd = false;
    }
}
