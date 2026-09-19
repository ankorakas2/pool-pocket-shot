using UnityEngine;

public sealed class Table : MonoBehaviour
{
    public Vector3[] PocketCenters { get; private set; }
    public float[] PocketRadii { get; private set; }
    public Transform PlayRoot => transform;

    public void Build(PhysicsMaterial cloth, PhysicsMaterial cushion, LookLibrary look)
    {
        PocketCenters = new Vector3[6];
        PocketRadii = new float[6];

        var length = PoolConstants.PlayingLength;
        var width = PoolConstants.PlayingWidth;
        var rail = PoolConstants.RailWidth;
        var hx = width * 0.5f;
        var hz = length * 0.5f;

        BuildRoom(look);

        var felt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        felt.name = "Felt";
        felt.transform.SetParent(transform, false);
        felt.transform.localScale = new Vector3(width + 0.02f, 0.04f, length + 0.02f);
        felt.transform.localPosition = new Vector3(0f, -0.02f, 0f);
        felt.GetComponent<MeshRenderer>().sharedMaterial = look.Felt;
        DestroyCollider(felt);
        var feltCol = felt.AddComponent<BoxCollider>();
        feltCol.sharedMaterial = cloth;
        feltCol.size = Vector3.one;
        feltCol.contactOffset = 0.0008f;

        BuildFrame(hx, hz, rail, look);
        BuildLegs(hx, hz, rail, look.Wood);

        var sideMouth = PoolConstants.SidePocketRadius * 1.12f;
        var cornerMouth = PoolConstants.CornerPocketRadius * 1.08f;
        LongCushion("Rail_PosX_Foot", hx + 0.027f, sideMouth, hz - cornerMouth, cushion, look.Cushion);
        LongCushion("Rail_PosX_Head", hx + 0.027f, -(hz - cornerMouth), -sideMouth, cushion, look.Cushion);
        LongCushion("Rail_NegX_Foot", -hx - 0.027f, sideMouth, hz - cornerMouth, cushion, look.Cushion);
        LongCushion("Rail_NegX_Head", -hx - 0.027f, -(hz - cornerMouth), -sideMouth, cushion, look.Cushion);
        EndCushion("Rail_PosZ", hz + 0.027f, -(hx - cornerMouth), hx - cornerMouth, cushion, look.Cushion);
        EndCushion("Rail_NegZ", -hz - 0.027f, -(hx - cornerMouth), hx - cornerMouth, cushion, look.Cushion);

        var cout = PoolConstants.CornerPocketOut;
        var sout = PoolConstants.SidePocketOut;
        Vector3[] pockets =
        {
            new Vector3(-hx - cout, 0f, -hz - cout),
            new Vector3(hx + cout, 0f, -hz - cout),
            new Vector3(-hx - sout, 0f, 0f),
            new Vector3(hx + sout, 0f, 0f),
            new Vector3(-hx - cout, 0f, hz + cout),
            new Vector3(hx + cout, 0f, hz + cout)
        };

        for (var i = 0; i < pockets.Length; i++)
        {
            var side = Mathf.Abs(pockets[i].z) < 0.01f;
            var radius = side ? PoolConstants.SidePocketRadius : PoolConstants.CornerPocketRadius;
            PocketCenters[i] = pockets[i] + Vector3.up * 0.01f;
            PocketRadii[i] = radius * 0.8f;
            var hole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hole.name = $"Pocket_{i}";
            hole.transform.SetParent(transform, false);
            hole.transform.localPosition = PocketCenters[i] + Vector3.down * 0.02f;
            hole.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            hole.transform.localScale = new Vector3(radius * 1.85f, 0.03f, radius * 1.85f);
            hole.GetComponent<MeshRenderer>().sharedMaterial = look.Pocket;
            DestroyCollider(hole);

            var triggerGo = new GameObject($"PocketTrigger_{i}");
            triggerGo.transform.SetParent(transform, false);
            triggerGo.transform.localPosition = PocketCenters[i];
            triggerGo.transform.localScale = Vector3.one;
            var trigger = triggerGo.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = radius * 0.72f;
            trigger.contactOffset = 0.0008f;
            var pocket = triggerGo.AddComponent<PocketTrigger>();
            pocket.Index = i;

            var rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = $"PocketRim_{i}";
            rim.transform.SetParent(transform, false);
            rim.transform.localPosition = PocketCenters[i] + Vector3.up * 0.012f;
            rim.transform.localScale = new Vector3(radius * 2.15f, 0.008f, radius * 2.15f);
            rim.GetComponent<MeshRenderer>().sharedMaterial = look.Brass;
            DestroyCollider(rim);
        }

        var kitchen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        kitchen.name = "KitchenLine";
        kitchen.transform.SetParent(transform, false);
        kitchen.transform.localPosition = new Vector3(0f, 0.0015f, PoolConstants.HeadStringZ);
        kitchen.transform.localScale = new Vector3(width - 0.08f, 0.001f, 0.006f);
        kitchen.GetComponent<MeshRenderer>().sharedMaterial = look.Diamond;
        DestroyCollider(kitchen);
    }

