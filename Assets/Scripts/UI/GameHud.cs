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

    public void Build(MatchFlow flow, CueController cue)
    {
        _flow = flow;
        _cue = cue;
        var canvasGo = new GameObject("Canvas");
        canvasGo.transform.SetParent(transform, false);
        _canvas = canvasGo.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();

        _menu = BuildMenu();
        _play = BuildPlay();
        _over = BuildOver();
        ShowMenu();
    }

    GameObject BuildMenu()
    {
        var root = Panel("Menu", new Color(0.05f, 0.07f, 0.1f, 0.92f));
        Label(root.transform, "PocketShot", 72, new Vector2(0, 320), new Vector2(900, 120));
        Label(root.transform, "3D pool — US 8-ball & 9-ball", 32, new Vector2(0, 230), new Vector2(900, 50));
        Btn(root.transform, "8-ball · Pass & Play", new Vector2(0, 110), () => _flow.StartMatch(GameModeKind.EightBall, false, 1));
        Btn(root.transform, "8-ball · vs CPU (Easy)", new Vector2(0, 20), () => _flow.StartMatch(GameModeKind.EightBall, true, 0));
        Btn(root.transform, "8-ball · vs CPU (Hard)", new Vector2(0, -70), () => _flow.StartMatch(GameModeKind.EightBall, true, 2));
        Btn(root.transform, "9-ball · Pass & Play", new Vector2(0, -160), () => _flow.StartMatch(GameModeKind.NineBall, false, 1));
        Btn(root.transform, "9-ball · vs CPU", new Vector2(0, -250), () => _flow.StartMatch(GameModeKind.NineBall, true, 1));
        Btn(root.transform, "Quit", new Vector2(0, -360), () => Application.Quit());
        return root;
    }

    GameObject BuildPlay()
    {
        var root = new GameObject("PlayHud");
        root.transform.SetParent(_canvas.transform, false);
        StretchOn(root);

        _who = Label(root.transform, "Player 1", 36, new Vector2(-620, 460), new Vector2(600, 60), TextAnchor.MiddleLeft);
        _status = Label(root.transform, "Open table", 28, new Vector2(-620, 400), new Vector2(700, 40), TextAnchor.MiddleLeft);
        _extra = Label(root.transform, "", 24, new Vector2(0, 360), new Vector2(1000, 40));

        var shoot = Btn(root.transform, "SHOOT", new Vector2(720, -420), () => _flow.Shoot());
        shoot.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 120);

        _placeBtn = Btn(root.transform, "PLACE CUE", new Vector2(720, -280), () => _flow.ConfirmPlacement());
        _placeBtn.SetActive(false);

        Btn(root.transform, "Menu", new Vector2(820, 460), () => _flow.ReturnToMenu()).GetComponent<RectTransform>().sizeDelta = new Vector2(180, 70);

        Label(root.transform, "Power", 22, new Vector2(-720, -360), new Vector2(200, 30), TextAnchor.MiddleLeft);
        var sliderGo = new GameObject("Power");
        sliderGo.transform.SetParent(root.transform, false);
        var srt = sliderGo.AddComponent<RectTransform>();
        srt.anchoredPosition = new Vector2(-520, -430);
        srt.sizeDelta = new Vector2(520, 70);
        var bg = sliderGo.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.18f, 0.9f);
        _power = sliderGo.AddComponent<Slider>();
        _power.minValue = 0.05f;
        _power.maxValue = 1f;
        _power.value = 0.5f;
        var fill = new GameObject("Fill");
        fill.transform.SetParent(sliderGo.transform, false);
        var fr = fill.AddComponent<RectTransform>();
        fr.anchorMin = new Vector2(0, 0.2f);
        fr.anchorMax = new Vector2(1, 0.8f);
        fr.offsetMin = Vector2.zero;
        fr.offsetMax = Vector2.zero;
        var fi = fill.AddComponent<Image>();
        fi.color = new Color(0.95f, 0.7f, 0.15f);
        _power.fillRect = fr;
        _power.targetGraphic = bg;
        _power.onValueChanged.AddListener(v => _cue.Power = v);

        Label(root.transform, "English", 22, new Vector2(-720, -250), new Vector2(200, 30), TextAnchor.MiddleLeft);
        var pad = new GameObject("SpinPad");
        pad.transform.SetParent(root.transform, false);
        _spinPad = pad.AddComponent<RectTransform>();
        _spinPad.anchoredPosition = new Vector2(-720, -140);
        _spinPad.sizeDelta = new Vector2(160, 160);
        var padImg = pad.AddComponent<Image>();
        padImg.color = new Color(0.9f, 0.9f, 0.92f, 0.85f);
        var knob = new GameObject("Knob");
        knob.transform.SetParent(pad.transform, false);
        _spinKnob = knob.AddComponent<RectTransform>();
        _spinKnob.sizeDelta = new Vector2(28, 28);
        _spinKnob.anchoredPosition = Vector2.zero;
        knob.AddComponent<Image>().color = new Color(0.8f, 0.15f, 0.15f);
        var spin = pad.AddComponent<SpinPad>();
        spin.Pad = _spinPad;
        spin.Knob = _spinKnob;
        spin.OnChanged = v => _cue.English = v;

        return root;
    }

    GameObject BuildOver()
    {
        var root = Panel("GameOver", new Color(0.02f, 0.02f, 0.05f, 0.82f));
        _overText = Label(root.transform, "Win", 48, Vector2.zero, new Vector2(1000, 120));
        Btn(root.transform, "Back to menu", new Vector2(0, -160), () => _flow.ReturnToMenu());
        return root;
    }

    public void ShowMenu()
    {
        _menu.SetActive(true);
        _play.SetActive(false);
        _over.SetActive(false);
    }

    public void ShowMatch()
    {
        _menu.SetActive(false);
        _play.SetActive(true);
        _over.SetActive(false);
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
        var rt = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        StretchOnRt(rt);
    }

    Text Label(Transform parent, string text, int size, Vector2 pos, Vector2 sizeDelta, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        var t = go.AddComponent<Text>();
        t.font = UiFont();
        t.fontSize = size;
        t.color = Color.white;
        t.alignment = anchor;
        t.text = text;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        return t;
    }

    GameObject Btn(Transform parent, string text, Vector2 pos, UnityEngine.Events.UnityAction click)
    {
        var go = new GameObject(text);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(520, 80);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.12f, 0.45f, 0.28f, 0.95f);
        var b = go.AddComponent<Button>();
        b.targetGraphic = img;
        b.onClick.AddListener(click);
        var t = Label(go.transform, text, 28, Vector2.zero, new Vector2(500, 70));
        t.raycastTarget = false;
        return go;
    }

    static Font UiFont()
    {
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
               ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
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
