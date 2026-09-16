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

    public void Bind(Ball cue, Table table, Camera cam)
    {
        _cue = cue;
        _table = table;
        _cam = cam;
        _stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
        _stick.name = "CueStick";
        _stick.localScale = new Vector3(0.012f, 0.72f, 0.012f);
        Object.Destroy(_stick.GetComponent<Collider>());
        var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(sh);
        var wood = new Color(0.45f, 0.28f, 0.12f);
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", wood);
        }

        mat.color = wood;
        _stick.GetComponent<MeshRenderer>().sharedMaterial = mat;

        var aimGo = new GameObject("AimLine");
        _aim = aimGo.AddComponent<LineRenderer>();
        _aim.positionCount = 2;
        _aim.startWidth = 0.012f;
        _aim.endWidth = 0.004f;
        _aim.material = new Material(Shader.Find("Sprites/Default"));
        _aim.startColor = new Color(1f, 0.95f, 0.55f, 0.9f);
        _aim.endColor = new Color(1f, 0.95f, 0.55f, 0.15f);
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
        var pull = 0.42f + Power * 0.35f + _anim;
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
