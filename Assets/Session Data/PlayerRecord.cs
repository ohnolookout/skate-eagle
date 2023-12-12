using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public enum CompletionStatus { Complete, Incomplete, Locked}

[Serializable]
public class PlayerRecord
{
	public string levelName;
	public string _UID;
	public float bestTime = float.PositiveInfinity;
	public DateTime date;
	public Medal medal = Medal.Participant;
	public CompletionStatus status;

	public PlayerRecord(Level level)
	{
		levelName = level.Name;
		_UID = level.UID;
		status = CompletionStatus.Locked;
		medal = Medal.Participant;
		date = DateTime.Now;
	}

	public PlayerRecord(string UID)
    {
		levelName = UID;
		_UID = UID;
		status = CompletionStatus.Locked;
		medal = Medal.Participant;
		date = DateTime.Now;
	}

	public PlayerRecord()
    {

    }

	public bool Update(FinishScreenData finishData, SessionData sessionData)
    {
		if (finishData.finishType == FinishScreenType.Participant)
		{
			return false;
		}
		if (finishData.finishType == FinishScreenType.NewMedal)
		{
			sessionData.AdjustMedalCount(finishData.medal, medal);
			medal = finishData.medal;
			status = CompletionStatus.Complete;
		}
		date = DateTime.Now;
		bestTime = finishData.attemptTime;
		return true;
	}

	public string UID
    {
        get
        {
			return _UID;
        }
    }

}