using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using static UnityEngine.Rendering.HableCurve;

public class FinishLineCreatorWindow : EditorWindow
{
    private GroundManager _groundManager;
    private GroundSegment _segment;
    private GroundEditManager _groundEditor;
    private int _flagPointIndex = 2;
    private int _backstopPointIndex = 3;
    private int _flagXOffset = 50;
    private int _backstopXOffset = 0;
    private bool _backstopIsActive = true;

    public static void ShowWindow()
    {
        GetWindow<FinishLineCreatorWindow>("Finish Line Creator");
    }


    //public void Init(GroundSegment segment, GroundManager groundManager, GroundEditManager groundEditor)
    //{
    //    _segment = segment;
    //    _groundManager = groundManager;
    //    _groundEditor = groundEditor;
    //    var finishParams = groundManager.FinishLine.Parameters;
    //    if(finishParams != null)
    //    {
    //        _flagPointIndex = finishParams.flagPointIndex;
    //        _flagXOffset = finishParams.flagPointXOffset;
    //        _backstopPointIndex = finishParams.backstopPointIndex;
    //        _backstopXOffset = finishParams.backstopPointXOffset;
    //        _backstopIsActive = finishParams.backstopIsActive;
    //    }
    //}

    //private void OnGUI()
    //{
    //    GUILayout.Label("Finish Line Creator", EditorStyles.boldLabel);

    //    _flagPointIndex = EditorGUILayout.IntField("Flag Point Index", _flagPointIndex, GUILayout.ExpandWidth(false));
    //    _flagXOffset = EditorGUILayout.IntField("Flag X Offset", _flagXOffset, GUILayout.ExpandWidth(false));
    //    _backstopPointIndex = EditorGUILayout.IntField("Backstop Point Index", _backstopPointIndex, GUILayout.ExpandWidth(false));
    //    _backstopXOffset = EditorGUILayout.IntField("Backstop X Offset", _backstopXOffset, GUILayout.ExpandWidth(false));
    //    _backstopIsActive = EditorGUILayout.Toggle("Backstop Active", _backstopIsActive, GUILayout.ExpandWidth(false));

    //    if (GUILayout.Button("Apply Finish Line"))
    //    {
    //        if (_groundManager.FinishSegment != null && _groundManager.FinishSegment != _segment)
    //        {
    //            var doOverwrite = EditorUtility.DisplayDialog("Warning", "This will overwrite current finish line.", "OK", "Cancel");
    //            if (!doOverwrite)
    //            {
    //                return;
    //            }
    //        }
    //        var flagPosition = _segment.transform.TransformPoint(_segment.Curve.GetPoint(_flagPointIndex).Position + new Vector3(_flagXOffset, 0));
    //        var backstopPosition = _segment.transform.TransformPoint(_segment.Curve.GetPoint(_backstopPointIndex).Position + new Vector3(_backstopXOffset, 0));


    //        var serializedFinishLine = new SerializedFinishLine(_flagPointIndex, _flagXOffset, _backstopPointIndex, _backstopXOffset, _backstopIsActive, flagPosition, backstopPosition);

    //        _groundEditor.SetFinishLine(_segment, serializedFinishLine);
    //    }

    //    if(GUILayout.Button("Clear Finish Line"))
    //    {
    //        if(!EditorUtility.DisplayDialog("Warning", "This will clear the current finish line.", "OK", "Cancel"))
    //        {
    //            return;
    //        }

    //        _groundEditor.ClearFinishLine();
    //    }

    //    if (GUILayout.Button("Close"))
    //    {
    //        Close();
    //    }
    //}
}
