using UnityEngine;

public sealed class CameraRig : MonoBehaviour
{
    const float AimBackMin = 0.38f;
    const float AimBackMax = 2.35f;
    const float OverviewZoomMin = 1.8f;
    const float OverviewZoomMax = 6.5f;

    public Transform Pivot;
    public Camera Cam;

    Ball _cueBall;
    CueController _cue;
    bool _aimView;
    Vector3 _pan;
    float _lastPinch;
    float _overviewZoom = 4.35f;
    float _aimBack = 1.05f;

    public void Build()
    {
        var pivot = new GameObject("CameraPivot");
        Pivot = pivot.transform;
        Pivot.position = Vector3.up * 0.05f;
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        camGo.transform.SetParent(Pivot, false);
        Cam = camGo.AddComponent<Camera>();
        Cam.nearClipPlane = 0.03f;
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
        HandleZoom();
        if (_aimView && _cue != null && _cueBall != null && (_cue.PlacingCue || _cueBall.isActiveAndEnabled))
        {
            ApplyAimView();
            return;
        }

        HandleOverviewPan();
        ApplyOverview();
    }

    void HandleZoom()
    {
        if (Input.touchCount == 2)
        {
            var dist = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
            if (_lastPinch > 1f)
            {
                var delta = dist - _lastPinch;
                if (_aimView)
                {
                    _aimBack = Mathf.Clamp(_aimBack - delta * 0.008f, AimBackMin, AimBackMax);
                }
                else
                {
                    _overviewZoom = Mathf.Clamp(_overviewZoom - delta * 0.01f, OverviewZoomMin, OverviewZoomMax);
                }
            }

            _lastPinch = dist;
        }
        else
        {
            _lastPinch = 0f;
        }

        var scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            if (_aimView)
            {
                _aimBack = Mathf.Clamp(_aimBack - scroll * 0.12f, AimBackMin, AimBackMax);
            }
            else
            {
                _overviewZoom = Mathf.Clamp(_overviewZoom - scroll * 0.2f, OverviewZoomMin, OverviewZoomMax);
            }
        }
    }

    void HandleOverviewPan()
    {
        if (Input.touchCount != 2)
        {
            return;
        }

        var mid = (Input.GetTouch(0).deltaPosition + Input.GetTouch(1).deltaPosition) * 0.5f;
        _pan += new Vector3(-mid.x, 0f, -mid.y) * 0.0015f;
        _pan.x = Mathf.Clamp(_pan.x, -0.8f, 0.8f);
        _pan.z = Mathf.Clamp(_pan.z, -1.2f, 1.2f);
    }

    void ApplyOverview()
    {
        if (Pivot == null || Cam == null)
        {
            return;
        }

        Cam.fieldOfView = 40f;
        Cam.nearClipPlane = 0.04f;
        Pivot.position = _pan + Vector3.up * 0.05f;
        Pivot.rotation = Quaternion.Euler(0f, 18f, 0f);
        Cam.transform.localPosition = Quaternion.Euler(58f, 0f, 0f) * new Vector3(0f, 0f, -_overviewZoom);
        Cam.transform.LookAt(Pivot.position + Vector3.up * 0.1f);
    }

    void ApplyAimView()
    {
        var ball = _cueBall.transform.position;
        var zoomT = Mathf.InverseLerp(AimBackMin, AimBackMax, _aimBack);
        if (_cue.PlacingCue)
        {
            Cam.fieldOfView = Mathf.Lerp(42f, 50f, zoomT);
            Cam.nearClipPlane = 0.03f;
            var placeCam = ball + new Vector3(0.12f, Mathf.Lerp(0.42f, 1.35f, zoomT), Mathf.Lerp(-0.32f, -1.05f, zoomT));
            Cam.transform.position = placeCam;
            Cam.transform.LookAt(ball + Vector3.up * 0.02f);
            return;
        }

        var fwd = _cue.AimDirection;
        if (fwd.sqrMagnitude < 0.01f)
        {
            fwd = Vector3.forward;
        }

        fwd.y = 0f;
        fwd.Normalize();
        var height = Mathf.Lerp(0.09f, 0.46f, zoomT);
        var lookAhead = Mathf.Lerp(0.55f, 1.9f, zoomT);
        var desired = ball - fwd * _aimBack + Vector3.up * height;
        var lookAt = ball + fwd * lookAhead + Vector3.up * 0.03f;
        Cam.fieldOfView = Mathf.Lerp(44f, 56f, zoomT);
        Cam.nearClipPlane = Mathf.Lerp(0.02f, 0.05f, zoomT);
        Cam.transform.position = desired;
        Cam.transform.rotation = Quaternion.LookRotation(lookAt - desired, Vector3.up);
    }
}
