using UnityEngine;
using UnityEngine.UI;

// 어두운 풀스크린 패널(UI/SpotlightHole 셰이더 머티리얼 사용)에
// 월드 타깃(타일·유닛) 위치로 부드러운 원형 구멍을 내서 그 안쪽만 밝게 보여준다.
// 사용법:
//   1) 풀스크린 stretch Image 에 UI/SpotlightHole 머티리얼을 넣고
//   2) 이 컴포넌트를 같은 오브젝트(또는 아무 데나)에 붙인 뒤
//   3) overlay, worldCamera, target 을 연결한다.
[DisallowMultipleComponent]
public class TutorialSpotlight : MonoBehaviour, ICanvasRaycastFilter
{
    [Header("연결")]
    [SerializeField] private Graphic overlay;     // 어두운 패널 Image (셰이더 머티리얼 사용)
    [SerializeField] private Camera worldCamera;   // 월드 타깃을 비추는 카메라 (보통 Main)
    [SerializeField] private GameObject overlayObject; // 켜고 끌 오버레이 오브젝트(비우면 overlay 의 GameObject)

    [Header("클릭")]
    [Tooltip("켜면 구멍 안만 클릭 통과, 바깥(어두운 곳)은 클릭 차단")]
    [SerializeField] private bool blockClicksOutsideHoles = true;

    [Header("구멍 1")]
    [SerializeField] private Transform target;     // 밝게 비출 월드 오브젝트(타일/유닛)
    [SerializeField] private float radius = 120f;     // 화면 픽셀 기준 반지름
    [SerializeField] private float softness = 40f;    // 가장자리 부드러움(px)

    [Header("구멍 2 (없으면 비워둠)")]
    [SerializeField] private Transform target2;    // 두 번째 강조 대상 (옵션)
    [SerializeField] private float radius2 = 120f;
    [SerializeField] private float softness2 = 40f;

    [Header("공통")]
    [SerializeField] private bool scaleWithScreen = true; // 해상도에 맞춰 반지름 비례

    [Tooltip("기준 화면 높이(scaleWithScreen 켤 때 radius 가 이 높이 기준값)")]
    [SerializeField] private float referenceHeight = 1080f;

    [Header("UI 타깃")]
    [Tooltip("타깃이 UI(RectTransform)면 요소 크기에 맞춰 구멍 크기 자동 계산")]
    [SerializeField] private bool autoSizeForUI = true;
    [Tooltip("UI 자동 크기일 때 요소 바깥으로 더 키울 여백(px)")]
    [SerializeField] private float uiPadding = 24f;

    private Material mat; // 인스턴스 머티리얼 (원본 보호)
    private readonly Vector3[] corners = new Vector3[4]; // UI 코너 계산용(GC 방지)

    // 클릭 판정용으로 캐시한 현재 구멍의 화면좌표/반지름(스케일 적용 후)
    private Vector2 holeCenter1, holeCenter2;
    private float holeRadius1, holeRadius2;

    // Transform 대신 고정 월드좌표(Vector3)로 구멍을 낼 때 사용 (예: TileMap.GridToWorld)
    private bool useWorld1, useWorld2;
    private Vector3 worldPos1, worldPos2;

    void Awake()
    {
        if (overlay == null) overlay = GetComponent<Graphic>();
        if (worldCamera == null) worldCamera = Camera.main;
        if (overlayObject == null && overlay != null) overlayObject = overlay.gameObject;

        if (overlay != null && overlay.material != null)
        {
            // 머티리얼을 복제해서 이 오브젝트 전용으로 사용 (에셋 원본/다른 인스턴스 영향 방지)
            mat = Instantiate(overlay.material);
            overlay.material = mat;
        }
    }

    void LateUpdate()
    {
        UpdateHoles();
    }

