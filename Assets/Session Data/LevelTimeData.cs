using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class LevelTimeData
{
	public string levelName;
	public float bestTime = float.PositiveInfinity;
	public DateTime date;
	public Medal medal = Medal.Participant;
	public Level level;

	public LevelTimeData(Level level)
	{
		this.level = level;
		levelName = level.Name;

	}

	public LevelTimeData(Level completedLevel, float timeInSeconds)
	{
		level = completedLevel;
		levelName = completedLevel.Name;
		bestTime = timeInSeconds;
		date = DateTime.Now;
		medal = completedLevel.MedalTimes.MedalFromTime(timeInSeconds);
	}

	public void UpdateTime(float timeInSeconds, out Medal newMedal, out Medal? oldMedal)
	{
		if (timeInSeconds < bestTime)
		{
			bestTime = timeInSeconds;
			date = DateTime.Now;
			oldMedal = medal;
			medal = level.MedalTimes.MedalFromTime(timeInSeconds);
			newMedal = (Medal) medal;
			return;
		}
		newMedal = level.MedalTimes.MedalFromTime(timeInSeconds);
		oldMedal = null;
	}
}