using UnityEngine;

public static class BallFactory
{
    static readonly Color[] SolidColors =
    {
        Color.white,
        new Color(1f, 0.85f, 0.1f),
        new Color(0.15f, 0.35f, 0.85f),
        new Color(0.85f, 0.12f, 0.12f),
        new Color(0.45f, 0.12f, 0.55f),
        new Color(0.95f, 0.45f, 0.08f),
        new Color(0.08f, 0.45f, 0.18f),
        new Color(0.45f, 0.08f, 0.08f),
        new Color(0.06f, 0.06f, 0.06f)
    };

    public static Ball Create(int number, Transform parent, PhysicsMaterial ballPhysic, Shader lit)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.SetParent(parent, false);
        go.transform.localScale = Vector3.one * PoolConstants.BallDiameter;
        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 0.17f;
#if UNITY_6000_0_OR_NEWER
        rb.linearDamping = 0.35f;
        rb.angularDamping = 0.55f;
#else
        rb.drag = 0.35f;
        rb.angularDrag = 0.55f;
#endif
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.maxAngularVelocity = 40f;
        rb.sleepThreshold = 0.01f;
        var ball = go.AddComponent<Ball>();
        ball.Setup(number, MakeMaterial(number, lit), ballPhysic);
        return ball;
    }

    public static void RackEightBall(Ball[] balls, Transform table)
    {
        foreach (var b in balls)
        {
            if (b != null)
            {
                b.gameObject.SetActive(false);
            }
        }

        var cue = balls[0];
        cue.RememberHome(table, PoolConstants.HeadSpot);
        cue.RespawnOnTable(PoolConstants.HeadSpot);

        var apex = PoolConstants.FootSpot;
        var d = PoolConstants.BallDiameter * 1.02f;
        var rows = new[]
        {
            new[] { 1 },
            new[] { 14, 2 },
            new[] { 3, 8, 15 },
            new[] { 13, 4, 9, 5 },
            new[] { 6, 12, 7, 10, 11 }
        };

        for (var r = 0; r < rows.Length; r++)
        {
            var count = rows[r].Length;
            var z = apex.z + r * d * 0.8660254f;
            var startX = -(count - 1) * d * 0.5f;
            for (var i = 0; i < count; i++)
            {
                var n = rows[r][i];
                var pos = new Vector3(startX + i * d, PoolConstants.BallRadius, z);
                balls[n].RememberHome(table, pos);
                balls[n].RespawnOnTable(pos);
            }
        }
    }

    public static void RackNineBall(Ball[] balls, Transform table)
    {
        foreach (var b in balls)
        {
            if (b != null)
            {
                b.gameObject.SetActive(false);
            }
        }

        balls[0].RememberHome(table, PoolConstants.HeadSpot);
        balls[0].RespawnOnTable(PoolConstants.HeadSpot);

        var apex = PoolConstants.FootSpot;
        var d = PoolConstants.BallDiameter * 1.02f;
        int[][] diamond =
        {
            new[] { 1 },
            new[] { 2, 3 },
            new[] { 4, 9, 5 },
            new[] { 6, 7 },
            new[] { 8 }
        };

        for (var r = 0; r < diamond.Length; r++)
        {
            var count = diamond[r].Length;
            var z = apex.z + r * d * 0.8660254f;
            var startX = -(count - 1) * d * 0.5f;
            for (var i = 0; i < count; i++)
            {
                var n = diamond[r][i];
                var pos = new Vector3(startX + i * d, PoolConstants.BallRadius, z);
                balls[n].RememberHome(table, pos);
                balls[n].RespawnOnTable(pos);
            }
        }
    }

    static Material MakeMaterial(int number, Shader lit)
    {
        var m = new Material(lit);
        Color color;
        if (number == 0)
        {
            color = Color.white;
        }
        else if (number == 8)
        {
            color = SolidColors[8];
        }
        else if (number < 8)
        {
            color = SolidColors[number];
        }
        else
        {
            color = Color.Lerp(SolidColors[number - 8], Color.white, 0.35f);
        }

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
            m.SetFloat("_Smoothness", 0.72f);
        }

        var tex = NumberTexture(number, color);
        if (m.HasProperty("_BaseMap"))
        {
            m.SetTexture("_BaseMap", tex);
        }

        if (m.HasProperty("_MainTex"))
        {
            m.SetTexture("_MainTex", tex);
        }

        return m;
    }

    static Texture2D NumberTexture(int number, Color baseColor)
    {
        const int s = 64;
        var t = new Texture2D(s, s, TextureFormat.RGBA32, false);
        t.wrapMode = TextureWrapMode.Clamp;
        var stripe = number >= 9;
        for (var y = 0; y < s; y++)
        {
            for (var x = 0; x < s; x++)
            {
                var c = baseColor;
                if (stripe && y > 18 && y < 46)
                {
                    c = Color.white;
                }

                var dx = x - s / 2;
                var dy = y - s / 2;
                if (dx * dx + dy * dy < 90)
                {
                    c = Color.white;
                }

                t.SetPixel(x, y, c);
            }
        }

        t.Apply();
        return t;
    }
}
