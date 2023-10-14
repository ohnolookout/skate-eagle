using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LevelRecords
{
	public string levelName;
	public float bestTime = float.PositiveInfinity;
	public DateTime date;
	public Medal medal = Medal.Participant;
	public int attemptsCount = 0;

	public LevelRecords(Level level)
	{
		levelName = level.Name;

	}

	public LevelRecords(Level completedLevel, float timeInSeconds)
	{
		levelName = completedLevel.Name;
		bestTime = timeInSeconds;
		date = DateTime.Now;
		medal = completedLevel.MedalTimes.MedalFromTime(timeInSeconds);
	}

	public void AddAttempt()
    {
		attemptsCount++;
    }
}