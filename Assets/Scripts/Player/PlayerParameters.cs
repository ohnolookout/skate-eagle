public class PlayerParameters
{
    public readonly float JumpForce = 40;
    public readonly float DownForce = 95;
    public readonly float FlipDelay = 0.75f;
    public readonly float StompSpeedLimit = -250;
    public readonly float FullJumpDuration = 0.25f;
    public readonly float MinJumpDuration = 0.15f;
    public readonly int StompThreshold = 2;
    public readonly int JumpLimit = 2;
    public float JumpMultiplier = 1;
    public float RotationStart = 0;
    public float JumpStartTime;
    public int StompCharge = 0;
    public int JumpCount = 0;
    public int FlipBoost = 70;
    public float RotationAccel = 1400;

    public PlayerParameters(int startingStompCharge = 0)
    {
        StompCharge = startingStompCharge;
    }
}