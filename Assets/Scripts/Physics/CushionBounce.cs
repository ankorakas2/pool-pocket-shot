using UnityEngine;

public sealed class CushionBounce : MonoBehaviour
{
    const float DeadSpeed = 0.028f;
    const float PhysxHandlesAbove = 0.42f;
    const float Restitution = 0.8f;
    const float MinRebound = 0.09f;

    void OnCollisionEnter(Collision collision)
    {
        Kick(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        Kick(collision);
    }

    static void Kick(Collision collision)
    {
        var body = collision.rigidbody;
        if (body == null || body.isKinematic)
        {
            return;
        }

        var n = Vector3.zero;
        var count = collision.contactCount;
        for (var i = 0; i < count; i++)
        {
            var c = collision.GetContact(i);
            var away = body.worldCenterOfMass - c.point;
            away.y = 0f;
            if (away.sqrMagnitude > 1e-8f)
            {
                n += away.normalized;
            }
        }

        n.y = 0f;
        if (n.sqrMagnitude < 0.0001f)
        {
            return;
        }

        n.Normalize();
        var v = RbUtil.GetLinear(body);
        var planar = new Vector3(v.x, 0f, v.z);
        if (planar.sqrMagnitude < DeadSpeed * DeadSpeed)
        {
            return;
        }

        var into = Vector3.Dot(v, n);
        if (into > 0.05f)
        {
            return;
        }

        var approach = -into;
        if (approach > PhysxHandlesAbove)
        {
            return;
        }

        var tangent = v - n * into;
        var bounce = Mathf.Max(MinRebound, approach * Restitution);
        var next = tangent * 0.94f + n * bounce;
        next.y = v.y;
        body.WakeUp();
        RbUtil.SetLinear(body, next);
    }
}
