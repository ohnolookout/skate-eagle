using UnityEngine;
using System;
using UnityEngine.U2D;
using System.Collections.Generic;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class CurvePointObject : MonoBehaviour, ICameraTargetable
{
    #region Declarations
    public CurvePoint curvePoint;
    private Ground _parentGround;
    private Action<CurvePointObject> _onCurvePointChange;
    private CurvePointObject _nextLeftCurvePointObject;
    private CurvePointObject _nextRightCurvePointObject;
    public Action<CurvePointObject> OnCurvePointChange;
    public List<GameObject> rightTargetObjects = new();
    public List<GameObject> leftTargetObjects = new();
    private LinkedCameraTarget _linkedCameraTarget = new();
    [SerializeField] private bool _doTargetLow = true;
    [SerializeField] private bool _doTargetHigh = false;
    public CurvePoint CurvePoint => curvePoint;
    public Ground ParentGround { get => _parentGround; set => _parentGround = value; }
    public Vector3 WorldPosition => transform.position + ParentGround.transform.position;
    public List<GameObject> RightTargetObjects { get => rightTargetObjects; set => rightTargetObjects = value; }
    public List<GameObject> LeftTargetObjects { get => leftTargetObjects; set => leftTargetObjects = value; }
    public LinkedCameraTarget LinkedCameraTarget { get => _linkedCameraTarget; set => _linkedCameraTarget = value; }
    public bool DoTargetLow { get => _doTargetLow; set => _doTargetLow = value; }
    public bool DoTargetHigh { get => _doTargetHigh; set => _doTargetHigh = value; }
    public CurvePointObject NextLeftCurvePointObject 
    { 
        get  
        {
            var index = ParentGround.CurvePointObjects.IndexOf(this);
            
            if (index != 0)
            {
                return ParentGround.CurvePointObjects[index - 1];
            }

            return _nextLeftCurvePointObject;
        }
        set => _nextLeftCurvePointObject = value; 
    }
    public CurvePointObject NextRightCurvePointObject
    {
        get
        {
            var index = ParentGround.CurvePointObjects.IndexOf(this);

            if (index != ParentGround.CurvePointObjects.Count - 1)
            {
                return ParentGround.CurvePointObjects[index + 1];
            }

            return _nextRightCurvePointObject;
        }
        set => _nextRightCurvePointObject = value;
    }
    #endregion

    #region Monobehaviors
    void OnDrawGizmosSelected()
    {
        LinkedCameraTarget.DrawTargets();
    }

    #endregion
    public void SetCurvePoint(CurvePoint curvePoint)
    {
        this.curvePoint = curvePoint;
        this.curvePoint.Object = this; // Set the object reference in the CurvePoint
        transform.position = this.curvePoint.Position;
    }

    public void TangentsChanged(Vector3 updatedleftTang, Vector3 updatedRightTang)
    {
        curvePoint.LeftTangent = updatedleftTang - WorldPosition;
        curvePoint.RightTangent = updatedRightTang - WorldPosition;

        _onCurvePointChange?.Invoke(this);
    }

    public void LeftTangentChanged(Vector3 updatedTang)
    {
        curvePoint.LeftTangent = updatedTang - WorldPosition;

        if (curvePoint.IsSymmetrical)
        {
            curvePoint.RightTangent = -curvePoint.LeftTangent; // If it's symmetrical, update the right tangent accordingly
        }
        else if (curvePoint.Mode == ShapeTangentMode.Continuous)
        {
            var rightMagnitude = curvePoint.RightTangent.magnitude;
            curvePoint.RightTangent = -curvePoint.LeftTangent.normalized * rightMagnitude; // Maintain the same magnitude for the left tangent
        }

        _onCurvePointChange?.Invoke(this);
    }

    public void RightTangentChanged(Vector3 updatedTang)
    {

        curvePoint.RightTangent = updatedTang - WorldPosition;

        if(curvePoint.IsSymmetrical)
        {
            curvePoint.LeftTangent = -curvePoint.RightTangent; // If it's symmetrical, update the right tangent accordingly
        } else if (curvePoint.Mode == ShapeTangentMode.Continuous)
        {
            var leftMagnitude = curvePoint.LeftTangent.magnitude;
            curvePoint.LeftTangent = -curvePoint.RightTangent.normalized * leftMagnitude; // Maintain the same magnitude for the left tangent
        }
        
        _onCurvePointChange?.Invoke(this);
    }

    public void PositionChanged(Vector3 updatedPosition)
    {
        var localPosition = updatedPosition - ParentGround.transform.position;
        curvePoint.Position = localPosition;
        transform.position = localPosition;
    }

    public void SettingsChanged(ShapeTangentMode mode, bool isSymmetrical)
    {
        curvePoint.Mode = mode;
        curvePoint.IsSymmetrical = isSymmetrical;
        if (isSymmetrical)
        {
            curvePoint.RightTangent = -curvePoint.LeftTangent; // If it's symmetrical, update the right tangent accordingly
        }
        else if (curvePoint.Mode == ShapeTangentMode.Continuous)
        {
            var rightMagnitude = curvePoint.RightTangent.magnitude;
            curvePoint.RightTangent = -curvePoint.LeftTangent.normalized * rightMagnitude; // Maintain the same magnitude for the left tangent
        }
        _onCurvePointChange?.Invoke(this);
    }

    public void PopulateDefaultTargets()
    {
        Debug.Log("Populating default targets for CurvePointObject: " + name);
        _linkedCameraTarget.Target = CameraTargetUtility.GetTarget(CameraTargetType.CurvePointLow, transform);

        if (NextLeftCurvePointObject != null && !leftTargetObjects.Contains(NextLeftCurvePointObject.gameObject))
        {
            leftTargetObjects.Add(NextLeftCurvePointObject.gameObject);
        }

        if (NextRightCurvePointObject != null && !rightTargetObjects.Contains(NextRightCurvePointObject.gameObject))
        {
            rightTargetObjects.Add(NextRightCurvePointObject.gameObject);
        }
    }
}


