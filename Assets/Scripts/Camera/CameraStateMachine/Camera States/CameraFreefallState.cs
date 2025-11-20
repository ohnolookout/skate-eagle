using UnityEngine;

public class CameraFreefallState : CameraState
{
    public CameraFreefallState(CameraStateMachine cameraMachine) : base(cameraMachine)
    {
    }

    public override void EnterState()
    {
        // Configure camera for freefall (e.g., follow vertical motion, widen view).
    }

    public override void ExitState()
    {
        // Restore settings when landing or leaving freefall.
    }

    public override void FixedUpdateState()
    {
        // Follow player in air; apply smoothing/limits appropriate for freefall.
    }
}