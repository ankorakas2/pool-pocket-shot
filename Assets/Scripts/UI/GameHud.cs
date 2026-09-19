using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class GameHud : MonoBehaviour
{
    MatchFlow _flow;
    CueController _cue;
    Canvas _canvas;
    GameObject _menu;
    GameObject _play;
    GameObject _over;
    Text _who;
    Text _status;
    Text _extra;
    Text _overText;
    Slider _power;
    RectTransform _spinKnob;
    RectTransform _spinPad;
    GameObject _placeBtn;

    LookLibrary _look;

    public void Build(MatchFlow flow, CueController cue, LookLibrary look)
    {
        _flow = flow;
        _cue = cue;
        _look = look;
        var canvasGo = new GameObject("Canvas");
        _canvas = canvasGo.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGo);

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
        DontDestroyOnLoad(es);

        _menu = BuildMenu();
        _play = BuildPlay();
        _over = BuildOver();
        ShowMenu();
    }

    GameObject BuildMenu()
    {
        var root = Panel("Menu", new Color(0.03f, 0.05f, 0.04f, 0.9f));
        var card = ImageGo(root.transform, "Card", _look.UiPanel, new Vector2(0, 20), new Vector2(760, 920), new Color(1f, 1f, 1f, 0.97f));
        var parent = card.transform;
        Label(parent, "POOL POCKET SHOT", 54, new Vector2(0, 360), new Vector2(700, 80), TextAnchor.MiddleCenter, new Color(0.93f, 0.8f, 0.38f));
        Label(parent, "US 8-ball  ·  9-ball", 26, new Vector2(0, 290), new Vector2(700, 40), TextAnchor.MiddleCenter, new Color(0.75f, 0.82f, 0.72f));
        MenuBtn(parent, "8-Ball  ·  Pass & Play", new Vector2(0, 170), () => _flow.StartMatch(GameModeKind.EightBall, false, 1));
        MenuBtn(parent, "8-Ball  ·  vs CPU Easy", new Vector2(0, 70), () => _flow.StartMatch(GameModeKind.EightBall, true, 0));
        MenuBtn(parent, "8-Ball  ·  vs CPU Hard", new Vector2(0, -30), () => _flow.StartMatch(GameModeKind.EightBall, true, 2));
        MenuBtn(parent, "9-Ball  ·  Pass & Play", new Vector2(0, -130), () => _flow.StartMatch(GameModeKind.NineBall, false, 1));
        MenuBtn(parent, "9-Ball  ·  vs CPU", new Vector2(0, -230), () => _flow.StartMatch(GameModeKind.NineBall, true, 1));
        MenuBtn(parent, "Quit", new Vector2(0, -340), () => Application.Quit(), true);
        return root;
    }

    GameObject BuildPlay()
    {
        var root = new GameObject("PlayHud", typeof(RectTransform));
        root.transform.SetParent(_canvas.transform, false);
        StretchOn(root);

        var bar = ImageGo(root.transform, "TopBar", _look.UiPanel, Vector2.zero, new Vector2(1840, 110), new Color(1, 1, 1, 0.88f));
        var barRt = bar.GetComponent<RectTransform>();
        barRt.anchorMin = new Vector2(0.5f, 1f);
        barRt.anchorMax = new Vector2(0.5f, 1f);
        barRt.pivot = new Vector2(0.5f, 1f);
        barRt.anchoredPosition = new Vector2(0f, -16f);
        _who = Label(bar.transform, "Player 1", 32, new Vector2(-620, 12), new Vector2(600, 44), TextAnchor.MiddleLeft, new Color(0.95f, 0.86f, 0.45f));
        _status = Label(bar.transform, "Open table", 24, new Vector2(-620, -22), new Vector2(700, 36), TextAnchor.MiddleLeft, new Color(0.82f, 0.88f, 0.8f));
        _extra = Label(root.transform, "", 24, new Vector2(0, 120), new Vector2(1000, 40), TextAnchor.MiddleCenter, Color.white);

        var shoot = MenuBtn(root.transform, "SHOOT", Vector2.zero, () => _flow.Shoot());
        var shootRt = shoot.GetComponent<RectTransform>();
        shootRt.anchorMin = new Vector2(1f, 0f);
        shootRt.anchorMax = new Vector2(1f, 0f);
        shootRt.pivot = new Vector2(1f, 0f);
        shootRt.anchoredPosition = new Vector2(-36f, 36f);
        shootRt.sizeDelta = new Vector2(280, 110);

        _placeBtn = MenuBtn(root.transform, "PLACE CUE", Vector2.zero, () => _flow.ConfirmPlacement());
        var placeRt = _placeBtn.GetComponent<RectTransform>();
        placeRt.anchorMin = new Vector2(1f, 0f);
        placeRt.anchorMax = new Vector2(1f, 0f);
        placeRt.pivot = new Vector2(1f, 0f);
        placeRt.anchoredPosition = new Vector2(-36f, 160f);
        _placeBtn.SetActive(false);

        var menuBtn = MenuBtn(root.transform, "Menu", Vector2.zero, () => _flow.ReturnToMenu());
        var menuRt = menuBtn.GetComponent<RectTransform>();
        menuRt.anchorMin = new Vector2(1f, 1f);
        menuRt.anchorMax = new Vector2(1f, 1f);
        menuRt.pivot = new Vector2(1f, 1f);
        menuRt.anchoredPosition = new Vector2(-24f, -24f);
        menuRt.sizeDelta = new Vector2(180, 64);

        var sliderGo = new GameObject("Power", typeof(RectTransform));
        sliderGo.transform.SetParent(root.transform, false);
        var srt = sliderGo.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0f, 0f);
        srt.anchorMax = new Vector2(0f, 0f);
        srt.pivot = new Vector2(0f, 0f);
        srt.anchoredPosition = new Vector2(40f, 40f);
        srt.sizeDelta = new Vector2(520, 70);
        var bg = sliderGo.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);
        _power = sliderGo.AddComponent<Slider>();
        _power.minValue = 0.05f;
        _power.maxValue = 1f;
        _power.value = 0.5f;
        var fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(sliderGo.transform, false);
        var fr = fill.GetComponent<RectTransform>();
        fr.anchorMin = new Vector2(0, 0.2f);
        fr.anchorMax = new Vector2(1, 0.8f);
        fr.offsetMin = Vector2.zero;
        fr.offsetMax = Vector2.zero;
        var fi = fill.AddComponent<Image>();
        fi.color = new Color(0.95f, 0.7f, 0.15f);
        _power.fillRect = fr;
        _power.targetGraphic = bg;
        _power.onValueChanged.AddListener(v => _cue.Power = v);

        var powerLbl = Label(root.transform, "POWER", 18, Vector2.zero, new Vector2(200, 28), TextAnchor.MiddleLeft, new Color(0.9f, 0.82f, 0.45f));
        var powerLblRt = powerLbl.GetComponent<RectTransform>();
        powerLblRt.anchorMin = new Vector2(0f, 0f);
        powerLblRt.anchorMax = new Vector2(0f, 0f);
        powerLblRt.pivot = new Vector2(0f, 0f);
        powerLblRt.anchoredPosition = new Vector2(40f, 118f);

        var pad = ImageGo(root.transform, "SpinPad", _look.UiWhite, Vector2.zero, new Vector2(150, 150), new Color(0.92f, 0.92f, 0.9f, 0.95f));
        _spinPad = pad.GetComponent<RectTransform>();
        _spinPad.anchorMin = new Vector2(0f, 0f);
        _spinPad.anchorMax = new Vector2(0f, 0f);
        _spinPad.pivot = new Vector2(0f, 0f);
        _spinPad.anchoredPosition = new Vector2(40f, 170f);
        var knob = ImageGo(pad.transform, "Knob", _look.UiWhite, Vector2.zero, new Vector2(28, 28), new Color(0.75f, 0.12f, 0.12f));
        _spinKnob = knob.GetComponent<RectTransform>();
        var spin = pad.AddComponent<SpinPad>();
        spin.Pad = _spinPad;
        spin.Knob = _spinKnob;
        spin.OnChanged = v => _cue.English = v;

        var engLbl = Label(root.transform, "ENGLISH", 18, Vector2.zero, new Vector2(200, 28), TextAnchor.MiddleLeft, new Color(0.9f, 0.82f, 0.45f));
        var engRt = engLbl.GetComponent<RectTransform>();
        engRt.anchorMin = new Vector2(0f, 0f);
        engRt.anchorMax = new Vector2(0f, 0f);
        engRt.pivot = new Vector2(0f, 0f);
        engRt.anchoredPosition = new Vector2(40f, 328f);

        return root;
    }

    GameObject BuildOver()
    {
        var root = Panel("GameOver", new Color(0.02f, 0.03f, 0.03f, 0.78f));
        ImageGo(root.transform, "Card", _look.UiPanel, Vector2.zero, new Vector2(900, 360), Color.white);
        _overText = Label(root.transform, "Win", 42, new Vector2(0, 40), new Vector2(800, 120), TextAnchor.MiddleCenter, new Color(0.95f, 0.85f, 0.4f));
        MenuBtn(root.transform, "Back to menu", new Vector2(0, -90), () => _flow.ReturnToMenu());
        return root;
    }

    void OnDestroy()
    {
        if (_canvas != null)
        {
            Destroy(_canvas.gameObject);
        }
    }

    public void ShowMenu()
    {
        if (_menu != null)
        {
            _menu.SetActive(true);
        }

        if (_play != null)
        {
            _play.SetActive(false);
        }

        if (_over != null)
        {
            _over.SetActive(false);
        }
    }

    public void ShowMatch()
    {
        if (_menu != null)
        {
            _menu.SetActive(false);
        }

        if (_play != null)
        {
            _play.SetActive(true);
        }

        if (_over != null)
        {
            _over.SetActive(false);
        }

        if (_power != null)
        {
            _power.value = _cue.Power;
        }
    }

    public void ShowGameOver(string msg)
    {
        _menu.SetActive(false);
        _play.SetActive(true);
        _over.SetActive(true);
        _overText.text = msg;
    }

    public void SetStatus(string who, string status, string extra, bool placing)
    {
        if (_who != null)
        {
            _who.text = who;
        }

        if (_status != null)
        {
            _status.text = status;
        }

        if (_extra != null)
        {
            _extra.text = extra;
        }

        if (_placeBtn != null)
        {
            _placeBtn.SetActive(placing);
        }
    }

    GameObject Panel(string name, Color c)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_canvas.transform, false);
        var img = go.AddComponent<Image>();
        img.color = c;
        StretchOnRt(go.GetComponent<RectTransform>());
        return go;
    }

    static void StretchOnRt(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void StretchOn(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null)
        {
            rt = go.AddComponent<RectTransform>();
        }

        StretchOnRt(rt);
    }

    Text Label(Transform parent, string text, int size, Vector2 pos, Vector2 sizeDelta, TextAnchor anchor = TextAnchor.MiddleCenter, Color? color = null)
    {
        var go = new GameObject("Label", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        var t = go.AddComponent<Text>();
        t.font = UiFont();
        t.fontSize = size;
        t.color = color ?? Color.white;
        t.alignment = anchor;
        t.text = text;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.fontStyle = FontStyle.Bold;
        t.raycastTarget = false;
        return t;
    }

    GameObject MenuBtn(Transform parent, string text, Vector2 pos, UnityEngine.Events.UnityAction click, bool gold = false)
    {
        var go = ImageGo(parent, text, gold ? _look.UiGold : _look.UiButton, pos, new Vector2(560, 78), Color.white);
        var b = go.AddComponent<Button>();
        b.targetGraphic = go.GetComponent<Image>();
        b.onClick.AddListener(click);
        var t = Label(go.transform, text, 26, Vector2.zero, new Vector2(540, 70));
        t.raycastTarget = false;
        t.color = gold ? new Color(0.12f, 0.08f, 0.04f) : Color.white;
        return go;
    }

    GameObject ImageGo(Transform parent, string name, Sprite sprite, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = color;
        return go;
    }

    GameObject Btn(Transform parent, string text, Vector2 pos, UnityEngine.Events.UnityAction click)
    {
        return MenuBtn(parent, text, pos, click);
    }

    static Font UiFont()
    {
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
               ?? Resources.GetBuiltinResource<Font>("Arial.ttf")
               ?? Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial" }, 16);
    }
}

public sealed class SpinPad : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform Pad;
    public RectTransform Knob;
    public System.Action<Vector2> OnChanged;

    public void OnPointerDown(PointerEventData eventData) => Apply(eventData);
    public void OnDrag(PointerEventData eventData) => Apply(eventData);

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    void Apply(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Pad, eventData.position, eventData.pressEventCamera, out var local);
        var half = Pad.sizeDelta * 0.5f;
        local.x = Mathf.Clamp(local.x, -half.x, half.x);
        local.y = Mathf.Clamp(local.y, -half.y, half.y);
        Knob.anchoredPosition = local;
        var v = new Vector2(local.x / half.x, local.y / half.y);
        OnChanged?.Invoke(v);
    }
}
