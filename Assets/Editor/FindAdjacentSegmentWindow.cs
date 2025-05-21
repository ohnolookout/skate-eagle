using UnityEditor;
using UnityEngine;
public class FindAdjacentSegmentWindow : EditorWindow
{
    private GroundEditManager _editManager;
    private GroundSegment _segment;
    private LevelDesigner _levelDesigner;

    public void Init(LevelDesigner levelDesigner, GroundEditManager editManager, GroundSegment segment)
    {
        _levelDesigner = levelDesigner;
        _editManager = editManager;
        _segment = segment;
    }

    private void OnGUI()
    {
        GUILayout.Label("Find Adjacent Segment", EditorStyles.boldLabel);

        GUILayout.Space(20);

        if (GUILayout.Button("Find Right Segment Neutral"))
        {
            _segment.NextRightSegment = _editManager.FindNextSegment(_segment, true, false, false);
            _levelDesigner.SetLevelDirty();
        }

        if (GUILayout.Button("Find Right Segment Up"))
        {
            _segment.NextRightSegment = _editManager.FindNextSegment(_segment, true, true, false);
            _levelDesigner.SetLevelDirty();
        }

        if (GUILayout.Button("Find Right Segment Down"))
        {
            _segment.NextRightSegment = _editManager.FindNextSegment(_segment, true, false, true);
            _levelDesigner.SetLevelDirty();
        }

        if (GUILayout.Button("Find Left Segment Neutral"))
        {
            _segment.NextLeftSegment = _editManager.FindNextSegment(_segment, false, false, false);
            _levelDesigner.SetLevelDirty();
        }

        if (GUILayout.Button("Find Left Segment Up"))
        {
            _segment.NextLeftSegment = _editManager.FindNextSegment(_segment, false, true, false);
            _levelDesigner.SetLevelDirty();
        }

        if (GUILayout.Button("Find Left Segment Down"))
        {
            _segment.NextLeftSegment = _editManager.FindNextSegment(_segment, false, false, true);
            _levelDesigner.SetLevelDirty();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Close"))
        {
            Close();
        }
    }
}