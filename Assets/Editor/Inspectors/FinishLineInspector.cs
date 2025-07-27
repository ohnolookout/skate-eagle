using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(FinishLine))]
public class FinishLineInspector : Editor
{
    private FinishLine _finishLine;
    public override void OnInspectorGUI()
    {
        _finishLine = (FinishLine)target;

        EditorGUI.BeginChangeCheck();

        int flagXOffset = EditorGUILayout.IntField("Flag X Offset", _finishLine.FlagXOffset);

        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_finishLine, "Change Flag X Offset");
            _finishLine.SetFlagOffset(flagXOffset);
        }

        EditorGUI.BeginChangeCheck();

        int backstopXOffset = EditorGUILayout.IntField("Backstop X Offset", _finishLine.BackstopXOffset);

        if(EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(_finishLine, "Change Backstop X Offset");
            _finishLine.SetBackstopOffset(backstopXOffset);
        }

        if(GUILayout.Button("Toggle Backstop Active"))
        {
            Undo.RecordObject(_finishLine, "Toggle Backstop Active");
            _finishLine.ActivateBackstop(!_finishLine.BackstopIsActive);
        }

        if(GUILayout.Button("Clear Flag"))
        {
            Undo.RecordObject(_finishLine, "Clear Flag");
            _finishLine.ClearFlag();
        }

        if (GUILayout.Button("Clear Backstop"))
        {
            Undo.RecordObject(_finishLine, "Clear Backstop");
            _finishLine.ClearBackstop();
        }

        if(GUILayout.Button("Clear Finish"))
        {
            Undo.RecordObject(_finishLine, "Clear Finish");
            _finishLine.Clear();
        }

        GUILayout.Space(20);
        GUILayout.Label("Default Inspector", EditorStyles.boldLabel);
        DrawDefaultInspector();
    }
}
