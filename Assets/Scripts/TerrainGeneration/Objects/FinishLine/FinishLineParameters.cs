using System;
using UnityEngine;

[Serializable]
public class FinishLineParameters
{
    public int flagPointIndex;
    public int flagPointXOffset;
    public int backstopPointIndex;
    public int backstopPointXOffset;
    public bool backstopIsActive;
    public Vector2 flagPosition;
    public Vector2 backstopPosition;
    public bool isForward;

    public FinishLineParameters(GroundSegment segment, int flagPointIndex, int flagPointXOffset,
                                int backstopPointIndex, int backstopPointXOffset,
                                bool backstopIsActive)
    {
        this.flagPointIndex = flagPointIndex;
        this.flagPointXOffset = flagPointXOffset;
        this.backstopPointIndex = backstopPointIndex;
        this.backstopPointXOffset = backstopPointXOffset;
        this.backstopIsActive = backstopIsActive;

        CalculatePositions(segment);
    }

    public FinishLineParameters(GroundSegment segment)
    {
        flagPointIndex = 2;
        flagPointXOffset = 50;
        backstopPointIndex = 3;
        backstopPointXOffset = 0;
        backstopIsActive = true;

        CalculatePositions(segment);
    }

    private void CalculatePositions(GroundSegment segment)
    {
        flagPosition = segment.transform.TransformPoint(segment.Curve.GetPoint(flagPointIndex).Position + new Vector3(flagPointXOffset, 0));
        backstopPosition = segment.transform.TransformPoint(segment.Curve.GetPoint(backstopPointIndex).Position + new Vector3(backstopPointXOffset, 0));

        isForward = flagPosition.x < backstopPosition.x;
    }
}
