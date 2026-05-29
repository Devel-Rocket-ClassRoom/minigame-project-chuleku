using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance {get;private set;}
    public TextMeshProUGUI recordScoreText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameEndscoreText;
    public TextMeshProUGUI newRecordText;

    [SerializeField] private int recordScore;
    private int currentScore;
    public bool gameEnd;

    private const string PrefRecordScore = "Record_Score";

    void Awake()
    {
        Instance = this;
        currentScore = 0;
        recordScore = PlayerPrefs.GetInt(PrefRecordScore,0);
        scoreText.text = $"현재 점수 : {currentScore}";
        newRecordText.gameObject.SetActive(false);
        gameEnd = false;
    }

    public void SetScore(int score)
    {
        currentScore+=score;
        scoreText.text = $"현재 점수 : {currentScore}";
    }

    public void EndScore()
    {
        recordScoreText.text = recordScore.ToString();

        bool isNewRecord = currentScore > recordScore;
        if(isNewRecord)
        {
            recordScore = currentScore;
            PlayerPrefs.SetInt(PrefRecordScore,recordScore);
            PlayerPrefs.Save();
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
            t += Time.fixedDeltaTime*speed;
            gameEndscoreText.text = Mathf.RoundToInt(Mathf.Lerp(0,currentScore,t)).ToString();
            yield return null;
        }
        gameEndscoreText.text = currentScore.ToString();

        gameEnd = true;

        if(isNewRecord)
        {
            newRecordText.gameObject.SetActive(true);
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
