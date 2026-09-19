using UnityEngine;

public sealed class LookLibrary
{
    public Material Lit;
    public Material Felt;
    public Material Wood;
    public Material Cushion;
    public Material Pocket;
    public Material Brass;
    public Material Floor;
    public Material Wall;
    public Material CueWood;
    public Material CueButt;
    public Material Ferrule;
    public Material Tip;
    public Material Diamond;
    public Sprite UiPanel;
    public Sprite UiButton;
    public Sprite UiGold;
    public Sprite UiWhite;

    public static LookLibrary Create()
    {
        var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
        var look = new LookLibrary { Lit = new Material(sh) };
        var woodA = Load("Wood_Albedo");
        var woodN = Load("Wood_Normal");
        var fabricN = Load("Fabric_Normal");
        var metalA = Load("Metal_Albedo");
        var metalN = Load("Metal_Normal");

        look.Felt = look.Make(new Color(0.1f, 0.46f, 0.24f), 0.11f, 0f, FeltTex(), fabricN, new Vector2(4.5f, 7f));
        look.Wood = look.Make(Color.white, 0.32f, 0f, woodA ?? WoodTex(), woodN, new Vector2(1.6f, 2.4f));
        look.Cushion = look.Make(new Color(0.07f, 0.34f, 0.18f), 0.16f, 0f, FeltTex(), fabricN, new Vector2(3f, 3f));
        look.Pocket = look.Make(new Color(0.04f, 0.03f, 0.03f), 0.12f, 0.05f, null);
        look.Brass = look.Make(new Color(0.85f, 0.68f, 0.28f), 0.55f, 0.72f, metalA, metalN, new Vector2(2f, 2f));
        look.Floor = look.Make(new Color(0.45f, 0.38f, 0.3f), 0.22f, 0f, woodA ?? WoodTex(), woodN, new Vector2(6f, 8f));
        look.Wall = look.Make(new Color(0.07f, 0.08f, 0.1f), 0.15f, 0f, null);
        look.CueWood = look.Make(new Color(1f, 0.92f, 0.78f), 0.42f, 0f, woodA ?? WoodTex(), woodN, new Vector2(2f, 8f));
        look.CueButt = look.Make(new Color(0.35f, 0.18f, 0.08f), 0.3f, 0f, woodA ?? WoodTex(), woodN, new Vector2(1f, 4f));
        look.Ferrule = look.Make(new Color(0.92f, 0.92f, 0.9f), 0.7f, 0.25f, metalA, metalN);
        look.Tip = look.Make(new Color(0.15f, 0.35f, 0.7f), 0.2f, 0f, fabricN);
        look.Diamond = look.Make(new Color(0.95f, 0.9f, 0.75f), 0.8f, 0.2f, metalA, metalN);
        look.UiPanel = RoundSprite(64, 64, 10, new Color(0.04f, 0.07f, 0.06f, 0.94f));
        look.UiButton = RoundSprite(64, 64, 12, new Color(0.1f, 0.38f, 0.22f, 1f));
        look.UiGold = RoundSprite(64, 64, 12, new Color(0.72f, 0.55f, 0.22f, 1f));
        look.UiWhite = RoundSprite(32, 32, 16, Color.white);
        return look;
    }

    public Material Ball(int number)
    {
        var m = Make(Color.white, 0.82f, 0.04f, BallTex(number));
        return m;
    }

    static Texture2D Load(string name)
    {
        return Resources.Load<Texture2D>("Look/" + name);
    }

    Material Make(Color color, float smooth, float metal, Texture albedo, Texture normal = null, Vector2? tile = null)
    {
        var m = new Material(Lit.shader);
        if (m.HasProperty("_BaseColor"))
        {
            m.SetColor("_BaseColor", color);
        }

        if (m.HasProperty("_Color"))
        {
            m.SetColor("_Color", color);
        }

        if (m.HasProperty("_Smoothness"))
        {
            m.SetFloat("_Smoothness", smooth);
        }

        if (m.HasProperty("_Metallic"))
        {
            m.SetFloat("_Metallic", metal);
        }

        var scale = tile ?? Vector2.one;
        if (albedo != null)
        {
            if (m.HasProperty("_BaseMap"))
            {
                m.SetTexture("_BaseMap", albedo);
                m.SetTextureScale("_BaseMap", scale);
            }

            if (m.HasProperty("_MainTex"))
            {
                m.SetTexture("_MainTex", albedo);
                m.SetTextureScale("_MainTex", scale);
            }
        }

        if (normal != null)
        {
            if (m.HasProperty("_BumpMap"))
            {
                m.SetTexture("_BumpMap", normal);
                m.SetTextureScale("_BumpMap", scale);
            }

            if (m.HasProperty("_BumpScale"))
            {
                m.SetFloat("_BumpScale", 1f);
            }

            m.EnableKeyword("_NORMALMAP");
        }

        return m;
    }

    static Texture2D FeltTex()
    {
        const int s = 256;
        var t = new Texture2D(s, s, TextureFormat.RGBA32, true);
        t.wrapMode = TextureWrapMode.Repeat;
        var pixels = new Color[s * s];
        for (var y = 0; y < s; y++)
        {
            for (var x = 0; x < s; x++)
            {
                var n = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                var n2 = Mathf.PerlinNoise(x * 0.35f + 20f, y * 0.35f);
                var g = 0.34f + n * 0.12f + n2 * 0.05f;
                var edge = Mathf.Min(x, y, s - 1 - x, s - 1 - y) / (s * 0.5f);
                g *= 0.78f + 0.22f * Mathf.Clamp01(edge * 2f);
                pixels[y * s + x] = new Color(0.05f + g * 0.08f, g, 0.12f + g * 0.25f, 1f);
            }
        }

        t.SetPixels(pixels);
        t.Apply();
        t.filterMode = FilterMode.Bilinear;
        return t;
    }

