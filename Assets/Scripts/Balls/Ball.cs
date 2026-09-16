using System;
using UnityEngine;

public enum BallGroup
{
    Cue,
    Solid,
    Stripe,
    Eight
}

public sealed class Ball : MonoBehaviour
{
    public int Number { get; private set; }
    public BallGroup Group { get; private set; }
    public bool Pocketed { get; private set; }
    public Rigidbody Body { get; private set; }

    public event Action<Ball, int> PocketedIn;

    Vector3 _rackLocal;
    Transform _home;

    public void Setup(int number, Material material, PhysicMaterial physic)
    {
        Number = number;
        Group = number == 0 ? BallGroup.Cue : number == 8 ? BallGroup.Eight : number < 8 ? BallGroup.Solid : BallGroup.Stripe;
        Body = GetComponent<Rigidbody>();
        var col = GetComponent<SphereCollider>();
        col.sharedMaterial = physic;
        col.radius = 0.5f;
        var renderer = GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        gameObject.name = number == 0 ? "CueBall" : $"Ball_{number}";
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    public void RememberHome(Transform tableRoot, Vector3 worldPos)
    {
        _home = tableRoot;
        _rackLocal = tableRoot.InverseTransformPoint(worldPos);
    }

    public void RespawnOnTable(Vector3 worldPos)
    {
        Pocketed = false;
        gameObject.SetActive(true);
        Body.isKinematic = false;
        transform.position = worldPos;
        transform.rotation = Quaternion.identity;
        RbUtil.SleepHard(Body);
        Body.WakeUp();
    }

    public void RestoreRack()
    {
        var pos = _home != null ? _home.TransformPoint(_rackLocal) : transform.position;
        RespawnOnTable(pos);
    }

    public void PlaceAndFreeze(Vector3 worldPos)
    {
        Pocketed = false;
        gameObject.SetActive(true);
        transform.position = worldPos;
        Body.isKinematic = false;
        RbUtil.SleepHard(Body);
    }

    public void MarkPocketed(int pocketIndex)
    {
        if (Pocketed)
        {
            return;
        }

        Pocketed = true;
        RbUtil.SleepHard(Body);
        Body.isKinematic = true;
        gameObject.SetActive(false);
        PocketedIn?.Invoke(this, pocketIndex);
    }

    public bool IsMoving()
    {
        if (!isActiveAndEnabled || Pocketed || Body == null || Body.isKinematic)
        {
            return false;
        }

        return RbUtil.GetLinear(Body).sqrMagnitude > PoolConstants.SettleLinear * PoolConstants.SettleLinear
               || Body.angularVelocity.sqrMagnitude > PoolConstants.SettleAngular * PoolConstants.SettleAngular;
    }
}
