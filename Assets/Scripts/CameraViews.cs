using System;
using UnityEngine;

public class CameraViews : MonoBehaviour
{
    [Serializable]
    public class ViewPreset
    {
        public string name = "View";
        public float yaw;
        public float pitch = 8f;
        public float distance = 6f;
        public Vector3 focusOffset = new Vector3(0f, 0.8f, 0f);
        public float minDistance = 3f;
        public float maxDistance = 10f;
    }

    public OrbitCameraController orbit;
    public ViewPreset[] views;

    // Called by UI buttons: 0 = Overview, 1 = Front, 2 = Tires, 3 = Cabin
    public void GoToView(int index)
    {
        if (!orbit || views == null || index < 0 || index >= views.Length) return;
        var v = views[index];
        orbit.SetView(v.yaw, v.pitch, v.distance, v.focusOffset, v.minDistance, v.maxDistance);
    }

    // Fills in starting values when you first add the component
    void Reset()
    {
        orbit = GetComponent<OrbitCameraController>();
        views = new[]
        {
            new ViewPreset { name = "Overview", yaw = -34f, pitch = 5.5f, distance = 7.2f,
                focusOffset = new Vector3(0f, 0.8f, 0f), minDistance = 3f, maxDistance = 10f },
            new ViewPreset { name = "Front", yaw = 180f, pitch = 8f, distance = 6f,
                focusOffset = new Vector3(0f, 0.7f, 0f), minDistance = 3f, maxDistance = 10f },
            new ViewPreset { name = "Tires", yaw = -90f, pitch = 8f, distance = 1.8f,
                focusOffset = new Vector3(0.8f, 0.35f, 1.3f), minDistance = 1f, maxDistance = 4f },
            new ViewPreset { name = "Cabin", yaw = -60f, pitch = 15f, distance = 2.5f,
                focusOffset = new Vector3(0f, 1.1f, 0.1f), minDistance = 0.8f, maxDistance = 4f },
        };
    }
}