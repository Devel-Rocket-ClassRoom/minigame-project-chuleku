using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MouseFollowCamera : MonoBehaviour
{
    [SerializeField] private float positionStrength = 3f;
    [SerializeField] private float smoothness = 5f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomMin = -8f;
    [SerializeField] private float zoomMax = 12f;
    [SerializeField] private float dragThreshold = 0.25f;
    [SerializeField] private float maxMoveDistanceZoomedIn = 20f;  // 줌 인(확대) 상태에서의 최대 이동 범위
    [SerializeField] private float maxMoveDistanceZoomedOut = 4f;  // 줌 아웃(축소) 상태에서의 최대 이동 범위
    [SerializeField] private float keyboardMoveSpeed = 10f; // Player/Move(WASD) 이동 속도(유닛/초)
    [SerializeField] private float edgeSize = 20f;        // 화면 끝 감지 영역(픽셀)

    [SerializeField] private float edgeMoveSpeed = 10f;   // 엣지 스크롤 이동 속도(유닛/초)
    private const float maxMoveSpeed = 20f;
    [SerializeField] private float groundY = 0f;          // 줌-투-커서 기준 지면 높이(Y)

    [Header("이동속도 설정 슬라이더 (선택)")]
    [SerializeField] private Slider edgeSpeedSlider;
    [SerializeField] private Slider keyboardSpeedSlider;

    private const string PrefEdgeSpeed = "cam_edge_speed";
    private const string PrefKeyboardSpeed = "cam_keyboard_speed";

    private Vector3 forward;
    private float zoomDistance;
    private float prevZoomDist;
    private Camera cam;

    private Vector3 originXZ;
    private Vector3 dragAnchorPos;
    private bool wasPressed;
    private bool ignoreCurrentDrag;
    private float pressStartTime;
    private bool dragStarted;

    private InputAction mouseClick;
    private InputAction wheelValue;
    private InputAction moveValue;

    void Start()
    {
        forward = transform.forward;
        dragAnchorPos = transform.position;
        originXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        mouseClick = InputSystem.actions.FindAction("Player/Attack");
        wheelValue = InputSystem.actions.FindAction("Player/Wheel");
        moveValue = InputSystem.actions.FindAction("Player/Move");

        // 저장된 이동 속도 복원 (없으면 인스펙터 기본값 사용)
        edgeMoveSpeed = PlayerPrefs.GetFloat(PrefEdgeSpeed, edgeMoveSpeed);
        keyboardMoveSpeed = PlayerPrefs.GetFloat(PrefKeyboardSpeed, keyboardMoveSpeed);

        // 슬라이더 범위를 0~최대속도로 강제하고 현재 값 반영 (onValueChanged → Set 메서드 연결)
        if (edgeSpeedSlider != null)
        {
            edgeSpeedSlider.minValue = 0f;
            edgeSpeedSlider.maxValue = maxMoveSpeed;
            edgeSpeedSlider.value = edgeMoveSpeed;
        }
        if (keyboardSpeedSlider != null)
        {
            keyboardSpeedSlider.minValue = 0f;
            keyboardSpeedSlider.maxValue = maxMoveSpeed;
            keyboardSpeedSlider.value = keyboardMoveSpeed;
        }
    }

    // 설정창 슬라이더의 onValueChanged에 연결 — 화면 가장자리 이동 속도 조절
    public void SetEdgeMoveSpeed(float v)
    {
        edgeMoveSpeed = Mathf.Clamp(v, 0f, maxMoveSpeed);
        PlayerPrefs.SetFloat(PrefEdgeSpeed, edgeMoveSpeed);
    }

    // 설정창 슬라이더의 onValueChanged에 연결 — 키보드(WASD) 이동 속도 조절
    public void SetKeyboardMoveSpeed(float v)
    {
        keyboardMoveSpeed = Mathf.Clamp(v, 0f, maxMoveSpeed);
        PlayerPrefs.SetFloat(PrefKeyboardSpeed, keyboardMoveSpeed);
    }

    // 슬라이더 초기값 표시용 (설정창 열 때 호출)
    public float GetEdgeMoveSpeed() => edgeMoveSpeed;
    public float GetKeyboardMoveSpeed() => keyboardMoveSpeed;

    void LateUpdate()
    {
        if(!TutorialManager.Instance.IsTutorial)return;
        UpdateZoom();
        ApplyZoomDelta();
        UpdateKeyboardMove();
        UpdateEdgeScroll();
        // UpdateFollow();
    }

    // Player/Move(WASD·게임패드 스틱)로 카메라를 XZ 평면에서 직접 이동.
    // 마우스 드래그와 같은 maxMoveDistance 범위 제한을 공유한다.
    void UpdateKeyboardMove()
    {
        if (moveValue == null) return;

        Vector2 input = moveValue.ReadValue<Vector2>();
        if (input.sqrMagnitude < 0.0001f) return;

        Vector3 delta = new Vector3(input.x, 0f, input.y) * keyboardMoveSpeed * Time.deltaTime;
        Vector3 targetPos = ClampToMaxDistance(transform.position + delta);
        transform.position = targetPos;

        // 이동 직후 마우스 드래그 기준점도 같이 옮겨, 다음 드래그가 튀지 않게 한다.
        dragAnchorPos = targetPos;
        DefenceGameManager.Instance.closeButton();
    }

    // 마우스가 화면 가장자리에 닿으면 그 방향으로 카메라를 이동.
    // 이동 범위는 ClampToMaxDistance(줌 비례)를 그대로 공유한다.
    void UpdateEdgeScroll()
    {
        if (Mouse.current == null) return;
        if (dragStarted) return; // 드래그 중이면 입력 충돌 방지로 멈춤

        Vector2 m = Mouse.current.position.ReadValue();
        // Confined 커서는 가장자리에서 위치가 Screen.width/Height와 같거나 미세하게 벗어난 값으로
        // 들어올 수 있다. "창 밖이면 무시"로 빠지면 정작 끝에 닿았을 때 스크롤이 멈추므로,
        // 경계로 clamp해서 "끝에 닿음"으로 처리한다.
        m.x = Mathf.Clamp(m.x, 0f, Screen.width);
        m.y = Mathf.Clamp(m.y, 0f, Screen.height);

        Vector3 dir = Vector3.zero;
        if (m.x <= edgeSize) dir.x = -1f;                       // 왼쪽
        else if (m.x >= Screen.width - edgeSize) dir.x = 1f;    // 오른쪽
        if (m.y <= edgeSize) dir.z = -1f;                       // 아래
        else if (m.y >= Screen.height - edgeSize) dir.z = 1f;   // 위

        if (dir == Vector3.zero) return;

        Vector3 targetPos = ClampToMaxDistance(transform.position + dir * edgeMoveSpeed * Time.deltaTime);
        transform.position = targetPos;
        dragAnchorPos = targetPos; // WASD 이동과 동일하게 드래그 기준점 동기화
        DefenceGameManager.Instance.closeButton();
    }

    void UpdateZoom()
    {
        if (Time.timeScale == 0f) return; // ESC 패널 등 일시정지(timeScale=0) 중엔 줌 막기
        if (wheelValue == null) return;
        float scroll = wheelValue.ReadValue<Vector2>().y;
        if (Mathf.Abs(scroll) < 0.01f) return;
        zoomDistance = Mathf.Clamp(zoomDistance + scroll * zoomSpeed, zoomMin, zoomMax);
    }

    void ApplyZoomDelta()
    {
        float delta = zoomDistance - prevZoomDist;
        if (Mathf.Abs(delta) > 0.0001f)
        {
            // 줌 전 마우스 아래 지면 지점
            bool hasBefore = TryGroundPointUnderMouse(out Vector3 before);

            transform.position += forward * delta;
            dragAnchorPos += forward * delta;

            // 줌 후 같은 지점이 커서 아래에 머물도록 XZ 보정 → 커서 방향으로 줌
            if (hasBefore && TryGroundPointUnderMouse(out Vector3 after))
            {
                Vector3 shift = before - after;
                shift.y = 0f;
                transform.position += shift;
                dragAnchorPos += shift;
            }
        }
        prevZoomDist = zoomDistance;

        // 줌 상태가 바뀌면 현재 위치를 새 이동 범위로 다시 클램프.
        // 줌 아웃 시 허용 범위가 줄어들어 카메라가 안쪽으로 밀리고, 맵 끝이 화면 경계에 고정된다.
        transform.position = ClampToMaxDistance(transform.position);
        dragAnchorPos = ClampToMaxDistance(dragAnchorPos);
    }

    // 마우스 커서가 가리키는 지면(y = groundY 평면) 위의 월드 좌표를 구한다.
    bool TryGroundPointUnderMouse(out Vector3 point)
    {
        point = Vector3.zero;
        if (cam == null || Mouse.current == null) return false;

        Vector2 m = Mouse.current.position.ReadValue();
        if (m.x < 0f || m.y < 0f || m.x > Screen.width || m.y > Screen.height) return false;

        Ray ray = cam.ScreenPointToRay(m);
        Plane ground = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
        if (ground.Raycast(ray, out float enter))
        {
            point = ray.GetPoint(enter);
            return true;
        }
        return false;
    }

    // 현재 줌 레벨에 따른 최대 이동 범위. 줌 인일수록 더 멀리 이동 가능.
    float CurrentMaxDistance()
    {
        float t = Mathf.InverseLerp(zoomMin, zoomMax, zoomDistance); // 0 = 아웃, 1 = 인
        return Mathf.Lerp(maxMoveDistanceZoomedOut, maxMoveDistanceZoomedIn, t);
    }

    void UpdateFollow()
    {
        bool isPressed = mouseClick != null && mouseClick.IsPressed() && Mouse.current != null;

        if (isPressed && !wasPressed)
        {
            pressStartTime = Time.unscaledTime;
            dragStarted = false;
            ignoreCurrentDrag = IsClickBlocked();
        }
        if (!isPressed)
        {
            ignoreCurrentDrag = false;
            dragStarted = false;
        }
        wasPressed = isPressed;

        if (!isPressed || ignoreCurrentDrag) return;

        if (Time.unscaledTime - pressStartTime < dragThreshold) return;

        if (!dragStarted)
        {
            dragStarted = true;
            dragAnchorPos = transform.position;
        }

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 norm = new Vector2(
            Mathf.Clamp((mouseScreen.x / Screen.width) * 2f - 1f, -1f, 1f),
            Mathf.Clamp((mouseScreen.y / Screen.height) * 2f - 1f, -1f, 1f)
        );

        Vector3 targetPos = dragAnchorPos + new Vector3(norm.x * positionStrength, 0f, norm.y * positionStrength);
        targetPos = ClampToMaxDistance(targetPos);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothness);
        DefenceGameManager.Instance.closeButton();
    }

    Vector3 ClampToMaxDistance(Vector3 pos)
    {
        float maxMoveDistance = CurrentMaxDistance();
        Vector3 offset = new Vector3(pos.x - originXZ.x, 0f, pos.z - originXZ.z);
        float sqr = offset.sqrMagnitude;
        if (sqr > maxMoveDistance * maxMoveDistance)
        {
            offset = offset.normalized * maxMoveDistance;
            pos.x = originXZ.x + offset.x;
            pos.z = originXZ.z + offset.z;
        }
        return pos;
    }

    bool IsClickBlocked()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
