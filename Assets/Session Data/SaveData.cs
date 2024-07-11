using System;
using AYellowpaper.SerializedCollections;

[Serializable]
public class SaveData
{
	public DateTime startDate;
	public DateTime lastSaved;
	public SerializedDictionary<string, PlayerRecord> recordDict;
	public SerializedDictionary<string, PlayerRecord> dirtyRecords;

	public SaveData()
	{
		startDate = DateTime.Now;
		lastSaved = DateTime.Now;
		recordDict = new();
		dirtyRecords = new();
	}

	public SaveData(SerializedDictionary<string, PlayerRecord> records, SerializedDictionary<string, PlayerRecord> dirty, DateTime start, DateTime last)
    {
		recordDict = records;
		dirtyRecords = dirty;
		startDate = start;
		lastSaved = last;
    }

}



