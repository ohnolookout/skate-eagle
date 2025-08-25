using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;

public enum CameraTargetType
{
    CurvePointLow,
    CurvePointHigh,
    Player,
    FinishFlag,
    Backstop
}
public static class CameraTargetUtility
{
    //GroundSegmentLowPoint
    public static Vector2 GroundSegmentLowPointInfluence => new Vector2(0, 1f);
    public static float GroundSegmentLowpointDuration = 0.75f;
    public static Vector2 GroundSegmentLowPointOffset = new Vector2(0, 25);

    //GroundSegmentHighPoint
    public static Vector2 GroundSegmentHighPointInfluence => new Vector2(0, 0.15f);
    public static float GroundSegmentHighpointDuration = 0.75f;
    public static Vector2 GroundSegmentHighPointOffset = new Vector2(0, -12);

    //Player
    public static Vector2 PlayerInfluence => new Vector2(1, .25f);
    public static float PlayerDuration = 0;
    public static Vector2 PlayerOffset = new Vector2(10, -5);


    private static Dictionary<CameraTargetType, Vector2> InfluenceDict = new()
    {
        { CameraTargetType.CurvePointLow, GroundSegmentLowPointInfluence },
        { CameraTargetType.CurvePointHigh, GroundSegmentHighPointInfluence },
        { CameraTargetType.Player, PlayerInfluence }
    };

    private static Dictionary<CameraTargetType, float> DurationDict = new()
    {
        { CameraTargetType.CurvePointLow, GroundSegmentLowpointDuration },
        { CameraTargetType.CurvePointHigh, GroundSegmentHighpointDuration },
        { CameraTargetType.Player, PlayerDuration }
    };

    private static Dictionary<CameraTargetType, Vector2> OffsetDict = new()
    {
        { CameraTargetType.CurvePointLow, GroundSegmentLowPointOffset },
        { CameraTargetType.CurvePointHigh, GroundSegmentHighPointOffset },
        { CameraTargetType.Player, PlayerOffset }
    };

    public static CameraTarget GetTarget(CameraTargetType type, Transform targetTransform)
    {
        return new()
        {
            TargetTransform = targetTransform,
            SerializedPosition = targetTransform.TransformPoint(targetTransform.position),
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
