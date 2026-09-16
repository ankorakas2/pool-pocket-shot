using System.Collections.Generic;
using UnityEngine;

public sealed class PhysicsSettledDetector : MonoBehaviour
{
    readonly List<Ball> _balls = new List<Ball>();
    float _quiet;

    public bool Settled { get; private set; }

    public void Bind(IEnumerable<Ball> balls)
    {
        _balls.Clear();
        _balls.AddRange(balls);
        ResetWatch();
    }

    public void ResetWatch()
    {
        Settled = false;
        _quiet = 0f;
    }

    public void Tick(float dt)
    {
        var moving = false;
        for (var i = 0; i < _balls.Count; i++)
        {
            var b = _balls[i];
            if (b != null && b.IsMoving())
            {
                moving = true;
                break;
            }
        }

        if (moving)
        {
            _quiet = 0f;
            Settled = false;
            return;
        }

        _quiet += dt;
        if (_quiet >= PoolConstants.SettleHold)
        {
            Settled = true;
            for (var i = 0; i < _balls.Count; i++)
            {
                var b = _balls[i];
                if (b != null && b.Body != null && b.isActiveAndEnabled && !b.Pocketed)
                {
                    RbUtil.SleepHard(b.Body);
                }
            }
        }
    }
}
