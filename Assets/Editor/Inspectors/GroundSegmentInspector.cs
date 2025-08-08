using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(GroundSegment))]
public class GroundSegmentInspector: Editor
{
    private Tool lastTool = Tool.None;
    public void OnEnable()
    {
        lastTool = Tools.current;
        Tools.current = Tool.None;
        Tools.hidden = true;
    }

    private void OnDestroy()
    {
        Tools.current = lastTool;
        Tools.hidden = false;
    }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

    public void OnSceneGUI()
    {
        var segment = (GroundSegment)target;

        if (!GroundInspector.DebugSegments)
        {
            Selection.activeObject = segment.parentGround;
        }

    }

}