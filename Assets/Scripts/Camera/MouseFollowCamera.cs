using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MouseFollowCamera : MonoBehaviour
{
    [SerializeField] private float positionStrength = 3f;
    [SerializeField] private float smoothness = 5f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomMin = -8f;
    [SerializeField] private float zoomMax = 12f;
    [SerializeField] private float dragThreshold = 0.25f;
    [SerializeField] private float maxMoveDistance = 8f;
    [SerializeField] private float keyboardMoveSpeed = 10f; // Player/Move(WASD) 이동 속도(유닛/초)

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
    }

    void LateUpdate()
    {
        if(!TutorialManager.Instance.IsTutorial)return;
        UpdateZoom();
        ApplyZoomDelta();
        UpdateKeyboardMove();
        UpdateFollow();
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

    void UpdateZoom()
    {
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
            transform.position += forward * delta;
            dragAnchorPos += forward * delta;
        }
        prevZoomDist = zoomDistance;
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
