using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MouseFollowCamera : MonoBehaviour
{
    [SerializeField] private float positionStrength = 3f;
    [SerializeField] private float rotationStrength = 4f;
    [SerializeField] private float smoothness = 5f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float zoomMin = -8f;
    [SerializeField] private float zoomMax = 12f;
    [SerializeField] private TileMap tileMap;

    private Vector3 forward;
    private Quaternion baseRotation;
    private float zoomDistance;
    private float prevZoomDist;
    private Camera cam;

    private Vector3 dragAnchorPos;
    private bool wasPressed;
    private bool ignoreCurrentDrag;

    private InputAction mouseClick;
    private InputAction wheelValue;

    void Start()
    {
        forward = transform.forward;
        baseRotation = transform.rotation;
        dragAnchorPos = transform.position;
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (tileMap == null)
        {
            var tm = GameObject.FindWithTag("TileMap");
            if (tm != null) tileMap = tm.GetComponent<TileMap>();
        }
        mouseClick = InputSystem.actions.FindAction("Player/Attack");
        wheelValue = InputSystem.actions.FindAction("Player/Wheel");
    }

    void LateUpdate()
    {
        UpdateZoom();
        ApplyZoomDelta();
        UpdateFollow();
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
            ignoreCurrentDrag = IsClickBlocked();
            if (!ignoreCurrentDrag)
            {
                dragAnchorPos = transform.position;
            }
        }
        if (!isPressed)
        {
            ignoreCurrentDrag = false;
        }
        wasPressed = isPressed;

        if (!isPressed || ignoreCurrentDrag) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 norm = new Vector2(
            Mathf.Clamp((mouseScreen.x / Screen.width) * 2f - 1f, -1f, 1f),
            Mathf.Clamp((mouseScreen.y / Screen.height) * 2f - 1f, -1f, 1f)
        );

        Vector3 targetPos = dragAnchorPos + new Vector3(norm.x * positionStrength, 0f, norm.y * positionStrength);
        Quaternion targetRot = baseRotation * Quaternion.Euler(-norm.y * rotationStrength, norm.x * rotationStrength, 0f);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * smoothness);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothness);
    }

    bool IsClickBlocked()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return true;

        if (tileMap == null || cam == null) return false;

        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        var plane = new Plane(Vector3.up, new Vector3(0f, tileMap.Origin.y, 0f));
        if (!plane.Raycast(ray, out float dist)) return false;

        var world = ray.GetPoint(dist);
        var (gx, gz) = tileMap.WorldToGrid(world);
        return tileMap.IsInBounds(gx, gz);
    }
}
