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

        var sideMouth = 0.09f;
        var cornerMouth = 0.094f;
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
            BuildPocket(i, pockets[i], side, radius, rail, look);
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

        var h = 0.05f;
        var cornerGap = 0.13f;
        var sideGap = 0.1f;
        var longLen = hz - cornerGap - sideGap;
        var headZ = -(hz - cornerGap + sideGap) * 0.5f;
        var footZ = (hz - cornerGap + sideGap) * 0.5f;
        Wood("Wood_PosX_Head", new Vector3(hx + rail * 0.5f, h * 0.5f, headZ), new Vector3(rail, h, longLen));
        Wood("Wood_PosX_Foot", new Vector3(hx + rail * 0.5f, h * 0.5f, footZ), new Vector3(rail, h, longLen));
        Wood("Wood_NegX_Head", new Vector3(-hx - rail * 0.5f, h * 0.5f, headZ), new Vector3(rail, h, longLen));
        Wood("Wood_NegX_Foot", new Vector3(-hx - rail * 0.5f, h * 0.5f, footZ), new Vector3(rail, h, longLen));
        var endLen = (hx - cornerGap) * 2f;
        Wood("Wood_PosZ", new Vector3(0f, h * 0.5f, hz + rail * 0.5f), new Vector3(endLen, h, rail));
        Wood("Wood_NegZ", new Vector3(0f, h * 0.5f, -hz - rail * 0.5f), new Vector3(endLen, h, rail));

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
        BuildCushion(name, new Vector3(x, PoolConstants.BallRadius, (zMin + zMax) * 0.5f),
            new Vector3(0.05f, PoolConstants.CushionHeight, Mathf.Abs(zMax - zMin)), cushion, mat);
        Jaw(new Vector3(x, PoolConstants.BallRadius, zMin), mat);
        Jaw(new Vector3(x, PoolConstants.BallRadius, zMax), mat);
    }

    void EndCushion(string name, float z, float xMin, float xMax, PhysicsMaterial cushion, Material mat)
    {
        BuildCushion(name, new Vector3((xMin + xMax) * 0.5f, PoolConstants.BallRadius, z),
            new Vector3(Mathf.Abs(xMax - xMin), PoolConstants.CushionHeight, 0.05f), cushion, mat);
        Jaw(new Vector3(xMin, PoolConstants.BallRadius, z), mat);
        Jaw(new Vector3(xMax, PoolConstants.BallRadius, z), mat);
    }

    void Jaw(Vector3 pos, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "CushionJaw";
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = new Vector3(0.034f, PoolConstants.CushionHeight * 0.5f, 0.034f);
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        DestroyCollider(go);
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
        go.AddComponent<CushionBounce>();
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    void BuildPocket(int index, Vector3 pos, bool side, float radius, float rail, LookLibrary look)
    {
        PocketCenters[index] = pos + Vector3.up * 0.008f;
        PocketRadii[index] = radius * 0.82f;

        var mouth = side ? 0.07f : 0.076f;
        var depth = 0.12f;
        var segs = 28;
        var root = new GameObject($"Pocket_{index}");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = pos;

        Visual("HoleMouth", root.transform, PocketMesh.Disc(mouth, segs, true), look.Hole,
            new Vector3(0f, 0.002f, 0f), Quaternion.identity, false);
        Visual("HoleFloor", root.transform, PocketMesh.Disc(mouth * 0.98f, segs, true), look.Hole,
            new Vector3(0f, -depth, 0f), Quaternion.identity, false);
        Visual("LeatherWall", root.transform, PocketMesh.Tube(mouth, depth, segs), look.Leather,
            new Vector3(0f, 0.002f, 0f), Quaternion.identity, true);
        Visual("LeatherLip", root.transform, PocketMesh.Ring(mouth * 0.96f, mouth * 1.18f, segs), look.Leather,
            new Vector3(0f, 0.006f, 0f), Quaternion.identity, true);
        Visual("MetalRim", root.transform, PocketMesh.Ring(mouth * 1.14f, mouth * 1.34f, segs), look.PocketRim,
            new Vector3(0f, 0.011f, 0f), Quaternion.identity, true);

        Vector3 outward;
        if (side)
        {
            outward = new Vector3(Mathf.Sign(pos.x), 0f, 0f);
        }
        else
        {
            outward = new Vector3(Mathf.Sign(pos.x), 0f, Mathf.Sign(pos.z)).normalized;
        }

        var h = 0.05f;
        var wrap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wrap.name = "PocketWood";
        wrap.transform.SetParent(root.transform, false);
        wrap.transform.localPosition = outward * (mouth + 0.058f) + Vector3.up * (h * 0.5f);
        wrap.transform.localScale = new Vector3(rail * 0.72f, h * 0.5f, rail * 0.72f);
        wrap.GetComponent<MeshRenderer>().sharedMaterial = look.Wood;
        DestroyCollider(wrap);

        var triggerGo = new GameObject($"PocketTrigger_{index}");
        triggerGo.transform.SetParent(transform, false);
        triggerGo.transform.localPosition = PocketCenters[index];
        var trigger = triggerGo.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = radius * 0.78f;
        trigger.contactOffset = 0.0008f;
        var pocket = triggerGo.AddComponent<PocketTrigger>();
        pocket.Index = index;
    }

    static void Visual(string name, Transform parent, Mesh mesh, Material mat, Vector3 localPos, Quaternion localRot, bool castShadows)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = localRot;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var rend = go.AddComponent<MeshRenderer>();
        rend.sharedMaterial = mat;
        rend.shadowCastingMode = castShadows
            ? UnityEngine.Rendering.ShadowCastingMode.On
            : UnityEngine.Rendering.ShadowCastingMode.Off;
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
