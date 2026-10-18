using UnityEngine;
using UnityEngine.UI;

public class AndroidBridge : MonoBehaviour
{
    [Header("Drag the existing UI buttons here")]
    public Button overviewButton;
    public Button frontButton;
    public Button tiresButton;
    public Button cabinButton;
    public Button openDoorsButton;

    // Android can only call methods that take ONE string parameter,
    // so each method has an unused "msg" argument.
    public void Overview(string msg)  => Press(overviewButton);
    public void Front(string msg)     => Press(frontButton);
    public void Tires(string msg)     => Press(tiresButton);
    public void Cabin(string msg)     => Press(cabinButton);
    public void OpenDoors(string msg) => Press(openDoorsButton);

    // Optional: one entry point that takes the button name as text
    public void Press(string buttonName)
    {
        switch (buttonName.ToLower())
        {
            case "overview":  Press(overviewButton);  break;
            case "front":     Press(frontButton);     break;
            case "tires":     Press(tiresButton);     break;
            case "cabin":     Press(cabinButton);     break;
            case "opendoors": Press(openDoorsButton); break;
            default: Debug.LogWarning("Unknown button: " + buttonName); break;
        }
    }

    void Press(Button b)
    {
        if (b != null) b.onClick.Invoke(); // runs whatever the button already does
    }
}