    void BuildRoom(LookLibrary look)
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "RoomFloor";
        floor.transform.SetParent(transform, false);
        floor.transform.localPosition = new Vector3(0f, -0.72f, 0f);
        floor.transform.localScale = new Vector3(8f, 0.08f, 10f);
        floor.GetComponent<MeshRenderer>().sharedMaterial = look.Floor;
        DestroyCollider(floor);

        void Wall(string name, Vector3 pos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = look.Wall;
            DestroyCollider(go);
        }

        Wall("Wall_N", new Vector3(0f, 1.8f, 4.6f), new Vector3(8f, 5.2f, 0.12f));
        Wall("Wall_S", new Vector3(0f, 1.8f, -4.6f), new Vector3(8f, 5.2f, 0.12f));
        Wall("Wall_E", new Vector3(3.9f, 1.8f, 0f), new Vector3(0.12f, 5.2f, 9.2f));
        Wall("Wall_W", new Vector3(-3.9f, 1.8f, 0f), new Vector3(0.12f, 5.2f, 9.2f));

        var ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(transform, false);
        ceiling.transform.localPosition = new Vector3(0f, 4.5f, 0f);
        ceiling.transform.localScale = new Vector3(8f, 0.08f, 10f);
        ceiling.GetComponent<MeshRenderer>().sharedMaterial = look.Wall;
        DestroyCollider(ceiling);

