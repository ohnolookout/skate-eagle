using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(GroundSegment))]
public class GroundSegmentEditor: Editor
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

}