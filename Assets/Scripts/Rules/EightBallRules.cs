using UnityEngine;

public sealed class EightBallRules : IGameMode
{
    public GameModeKind Kind => GameModeKind.EightBall;
    public string StatusLabel { get; private set; } = "Open table";
    public bool BallInHand { get; private set; }
    public bool KitchenOnlyPlacement { get; private set; } = true;
    public string FoulMessage { get; private set; }
    public string WinnerMessage { get; private set; }

    Ball[] _balls;
    BallGroup[] _assigned = { BallGroup.Cue, BallGroup.Cue };
    bool _broken;
    bool _open = true;

    public void StartMatch(Ball[] balls)
    {
        _balls = balls;
        _assigned[0] = BallGroup.Cue;
        _assigned[1] = BallGroup.Cue;
        _broken = false;
        _open = true;
        BallInHand = false;
        KitchenOnlyPlacement = true;
        StatusLabel = "Open table — break";
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
        var other = 1 - player;
        var pocketed = shot.Pocketed ?? System.Array.Empty<Ball>();
        var eightIn = Has(pocketed, 8);
        var cueIn = shot.CueScratch;

        if (!_broken)
        {
            _broken = true;
            KitchenOnlyPlacement = false;
            if (cueIn || !shot.CueContactedAnything)
            {
                return Foul("Foul on break — ball in hand in kitchen", true, true);
            }

            AssignIfOpen(pocketed, player);
            if (eightIn)
            {
                RespotEight();
            }

            if (LegalPocket(pocketed, player) && !cueIn)
            {
                StatusLabel = Label(player);
                return TurnOutcome.Continue;
            }

            StatusLabel = Label(player);
            return TurnOutcome.Switch;
        }

        var group = _assigned[player];
        var needEight = group != BallGroup.Cue && Remaining(group) == 0;

        if (!shot.CueContactedAnything)
        {
            if (eightIn)
            {
                WinnerMessage = $"Player {other + 1} wins (8 illegally pocketed).";
                return TurnOutcome.Loss;
            }

            return Foul("No ball contacted", true, false);
        }

        if (cueIn)
        {
            if (eightIn)
            {
                WinnerMessage = $"Player {other + 1} wins (scratch on the 8).";
                return TurnOutcome.Loss;
            }

            return Foul("Scratch — ball in hand", true, false);
        }

        var first = shot.FirstContact;
        if (needEight)
        {
            if (first == null || first.Number != 8)
            {
                if (eightIn)
                {
                    WinnerMessage = $"Player {other + 1} wins (wrong first contact on 8).";
                    return TurnOutcome.Loss;
                }

                return Foul("Must hit the 8-ball first", true, false);
            }

            if (eightIn)
            {
                WinnerMessage = $"Player {player + 1} wins!";
                return TurnOutcome.Win;
            }

            StatusLabel = Label(player);
            return TurnOutcome.Switch;
        }

        if (eightIn)
        {
            WinnerMessage = $"Player {other + 1} wins (8-ball too early).";
            return TurnOutcome.Loss;
        }

        if (_open)
        {
            if (first.Group == BallGroup.Eight)
            {
                return Foul("Cannot hit 8 first while table is open", true, false);
            }

            AssignIfOpen(pocketed, player);
            if (LegalPocket(pocketed, player))
            {
                StatusLabel = Label(player);
                return TurnOutcome.Continue;
            }

            StatusLabel = Label(player);
            return TurnOutcome.Switch;
        }

        if (first.Group != group)
        {
            return Foul("Wrong ball first", true, false);
        }

        if (LegalPocket(pocketed, player))
        {
            StatusLabel = Label(player);
            return TurnOutcome.Continue;
        }

        StatusLabel = Label(player);
        return TurnOutcome.Switch;
    }

    TurnOutcome Foul(string msg, bool ballInHand, bool kitchen)
    {
        FoulMessage = msg;
        BallInHand = ballInHand;
        KitchenOnlyPlacement = kitchen;
        StatusLabel = msg;
        return TurnOutcome.Foul;
    }

    void AssignIfOpen(Ball[] pocketed, int player)
    {
        if (!_open)
        {
            return;
        }

        var solids = 0;
        var stripes = 0;
        foreach (var b in pocketed)
        {
            if (b.Group == BallGroup.Solid)
            {
                solids++;
            }

            if (b.Group == BallGroup.Stripe)
            {
                stripes++;
            }
        }

        if (solids > 0 && stripes == 0)
        {
            _assigned[player] = BallGroup.Solid;
            _assigned[1 - player] = BallGroup.Stripe;
            _open = false;
        }
        else if (stripes > 0 && solids == 0)
        {
            _assigned[player] = BallGroup.Stripe;
            _assigned[1 - player] = BallGroup.Solid;
            _open = false;
        }
    }

    bool LegalPocket(Ball[] pocketed, int player)
    {
        var group = _assigned[player];
        foreach (var b in pocketed)
        {
            if (b.Group == BallGroup.Cue || b.Group == BallGroup.Eight)
            {
                continue;
            }

            if (_open || b.Group == group)
            {
                return true;
            }
        }

        return false;
    }

    int Remaining(BallGroup group)
    {
        var n = 0;
        foreach (var b in _balls)
        {
            if (b != null && !b.Pocketed && b.Group == group)
            {
                n++;
            }
        }

        return n;
    }

    static bool Has(Ball[] pocketed, int number)
    {
        foreach (var b in pocketed)
        {
            if (b.Number == number)
            {
                return true;
            }
        }

        return false;
    }

    void RespotEight()
    {
        var eight = _balls[8];
        eight.RespawnOnTable(PoolConstants.FootSpot);
    }

    string Label(int player)
    {
        if (_open)
        {
            return "Open table";
        }

        var g = _assigned[player];
        var mine = Remaining(g);
        return $"P{player + 1}: {(g == BallGroup.Solid ? "Solids" : "Stripes")} ({mine} left)";
    }
}
