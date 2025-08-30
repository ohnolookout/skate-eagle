using System;
using UnityEngine;

public class PlayerParameters
{
    public readonly float JumpForce = .8f; //40 on add force normal
    public readonly float DownForce = 95;
    public readonly float FlipDelay = 0.75f;
    public readonly float StompSpeedLimit = -250;
    public readonly float MinJumpDuration = 0.15f;
    public readonly float FullJumpDuration = 0.25f;
    public readonly float MinSecondJumpInterval = .35f;
    public readonly float SecondJumpDampen = 0.25f;
    public readonly float SecondJumpDampenMinVel = 50;
    public readonly float SecondJumpDampenMaxVel = 80;
    public readonly float MinSecondJumpVelDampen = 0.025f;
    public readonly int StompThreshold = 2;
    public readonly int JumpLimit = 2;
    public float JumpMultiplier = 1;
    public float RotationStart = 0;
    public float JumpStartTime;
    public int StompCharge = 0;
    public int JumpCount = 0;
    public int FlipBoost = 90;
    public float RotationAccel = 1500;

    public PlayerParameters(int startingStompCharge = 0)
    {
        StompCharge = startingStompCharge;
    }

    public void IncrementJumpCount()
    {
        JumpCount = Math.Min(JumpCount+1, JumpLimit);
    }

    public void ResetJumpCount()
    {
        JumpCount = 0;
    }
}