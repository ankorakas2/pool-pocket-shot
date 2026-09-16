using UnityEngine;

public sealed class CameraRig : MonoBehaviour
{
    public Transform Pivot;
    public Camera Cam;
    public float Zoom = 3.6f;
    public float Pitch = 55f;
    public float Yaw = 0f;
    Vector3 _pan;
    float _lastPinch;

    public void Build()
    {
        var pivot = new GameObject("CameraPivot");
        Pivot = pivot.transform;
        Pivot.position = Vector3.up * 0.05f;
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        camGo.transform.SetParent(Pivot, false);
        Cam = camGo.AddComponent<Camera>();
        Cam.nearClipPlane = 0.05f;
        Cam.farClipPlane = 40f;
        Cam.fieldOfView = 45f;
        camGo.AddComponent<AudioListener>();
        Apply();
    }

    void Update()
    {
        if (Input.touchCount == 2)
        {
            var a = Input.GetTouch(0);
            var b = Input.GetTouch(1);
            var dist = Vector2.Distance(a.position, b.position);
            if (_lastPinch > 1f)
            {
                var delta = dist - _lastPinch;
                Zoom = Mathf.Clamp(Zoom - delta * 0.01f, 1.8f, 6.5f);
            }

            _lastPinch = dist;
            var mid = (a.deltaPosition + b.deltaPosition) * 0.5f;
            _pan += new Vector3(-mid.x, 0f, -mid.y) * 0.0015f;
            _pan.x = Mathf.Clamp(_pan.x, -0.8f, 0.8f);
            _pan.z = Mathf.Clamp(_pan.z, -1.2f, 1.2f);
        }
        else
        {
            _lastPinch = 0f;
        }

        var scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Zoom = Mathf.Clamp(Zoom - scroll * 0.2f, 1.8f, 6.5f);
        }

        Apply();
    }

    void Apply()
    {
        if (Pivot == null || Cam == null)
        {
            return;
        }

        Pivot.position = _pan + Vector3.up * 0.05f;
        Pivot.rotation = Quaternion.Euler(0f, Yaw, 0f);
        Cam.transform.localPosition = Quaternion.Euler(Pitch, 0f, 0f) * new Vector3(0f, 0f, -Zoom);
        Cam.transform.LookAt(Pivot.position + Vector3.up * 0.1f);
    }
}
