using UnityEngine;

public enum GameModeKind
{
    EightBall,
    NineBall
}

public enum TurnOutcome
{
    Continue,
    Switch,
    Foul,
    Win,
    Loss
}

public interface IGameMode
{
    GameModeKind Kind { get; }
    string StatusLabel { get; }
    bool BallInHand { get; }
    bool KitchenOnlyPlacement { get; }
    void StartMatch(Ball[] balls);
    void OnBreakPlaced();
    TurnOutcome Resolve(ShotSnapshot shot, int player);
    string FoulMessage { get; }
    string WinnerMessage { get; }
}
