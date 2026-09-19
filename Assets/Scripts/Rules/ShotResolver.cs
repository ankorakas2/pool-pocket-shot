using System.Collections.Generic;
using UnityEngine;

public sealed class ShotResolver : MonoBehaviour
{
    public Ball Cue { get; private set; }
    public Ball FirstContact { get; private set; }
    public bool CueScratch { get; private set; }
    public readonly List<Ball> PocketedThisShot = new List<Ball>();
    public readonly List<int> PocketIndexes = new List<int>();
    public bool CueHitRail;
    public bool ObjectHitRail;

    bool _armed;
    HashSet<Ball> _seen = new HashSet<Ball>();
    float _ignorePocketsUntil;
    public bool IgnoreCueBall;

    public void Bind(Ball cue, PocketTrigger[] pockets, Ball[] balls)
    {
        Cue = cue;
        foreach (var p in pockets)
        {
            p.BallEntered += OnPocket;
        }

        foreach (var b in balls)
        {
            if (b == null)
            {
                continue;
            }

            var relay = b.gameObject.AddComponent<CollisionRelay>();
            relay.Ball = b;
            relay.Hit += OnHit;
        }
    }

    public void BeginShot()
    {
        _armed = true;
        FirstContact = null;
        CueScratch = false;
        CueHitRail = false;
        ObjectHitRail = false;
        PocketedThisShot.Clear();
        PocketIndexes.Clear();
        _seen.Clear();
    }

    public void IgnorePocketsFor(float seconds)
    {
        _ignorePocketsUntil = Time.time + seconds;
    }

    public ShotSnapshot EndShot()
    {
        _armed = false;
        return new ShotSnapshot
        {
            FirstContact = FirstContact,
            CueScratch = CueScratch,
            Pocketed = PocketedThisShot.ToArray(),
            PocketIndexes = PocketIndexes.ToArray(),
            CueContactedAnything = FirstContact != null
        };
    }

    void OnPocket(Ball ball, int pocket)
    {
        if (ball == null || ball.Pocketed || Time.time < _ignorePocketsUntil)
        {
            return;
        }

        if (IgnoreCueBall && ball.Group == BallGroup.Cue)
        {
            return;
        }

        if (ball.Group == BallGroup.Cue)
        {
            CueScratch = true;
        }

        ball.MarkPocketed(pocket);
        PocketedThisShot.Add(ball);
        PocketIndexes.Add(pocket);
    }

    void OnHit(Ball self, Collision collision)
    {
        if (!_armed)
        {
            return;
        }

        var otherBall = collision.collider.GetComponentInParent<Ball>();
        if (self.Group == BallGroup.Cue && otherBall != null && otherBall.Group != BallGroup.Cue && FirstContact == null)
        {
            FirstContact = otherBall;
        }

        if (otherBall == null)
        {
            if (self.Group == BallGroup.Cue)
            {
                CueHitRail = true;
            }
            else
            {
                ObjectHitRail = true;
            }
        }
    }
}

public struct ShotSnapshot
{
    public Ball FirstContact;
    public bool CueScratch;
    public Ball[] Pocketed;
    public int[] PocketIndexes;
    public bool CueContactedAnything;
}

public sealed class CollisionRelay : MonoBehaviour
{
    public Ball Ball;
    public event System.Action<Ball, Collision> Hit;

    void OnCollisionEnter(Collision collision)
    {
        Hit?.Invoke(Ball, collision);
    }
}
