using UnityEngine;

public sealed class Table : MonoBehaviour
{
    public Vector3[] PocketCenters { get; private set; }
    public float[] PocketRadii { get; private set; }
    public Transform PlayRoot => transform;

    public void Build(PhysicMaterial cloth, PhysicMaterial cushion, Material feltMat, Material woodMat, Material pocketMat)
    {
        PocketCenters = new Vector3[6];
        PocketRadii = new float[6];

        var length = PoolConstants.PlayingLength;
        var width = PoolConstants.PlayingWidth;
        var rail = PoolConstants.RailWidth;
        var hx = width * 0.5f;
        var hz = length * 0.5f;

        var felt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        felt.name = "Felt";
        felt.transform.SetParent(transform, false);
        felt.transform.localScale = new Vector3(width + 0.02f, 0.04f, length + 0.02f);
        felt.transform.localPosition = new Vector3(0f, -0.02f, 0f);
        felt.GetComponent<MeshRenderer>().sharedMaterial = feltMat;
        DestroyCollider(felt);
        var feltCol = felt.AddComponent<BoxCollider>();
        feltCol.sharedMaterial = cloth;
        feltCol.size = Vector3.one;

        BuildFrame(hx, hz, rail, woodMat);

        var gap = PoolConstants.PocketGap;
        BuildCushion("Rail_PosX_Foot", new Vector3(hx + 0.027f, PoolConstants.CushionHeight * 0.5f, hz * 0.5f + 0.02f), new Vector3(0.055f, PoolConstants.CushionHeight, hz - gap), cushion);
        BuildCushion("Rail_PosX_Head", new Vector3(hx + 0.027f, PoolConstants.CushionHeight * 0.5f, -hz * 0.5f - 0.02f), new Vector3(0.055f, PoolConstants.CushionHeight, hz - gap), cushion);
        BuildCushion("Rail_NegX_Foot", new Vector3(-hx - 0.027f, PoolConstants.CushionHeight * 0.5f, hz * 0.5f + 0.02f), new Vector3(0.055f, PoolConstants.CushionHeight, hz - gap), cushion);
        BuildCushion("Rail_NegX_Head", new Vector3(-hx - 0.027f, PoolConstants.CushionHeight * 0.5f, -hz * 0.5f - 0.02f), new Vector3(0.055f, PoolConstants.CushionHeight, hz - gap), cushion);
        BuildCushion("Rail_PosZ", new Vector3(0f, PoolConstants.CushionHeight * 0.5f, hz + 0.027f), new Vector3(width - gap, PoolConstants.CushionHeight, 0.055f), cushion);
        BuildCushion("Rail_NegZ", new Vector3(0f, PoolConstants.CushionHeight * 0.5f, -hz - 0.027f), new Vector3(width - gap, PoolConstants.CushionHeight, 0.055f), cushion);

        Vector3[] pockets =
        {
            new Vector3(-hx, 0f, -hz),
            new Vector3(hx, 0f, -hz),
            new Vector3(-hx, 0f, 0f),
            new Vector3(hx, 0f, 0f),
            new Vector3(-hx, 0f, hz),
            new Vector3(hx, 0f, hz)
        };

        for (var i = 0; i < pockets.Length; i++)
        {
            var side = Mathf.Abs(pockets[i].z) < 0.01f;
            var radius = side ? PoolConstants.SidePocketRadius : PoolConstants.CornerPocketRadius;
            PocketCenters[i] = pockets[i] + Vector3.up * 0.01f;
            PocketRadii[i] = radius;
            var hole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hole.name = $"Pocket_{i}";
            hole.transform.SetParent(transform, false);
            hole.transform.localPosition = PocketCenters[i] + Vector3.down * 0.02f;
            hole.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            hole.transform.localScale = new Vector3(radius * 2.1f, 0.03f, radius * 2.1f);
            hole.GetComponent<MeshRenderer>().sharedMaterial = pocketMat;
            DestroyCollider(hole);
            var trigger = hole.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.55f;
            var pocket = hole.AddComponent<PocketTrigger>();
            pocket.Index = i;
        }

        var kitchen = new GameObject("KitchenLine");
        kitchen.transform.SetParent(transform, false);
        kitchen.transform.localPosition = new Vector3(0f, 0.001f, PoolConstants.HeadStringZ);
        var line = kitchen.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = false;
        line.SetPosition(0, new Vector3(-hx, 0.002f, 0f));
        line.SetPosition(1, new Vector3(hx, 0.002f, 0f));
        line.startWidth = 0.008f;
        line.endWidth = 0.008f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = new Color(1f, 1f, 1f, 0.35f);
        line.endColor = new Color(1f, 1f, 1f, 0.35f);
    }

