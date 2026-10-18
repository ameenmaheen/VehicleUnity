using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
#endif

public class OrbitCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    [Tooltip("Point the camera looks at, relative to the target (car mid-height).")]
    public Vector3 focusOffset = new Vector3(0f, 0.8f, 0f);

    [Header("Start view")]
    public float yaw = -90f;
    public float pitch = 5.5f;
    public float distance = 7.2f;

    [Header("Limits")]
    public float minPitch = 2f;
    public float maxPitch = 60f;
    public float minDistance = 3f;
    public float maxDistance = 10f;

    [Header("Sensitivity")]
    [Tooltip("Degrees of rotation per pixel dragged. Make negative to invert.")]
    public float rotateSpeed = 0.2f;
    [Tooltip("1 = distance follows the fingers exactly. Higher = more sensitive.")]
    public float pinchSensitivity = 1f;
    [Tooltip("Mouse wheel zoom per notch (0.1 = 10%).")]
    public float scrollZoomPercent = 0.1f;

    [Header("Smoothing")]
    public float damping = 12f;

    // Where we want to be vs. where we are (current values ease toward desired)
    float desiredYaw, desiredPitch, desiredDistance;
    Vector3 desiredFocusOffset;
    float curYaw, curPitch, curDistance;
    Vector3 curFocusOffset;

    bool pinching;
    float lastPinchDistance;
    bool inputBlockedByUI;
#if !ENABLE_INPUT_SYSTEM
    Vector3 lastMousePos;
#endif

    void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        EnhancedTouchSupport.Enable();
#endif
        desiredYaw = curYaw = yaw;
        desiredPitch = curPitch = pitch;
        desiredDistance = curDistance = distance;
        desiredFocusOffset = curFocusOffset = focusOffset;
    }

    void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        EnhancedTouchSupport.Disable();
#endif
    }

    void Update() => HandleInput();

    void LateUpdate()
    {
        if (!target) return;

        // Frame-rate independent easing
        float t = 1f - Mathf.Exp(-damping * Time.deltaTime);
        curYaw = Mathf.Lerp(curYaw, desiredYaw, t);
        curPitch = Mathf.Lerp(curPitch, desiredPitch, t);
        curDistance = Mathf.Lerp(curDistance, desiredDistance, t);
        curFocusOffset = Vector3.Lerp(curFocusOffset, desiredFocusOffset, t);

        Vector3 focus = target.position + curFocusOffset;
        Quaternion orbit = Quaternion.Euler(curPitch, curYaw, 0f);
        Vector3 pos = focus + orbit * new Vector3(0f, 0f, -curDistance);
        pos.y = Mathf.Max(pos.y, 0.1f); // never dip below the ground

        transform.SetPositionAndRotation(
            pos, Quaternion.LookRotation(focus - pos, Vector3.up));
    }

    // ---------- Public: used later by the front / tires / cabin buttons ----------
    public void SetView(float newYaw, float newPitch, float newDistance,
                        Vector3 newFocusOffset, float newMinDistance, float newMaxDistance)
    {
        minDistance = newMinDistance;
        maxDistance = newMaxDistance;
        // Take the shortest way around instead of spinning the long way
        desiredYaw += Mathf.DeltaAngle(desiredYaw, newYaw);
        desiredPitch = Mathf.Clamp(newPitch, minPitch, maxPitch);
        desiredDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
        desiredFocusOffset = newFocusOffset;
    }

    // In Play mode: orbit to a view you like, then use this component's
    // three-dot menu > "Log Current View" to read the numbers.
    [ContextMenu("Log Current View")]
    void LogCurrentView()
    {
        Debug.Log($"yaw {desiredYaw:F0}, pitch {desiredPitch:F0}, " +
                  $"distance {desiredDistance:F1}, focus {desiredFocusOffset}");
    }

    // ---------- Input ----------
    void HandleInput()
    {
#if ENABLE_INPUT_SYSTEM
        var touches = ETouch.activeTouches;

        if (touches.Count >= 2)
        {
            Pinch(touches[0].screenPosition, touches[1].screenPosition);
            return;
        }
        pinching = false;

        if (touches.Count == 1)
        {
            var touch = touches[0];
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                inputBlockedByUI = IsOverUI(touch.touchId);
            if (!inputBlockedByUI && touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
                Rotate(touch.delta);
            return;
        }

        // Mouse (for testing in the Editor)
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame) inputBlockedByUI = IsOverUI(-1);
        if (mouse.leftButton.isPressed && !inputBlockedByUI)
            Rotate(mouse.delta.ReadValue());

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f && !IsOverUI(-1))
            ZoomByFactor(1f - Mathf.Sign(scroll) * scrollZoomPercent);
#else
        if (Input.touchCount >= 2)
        {
            Pinch(Input.GetTouch(0).position, Input.GetTouch(1).position);
            return;
        }
        pinching = false;

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) inputBlockedByUI = IsOverUI(touch.fingerId);
            if (!inputBlockedByUI && touch.phase == TouchPhase.Moved) Rotate(touch.deltaPosition);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            inputBlockedByUI = IsOverUI(-1);
            lastMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0) && !inputBlockedByUI)
        {
            Rotate((Vector2)(Input.mousePosition - lastMousePos));
            lastMousePos = Input.mousePosition;
        }

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f && !IsOverUI(-1))
            ZoomByFactor(1f - Mathf.Sign(scroll) * scrollZoomPercent);
#endif
    }

    void Rotate(Vector2 delta)
    {
        desiredYaw += delta.x * rotateSpeed;
        desiredPitch = Mathf.Clamp(desiredPitch - delta.y * rotateSpeed, minPitch, maxPitch);
    }

    void Pinch(Vector2 a, Vector2 b)
    {
        float d = Vector2.Distance(a, b);
        if (pinching && d > 1f && lastPinchDistance > 1f)
            ZoomByFactor(Mathf.Pow(lastPinchDistance / d, pinchSensitivity));
        lastPinchDistance = d;
        pinching = true;
    }

    void ZoomByFactor(float factor)
    {
        desiredDistance = Mathf.Clamp(desiredDistance * factor, minDistance, maxDistance);
    }

    static bool IsOverUI(int pointerId)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
    }
}