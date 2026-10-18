using UnityEngine;

public class Door : MonoBehaviour
{
    [Tooltip("Local axis the door swings around (the hinge): (1,0,0), (0,1,0) or (0,0,1).")]
    public Vector3 hingeAxis = Vector3.forward;
    [Tooltip("Degrees to open. Use a negative number to swing the other way.")]
    public float openAngle = 45f;
    [Tooltip("Degrees per second.")]
    public float speed = 120f;

    Quaternion closedRotation;
    Quaternion openRotation;
    bool isOpen;

    void Awake()
    {
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.AngleAxis(openAngle, hingeAxis);
    }

    void Update()
    {
        Quaternion target = isOpen ? openRotation : closedRotation;
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation, target, speed * Time.deltaTime);
    }

    public void Toggle() => isOpen = !isOpen;
    public void SetOpen(bool open) => isOpen = open;
    public bool IsOpen => isOpen;
}