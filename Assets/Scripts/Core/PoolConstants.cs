using UnityEngine;

public static class PoolConstants
{
    public const float BallDiameter = 0.05715f;
    public const float BallRadius = BallDiameter * 0.5f;
    public const float PlayingLength = 2.54f;
    public const float PlayingWidth = 1.27f;
    public const float RailWidth = 0.12f;
    public const float CushionHeight = 0.045f;
    public const float CornerPocketRadius = 0.072f;
    public const float SidePocketRadius = 0.064f;
    public const float PocketGap = 0.11f;
    public const float SettleLinear = 0.035f;
    public const float SettleAngular = 0.45f;
    public const float SettleHold = 0.28f;
    public const float MaxShotImpulse = 4.8f;
    public const float MinShotImpulse = 0.35f;

    public static float HeadStringZ => -PlayingLength * 0.25f;
    public static float FootSpotZ => PlayingLength * 0.25f;
    public static Vector3 HeadSpot => new Vector3(0f, BallRadius, HeadStringZ);
    public static Vector3 FootSpot => new Vector3(0f, BallRadius, FootSpotZ);
}
