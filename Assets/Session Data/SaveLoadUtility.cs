using System;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using LootLocker.Requests;
using Newtonsoft.Json;
public static class SaveLoadUtility
{
    #region Declarations
    public static string SavePath => Application.persistentDataPath + "/SaveData.dat";
	private const int TimeoutInSeconds = 5;
    #endregion

    #region Basic Save/Load
    public static async void SaveGame(SessionData session, GameManager gameManager)
	{
		session.SaveData.lastSaved = DateTime.Now;
		string data = JsonConvert.SerializeObject(session.SaveData);
		WriteToSavePath(data);
		Debug.Log("Game data saved locally.");

		var backupSaved = await SaveToCloud(SavePath, gameManager.LoginStatus);
        if (!backupSaved)
		{
			gameManager.LoginStatus = LoginStatus.Offline;
		}
		Debug.Log($"Game data backed up: {backupSaved}");
	}
	private static void WriteToSavePath(string data)
	{
		if (!File.Exists(SavePath))
		{
			File.Create(SavePath);
		}
		File.WriteAllText(SavePath, data);
	}

	public static SessionData LoadGame(GameManager gameManager)
	{
		if (!File.Exists(SavePath))
		{
			Debug.Log("No save data found. Creating new game...");
			return NewGame(gameManager);
		}

		Debug.Log("Retrieving saved file at " + SavePath);
		string data = File.ReadAllText(SavePath);
		SaveData loadedGame = JsonConvert.DeserializeObject<SaveData>(data);
		Debug.Log($"Loaded data file with {loadedGame.recordDict.Count} entries, first created on {loadedGame.startDate}");
		Debug.Log($"Dirty records: {loadedGame.dirtyRecords.Count}");
		return new SessionData(loadedGame);
	}
	public static SessionData NewGame(GameManager gameManager)
	{
		SaveData toSave = new SaveData();
		SessionData newSession = new(toSave);
		Debug.Log("New game created!");
		SaveGame(newSession, gameManager);
		return new SessionData(toSave);
	}
	public static async Task UpdateRecord(GameManager gameManager, FinishScreenData finishData)
	{
		Level currentLevel = gameManager.CurrentLevel;
		SessionData session = gameManager.Session;
		bool isNewBest = session.UpdateRecord(finishData, currentLevel);
		bool uploadSuccessful = false;
		//Will need to change to not be guest later;
		if (isNewBest && gameManager.LoginStatus != LoginStatus.Offline)
		{
			Debug.Log("Uploading new best time to leaderboard...");
			uploadSuccessful = await gameManager.Leaderboard.UpdateLeaderboardRecord(session.Record(currentLevel.levelUID));
		}
		if (isNewBest && !uploadSuccessful)
		{
			Debug.Log("Setting record to dirty because player is not logged in.");
			gameManager.LoginStatus = LoginStatus.Offline;
			session.SaveData.dirtyRecords[currentLevel.levelUID] = session.Record(currentLevel.levelUID);
		}
		SaveGame(session, gameManager);
	}
	#endregion

	#region Cloud Saves
	private static async Task<bool> SaveToCloud(string path, LoginStatus loginStatus)
    {
		var uploadSuccessful = false;
		if (loginStatus == LoginStatus.Guest || loginStatus == LoginStatus.LoggedIn)
		{
			uploadSuccessful = await UploadFileFromPath(path);
		}
		return uploadSuccessful;
	}

	private static async Task<bool> UploadFileFromPath(string path)
	{
		Debug.Log("Beginning upload from path...");
		int fileID = PlayerPrefs.GetInt("PlayerSaveDataFileID", 0);
		if ( fileID != 0)
        {
			var response = await UpdatePlayerFileTask(fileID, path);
			if (response != null && response.success == true)
			{
				Debug.Log("File was updated!");
				return true;
			}
        }

		var secondResponse = await NewPlayerFileTask(path);
		if (secondResponse != null && secondResponse.success)
		{
			Debug.Log("New file was uploaded!");
			return true;
		}

		return false;
	}
	private static async Task<LootLockerPlayerFile> UpdatePlayerFileTask(int fileID, string path)
    {
		float timeElapsed = 0;
		LootLockerPlayerFile returnResponse = null;
		LootLockerSDKManager.UpdatePlayerFile(fileID, path, (response) =>
		{
			returnResponse = response;
		});
		while(returnResponse == null)
		{
			timeElapsed += Time.deltaTime;
			if (timeElapsed > TimeoutInSeconds)
			{
				Debug.Log("Update player file timed out.");
				break;
			}
			await Task.Delay(10);
        }
		return returnResponse;
	}
	private static async Task<LootLockerPlayerFile> NewPlayerFileTask(string path)
	{
		float timeElapsed = 0;
		LootLockerPlayerFile returnResponse = null;
		string filePurpose = "saveFile";
		LootLockerSDKManager.UploadPlayerFile(path, filePurpose, (response) =>
		{
			PlayerPrefs.SetInt("PlayerSaveDataFileID", response.id);
			returnResponse = response;
		});
		while (returnResponse == null)
		{
			timeElapsed += Time.deltaTime;
			if(timeElapsed > TimeoutInSeconds)
            {
				Debug.Log("New player file timed out.");
				break;
            }
			await Task.Delay(10);
		}
		return returnResponse;
	}
	#endregion

	#region PlayerManagement
	public static async Task<PlayerNameResponse> SetPlayerNameTask(string name)
	{
		float timeElapsed = 0;
		PlayerNameResponse returnResponse = null;
		LootLockerSDKManager.SetPlayerName(name, (response) =>
		{
			returnResponse = response;
		});
		while (returnResponse == null)
		{
			timeElapsed += Time.deltaTime;
			if (timeElapsed > TimeoutInSeconds)
			{
				Debug.Log("Set player name out.");
				break;
			}
			await Task.Delay(10);
		}
		return returnResponse;
	}

	#endregion


}



