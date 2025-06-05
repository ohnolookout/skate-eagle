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

    public FinishLineParameters(int flagPointIndex, int flagPointXOffset,
                                int backstopPointIndex, int backstopPointXOffset,
                                bool backstopIsActive)
    {
        this.flagPointIndex = flagPointIndex;
        this.flagPointXOffset = flagPointXOffset;
        this.backstopPointIndex = backstopPointIndex;
        this.backstopPointXOffset = backstopPointXOffset;
        this.backstopIsActive = backstopIsActive;
    }

    public FinishLineParameters()
    {
        flagPointIndex = 2;
        flagPointXOffset = 50;
        backstopPointIndex = 3;
        backstopPointXOffset = 0;
        backstopIsActive = true;
    }
}
