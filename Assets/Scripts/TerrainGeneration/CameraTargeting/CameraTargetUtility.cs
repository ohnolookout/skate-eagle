using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;

public enum CameraTargetType
{
    GroundSegmentLowPoint,
    GroundSegmentHighPoint,
    Player,
    FinishFlag,
    Backstop
}
public static class CameraTargetUtility
{
    //GroundSegmentLowPoint
    public static Vector2 GroundSegmentLowPointInfluence => new Vector2(0, 1);
    public static float GroundSegmentLowpointDuration = 0.5f;
    public static Vector2 GroundSegmentLowPointOffset = new Vector2(0, 10);

    //GroundSegmentHighPoint
    public static Vector2 GroundSegmentHighPointInfluence => new Vector2(0, 0.15f);
    public static float GroundSegmentHighpointDuration = 0.1f;
    public static Vector2 GroundSegmentHighPointOffset = new Vector2(0, -12);

    //Player
    public static Vector2 PlayerInfluence => new Vector2(1, 0.25f);
    public static float PlayerDuration = 0;
    public static Vector2 PlayerOffset = new Vector2(10, 10);


    private static Dictionary<CameraTargetType, Vector2> InfluenceDict = new()
    {
        { CameraTargetType.GroundSegmentLowPoint, GroundSegmentLowPointInfluence },
        { CameraTargetType.GroundSegmentHighPoint, GroundSegmentHighPointInfluence },
        { CameraTargetType.Player, PlayerInfluence }
    };

    private static Dictionary<CameraTargetType, float> DurationDict = new()
    {
        { CameraTargetType.GroundSegmentLowPoint, GroundSegmentLowpointDuration },
        { CameraTargetType.GroundSegmentHighPoint, GroundSegmentHighpointDuration },
        { CameraTargetType.Player, PlayerDuration }
    };

    private static Dictionary<CameraTargetType, Vector2> OffsetDict = new()
    {
        { CameraTargetType.GroundSegmentLowPoint, GroundSegmentLowPointOffset },
        { CameraTargetType.GroundSegmentHighPoint, GroundSegmentHighPointOffset },
        { CameraTargetType.Player, PlayerOffset }
    };

    public static CameraTarget GetTarget(CameraTargetType type, Transform targetTransform)
    {
        return new()
        {
            TargetTransform = targetTransform,
            TargetInfluenceH = InfluenceDict[type].x,
            TargetInfluenceV = InfluenceDict[type].y,
            TargetOffset = OffsetDict[type]
        };
    }

    public static float GetDuration(CameraTargetType type)
    {
        return DurationDict[type];
    }

}
