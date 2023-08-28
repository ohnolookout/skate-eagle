using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public static class SaveSerial
{

	public static SaveData SaveGame()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath
					 + "/SaveData.dat");
		SaveData data = new SaveData();
		bf.Serialize(file, data);
		file.Close();
		Debug.Log("Game data saved!");
		return data;
	}

	public static SaveData LoadGame()
	{
		if (File.Exists(Application.persistentDataPath
					   + "/SaveData.dat"))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file =
					   File.Open(Application.persistentDataPath
					   + "/SaveData.dat", FileMode.Open);
			SaveData data = (SaveData)bf.Deserialize(file);
			file.Close();
			Debug.Log("Game data loaded!");
			return data;
		}
		Debug.LogError("There is no save data!");
		return null;
	}

	public static void ResetData()
	{
		if (File.Exists(Application.persistentDataPath
					  + "/SaveData.dat"))
		{
			File.Delete(Application.persistentDataPath
							  + "/SaveData.dat");
			Debug.Log("Data reset complete!");
		}
		else
		Debug.LogError("No save data to delete.");
	}
}

[Serializable]
public class SaveData
{
	public DateTime startDate;
	public LevelTimeData[] levelTimes;

	public SaveData()
    {
		startDate = DateTime.Now;
		levelTimes = new LevelTimeData[0];
    }

	public void UpdateTimes(LevelTimeData[] newTimes)
    {
		levelTimes = newTimes;
    }

}


[Serializable]
public class LevelTimeData
{
	public string levelName;
	public float? bestTime = null;
	public DateTime date;
	public Medal? medal = null;
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

	public void UpdateTime(float timeInSeconds, out Medal? newMedal, out Medal? oldMedal)
    {
		
		if(timeInSeconds < bestTime || bestTime is null)
        {
			bestTime = timeInSeconds;
			date = DateTime.Now;
			oldMedal = medal;
			medal = level.MedalTimes.MedalFromTime(timeInSeconds);
			newMedal = medal;
			return;
		}
		newMedal = null;
		oldMedal = null;
	}
}
