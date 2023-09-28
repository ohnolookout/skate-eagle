using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FinishUtility
{
    public static FinishScreenData GenerateFinishData(Level level, LevelRecords levelRecords, float attemptTime)
    {
        FinishScreenType finishType = FindFinishType(level, levelRecords, attemptTime, out Medal displayMedal);
        FinishScreenData finishData = new(finishType, attemptTime, levelRecords.bestTime, displayMedal);
        return finishData;
    }

    public static FinishScreenType FindFinishType(Level level, LevelRecords levelTimeData, float attemptTime, out Medal displayMedal)
    {
        Medal attemptMedal = level.MedalFromTime(attemptTime);
        if (levelTimeData.bestTime <= attemptTime)
        {
            displayMedal = Medal.Participant;
            return FinishScreenType.Participant;
        }
        if ((int)attemptMedal >= (int)levelTimeData.medal)
        {
            displayMedal = Medal.Participant;
            return FinishScreenType.NewBestTime;
        }
        displayMedal = attemptMedal;
        return FinishScreenType.NewMedal;

    }
}

public struct FinishScreenData
{
    public float attemptTime, previousBest;
    public Medal medal;
    public FinishScreenType finishType;
    public FinishScreenData(FinishScreenType finishType, float attemptTime, float previousBest, Medal medal)
    {
        this.attemptTime = attemptTime;
        this.previousBest = previousBest;
        this.medal = medal;
        this.finishType = finishType;
    }
}
