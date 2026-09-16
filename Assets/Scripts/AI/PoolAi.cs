using UnityEngine;

public sealed class PoolAi : MonoBehaviour
{
    public void ChooseShot(Ball[] balls, Ball cue, Table table, IGameMode mode, int difficulty, CueController cueCtl)
    {
        var noise = difficulty <= 0 ? 14f : difficulty == 1 ? 7f : 2.2f;
        var bestScore = -999f;
        var bestYaw = cueCtl.AimYaw;
        var bestPower = 0.55f;
        var pockets = table.PocketCenters;

        foreach (var ball in balls)
        {
            if (ball == null || ball.Pocketed || ball.Group == BallGroup.Cue)
            {
                continue;
            }

            if (!IsLegalTarget(ball, balls, mode))
            {
                continue;
            }

            for (var i = 0; i < pockets.Length; i++)
            {
                var pocket = pockets[i];
                var toPocket = pocket - ball.transform.position;
                toPocket.y = 0f;
                if (toPocket.sqrMagnitude < 0.01f)
                {
                    continue;
                }

                var dir = toPocket.normalized;
                var ghost = ball.transform.position - dir * (PoolConstants.BallDiameter * 1.02f);
                var toGhost = ghost - cue.transform.position;
                toGhost.y = 0f;
                if (toGhost.sqrMagnitude < 0.01f)
                {
                    continue;
                }

                var aim = toGhost.normalized;
                var align = Vector3.Dot(aim, dir);
                if (align < 0.2f)
                {
                    continue;
                }

                var dist = toGhost.magnitude + toPocket.magnitude;
                var score = align * 2f - dist * 0.15f;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestYaw = Mathf.Atan2(aim.x, aim.z) * Mathf.Rad2Deg;
                    bestPower = Mathf.Clamp01(0.28f + dist * 0.12f);
                }
            }
        }

        if (bestScore < -900f)
        {
            var fallback = FindAnyObjectBall(balls);
            if (fallback != null)
            {
                var d = fallback.transform.position - cue.transform.position;
                d.y = 0f;
                bestYaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
                bestPower = 0.5f;
            }
        }

        cueCtl.AimYaw = bestYaw + Random.Range(-noise, noise);
        cueCtl.Power = Mathf.Clamp01(bestPower + Random.Range(-0.08f, 0.08f));
        cueCtl.English = difficulty >= 2 ? new Vector2(Random.Range(-0.15f, 0.15f), 0f) : Vector2.zero;
    }

    static bool IsLegalTarget(Ball ball, Ball[] balls, IGameMode mode)
    {
        if (mode.Kind == GameModeKind.NineBall)
        {
            var lowest = 99;
            foreach (var b in balls)
            {
                if (b != null && !b.Pocketed && b.Number >= 1 && b.Number <= 9)
                {
                    lowest = Mathf.Min(lowest, b.Number);
                }
            }

            return ball.Number == lowest;
        }

        if (mode.StatusLabel != null && mode.StatusLabel.Contains("Solids"))
        {
            return ball.Group == BallGroup.Solid || (ball.Group == BallGroup.Eight && CountGroup(balls, BallGroup.Solid) == 0);
        }

        if (mode.StatusLabel != null && mode.StatusLabel.Contains("Stripes"))
        {
            return ball.Group == BallGroup.Stripe || (ball.Group == BallGroup.Eight && CountGroup(balls, BallGroup.Stripe) == 0);
        }

        return ball.Group != BallGroup.Eight;
    }

    static int CountGroup(Ball[] balls, BallGroup g)
    {
        var n = 0;
        foreach (var b in balls)
        {
            if (b != null && !b.Pocketed && b.Group == g)
            {
                n++;
            }
        }

        return n;
    }

    static Ball FindAnyObjectBall(Ball[] balls)
    {
        foreach (var b in balls)
        {
            if (b != null && !b.Pocketed && b.Group != BallGroup.Cue)
            {
                return b;
            }
        }

        return null;
    }
}
