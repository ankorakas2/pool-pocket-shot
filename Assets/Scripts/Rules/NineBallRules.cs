using UnityEngine;

public sealed class NineBallRules : IGameMode
{
    public GameModeKind Kind => GameModeKind.NineBall;
    public string StatusLabel { get; private set; } = "9-ball";
    public bool BallInHand { get; private set; }
    public bool KitchenOnlyPlacement { get; private set; }
    public string FoulMessage { get; private set; }
    public string WinnerMessage { get; private set; }

    Ball[] _balls;
    bool _broken;

    public void StartMatch(Ball[] balls)
    {
        _balls = balls;
        _broken = false;
        BallInHand = false;
        KitchenOnlyPlacement = true;
        StatusLabel = "9-ball — hit lowest number first";
        FoulMessage = "";
        WinnerMessage = "";
    }

    public void OnBreakPlaced()
    {
        KitchenOnlyPlacement = false;
        BallInHand = false;
    }

    public TurnOutcome Resolve(ShotSnapshot shot, int player)
    {
        FoulMessage = "";
        var pocketed = shot.Pocketed ?? System.Array.Empty<Ball>();
        var nineIn = Has(pocketed, 9);
        var lowest = LowestOnTable();

        if (!_broken)
        {
            _broken = true;
            KitchenOnlyPlacement = false;
            if (shot.CueScratch || !shot.CueContactedAnything)
            {
                if (nineIn)
                {
                    Respot(9);
                }

                return Foul("Foul on break — ball in hand", true, false);
            }

            if (nineIn && shot.FirstContact != null && shot.FirstContact.Number == 1)
            {
                WinnerMessage = $"Player {player + 1} wins on the break!";
                return TurnOutcome.Win;
            }

            if (nineIn)
            {
                Respot(9);
            }

            if (AnyObject(pocketed))
            {
                StatusLabel = Status(player);
                return TurnOutcome.Continue;
            }

            StatusLabel = Status(player);
            return TurnOutcome.Switch;
        }

        if (shot.CueScratch || !shot.CueContactedAnything)
        {
            if (nineIn)
            {
                Respot(9);
            }

            return Foul(shot.CueScratch ? "Scratch — ball in hand" : "No ball contacted", true, false);
        }

        var legal = lowest != null ? lowest.Number : 1;
        if (shot.FirstContact == null || shot.FirstContact.Number != legal)
        {
            if (nineIn)
            {
                Respot(9);
            }

            return Foul($"Must hit {legal} first", true, false);
        }

        if (nineIn)
        {
            WinnerMessage = $"Player {player + 1} wins!";
            return TurnOutcome.Win;
        }

        if (AnyObject(pocketed))
        {
            StatusLabel = Status(player);
            return TurnOutcome.Continue;
        }

        StatusLabel = Status(player);
        return TurnOutcome.Switch;
    }

    TurnOutcome Foul(string msg, bool hand, bool kitchen)
    {
        FoulMessage = msg;
        BallInHand = hand;
        KitchenOnlyPlacement = kitchen;
        StatusLabel = msg;
        return TurnOutcome.Foul;
    }

    Ball LowestOnTable()
    {
        Ball best = null;
        foreach (var b in _balls)
        {
            if (b == null || b.Number == 0)
            {
                continue;
            }

            if (b.Pocketed)
            {
                continue;
            }

            if (b.Number > 9)
            {
                continue;
            }

            if (!b.gameObject.activeSelf && b.Pocketed)
            {
                continue;
            }

            if (!b.Pocketed && (best == null || b.Number < best.Number))
            {
                best = b;
            }
        }

        return best;
    }

    void Respot(int n)
    {
        if (_balls[n] != null)
        {
            _balls[n].RespawnOnTable(PoolConstants.FootSpot);
        }
    }

    static bool Has(Ball[] pocketed, int n)
    {
        foreach (var b in pocketed)
        {
            if (b.Number == n)
            {
                return true;
            }
        }

        return false;
    }

    static bool AnyObject(Ball[] pocketed)
    {
        foreach (var b in pocketed)
        {
            if (b.Group != BallGroup.Cue)
            {
                return true;
            }
        }

        return false;
    }

    string Status(int player)
    {
        var low = LowestOnTable();
        return $"P{player + 1} — legal ball {(low != null ? low.Number.ToString() : "9")}";
    }
}