    private void UpdateHoles()
    {
        if (mat == null) return; // UI 전용이면 worldCamera 없어도 됨

        float k = (scaleWithScreen && referenceHeight > 0f) ? Screen.height / referenceHeight : 1f;

        ApplyHole(target, useWorld1, worldPos1, "_Center", "_Radius", "_Softness", radius, softness, k,
                  out holeCenter1, out holeRadius1);
        ApplyHole(target2, useWorld2, worldPos2, "_Center2", "_Radius2", "_Softness2", radius2, softness2, k,
                  out holeCenter2, out holeRadius2);
    }

    // ───────── 단계별 제어용 공개 API ─────────

    // 이번 단계에서 강조할 타깃 지정 + 오버레이 켜기. b 는 옵션(2번째 구멍).
    // 호출할 때마다 이전 타깃은 덮어쓰여서, "이전에 밝힌 게 계속 남는" 문제가 사라진다.
    public void Show(Transform a, Transform b = null)
    {
        EnsureOverlayObject();
        target = a;  useWorld1 = false;
        target2 = b; useWorld2 = false;
        if (overlayObject != null && !overlayObject.activeSelf) overlayObject.SetActive(true);
        UpdateHoles(); // 켜자마자 위치 즉시 반영(한 프레임 깜빡임 방지)
    }

    // 월드 좌표(Vector3)로 강조. 예: spotlight.Show(tileMap.GridToWorld(x, z));
    // TileMap.Origin / GridToWorld 결과를 그대로 넣으면 된다.
    public void Show(Vector3 worldA)
    {
        EnsureOverlayObject();
        target = null;  useWorld1 = true;  worldPos1 = worldA;
        target2 = null; useWorld2 = false;
        if (overlayObject != null && !overlayObject.activeSelf) overlayObject.SetActive(true);
        UpdateHoles();
    }

    // 월드 좌표 두 곳 동시 강조. 예: 시작점 Origin + 끝점 GridToWorld(19,9)
    public void Show(Vector3 worldA, Vector3 worldB)
    {
        EnsureOverlayObject();
        target = null;  useWorld1 = true; worldPos1 = worldA;
        target2 = null; useWorld2 = true; worldPos2 = worldB;
        if (overlayObject != null && !overlayObject.activeSelf) overlayObject.SetActive(true);
        UpdateHoles();
    }

    // 강조 끄기: 타깃 비우고 오버레이 자체를 꺼서 화면 어둡게 안 함.
    public void Hide()
    {
        EnsureOverlayObject();
        target = null;  useWorld1 = false;
        target2 = null; useWorld2 = false;
        holeRadius1 = holeRadius2 = 0f;
        if (overlayObject != null) overlayObject.SetActive(false);
    }

    // Awake 순서와 무관하게 overlayObject 참조를 확보(다른 매니저 Awake 에서 호출돼도 안전)
    private void EnsureOverlayObject()
    {
        if (overlayObject != null) return;
        if (overlay == null) overlay = GetComponent<Graphic>();
        if (overlay != null) overlayObject = overlay.gameObject;
    }