        var lamp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lamp.name = "LampShade";
        lamp.transform.SetParent(transform, false);
        lamp.transform.localPosition = new Vector3(0f, 1.85f, 0f);
        lamp.transform.localScale = new Vector3(0.55f, 0.08f, 0.55f);
        lamp.GetComponent<MeshRenderer>().sharedMaterial = look.Brass;
        DestroyCollider(lamp);
    }

    void BuildFrame(float hx, float hz, float rail, LookLibrary look)
    {
        void Wood(string name, Vector3 pos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = look.Wood;
            DestroyCollider(go);
        }

        var h = 0.09f;
        Wood("Wood_PosX", new Vector3(hx + rail * 0.5f, h * 0.5f, 0f), new Vector3(rail, h, PoolConstants.PlayingLength + rail * 2f));
        Wood("Wood_NegX", new Vector3(-hx - rail * 0.5f, h * 0.5f, 0f), new Vector3(rail, h, PoolConstants.PlayingLength + rail * 2f));
        Wood("Wood_PosZ", new Vector3(0f, h * 0.5f, hz + rail * 0.5f), new Vector3(PoolConstants.PlayingWidth + rail * 2f, h, rail));
        Wood("Wood_NegZ", new Vector3(0f, h * 0.5f, -hz - rail * 0.5f), new Vector3(PoolConstants.PlayingWidth + rail * 2f, h, rail));

        void Diamond(Vector3 pos)
        {
            var d = GameObject.CreatePrimitive(PrimitiveType.Cube);
            d.name = "Diamond";
            d.transform.SetParent(transform, false);
            d.transform.localPosition = pos;
            d.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            d.transform.localScale = new Vector3(0.018f, 0.008f, 0.018f);
            d.GetComponent<MeshRenderer>().sharedMaterial = look.Diamond;
            DestroyCollider(d);
        }

        for (var i = -2; i <= 2; i++)
        {
            if (i == 0)
            {
                continue;
            }

            Diamond(new Vector3(hx + rail * 0.5f, h + 0.002f, i * hz * 0.4f));
            Diamond(new Vector3(-hx - rail * 0.5f, h + 0.002f, i * hz * 0.4f));
        }

        Diamond(new Vector3(0f, h + 0.002f, hz + rail * 0.5f));
        Diamond(new Vector3(0f, h + 0.002f, -hz - rail * 0.5f));
    }

    void BuildLegs(float hx, float hz, float rail, Material wood)
    {
        Vector3[] feet =
        {
            new Vector3(hx + rail * 0.15f, -0.36f, hz + rail * 0.15f),
            new Vector3(-hx - rail * 0.15f, -0.36f, hz + rail * 0.15f),
            new Vector3(hx + rail * 0.15f, -0.36f, -hz - rail * 0.15f),
            new Vector3(-hx - rail * 0.15f, -0.36f, -hz - rail * 0.15f)
        };
        foreach (var p in feet)
        {
            var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = "Leg";
            leg.transform.SetParent(transform, false);
            leg.transform.localPosition = p;
            leg.transform.localScale = new Vector3(0.09f, 0.72f, 0.09f);
            leg.GetComponent<MeshRenderer>().sharedMaterial = wood;
            DestroyCollider(leg);
        }
    }

    void LongCushion(string name, float x, float zMin, float zMax, PhysicsMaterial cushion, Material mat)
    {
        BuildCushion(name, new Vector3(x, PoolConstants.CushionHeight * 0.5f, (zMin + zMax) * 0.5f),
            new Vector3(0.055f, PoolConstants.CushionHeight, Mathf.Abs(zMax - zMin)), cushion, mat);
    }

    void EndCushion(string name, float z, float xMin, float xMax, PhysicsMaterial cushion, Material mat)
    {
        BuildCushion(name, new Vector3((xMin + xMax) * 0.5f, PoolConstants.CushionHeight * 0.5f, z),
            new Vector3(Mathf.Abs(xMax - xMin), PoolConstants.CushionHeight, 0.055f), cushion, mat);
    }

    void BuildCushion(string name, Vector3 pos, Vector3 scale, PhysicsMaterial cushion, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        var col = go.GetComponent<BoxCollider>();
        col.sharedMaterial = cushion;
        col.contactOffset = 0.0008f;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
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
        worldPos = ClampBounds(worldPos, kitchenOnly);
        worldPos = PushOutOfPockets(worldPos);
        return ClampBounds(worldPos, kitchenOnly);
    }

    Vector3 ClampBounds(Vector3 worldPos, bool kitchenOnly)
    {
        var hx = PoolConstants.PlayingWidth * 0.5f - PoolConstants.BallRadius * 1.35f;
        var hz = PoolConstants.PlayingLength * 0.5f - PoolConstants.BallRadius * 1.35f;
        worldPos.x = Mathf.Clamp(worldPos.x, -hx, hx);
        worldPos.z = Mathf.Clamp(worldPos.z, -hz, hz);
        worldPos.y = PoolConstants.BallRadius;
        if (kitchenOnly)
        {
            worldPos.z = Mathf.Min(worldPos.z, PoolConstants.HeadStringZ);
        }

        return worldPos;
    }

    public Vector3 PushOutOfPockets(Vector3 worldPos)
    {
        if (PocketCenters == null)
        {
            return worldPos;
        }

        for (var i = 0; i < PocketCenters.Length; i++)
        {
            var flat = worldPos - PocketCenters[i];
            flat.y = 0f;
            var need = PocketRadii[i] + PoolConstants.BallRadius * 1.45f;
            var dist = flat.magnitude;
            if (dist < need)
            {
                var dir = dist > 0.0001f ? flat / dist : (Vector3.zero - PocketCenters[i]);
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.0001f)
                {
                    dir = Vector3.back;
                }

                dir.Normalize();
                worldPos += dir * (need - dist + 0.004f);
                worldPos.y = PoolConstants.BallRadius;
            }
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
