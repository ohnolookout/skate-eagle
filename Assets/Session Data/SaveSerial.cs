using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public static class SaveSerial
{
	public static void SaveGame(SaveData data)
    {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Open(Application.persistentDataPath
					 + "/SaveData.dat", FileMode.OpenOrCreate);
		bf.Serialize(file, data);
		file.Close();
		Debug.Log("Game data saved!");
	}
	public static SaveData NewGame()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath
					 + "/SaveData.dat");
		SaveData data = new SaveData();
		bf.Serialize(file, data);
		file.Close();
		Debug.Log("New game created!");
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
			return data;
		}
		Debug.Log("There is no save data! Creating new game...");
		return NewGame();
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
	public LevelRecords[] levelTimes;

	public SaveData()
    {
		startDate = DateTime.Now;
		levelTimes = new LevelRecords[0];
    }

	public void UpdateLevelRecords(LevelRecords[] newTimes)
    {
		levelTimes = newTimes;
    }

}



