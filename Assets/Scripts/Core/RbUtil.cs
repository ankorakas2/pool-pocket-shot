using UnityEngine;

public static class RbUtil
{
    public static Vector3 GetLinear(Rigidbody rb)
    {
#if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity;
#else
        return rb.velocity;
#endif
    }

    public static void SetLinear(Rigidbody rb, Vector3 v)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = v;
#else
        rb.velocity = v;
#endif
    }

    public static void SleepHard(Rigidbody rb)
    {
        SetLinear(rb, Vector3.zero);
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();
    }
}