    // 타깃 하나를 셰이더의 한 구멍에 반영. target 이 null 이면 반지름 0(구멍 비활성)
    // 클릭 판정용 화면좌표/반지름은 outCenter/outRadius 로 돌려준다.
    private void ApplyHole(Transform t, bool useWorld, Vector3 worldPos,
                           string centerProp, string radiusProp, string softProp,
                           float r, float s, float k, out Vector2 outCenter, out float outRadius)
    {
        // 강조 대상 없음(Transform null && 월드좌표 모드 아님) → 구멍 끔
        if (t == null && !useWorld)
        {
            mat.SetVector(centerProp, new Vector4(-9999f, -9999f, 0f, 0f));
            mat.SetFloat(radiusProp, 0f);
            mat.SetFloat(softProp, s * k);
            outCenter = new Vector2(-9999f, -9999f);
            outRadius = 0f;
            return;
        }

        Vector2 sp;
        float finalR;

        if (useWorld)
        {
            // 고정 월드좌표(예: TileMap.GridToWorld) → 카메라 투영
            ProjectWorld(worldPos, r * k, out sp, out finalR);
        }
        else if (t is RectTransform rt)
        {
            // UI 타깃: 캔버스 렌더 모드에 맞는 카메라로 화면좌표 변환
            sp = UiToScreen(rt, out float uiAutoR);
            // 자동 크기면 요소 크기 기반(이미 실제 px), 아니면 수동 radius(해상도 스케일 적용)
            finalR = autoSizeForUI ? uiAutoR : r * k;
        }
        else
        {
            // 월드 Transform: 그 위치를 투영
            ProjectWorld(t.position, r * k, out sp, out finalR);
        }

        mat.SetVector(centerProp, new Vector4(sp.x, sp.y, 0f, 0f));
        mat.SetFloat(radiusProp, finalR);
        mat.SetFloat(softProp, s * k);
        outCenter = sp;
        outRadius = finalR;
    }

    // 월드 좌표 한 점을 화면좌표로 투영. 카메라 뒤/없음이면 화면 밖으로 밀어 구멍 끔.
    private void ProjectWorld(Vector3 world, float scaledRadius, out Vector2 sp, out float finalR)
    {
        Camera c = worldCamera != null ? worldCamera : Camera.main;
        if (c == null) { sp = new Vector2(-9999f, -9999f); finalR = 0f; return; }

        Vector3 wsp = c.WorldToScreenPoint(world);
        if (wsp.z < 0f) { sp = new Vector2(-9999f, -9999f); finalR = 0f; return; }

        sp = new Vector2(wsp.x, wsp.y);
        finalR = scaledRadius;
    }

    // UI RectTransform → 화면좌표(중심). autoRadius 는 요소를 덮는 원 반지름(px) + 여백.
    private Vector2 UiToScreen(RectTransform rt, out float autoRadius)
    {
        Canvas cv = rt.GetComponentInParent<Canvas>();
        // Overlay 면 카메라 null, 그 외(Camera/World)면 캔버스가 쓰는 카메라
        Camera cam = (cv != null && cv.renderMode != RenderMode.ScreenSpaceOverlay)
            ? cv.worldCamera
            : null;

        Vector2 center = RectTransformUtility.WorldToScreenPoint(cam, rt.position);

        autoRadius = 0f;
        rt.GetWorldCorners(corners);
        for (int i = 0; i < 4; i++)
        {
            Vector2 c = RectTransformUtility.WorldToScreenPoint(cam, corners[i]);
            autoRadius = Mathf.Max(autoRadius, Vector2.Distance(center, c));
        }
        autoRadius += uiPadding;
        return center;
    }

    // GraphicRaycaster 가 패널 클릭 판정 때 호출. true 면 "이 지점은 패널이 막음".
    // 구멍 안이면 false 를 돌려줘서 클릭이 통과(뒤의 월드/UI 클릭 가능)하게 한다.
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (!blockClicksOutsideHoles) return true; // 필터 끄면 평소대로 전부 막음

        bool inHole1 = holeRadius1 > 0f && Vector2.Distance(screenPoint, holeCenter1) <= holeRadius1;
        bool inHole2 = holeRadius2 > 0f && Vector2.Distance(screenPoint, holeCenter2) <= holeRadius2;

        // 구멍 안 → 막지 않음(false), 구멍 밖 → 막음(true)
        return !(inHole1 || inHole2);
    }

    // 런타임에 타깃/크기를 바꾸고 싶을 때 (단계별로 강조 대상 변경)
    public void SetTarget(Transform t) => target = t;
    public void SetTarget2(Transform t) => target2 = t; // null 넣으면 두 번째 구멍 끔
    public void SetRadius(float r) => radius = r;
    public void SetRadius2(float r) => radius2 = r;
}
