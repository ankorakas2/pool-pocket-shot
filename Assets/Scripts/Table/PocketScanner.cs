using UnityEngine;

public sealed class PocketScanner : MonoBehaviour
{
    Table _table;
    Ball[] _balls;
    PocketTrigger[] _triggers;
    public bool IgnoreCueBall;

    public void Bind(Table table, Ball[] balls)
    {
        _table = table;
        _balls = balls;
        _triggers = table.GetComponentsInChildren<PocketTrigger>(true);
    }

    void FixedUpdate()
    {
        if (_table == null || _balls == null)
        {
            return;
        }

        for (var i = 0; i < _balls.Length; i++)
        {
            var ball = _balls[i];
            if (ball == null || ball.Pocketed || !ball.isActiveAndEnabled)
            {
                continue;
            }

            if (IgnoreCueBall && ball.Group == BallGroup.Cue)
            {
                continue;
            }

            var p = ball.transform.position;
            if (p.y > PoolConstants.BallRadius * 2.2f)
            {
                continue;
            }

            for (var k = 0; k < _table.PocketCenters.Length; k++)
            {
                var d = p - _table.PocketCenters[k];
                d.y = 0f;
                if (d.sqrMagnitude <= _table.PocketRadii[k] * _table.PocketRadii[k])
                {
                    for (var t = 0; t < _triggers.Length; t++)
                    {
                        if (_triggers[t].Index == k)
                        {
                            _triggers[t].ForceEnter(ball);
                            break;
                        }
                    }

                    break;
                }
            }
        }
    }
}
