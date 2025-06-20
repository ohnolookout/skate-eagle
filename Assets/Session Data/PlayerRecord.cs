using System;

[Serializable]
public enum CompletionStatus { Complete, Incomplete, Locked}

[Serializable]
public class PlayerRecord
{
	public string levelName;
	public string levelUID;
	public string leaderboardKey;
	public float bestTime = float.PositiveInfinity;
	public DateTime date;
	public Medal medal = Medal.Participant;
	public CompletionStatus status = CompletionStatus.Locked;

	public PlayerRecord(Level level)
	{
		levelName = level.Name;
		levelUID = level.UID;
		leaderboardKey = level.LeaderboardKey;
		status = CompletionStatus.Locked;
		medal = Medal.Participant;
		date = DateTime.Now;
	}

	public PlayerRecord()
    {

    }

	public bool Update(FinishData finishData, SessionData sessionData)
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

}