using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DoorTapHandler : MonoBehaviour
{
    public Camera cam;
    [Tooltip("A press longer than this is a drag, not a tap.")]
    public float maxTapSeconds = 0.3f;
    [Tooltip("A finger that moves more than this is a drag, not a tap.")]
    public float maxTapMovePixels = 20f;
    public float maxRayDistance = 50f;

    bool tracking;
    Vector2 startPos;
    float startTime;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        if (!ReadPointer(out Vector2 pos, out bool pressed, out bool released, out bool multiTouch))
            return;

        if (pressed)
        {
            tracking = !IsOverUI();   // ignore presses that start on a button
            startPos = pos;
            startTime = Time.unscaledTime;
        }

        if (multiTouch) tracking = false;   // a pinch is never a tap

        if (released && tracking)
        {
            tracking = false;
            bool quick = Time.unscaledTime - startTime <= maxTapSeconds;
            bool still = (pos - startPos).magnitude <= maxTapMovePixels;
            if (quick && still) TryToggleDoor(pos);
        }
    }

    void TryToggleDoor(Vector2 screenPos)
    {
        if (!cam) return;
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            Door door = hit.collider.GetComponentInParent<Door>();
            if (door) door.Toggle();
        }
    }

    static bool IsOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

#if ENABLE_INPUT_SYSTEM
    bool ReadPointer(out Vector2 pos, out bool pressed, out bool released, out bool multi)
    {
        pos = default; pressed = released = multi = false;
        var p = Pointer.current;   // mouse or touchscreen, whichever was used last
        if (p == null) return false;

        pos = p.position.ReadValue();
        pressed = p.press.wasPressedThisFrame;
        released = p.press.wasReleasedThisFrame;

        var ts = Touchscreen.current;
        if (ts != null)
        {
            int count = 0;
            foreach (var t in ts.touches) if (t.press.isPressed) count++;
            multi = count > 1;
        }
        return true;
    }
#else
    bool ReadPointer(out Vector2 pos, out bool pressed, out bool released, out bool multi)
    {
        multi = false;
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            pos = t.position;
            pressed = t.phase == TouchPhase.Began;
            released = t.phase == TouchPhase.Ended;
            multi = Input.touchCount > 1;
            return true;
        }
        pos = Input.mousePosition;
        pressed = Input.GetMouseButtonDown(0);
        released = Input.GetMouseButtonUp(0);
        return true;
    }
#endif
}