using System.Collections;
using UnityEngine;
using TMPro;

// 화면 중앙에 안내/실패 메시지를 띄우는 토스트.
// 글자가 하나씩 안개처럼 떠오르고(글자별 알파 페이드인), 잠시 후 전체가 스르륵 사라진다(페이드아웃).
// 어디서든  CenterToast.Show("메시지")  로 호출.
//
// 세팅: 씬의 아무 GameObject에 이 컴포넌트만 붙이면 Canvas+TMP를 자동 생성한다.
//   스타일(폰트/크기/외곽선)을 직접 잡고 싶으면 text 에 TMP를 연결하면 그걸 쓴다.
[DisallowMultipleComponent]
public class CenterToast : MonoBehaviour
{
    public static CenterToast Instance { get; private set; }

    [Header("연결 (비우면 자동 생성)")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("타이밍")]
    [SerializeField] private float charDelay = 0.05f;        // 글자가 하나씩 등장하는 간격
    [SerializeField] private float charFadeDuration = 0.25f; // 글자 하나가 떠오르는 시간
    [SerializeField] private float holdDuration = 1.0f;      // 다 뜬 뒤 유지 시간
    [SerializeField] private float fadeOutDuration = 0.6f;   // 스르륵 사라지는 시간
    [SerializeField] private float riseDistance = 25f;       // 사라질 때 살짝 떠오르는 거리(px)

    [Header("모양 (자동 생성 시에만 사용)")]
    [SerializeField] private float fontSize = 48f;
    [SerializeField] private Color color = Color.white;
    [SerializeField] private int sortingOrder = 200;

    private Coroutine running;

    void Awake()
    {
        Instance = this;
        if (text == null) AutoCreate();
        if (canvasGroup == null && text != null)
            canvasGroup = text.GetComponentInParent<CanvasGroup>() ?? text.gameObject.AddComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (text != null) text.text = "";
    }

    // 어디서든 호출하는 진입점. 인스턴스 없으면 조용히 로그만.
    public static void Show(string message)
    {
        if (Instance == null) { Debug.LogWarning($"[CenterToast 미배치] {message}"); return; }
        Instance.ShowInternal(message);
    }

    public void ShowInternal(string message)
    {
        if (text == null || string.IsNullOrEmpty(message)) return;
        if (running != null) StopCoroutine(running); // 진행 중이면 새 메시지로 교체
        running = StartCoroutine(Play(message));
    }

    private IEnumerator Play(string message)
    {
        canvasGroup.alpha = 1f;
        Vector2 basePos = text.rectTransform.anchoredPosition;

        text.text = message;
        text.ForceMeshUpdate();
        int charCount = text.textInfo.characterCount;

        if (charCount == 0) // 공백뿐인 메시지 등
        {
            yield return WaitUnscaled(holdDuration);
            canvasGroup.alpha = 0f;
            text.text = "";
            running = null;
            yield break;
        }

        SetAllAlpha(0); // 전부 투명하게 시작

        // ── 글자 하나씩 떠오르기 (스태거된 알파 페이드인) ──
        float t = 0f;
        bool done = false;
        while (!done)
        {
            t += Time.unscaledDeltaTime;
            done = true;
            for (int i = 0; i < charCount; i++)
            {
                float ct = t - i * charDelay;
                float a = charFadeDuration > 0f ? Mathf.Clamp01(ct / charFadeDuration) : (ct >= 0f ? 1f : 0f);
                if (a < 1f) done = false;
                SetCharAlpha(i, (byte)(a * 255f));
            }
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            yield return null;
        }

        // ── 유지 ──
        yield return WaitUnscaled(holdDuration);

        // ── 전체가 스르륵 사라지기 (페이드아웃 + 살짝 상승) ──
        float f = 0f;
        while (f < fadeOutDuration)
        {
            f += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(f / fadeOutDuration);
            canvasGroup.alpha = 1f - k;
            text.rectTransform.anchoredPosition = basePos + Vector2.up * (riseDistance * k);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        text.rectTransform.anchoredPosition = basePos; // 원위치 복구
        text.text = "";
        running = null;
    }

    private IEnumerator WaitUnscaled(float sec)
    {
        float e = 0f;
        while (e < sec) { e += Time.unscaledDeltaTime; yield return null; }
    }

    private void SetAllAlpha(byte a)
    {
        int count = text.textInfo.characterCount;
        for (int i = 0; i < count; i++) SetCharAlpha(i, a);
        text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    // 글자 하나의 4개 정점 알파를 설정. 공백 등 비가시 글자는 무시.
    private void SetCharAlpha(int charIndex, byte alpha)
    {
        var info = text.textInfo;
        var ch = info.characterInfo[charIndex];
        if (!ch.isVisible) return;
        int mat = ch.materialReferenceIndex;
        int v = ch.vertexIndex;
        var cols = info.meshInfo[mat].colors32;
        cols[v + 0].a = alpha;
        cols[v + 1].a = alpha;
        cols[v + 2].a = alpha;
        cols[v + 3].a = alpha;
    }

    // 연결된 TMP가 없을 때 화면 중앙에 풀 오버레이 캔버스 + TMP를 만든다.
    private void AutoCreate()
    {
        var canvasGo = new GameObject("CenterToastOverlay", typeof(Canvas));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        var txtGo = new GameObject("ToastText", typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(canvasGo.transform, false);

        text = txtGo.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.color = color;
        text.raycastTarget = false;

        var rt = text.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 120f); // 정중앙보다 살짝 위
        rt.sizeDelta = new Vector2(1400f, 200f);

        canvasGroup = txtGo.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}
