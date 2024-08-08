using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using PlayFab.ClientModels;
using PlayFab;
using AYellowpaper.SerializedCollections;
public class SaveLoadUtility
{
	#region Declarations
	public static SaveLoadUtility Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new SaveLoadUtility();
			}
			return _instance;
		}
	}

	private static SaveLoadUtility _instance;
	public static string SavePath => Application.persistentDataPath + "/SaveData.dat";
	#endregion

	#region Local Save/Load
	public string SaveGame(SessionData session)
	{
		session.SaveData.lastSaved = DateTime.Now;
		string data = JsonConvert.SerializeObject(session.SaveData);
		WriteToSavePath(data);
		Debug.Log("Game data saved locally.");
		return data;
	}

	private void WriteToSavePath(string data)
	{
		if (!File.Exists(SavePath))
		{
			File.Create(SavePath);
		}
		File.WriteAllText(SavePath, data);
	}

	public SessionData LoadGame()
	{
		if (!File.Exists(SavePath))
		{
			Debug.Log("No save data found. Creating new game...");
			return NewGame();
		}

		string data = File.ReadAllText(SavePath);
		SaveData loadedGame = JsonConvert.DeserializeObject<SaveData>(data);
		return new SessionData(loadedGame);
	}

	public SessionData NewGame()
	{
		SaveData toSave = new SaveData();
		SessionData newSession = new(toSave);
		Debug.Log("New game created!");
		SaveGame(newSession);
		return new SessionData(toSave);
	}
	#endregion



	public static SaveData MergeSaveData(SaveData localSave, SaveData cloudSave)
	{
		var mergedRecords = MergeSerializedDict(localSave.recordDict, cloudSave.recordDict);
		var mergedDirtyRecords = MergeSerializedDict(localSave.dirtyRecords, cloudSave.dirtyRecords);
		var startDate = localSave.startDate < cloudSave.startDate ? localSave.startDate : cloudSave.startDate;
		var lastSaved = cloudSave.lastSaved;

		return new SaveData(mergedRecords, mergedDirtyRecords, startDate, lastSaved);
	}

	private static SerializedDictionary<string, PlayerRecord> MergeSerializedDict(SerializedDictionary<string, PlayerRecord> dict1, SerializedDictionary<string, PlayerRecord> dict2)
	{
		SerializedDictionary<string, PlayerRecord> mergedDict = new();

		foreach (var key in dict1.Keys.ToList())
		{
			mergedDict[key] = dict1[key];
		}

		foreach (var key in dict2.Keys.ToList())
		{
			if (!mergedDict.ContainsKey(key)
				|| dict2[key].bestTime < mergedDict[key].bestTime)
			{
				mergedDict[key] = dict2[key];
			}
		}

		return mergedDict;
	}
	/*

	public static bool MergeSaveTest()
    {
		DateTime localStart = new(2023, 1, 1, 1, 1, 1, 1);
		DateTime cloudStart = new(2023, 1, 1, 1, 1, 1, 1);
		DateTime localLastSaved = new(2023, 1, 1, 1, 1, 1, 1);
		DateTime cloudLastSaved = new(2024, 1, 1, 1, 1, 1, 1);

		SerializedDictionary<string, PlayerRecord> localRecords = new();
		SerializedDictionary<string, PlayerRecord> localDirty = new();
		SerializedDictionary<string, PlayerRecord> cloudRecords = new();
		SerializedDictionary<string, PlayerRecord> cloudDirty = new();

		PlayerRecord one = new();
		one.bestTime = 1;

		PlayerRecord two = new();
		two.bestTime = 2;

		PlayerRecord three = new();
		three.bestTime = 3;

		PlayerRecord four = new();
		four.bestTime = 4;

		PlayerRecord five = new();
		five.bestTime = 5;

		PlayerRecord six = new();
		six.bestTime = 6;

		PlayerRecord seven = new();
		seven.bestTime = 7;

		localRecords["a"] = four;
		localRecords["b"] = four;
		localRecords["c"] = four;
		localRecords["d"] = four;

		cloudRecords["a"] = three;
		cloudRecords["b"] = five;
		cloudRecords["c"] = four;
		cloudRecords["e"] = four;

		localDirty["a"] = four;
		localDirty["b"] = four;
		localDirty["c"] = four;
		localDirty["d"] = four;

		cloudDirty["a"] = three;
		cloudDirty["b"] = five;
		cloudDirty["c"] = four;
		cloudDirty["e"] = four;

		SaveData localSave = new(localRecords, localDirty, localStart, localLastSaved);
		SaveData cloudSave = new(cloudRecords, cloudDirty, cloudStart, cloudLastSaved);

		var mergedSave = MergeSaveData(localSave, cloudSave);

		bool success = true;

		Debug.Log("--------------BEGINNING MERGED SAVE TEST--------------");

		if(mergedSave.recordDict.Count != 5)
        {
			Debug.Log("FAIL");
			success = false;
		}

		if (mergedSave.dirtyRecords.Count != 5)
		{
			Debug.Log("FAIL");
			success = false;
		}

		if (mergedSave.recordDict["a"].bestTime != 3)
        {
			Debug.Log("FAIL");
			Debug.Log($"Best time at record 'a' = {mergedSave.recordDict["a"].bestTime}");
			success = false;
		}

		if (mergedSave.recordDict["b"].bestTime != 4)
		{
			Debug.Log("FAIL");
			Debug.Log($"Best time at record 'b' = {mergedSave.recordDict["b"].bestTime}");
			success = false;
		}

		if (mergedSave.recordDict["c"].bestTime != 4)
		{
			Debug.Log("FAIL");
			Debug.Log($"Best time at record 'c' = {mergedSave.recordDict["c"].bestTime}");
			success = false;
		}

		if (mergedSave.recordDict["d"].bestTime != 4)
		{
			Debug.Log("FAIL");
			Debug.Log($"Best time at record 'd' = {mergedSave.recordDict["d"].bestTime}");
			success = false;
		}

		if (mergedSave.recordDict["e"].bestTime != 4)
		{
			Debug.Log("FAIL");
			Debug.Log($"Best time at record 'e' = {mergedSave.recordDict["e"].bestTime}");
			success = false;
		}

		if (mergedSave.dirtyRecords["a"].bestTime != 3)
		{
			Debug.Log("FAIL");
			Debug.Log($"Best time at record 'a' = {mergedSave.dirtyRecords["a"].bestTime}");
			success = false;
		}

		if (mergedSave.dirtyRecords["b"].bestTime != 4)
		{
			Debug.Log("FAIL");
			Debug.Log($"Best time at record 'b' = {mergedSave.dirtyRecords["b"].bestTime}");
			success = false;
		}

		if (mergedSave.dirtyRecords["c"].bestTime != 4)
		{
			Debug.Log("FAIL");
			Debug.Log($"Best time at record 'c' = {mergedSave.dirtyRecords["c"].bestTime}");
			success = false;
		}

		if (mergedSave.dirtyRecords["d"].bestTime != 4)
		{
			Debug.Log("FAIL");
			Debug.Log($"Best time at record 'd' = {mergedSave.dirtyRecords["d"].bestTime}");
			success = false;
		}

		if (mergedSave.dirtyRecords["e"].bestTime != 4)
		{
			Debug.Log("FAIL");
			Debug.Log($"Best time at record 'e' = {mergedSave.dirtyRecords["e"].bestTime}");
			success = false;
		}

		if (mergedSave.startDate != localStart)
		{
			Debug.Log("FAIL");
			success = false;
		}

		if (mergedSave.lastSaved != cloudLastSaved)
		{
			Debug.Log("FAIL");
			success = false;
		}

		Debug.Log("SaveData merge test passed: " + success);
		return success;
	}
	*/
}



