using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FinishUtility
{
    public static FinishData GenerateFinishData(Level level, PlayerRecord playerRecord, float attemptTime)
    {
        FinishScreenType finishType = FindFinishType(level, playerRecord, attemptTime, out Medal displayMedal);
        FinishData finishData = new(level.UID, finishType, attemptTime, playerRecord.bestTime, displayMedal);
        return finishData;
    }

    public static FinishScreenType FindFinishType(Level level, PlayerRecord playerRecord, float attemptTime, out Medal displayMedal)
    {
        Medal attemptMedal = level.medalTimes.MedalFromTime(attemptTime);
        if (playerRecord.bestTime <= attemptTime)
        {
            displayMedal = Medal.Participant;
            return FinishScreenType.Participant;
        }
        if ((int)attemptMedal >= (int)playerRecord.medal)
        {
            displayMedal = Medal.Participant;
            return FinishScreenType.NewBestTime;
        }
        displayMedal = attemptMedal;
        return FinishScreenType.NewMedal;

    }
}

public struct FinishData
{
    public float attemptTime, previousBest;
    public Medal medal;
    public FinishScreenType finishType;
    public string levelUID;
    public FinishData(string levelUID, FinishScreenType finishType, float attemptTime, float previousBest, Medal medal)
    {
        this.levelUID = levelUID;
        this.attemptTime = attemptTime;
        this.previousBest = previousBest;
        this.medal = medal;
        this.finishType = finishType;
    }
}
