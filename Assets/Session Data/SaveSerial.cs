using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using RotaryHeart.Lib.SerializableDictionary;
using Newtonsoft.Json;
public static class SaveSerial
{
	public static void SaveGame(SaveData toSave)
	{
		string data = JsonConvert.SerializeObject(toSave);
		WriteToSavePath(data);
		Debug.Log("Game data saved!");
	}
	public static SaveData NewGame()
	{
		SaveData toSave = new SaveData();
		string data = JsonConvert.SerializeObject(toSave);
		WriteToSavePath(data);
		Debug.Log("New game created!");
		return toSave;
	}

	public static SaveData LoadGame()
	{
		if (File.Exists(SavePath))
		{
			string data = File.ReadAllText(SavePath);
			SaveData loadedGame = JsonConvert.DeserializeObject<SaveData>(data);
			return loadedGame;
		}
		Debug.Log("No save data found. Creating new game...");
		return NewGame();
	}

	public static void WriteToSavePath(string data)
    {
		if (!File.Exists(SavePath))
		{
			File.Create(SavePath);
		}
		File.WriteAllText(SavePath, data);
	}

	public static string SavePath
    {
        get
        {
			return Application.persistentDataPath
					 + "/SaveData.dat";
		}
    }
}

[Serializable]
public class SaveData
{
	public DateTime startDate;
	public SerializedDictionary<string, PlayerRecord> recordDict;

	public SaveData()
	{
		startDate = DateTime.Now;
		recordDict = new();
	}

	public SerializedDictionary<string, PlayerRecord> PlayerRecords()
    {
		return recordDict;
    }

	public void UpdateRecord(string UID, PlayerRecord record)
    {
		recordDict[UID] = record;
    }

	public void ReplaceAllRecords(SerializedDictionary<string, PlayerRecord> newRecords)
    {
		recordDict = newRecords;
    }

}



