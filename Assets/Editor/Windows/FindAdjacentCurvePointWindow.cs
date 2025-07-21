using UnityEditor;
using UnityEngine;
public class FindAdjacentCurvePointWindow : EditorWindow
{
    private GroundEditManager _editManager;
    private CurvePointObject _curvePointObject;
    private LevelDesigner _levelDesigner;

    public void Init(LevelDesigner levelDesigner, GroundEditManager editManager, CurvePointObject curvePointObject)
    {
        _levelDesigner = levelDesigner;
        _editManager = editManager;
        _curvePointObject = curvePointObject;
    }

    private void OnGUI()
    {
        GUILayout.Label("Find Adjacent Curve Point", EditorStyles.boldLabel);

        GUILayout.Space(20);

        if (GUILayout.Button("Find Right Curve Point Neutral"))
        {
            _curvePointObject.NextRightCurvePointObject = _editManager.FindNextCurvePoint(_curvePointObject, true, false, false);
            _levelDesigner.SetLevelDirty();
        }

        if (GUILayout.Button("Find Right Curve Point Up"))
        {
            _curvePointObject.NextRightCurvePointObject = _editManager.FindNextCurvePoint(_curvePointObject, true, true, false);
            _levelDesigner.SetLevelDirty();
        }

        if (GUILayout.Button("Find Right Curve Point Down"))
        {
            _curvePointObject.NextRightCurvePointObject = _editManager.FindNextCurvePoint(_curvePointObject, true, false, true);
            _levelDesigner.SetLevelDirty();
        }

        if (GUILayout.Button("Find Left Curve Point Neutral"))
        {
            _curvePointObject.NextLeftCurvePointObject = _editManager.FindNextCurvePoint(_curvePointObject, false, false, false);
            _levelDesigner.SetLevelDirty();
        }

        if (GUILayout.Button("Find Left Curve Point Up"))
        {
            _curvePointObject.NextLeftCurvePointObject = _editManager.FindNextCurvePoint(_curvePointObject, false, true, false);
            _levelDesigner.SetLevelDirty();
        }

        if (GUILayout.Button("Find Left Curve Point Down"))
        {
            _curvePointObject.NextLeftCurvePointObject = _editManager.FindNextCurvePoint(_curvePointObject, false, false, true);
            _levelDesigner.SetLevelDirty();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Close"))
        {
            Close();
        }
    }
}