    static Texture2D WoodTex()
    {
        const int s = 256;
        var t = new Texture2D(s, s, TextureFormat.RGBA32, true);
        t.wrapMode = TextureWrapMode.Repeat;
        var pixels = new Color[s * s];
        for (var y = 0; y < s; y++)
        {
            for (var x = 0; x < s; x++)
            {
                var grain = Mathf.PerlinNoise(x * 0.015f, y * 0.12f);
                var ring = Mathf.Sin((x + grain * 18f) * 0.11f) * 0.5f + 0.5f;
                var r = 0.28f + ring * 0.18f + grain * 0.06f;
                pixels[y * s + x] = new Color(r, r * 0.55f, r * 0.22f, 1f);
            }
        }

        t.SetPixels(pixels);
        t.Apply();
        return t;
    }

    static readonly Color[] Palette =
    {
        Color.white,
        new Color(1f, 0.82f, 0.08f),
        new Color(0.12f, 0.28f, 0.78f),
        new Color(0.82f, 0.1f, 0.1f),
        new Color(0.45f, 0.12f, 0.55f),
        new Color(0.92f, 0.42f, 0.06f),
        new Color(0.08f, 0.48f, 0.18f),
        new Color(0.42f, 0.07f, 0.07f),
        new Color(0.06f, 0.06f, 0.06f)
    };

    static Texture2D BallTex(int number)
    {
        const int w = 256;
        const int h = 128;
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.wrapMode = TextureWrapMode.Repeat;
        Color baseColor;
        var stripe = number >= 9;
        if (number == 0)
        {
            baseColor = new Color(0.96f, 0.96f, 0.94f);
        }
        else if (number == 8)
        {
            baseColor = Palette[8];
        }
        else if (number < 8)
        {
            baseColor = Palette[number];
        }
        else
        {
            baseColor = Palette[number - 8];
        }

        var pixels = new Color[w * h];
        for (var y = 0; y < h; y++)
        {
            var v = y / (h - 1f);
            for (var x = 0; x < w; x++)
            {
                var c = baseColor;
                if (stripe && v > 0.36f && v < 0.64f)
                {
                    c = Color.white;
                }

                if (number == 0)
                {
                    var spec = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(v - 0.32f) * 3f), 4f) * 0.12f;
                    c += new Color(spec, spec, spec);
                }

                pixels[y * w + x] = c;
            }
        }

        if (number > 0)
        {
            StampNumber(pixels, w, h, number);
        }

        t.SetPixels(pixels);
        t.Apply();
        t.filterMode = FilterMode.Bilinear;
        return t;
    }

    static void StampNumber(Color[] px, int w, int h, int number)
    {
        var cx = (int)(w * 0.25f);
        var cy = h / 2;
        var rad = 18;
        for (var y = -rad; y <= rad; y++)
        {
            for (var x = -rad; x <= rad; x++)
            {
                if (x * x + y * y > rad * rad)
                {
                    continue;
                }

                Set(px, w, h, cx + x, cy + y, Color.white);
            }
        }

        var label = number.ToString();
        var ink = number == 8 ? Color.black : new Color(0.08f, 0.08f, 0.1f);
        var start = cx - (label.Length * 8 - 2) / 2;
        for (var i = 0; i < label.Length; i++)
        {
            DrawDigit(px, w, h, start + i * 8, cy - 6, label[i] - '0', ink);
        }
    }

    static readonly int[] Font =
    {
        0b111101101101111, // 0
        0b010010010010010, // 1
        0b111001111100111, // 2
        0b111001111001111, // 3
        0b101101111001001, // 4
        0b111100111001111, // 5
        0b111100111101111, // 6
        0b111001001001001, // 7
        0b111101111101111, // 8
        0b111101111001111  // 9
    };

    static void DrawDigit(Color[] px, int w, int h, int ox, int oy, int digit, Color ink)
    {
        digit = Mathf.Clamp(digit, 0, 9);
        var bits = Font[digit];
        for (var row = 0; row < 5; row++)
        {
            for (var col = 0; col < 3; col++)
            {
                var on = ((bits >> ((4 - row) * 3 + (2 - col))) & 1) == 1;
                if (!on)
                {
                    continue;
                }

                for (var yy = 0; yy < 2; yy++)
                {
                    for (var xx = 0; xx < 2; xx++)
                    {
                        Set(px, w, h, ox + col * 2 + xx, oy + row * 2 + yy, ink);
                    }
                }
            }
        }
    }

    static void Set(Color[] px, int w, int h, int x, int y, Color c)
    {
        if ((uint)x >= (uint)w || (uint)y >= (uint)h)
        {
            return;
        }

        px[y * w + x] = c;
    }

    static Sprite RoundSprite(int w, int h, int radius, Color color)
    {
        var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
        t.wrapMode = TextureWrapMode.Clamp;
        var px = new Color[w * h];
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                var dx = Mathf.Min(x, w - 1 - x);
                var dy = Mathf.Min(y, h - 1 - y);
                float a;
                if (dx >= radius || dy >= radius)
                {
                    a = 1f;
                }
                else
                {
                    var d = Vector2.Distance(new Vector2(dx, dy), new Vector2(radius, radius));
                    a = Mathf.Clamp01(radius - d + 0.5f);
                }

                px[y * w + x] = new Color(color.r, color.g, color.b, color.a * a);
            }
        }

        t.SetPixels(px);
        t.Apply();
        t.filterMode = FilterMode.Bilinear;
        return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }
}
