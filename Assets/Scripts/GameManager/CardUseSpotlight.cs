using UnityEngine;
using UnityEngine.UI;

// 카드 사용 드롭존(화면 중앙 원형 영역)을 강조하는 스포트라이트.
// 어두운 풀스크린 패널 + 중앙 원형 구멍(UI/SpotlightHole 셰이더)으로,
// "여기로 카드를 끌어다 놓으면 사용된다"는 영역을 밝게 보여준다.
//
// DragCard 가 드래그 시작 시 Show(반지름), 드래그 종료(사용/취소) 시 Hide() 를 호출한다.
//
// 사용법: 씬의 아무 GameObject(예: 빈 오브젝트 "CardUseSpotlight")에 이 컴포넌트만 붙이면
//   오버레이(Canvas + Image + 머티리얼)를 런타임에 자동 생성한다. 별도 씬 세팅 불필요.
//   직접 만든 오버레이 Image 를 쓰고 싶으면 overlay 에 연결하면 그걸 사용한다.
[DisallowMultipleComponent]
public class CardUseSpotlight : MonoBehaviour
{
    public static CardUseSpotlight Instance { get; private set; }

    [Header("오버레이 (비우면 자동 생성)")]
    [SerializeField] private Graphic overlay;          // 어두운 패널 Image (UI/SpotlightHole 머티리얼)
    [SerializeField] private GameObject overlayObject; // 켜고 끌 오브젝트(비우면 overlay 의 GameObject)

    [Header("모양")]
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.7f); // 어두운 정도(알파)
    [SerializeField] private float softness = 25f;     // 구멍 가장자리 부드러움(px). 작게 둘수록 밝은 영역이 실제 사용 경계와 일치
    [SerializeField] private int sortingOrder = 150;   // 자동 생성 Canvas 정렬 순서(카드/기존 UI 위로 덮음)

    private Material mat;   // 인스턴스 머티리얼(원본 보호)
    private float radius;
    private bool visible;

    void Awake()
    {
        Instance = this;

        if (overlay == null && overlayObject == null) AutoCreateOverlay();
        if (overlay == null) overlay = GetComponentInChildren<Graphic>(true);
        if (overlayObject == null && overlay != null) overlayObject = overlay.gameObject;

        if (overlay != null && overlay.material != null)
        {
            mat = Instantiate(overlay.material); // 에셋 원본/다른 인스턴스 영향 방지
            overlay.material = mat;
            mat.SetColor("_Color", overlayColor);
        }

        if (overlayObject != null) overlayObject.SetActive(false); // 평소엔 꺼둠
    }

    // 오버레이가 연결되지 않았으면 풀스크린 어두운 패널을 코드로 만든다.
    private void AutoCreateOverlay()
    {
        var shader = Shader.Find("UI/SpotlightHole");
        if (shader == null)
        {
            Debug.LogError("CardUseSpotlight: 'UI/SpotlightHole' 셰이더를 찾을 수 없습니다. 오버레이를 직접 연결하세요.");
            return;
        }

        // 루트 ScreenSpaceOverlay 캔버스로 생성해야 화면 크기로 자동 확장된다.
        // (다른 Canvas의 자식으로 두면 중첩 캔버스가 되어 화면 크기로 안 늘어남)
        var canvasGo = new GameObject("CardUseSpotlightOverlay", typeof(Canvas));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        var imgGo = new GameObject("Dark", typeof(Image));
        imgGo.transform.SetParent(canvasGo.transform, false);
        var rt = imgGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero; // 풀스크린 stretch (부모=화면크기 캔버스)

        var img = imgGo.GetComponent<Image>();
        img.material = new Material(shader);
        img.color = Color.white;       // 어두운 정도는 머티리얼 _Color 가 담당
        img.raycastTarget = false;     // 드래그는 이미 캡처되어 있으므로 입력 방해 안 함

        overlay = img;
        overlayObject = canvasGo;
    }

    // 드래그 시작 시 호출. radius = 화면 중앙 사용 판정 반지름(px).
    public void Show(float useRadius)
    {
        radius = useRadius;
        visible = true;
        if (overlayObject != null && !overlayObject.activeSelf) overlayObject.SetActive(true);
        Apply();
    }

    // 카드 사용/취소(드래그 종료) 시 호출.
    public void Hide()
    {
        visible = false;
        if (overlayObject != null) overlayObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (visible) Apply(); // 해상도/창 크기 변화에도 중앙 유지
    }

    private void Apply()
    {
        if (mat == null) return;

        // 화면 중앙에 구멍 1개. (DragCard 의 사용 판정도 화면 중앙 기준이라 일치)
        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        mat.SetVector("_Center", new Vector4(center.x, center.y, 0f, 0f));
        mat.SetFloat("_Radius", radius);   // 밝은 영역 = 실제 사용 가능 영역
        mat.SetFloat("_Softness", softness);

        // 두 번째 구멍은 사용 안 함(비활성)
        mat.SetVector("_Center2", new Vector4(-9999f, -9999f, 0f, 0f));
        mat.SetFloat("_Radius2", 0f);
    }
}
