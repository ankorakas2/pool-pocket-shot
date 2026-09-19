using UnityEngine;

public sealed class CameraRig : MonoBehaviour
{
    public Transform Pivot;
    public Camera Cam;

    Ball _cueBall;
    CueController _cue;
    bool _aimView;
    Vector3 _pan;
    float _lastPinch;
    float _overviewZoom = 4.35f;

    public void Build()
    {
        var pivot = new GameObject("CameraPivot");
        Pivot = pivot.transform;
        Pivot.position = Vector3.up * 0.05f;
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        camGo.transform.SetParent(Pivot, false);
        Cam = camGo.AddComponent<Camera>();
        Cam.nearClipPlane = 0.04f;
        Cam.farClipPlane = 40f;
        Cam.fieldOfView = 52f;
        Cam.clearFlags = CameraClearFlags.SolidColor;
        Cam.backgroundColor = new Color(0.04f, 0.045f, 0.055f);
        Cam.allowMSAA = true;
        Cam.depth = 10;
        if (camGo.GetComponent<AudioListener>() == null)
        {
            camGo.AddComponent<AudioListener>();
        }

        foreach (var other in FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (other == Cam)
            {
                continue;
            }

            other.enabled = false;
            var listener = other.GetComponent<AudioListener>();
            if (listener != null)
            {
                listener.enabled = false;
            }
        }

        ApplyOverview();
    }

    public void Bind(Ball cueBall, CueController cue)
    {
        _cueBall = cueBall;
        _cue = cue;
    }

    public void SetAimView(bool on)
    {
        _aimView = on;
        if (!on)
        {
            ApplyOverview();
        }
    }

    void LateUpdate()
    {
        if (_aimView && _cueBall != null && _cueBall.isActiveAndEnabled && _cue != null)
        {
            ApplyAimView();
            return;
        }

        HandleOverviewInput();
        ApplyOverview();
    }

    void HandleOverviewInput()
    {
        if (Input.touchCount == 2)
        {
            var a = Input.GetTouch(0);
            var b = Input.GetTouch(1);
            var dist = Vector2.Distance(a.position, b.position);
            if (_lastPinch > 1f)
            {
                _overviewZoom = Mathf.Clamp(_overviewZoom - (dist - _lastPinch) * 0.01f, 1.8f, 6.5f);
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
            _overviewZoom = Mathf.Clamp(_overviewZoom - scroll * 0.2f, 1.8f, 6.5f);
        }
    }

    void ApplyOverview()
    {
        if (Pivot == null || Cam == null)
        {
            return;
        }

        Cam.fieldOfView = 40f;
        Pivot.position = _pan + Vector3.up * 0.05f;
        Pivot.rotation = Quaternion.Euler(0f, 18f, 0f);
        Cam.transform.localPosition = Quaternion.Euler(58f, 0f, 0f) * new Vector3(0f, 0f, -_overviewZoom);
        Cam.transform.LookAt(Pivot.position + Vector3.up * 0.1f);
    }

    void ApplyAimView()
    {
        var ball = _cueBall.transform.position;
        var fwd = _cue.AimDirection;
        if (fwd.sqrMagnitude < 0.01f)
        {
            fwd = Vector3.forward;
        }

        fwd.y = 0f;
        fwd.Normalize();
        var desired = ball - fwd * 1.12f + Vector3.up * 0.28f;
        var lookAt = ball + fwd * 1.7f + Vector3.up * 0.04f;
        Cam.fieldOfView = 54f;
        Cam.transform.position = desired;
        Cam.transform.rotation = Quaternion.LookRotation(lookAt - desired, Vector3.up);
    }
}