#if UNITY_EDITOR

[CustomEditor(typeof(CurvePointObject))]
public class CurvePointObjectEditor : Editor
{
    private SerializedObject _so;
    private SerializedProperty _serializedLeftTargetObjects;
    private SerializedProperty _serializedRightTargetObjects;
    public override void OnInspectorGUI()
    {
        var curvePointObject = (CurvePointObject)target;
        _so = new SerializedObject(curvePointObject);
        _serializedLeftTargetObjects = _so.FindProperty("leftTargetObjects");
        _serializedRightTargetObjects = _so.FindProperty("rightTargetObjects");
        

        //Curvepoint settings
        EditorGUI.BeginChangeCheck();

        var isSymmetrical = GUILayout.Toggle(curvePointObject.CurvePoint.IsSymmetrical, "Symmetrical");
        var mode = (ShapeTangentMode)EditorGUILayout.EnumPopup("Tangent Mode", curvePointObject.CurvePoint.Mode);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(curvePointObject, "Curve Point Settings");
            curvePointObject.SettingsChanged(mode, isSymmetrical);
        }

        if (GUILayout.Button("Reset Tangents"))
        {
            Undo.RecordObject(curvePointObject, "Reset Curve Point Tangents");
            curvePointObject.TangentsChanged(
                curvePointObject.CurvePoint.Position + new Vector3(-1, 0),
                curvePointObject.CurvePoint.Position + new Vector3(-1, 0));
        }

        //Camera targetting
        EditorGUI.BeginChangeCheck();

        var doTargetLow = GUILayout.Toggle(curvePointObject.DoTargetLow, "Do Target Low");

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(curvePointObject, "Curve Point Target Settings");
            curvePointObject.DoTargetLow = doTargetLow;
            //Need to purge from any other lists where it appears
        }

        if (curvePointObject.DoTargetLow)
        {
            //Show linked camera target settings
        }

        EditorGUI.BeginChangeCheck();

        var doTargetHigh = GUILayout.Toggle(curvePointObject.DoTargetHigh, "Do Target High");

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(curvePointObject, "Curve Point Target Settings");
            curvePointObject.DoTargetHigh = doTargetHigh;
            //Need to purge from any other lists where it appears
        }

        if (curvePointObject.DoTargetHigh)
        {
            //Show linked camera target settings
        }

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(_serializedLeftTargetObjects, true);
        EditorGUILayout.PropertyField(_serializedRightTargetObjects, true);

        if (EditorGUI.EndChangeCheck())
        {
            _so.ApplyModifiedProperties();
            _so.Update();
        }
    }
    public void OnSceneGUI()
    {
        var curvePointEditObject = (CurvePointObject)target;
        
        DrawCurvePointHandles(curvePointEditObject);
    }

    public static void DrawCurvePointHandles(CurvePointObject curvePointObject)
    {
        var objectPosition = curvePointObject.transform.position;
        var groundPosition = curvePointObject.ParentGround.transform.position;
        var worldPosition = curvePointObject.WorldPosition;


        //Position handle

        Handles.color = Color.green;
        EditorGUI.BeginChangeCheck();

        var positionHandle = Handles.FreeMoveHandle(
            worldPosition,
            HandleUtility.GetHandleSize(worldPosition) * .1f,
            Vector3.zero,
            Handles.SphereHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(curvePointObject, "Curve Point Edit");
            curvePointObject.PositionChanged(positionHandle);
        }


        Handles.color = Color.red;

        Handles.DrawLine(worldPosition, curvePointObject.CurvePoint.LeftTangentPosition + groundPosition);
        Handles.DrawLine(worldPosition, curvePointObject.CurvePoint.RightTangentPosition + groundPosition);

        Handles.color = Color.yellow;

        //Left tangent handle
        EditorGUI.BeginChangeCheck();

        var leftTangentHandle = Handles.FreeMoveHandle(
            curvePointObject.CurvePoint.LeftTangentPosition + groundPosition,
            HandleUtility.GetHandleSize(curvePointObject.CurvePoint.LeftTangentPosition) * .4f,
            Vector3.zero,
            Handles.ArrowHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(curvePointObject, "Curve Point Edit");
            curvePointObject.LeftTangentChanged(leftTangentHandle);
        }


        //Right tangent handle
        EditorGUI.BeginChangeCheck();

        var rightTangentHandle = Handles.FreeMoveHandle(
            curvePointObject.CurvePoint.RightTangentPosition + groundPosition,
            HandleUtility.GetHandleSize(curvePointObject.CurvePoint.RightTangentPosition) * .4f,
            Vector3.zero,
            Handles.ArrowHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(curvePointObject, "Curve Point Edit");
            curvePointObject.RightTangentChanged(rightTangentHandle);
        }
    }
}

#endif

