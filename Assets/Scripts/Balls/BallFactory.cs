using UnityEngine;

public static class BallFactory
{
    public static Ball Create(int number, Transform parent, PhysicsMaterial ballPhysic, LookLibrary look)
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
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.maxAngularVelocity = 40f;
        rb.sleepThreshold = 0.005f;
        rb.maxDepenetrationVelocity = 0.45f;
        var ball = go.AddComponent<Ball>();
        ball.Setup(number, look.Ball(number), ballPhysic);
        rb.isKinematic = true;
        go.SetActive(false);
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
        var d = PoolConstants.BallDiameter * 1.06f;
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
        var d = PoolConstants.BallDiameter * 1.06f;
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
}
