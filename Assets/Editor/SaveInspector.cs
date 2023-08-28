using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
[CustomEditor(typeof(SaveSerial))]

public class SaveInspector : Editor
{
    override public void OnInspectorGUI()
	{
		if (GUILayout.Button("Save Game"))
			SaveSerial.SaveGame();
		if (GUILayout.Button(
					"Load Game"))
			SaveSerial.LoadGame();
		if (GUILayout.Button(
					"Reset Save Data"))
			SaveSerial.ResetData();
	}

}
