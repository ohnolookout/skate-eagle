using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
[CustomEditor(typeof(SaveLoadUtility))]

public class SaveInspector : Editor
{
    override public void OnInspectorGUI()
	{
		if (GUILayout.Button("Save Game"))
			//SaveSerial.SaveGame();
		if (GUILayout.Button(
					"Load Game"))
			SaveLoadUtility.LoadGame(LoginStatus.Offline);
		if (GUILayout.Button(
					"New Game"))
			SaveLoadUtility.NewGame(LoginStatus.Offline);
	}

}