    void BuildFrame(float hx, float hz, float rail, Material woodMat)
    {
        void Wood(string name, Vector3 pos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = woodMat;
            DestroyCollider(go);
        }

        var h = 0.08f;
        Wood("Wood_PosX", new Vector3(hx + rail * 0.5f, h * 0.5f, 0f), new Vector3(rail, h, PoolConstants.PlayingLength + rail * 2f));
        Wood("Wood_NegX", new Vector3(-hx - rail * 0.5f, h * 0.5f, 0f), new Vector3(rail, h, PoolConstants.PlayingLength + rail * 2f));
        Wood("Wood_PosZ", new Vector3(0f, h * 0.5f, hz + rail * 0.5f), new Vector3(PoolConstants.PlayingWidth + rail * 2f, h, rail));
        Wood("Wood_NegZ", new Vector3(0f, h * 0.5f, -hz - rail * 0.5f), new Vector3(PoolConstants.PlayingWidth + rail * 2f, h, rail));
    }

    void BuildCushion(string name, Vector3 pos, Vector3 scale, PhysicMaterial cushion)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.GetComponent<MeshRenderer>().sharedMaterial = go.GetComponent<MeshRenderer>().material;
        var col = go.GetComponent<BoxCollider>();
        col.sharedMaterial = cushion;
        var r = go.GetComponent<MeshRenderer>();
        r.sharedMaterial = CreateTint(new Color(0.05f, 0.28f, 0.14f));
    }

    static Material CreateTint(Color c)
    {
        var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(sh);
        if (m.HasProperty("_BaseColor"))
        {
            m.SetColor("_BaseColor", c);
        }

        if (m.HasProperty("_Color"))
        {
            m.SetColor("_Color", c);
        }

        return m;
    }

    static void DestroyCollider(GameObject go)
    {
        var c = go.GetComponent<Collider>();
        if (c != null)
        {
            Object.Destroy(c);
        }
    }

    public bool IsInKitchen(Vector3 worldPos)
    {
        return worldPos.z <= PoolConstants.HeadStringZ + 0.001f;
    }

    public bool IsOnCloth(Vector3 worldPos)
    {
        var hx = PoolConstants.PlayingWidth * 0.5f - PoolConstants.BallRadius;
        var hz = PoolConstants.PlayingLength * 0.5f - PoolConstants.BallRadius;
        return Mathf.Abs(worldPos.x) <= hx && Mathf.Abs(worldPos.z) <= hz;
    }

    public Vector3 ClampOnCloth(Vector3 worldPos, bool kitchenOnly)
    {
        var hx = PoolConstants.PlayingWidth * 0.5f - PoolConstants.BallRadius * 1.2f;
        var hz = PoolConstants.PlayingLength * 0.5f - PoolConstants.BallRadius * 1.2f;
        worldPos.x = Mathf.Clamp(worldPos.x, -hx, hx);
        worldPos.z = Mathf.Clamp(worldPos.z, -hz, hz);
        worldPos.y = PoolConstants.BallRadius;
        if (kitchenOnly)
        {
            worldPos.z = Mathf.Min(worldPos.z, PoolConstants.HeadStringZ);
        }

        return worldPos;
    }

    public int ClosestPocket(Vector3 worldPos)
    {
        var best = 0;
        var bestD = float.MaxValue;
        for (var i = 0; i < PocketCenters.Length; i++)
        {
            var d = (PocketCenters[i] - worldPos).sqrMagnitude;
            if (d < bestD)
            {
                bestD = d;
                best = i;
            }
        }

        return best;
    }
}

public sealed class PocketTrigger : MonoBehaviour
{
    public int Index;
    public event System.Action<Ball, int> BallEntered;

    void OnTriggerEnter(Collider other)
    {
        var ball = other.GetComponentInParent<Ball>();
        ForceEnter(ball);
    }

    public void ForceEnter(Ball ball)
    {
        if (ball != null && !ball.Pocketed)
        {
            BallEntered?.Invoke(ball, Index);
        }
    }
}
