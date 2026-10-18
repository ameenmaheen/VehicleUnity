using UnityEngine;
using TMPro;

public class DoorController : MonoBehaviour
{
    [Tooltip("Leave empty to find every Door under this object automatically.")]
    public Door[] doors;
    [Tooltip("Optional: the button's text, so it can switch between Open and Close.")]
    public TMP_Text label;
    public string openText = "Open Doors";
    public string closeText = "Close Doors";

    void Awake()
    {
        if (doors == null || doors.Length == 0)
            doors = GetComponentsInChildren<Door>(true);
    }

    void Update()
    {
        if (label) label.text = AnyOpen() ? closeText : openText;
    }

    // Hook this to the button's On Click
    public void ToggleAll()
    {
        bool open = !AnyOpen();   // if any door is open, close all; otherwise open all
        foreach (var d in doors) d.SetOpen(open);
    }

    public void OpenAll()  { foreach (var d in doors) d.SetOpen(true); }
    public void CloseAll() { foreach (var d in doors) d.SetOpen(false); }

    bool AnyOpen()
    {
        foreach (var d in doors) if (d.IsOpen) return true;
        return false;
    }
}