#if UNITY_EDITOR

using System;
using System.Linq;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class Debugger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _statusLog;
    [SerializeField] private TextMeshProUGUI _detailsLog;    
    private static Debugger _instance;
    private Action _updateDetails;
    [SerializeField] private int _updateFramerate = 10;
    [SerializeField] private bool _logSessionDetails = false;

    public static Debugger Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            return null;
        }
    }

    private void Awake()
    {
        ClearStatus();
        ClearDetails();

        if (!Application.isEditor)
        {
            DestroyImmediate(gameObject);
        }
    }

    void Start()
    {
    }

    void Update()
    {
        if(Time.frameCount % _updateFramerate > 0)
        {
            return;
        }


        string detailsString = "";
        if (_logSessionDetails)
        {
            detailsString += LogSessionInfo(GameManager.Instance.SessionData);
        }

        _detailsLog.text = detailsString;
    }

    public void PostStatus(string status)
    {
        _statusLog.text = status;
    }

    public void PostDetails(string details)
    {
        _statusLog.text += "\n" + details;
    }

    public void ClearStatus()
    {
        _statusLog.text = string.Empty;
    }

    public void ClearDetails()
    {
        _detailsLog.text = string.Empty;
    }

    public void UpdateDetails()
    {
        if (_updateDetails != null)
        {
            _updateDetails();
        }
    }

    public void ResetProgress()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        SceneManager.LoadScene("Start_Menu");
        GameManager.Instance.ResetSaveData();
        _statusLog.text = "Progress reset.";
    }

    public void DeletePlayerAccount()
    {
        _statusLog.text = "Deleting account... Please wait.";
        GameManager.Instance.OnAccountReset += () => PostStatus("Account deleted.");
        GameManager.Instance.PlayFabManager.OnInitializationComplete += _ => PostStatus("PlayFab reinitialized.");
        GameManager.Instance.PlayFabManager.DeletePlayerAccount(GameManager.Instance);
    }

    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        _statusLog.text = "PlayerPrefs cleared.";

        EditorApplication.isPlaying = false; // Stop play mode in the editor
    }

    private string LogSessionInfo(SessionData sessionData)
    {
        // Aggregate medal counts  
        string medalCounts = string.Join(", ", sessionData.MedalCount);

        // Calculate total player records  
        int totalPlayerRecords = sessionData.SaveData.recordDict.Count;

        Dictionary<CompletionStatus, int> completionStatusCounts = new()
        {
            { CompletionStatus.Locked, 0 },
            { CompletionStatus.Incomplete, 0 },
            { CompletionStatus.Complete, 0 }
        };

        var records = sessionData.SaveData.recordDict.Values;

        foreach (var record in records)
        {
            completionStatusCounts[record.status]++;
        }


        string completionStatusSummary = string.Join(", ",
            completionStatusCounts.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

        // Log the message  
        string logMessage = $"Total Player Records: {totalPlayerRecords}\n" +
                            $"Completion Statuses: {completionStatusSummary}\n" +
                            $"Medal Counts: {medalCounts}";

        return logMessage;
    }
}

#endif