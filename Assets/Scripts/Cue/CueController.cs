using UnityEngine;
using UnityEngine.EventSystems;

public sealed class CueController : MonoBehaviour
{
    public float AimYaw;
    public float Power = 0.45f;
    public Vector2 English;
    public bool InputLocked;
    public bool PlacingCue;

    Ball _cue;
    Transform _stick;
    LineRenderer _aim;
    Camera _cam;
    Table _table;
    float _dragLastX;
    bool _draggingAim;
    float _anim;

    public Vector3 AimDirection => Quaternion.Euler(0f, AimYaw, 0f) * Vector3.forward;

    public void Bind(Ball cue, Table table, Camera cam, LookLibrary look)
    {
        _cue = cue;
        _table = table;
        _cam = cam;
        var root = new GameObject("CueStick");
        _stick = root.transform;
        Part(root.transform, "Butt", new Vector3(0f, -0.42f, 0f), new Vector3(0.022f, 0.22f, 0.022f), look.CueButt);
        Part(root.transform, "Shaft", new Vector3(0f, 0.05f, 0f), new Vector3(0.011f, 0.42f, 0.011f), look.CueWood);
        Part(root.transform, "Ferrule", new Vector3(0f, 0.48f, 0f), new Vector3(0.01f, 0.025f, 0.01f), look.Ferrule);
        Part(root.transform, "Tip", new Vector3(0f, 0.51f, 0f), new Vector3(0.009f, 0.012f, 0.009f), look.Tip);

        var aimGo = new GameObject("AimLine");
        _aim = aimGo.AddComponent<LineRenderer>();
        _aim.positionCount = 2;
        _aim.startWidth = 0.01f;
        _aim.endWidth = 0.0035f;
        _aim.numCapVertices = 4;
        _aim.material = new Material(Shader.Find("Sprites/Default"));
        _aim.startColor = new Color(1f, 0.92f, 0.45f, 0.95f);
        _aim.endColor = new Color(1f, 0.85f, 0.3f, 0.12f);
        _aim.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    static void Part(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        Object.Destroy(go.GetComponent<Collider>());
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    public void SetVisible(bool on)
    {
        if (_stick != null)
        {
            _stick.gameObject.SetActive(on);
        }

        if (_aim != null)
        {
            _aim.enabled = on;
        }
    }

    void Update()
    {
        if (_cue == null || !_cue.isActiveAndEnabled)
        {
            SetVisible(false);
            return;
        }

        if (PlacingCue)
        {
            SetVisible(false);
            HandlePlace();
            return;
        }

        if (!InputLocked)
        {
            HandleAim();
        }

        var dir = AimDirection;
        var pull = 0.55f + Power * 0.38f + _anim;
        var ballPos = _cue.transform.position;
        if (_stick != null)
        {
            _stick.position = ballPos - dir * pull;
            _stick.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90f, 0f, 0f);
        }

        UpdateAimLine(ballPos, dir);
    }

    void HandlePlace()
    {
        if (InputLocked || !PointerDown(out var screen))
        {
            return;
        }

        if (OverUi())
        {
            return;
        }

        var ray = _cam.ScreenPointToRay(screen);
        var plane = new Plane(Vector3.up, Vector3.zero);
        if (!plane.Raycast(ray, out var enter))
        {
            return;
        }

        var p = _table.ClampOnCloth(ray.GetPoint(enter), true);
        _cue.PlaceAndFreeze(p);
    }

    void HandleAim()
    {
        if (OverUi())
        {
            _draggingAim = false;
            return;
        }

        if (Input.touchCount >= 2)
        {
            _draggingAim = false;
            return;
        }

        if (PointerDown(out var screen))
        {
            if (!_draggingAim)
            {
                _draggingAim = true;
                _dragLastX = screen.x;
            }
            else
            {
                var dx = screen.x - _dragLastX;
                AimYaw += dx * 0.18f;
                _dragLastX = screen.x;
            }
        }
        else
        {
            _draggingAim = false;
        }
    }

    void UpdateAimLine(Vector3 origin, Vector3 dir)
    {
        if (_aim == null || !_aim.enabled)
        {
            return;
        }

        var start = origin + dir * (PoolConstants.BallRadius + 0.01f);
        var hitLen = 2.8f;
        if (Physics.SphereCast(start, PoolConstants.BallRadius * 0.92f, dir, out var hit, 3.2f))
        {
            hitLen = hit.distance;
        }

        _aim.SetPosition(0, start);
        _aim.SetPosition(1, start + dir * hitLen);
    }

    public void Fire()
    {
        if (_cue == null || _cue.Pocketed)
        {
            return;
        }

        var dir = AimDirection;
        var impulse = Mathf.Lerp(PoolConstants.MinShotImpulse, PoolConstants.MaxShotImpulse, Power);
        var right = Vector3.Cross(Vector3.up, dir).normalized;
        var up = Vector3.up;
        var point = _cue.transform.position - dir * PoolConstants.BallRadius
                    + right * English.x * PoolConstants.BallRadius * 0.7f
                    + up * English.y * PoolConstants.BallRadius * 0.7f;
        _cue.Body.WakeUp();
        _cue.Body.AddForceAtPosition(dir * impulse, point, ForceMode.Impulse);
        _anim = 0.12f;
    }

    void LateUpdate()
    {
        _anim = Mathf.MoveTowards(_anim, 0f, Time.deltaTime * 0.8f);
    }

    static bool OverUi()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            return true;
        }

        return EventSystem.current.IsPointerOverGameObject();
    }

    static bool PointerDown(out Vector3 screen)
    {
        if (Input.touchCount == 1)
        {
            var t = Input.GetTouch(0);
            screen = t.position;
            return t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled;
        }

        if (Input.GetMouseButton(0))
        {
            screen = Input.mousePosition;
            return true;
        }

        screen = default;
        return false;
    }
}
