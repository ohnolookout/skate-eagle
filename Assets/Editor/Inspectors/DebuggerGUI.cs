using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Debugger))]
public class DebuggerGUI : Editor
{
    SerializedObject _so;
    Debugger _debugger;

    private void OnEnable()
    {
        _debugger = (Debugger)target;
        _so = new(target);
    }


    public override void OnInspectorGUI()
    {
        GUILayout.Label("Debugger", EditorStyles.boldLabel);
        // Add custom GUI elements here
        if (GUILayout.Button("Reset Player Records", GUILayout.ExpandWidth(false)))
        {
            if(EditorUtility.DisplayDialog("Reset Player Records", "Are you sure you want to reset all player records? This action cannot be undone.", "Yes", "No"))
            {
                _debugger.ResetProgress();
            }
        }
        if (GUILayout.Button("Clear Player Prefs", GUILayout.ExpandWidth(false)))
        {
            if(EditorUtility.DisplayDialog("Clear Player Prefs", "Are you sure you want to clear all player preferences? This action cannot be undone.", "Yes", "No"))
            {
                _debugger.ClearPlayerPrefs();
            }
        }
        if (GUILayout.Button("Delete Player Account", GUILayout.ExpandWidth(false)))
        {
            if (EditorUtility.DisplayDialog("Delete Player Account", "Are you sure you want to delete your player account? This action cannot be undone.", "Yes", "No"))
            {
                _debugger.DeletePlayerAccount();
            }
        }

        GUILayout.Space(20);
        GUILayout.Label("Display", EditorStyles.boldLabel);

        if (GUILayout.Button("Clear Status", GUILayout.ExpandWidth(false)))
        {
            _debugger.ClearDetails();
        }
        if (GUILayout.Button("Clear Details", GUILayout.ExpandWidth(false)))
        {
            _debugger.ClearDetails();
        }

        GUILayout.Space(20);
        GUILayout.Label("Debugger Settings", EditorStyles.boldLabel);
        
        DrawDefaultInspector();
    }
}
