using System;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using LootLocker.Requests;
using Newtonsoft.Json;
public static class SaveSerial
{
	//Need to add handling for out of sync local/cloud saves
	public static async void SaveGame(SaveData toSave, LoginStatus loginStatus)
	{
		toSave.lastSaved = DateTime.Now;
		string data = JsonConvert.SerializeObject(toSave);
		WriteToSavePath(data);
		Debug.Log("Game data saved locally.");

		var backupSaved = await SaveBackup(SavePath, loginStatus);
		Debug.Log($"Game data backed up: {backupSaved}");
	}
	public static SaveData NewGame(LoginStatus loginStatus)
	{
		SaveData toSave = new SaveData();
		Debug.Log("New game created!");
		SaveGame(toSave, loginStatus);
		return toSave;
	}

	public static SaveData LoadGame(LoginStatus loginStatus)
	{
		if (File.Exists(SavePath))
		{
			Debug.Log("Retrieving saved file at " + SavePath);
			string data = File.ReadAllText(SavePath);
			SaveData loadedGame = JsonConvert.DeserializeObject<SaveData>(data);
			return loadedGame;
		}
		Debug.Log("No save data found. Creating new game...");
		return NewGame(loginStatus);
	}
	private static async Task<bool> SaveBackup(string path, LoginStatus loginStatus)
    {
		var uploadSuccessful = false;
		if (loginStatus == LoginStatus.Guest || loginStatus == LoginStatus.LoggedIn)
		{
			uploadSuccessful = await UploadFileFromPath(path);
		}
		return uploadSuccessful;
	}

	private static void WriteToSavePath(string data)
    {
		if (!File.Exists(SavePath))
		{
			File.Create(SavePath);
		}
		File.WriteAllText(SavePath, data);
	}
	private static async Task<bool> UploadFileFromPath(string path)
	{
		Debug.Log("Beginning upload from path...");
		int fileID = PlayerPrefs.GetInt("PlayerSaveDataFileID", 0);
		if ( fileID != 0)
        {
			var response = await UpdatePlayerFileTask(fileID, path);
			if (response.success == true)
			{
				Debug.Log("File was updated!");
				return true;
			}
        }

		var secondResponse = await NewPlayerFileTask(path);
		if (secondResponse.success)
		{
			Debug.Log("New file was uploaded!");
			return true;
		}

		return false;
	}

	private static async Task<LootLockerPlayerFile> UpdatePlayerFileTask(int fileID, string path)
    {
		LootLockerPlayerFile returnResponse = null;
		LootLockerSDKManager.UpdatePlayerFile(fileID, path, (response) =>
		{
			returnResponse = response;
		});
		while(returnResponse == null)
        {
			await Task.Delay(25);
        }
		return returnResponse;
	}
	private static async Task<LootLockerPlayerFile> NewPlayerFileTask(string path)
	{
		LootLockerPlayerFile returnResponse = null;
		string filePurpose = "saveFile";
		LootLockerSDKManager.UploadPlayerFile(path, filePurpose, (response) =>
		{
			PlayerPrefs.SetInt("PlayerSaveDataFileID", response.id);
			returnResponse = response;
		});
		while (returnResponse == null)
		{
			await Task.Delay(25);
		}
		return returnResponse;
	}

	public static string SavePath => Application.persistentDataPath + "/SaveData.dat";
}

[Serializable]
public class SaveData
{
	public DateTime startDate;
	public DateTime lastSaved;
	public SerializedDictionary<string, PlayerRecord> recordDict;

	public SaveData()
	{
		startDate = DateTime.Now;
		lastSaved = DateTime.Now;
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



