using System;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

[CustomEditor(typeof(CurvePointEditObject))]
public class CurvePointObjectInspector : Editor
{
    #region Declarations

    private CurvePointEditObject _curvePointObject;
    private SerializedObject _so;
    private GroundManager _groundManager;
    private EditManager _editManager;
    private bool _controlHeld = false;
    private bool _altHeld = false;
    private bool _aHeld = false;
    private Tool _lastTool = Tool.None;
    public static GUIContent rightTargetButton = new GUIContent("R", "Add/Remove Right Target");
    public static GUIContent leftTargetButton = new GUIContent("L", "Add/Remove Left Target");
    public static GUIContent highTargetButton = new GUIContent("H", "Add/Remove High Target");
    public static GUIContent doHighButton = new GUIContent("/\\", "Set High Target");
    public static GUIContent doLowButton = new GUIContent("\\/", "Set Low Target");
    public static GUIContent selectButton = new GUIContent("S", "Select Curve Point");
    public static GUIStyle buttonStyle = new GUIStyle();

    #endregion

    #region Tool Mgmt
    private void OnEnable()
    {
        _lastTool = Tools.current;
        Tools.current = Tool.None; // Disable the current tool to prevent conflicts with handles
        buttonStyle = new GUIStyle()
        {
            fontSize = 10,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(3, 3, 2, 2)
        };
    }

    private void OnDisable()
    {
        Tools.current = _lastTool; // Restore the last tool when the inspector is closed
    }
    #endregion

    public override void OnInspectorGUI()
    {
        #region Inspector Declarations
        _curvePointObject = (CurvePointEditObject)target;
        _so = new SerializedObject(_curvePointObject);
        var originalColor = GUI.backgroundColor;

        if (_groundManager == null)
        {
            _groundManager = FindFirstObjectByType<GroundManager>();
        }

        if (_editManager == null)
        {
            _editManager = FindFirstObjectByType<EditManager>();
        }
        #endregion

        #region Control/Alt Key Handling
        if (Event.current.control)
        {
            _controlHeld = true;
        }
        else if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.LeftControl)
        {
            _controlHeld = false;
        }
        #endregion

        #region Transform Controls

        GUILayout.Label("Transform Controls", EditorStyles.boldLabel);

        //Vector transform controls

        var startPos = _curvePointObject.transform.position;

        EditorGUI.BeginChangeCheck();
        var position = EditorGUILayout.Vector2Field("Position", _curvePointObject.transform.position);
        if (EditorGUI.EndChangeCheck())
        {
            _curvePointObject.PositionChanged(position);

            if (_editManager.editType == EditType.Shift || _controlHeld)
            {
                _editManager.ShiftCurvePoints(_curvePointObject, _curvePointObject.transform.position - startPos);
            }

            RefreshGround();
        }

        EditorGUI.BeginChangeCheck();
        var leftTangent = EditorGUILayout.Vector2Field("Left Tangent", _curvePointObject.CurvePoint.LeftTangent);
        if (EditorGUI.EndChangeCheck())
        {
            _curvePointObject.LeftTangentChanged(leftTangent + (Vector2)_curvePointObject.CurvePoint.Position);
            RefreshGround();
        }

        EditorGUI.BeginChangeCheck();
        var rightTangent = EditorGUILayout.Vector2Field("Right Tangent", _curvePointObject.CurvePoint.RightTangent);
        if (EditorGUI.EndChangeCheck())
        {
            _curvePointObject.RightTangentChanged(rightTangent + (Vector2)_curvePointObject.CurvePoint.Position);
            RefreshGround();
        }

        // Angle and magnitude controls

        GUILayout.Space(10);
        var defaultLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 50;
        GUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        var leftTangAngle = 180 - EditorGUILayout.FloatField("L Angle", 180 - _curvePointObject.LeftTangentAngle, GUILayout.ExpandWidth(false));
        if (EditorGUI.EndChangeCheck())
        {
            var newTang = BezierMath.ConvertAngleToVector(leftTangAngle, _curvePointObject.LeftTangentMagnitude);
            _curvePointObject.LeftTangentChanged((Vector3)(newTang + _curvePointObject.CurvePoint.Position));
            RefreshGround();
        }

        EditorGUI.BeginChangeCheck();
        var leftTangMag = EditorGUILayout.FloatField("L Mag", _curvePointObject.LeftTangentMagnitude, GUILayout.ExpandWidth(false));
        if (EditorGUI.EndChangeCheck())
        {
            if (Math.Abs(leftTangMag) < .5f)
            {
                leftTangMag = leftTangMag < 0 ? -0.5f : 0.5f;
            }
            var newTang = BezierMath.ConvertAngleToVector(leftTangAngle, leftTangMag);
            _curvePointObject.LeftTangentChanged((Vector3)(newTang + _curvePointObject.CurvePoint.Position));
            RefreshGround();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        EditorGUI.BeginChangeCheck();
        var rightTangAngle = EditorGUILayout.FloatField("R Angle", _curvePointObject.RightTangentAngle, GUILayout.ExpandWidth(false));
        if (EditorGUI.EndChangeCheck())
        {
            var newTang = BezierMath.ConvertAngleToVector(rightTangAngle, _curvePointObject.RightTangentMagnitude);
            _curvePointObject.RightTangentChanged((Vector3)(newTang + _curvePointObject.CurvePoint.Position));
            RefreshGround();
        }

        EditorGUI.BeginChangeCheck();
        var rightTangMag = EditorGUILayout.FloatField("R Mag", _curvePointObject.RightTangentMagnitude, GUILayout.ExpandWidth(false));
        if (EditorGUI.EndChangeCheck())
        {
            if (Math.Abs(rightTangMag) < .5f)
            {
                rightTangMag = rightTangMag < 0 ? -0.5f : 0.5f;
            }
            var newTang = BezierMath.ConvertAngleToVector(rightTangAngle, rightTangMag);
            _curvePointObject.RightTangentChanged((Vector3)(newTang + _curvePointObject.CurvePoint.Position));
            RefreshGround();
        }

        GUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = defaultLabelWidth;
        #endregion

        #region Tangent Options
        GUILayout.Space(10);
        GUILayout.Label("Tangent Options", EditorStyles.boldLabel);

        //Tangent settings
        EditorGUI.BeginChangeCheck();

        var currentMode = (int)_curvePointObject.CurvePoint.TangentMode;
        var mode = GUILayout.Toolbar(currentMode, Enum.GetNames(typeof(ShapeTangentMode)));

        var isSymmetrical = GUILayout.Toggle(_curvePointObject.CurvePoint.IsSymmetrical, "Symmetrical");

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_curvePointObject, "Curve Point Settings");
            _curvePointObject.TangentSettingsChanged((ShapeTangentMode)mode, isSymmetrical);
            RefreshGround();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset Angle", GUILayout.ExpandWidth(false)))
        {
            Undo.RecordObject(_curvePointObject, "Tangent Settings");
            _curvePointObject.TangentSettingsChanged(ShapeTangentMode.Continuous, _curvePointObject.CurvePoint.IsSymmetrical);

            float angle;
            var nextCurvePoint = NextCurvePoint(_curvePointObject);
            var previousCurvePoint = PreviousCurvePoint(_curvePointObject);

            if (previousCurvePoint == null && nextCurvePoint == null)
            {
                return;
            } else if (previousCurvePoint == null)
            {
                angle = nextCurvePoint.LeftTangentAngle;
            } else if (nextCurvePoint == null)
            {
                angle = previousCurvePoint.RightTangentAngle;
            }
            else
            {
                var dir = nextCurvePoint.CurvePoint.Position - previousCurvePoint.CurvePoint.Position;
                angle = (float)Math.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }

            Undo.RecordObject(_curvePointObject, "Tangent Angle");
            var rightTang = BezierMath.ConvertAngleToVector(angle, _curvePointObject.RightTangentMagnitude) + _curvePointObject.transform.position;
            _curvePointObject.RightTangentChanged(rightTang);
            RefreshGround();

        }

        GUI.backgroundColor = Color.orangeRed;
        if (GUILayout.Button("Reset Tangents", GUILayout.ExpandWidth(false)))
        {
            Undo.RecordObject(_curvePointObject, "Reset Curve Point Tangents");
            _curvePointObject.TangentsChanged(
                (Vector3)(_curvePointObject.CurvePoint.Position + new Vector3(-1, 0)),
                (Vector3)(_curvePointObject.CurvePoint.Position + new Vector3(-1, 0)));

            RefreshGround();
        }
        GUI.backgroundColor = originalColor;
        GUILayout.EndHorizontal();
        #endregion

        #region Add/Remove Buttons
        GUILayout.Space(10);
        GUILayout.Label("Add/Remove", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.skyBlue;
        if (GUILayout.Button("Add After", GUILayout.ExpandWidth(true)))
        {
            var ground = _curvePointObject.ParentGround;
            var index = _curvePointObject.transform.GetSiblingIndex() + 1;

            Selection.activeObject = _editManager.InsertCurvePoint(ground, index);
        }

        if (GUILayout.Button("Add Before", GUILayout.ExpandWidth(true)))
        {
            var ground = _curvePointObject.ParentGround;
            var index = _curvePointObject.transform.GetSiblingIndex();

            Selection.activeObject = _editManager.InsertCurvePoint(ground, index);
        }

        GUI.backgroundColor = Color.orangeRed;
        if (GUILayout.Button("Remove", GUILayout.ExpandWidth(true)))
        {
            _editManager.RemoveCurvePoint(_curvePointObject, true);
            return;
        }
        GUI.backgroundColor = originalColor;

        EditorGUILayout.EndHorizontal();
        #endregion

        #region Targeting Options

        GUILayout.Space(10);
        GUILayout.Label("Targeting", EditorStyles.boldLabel);

        //Camera targetting
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();

        var doTargetLow = GUILayout.Toggle(_curvePointObject.DoTargetLow, "Target Low", GUILayout.ExpandWidth(false));

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(_curvePointObject, "Curve Point Target Settings");
            _curvePointObject.DoTargetLow = doTargetLow;
            CameraTargetUtility.BuildGroundCameraTargets(_curvePointObject.ParentGround);
        }

        EditorGUILayout.EndHorizontal();

        if (_curvePointObject.DoTargetLow)
        {
            EditorGUI.BeginChangeCheck();

            _curvePointObject.LinkedCameraTarget.doUseManualOffset = GUILayout.Toggle(_curvePointObject.LinkedCameraTarget.doUseManualOffset, "Manual Offset", GUILayout.ExpandWidth(false));
            
            if(EditorGUI.EndChangeCheck())
            {
                Undo.RegisterFullObjectHierarchyUndo(_curvePointObject, "Curve Point Target Settings");
                _curvePointObject.LinkedCameraTarget.manualYOffset = _curvePointObject.LinkedCameraTarget.yOffset;

                if(!_curvePointObject.LinkedCameraTarget.doUseManualOffset)
                {
                    CameraTargetUtility.BuildGroundCameraTargets(_curvePointObject.ParentGround);
                }
            }

            if (_curvePointObject.LinkedCameraTarget.doUseManualOffset)
            {
                EditorGUI.BeginChangeCheck();

                var yOffset = EditorGUILayout.FloatField("Y Offset", _curvePointObject.LinkedCameraTarget.manualYOffset);
                _curvePointObject.LinkedCameraTarget.manualYOffset = Mathf.Max(CameraTargetUtility.MinYOffsetT, yOffset);

                if(EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterFullObjectHierarchyUndo(_curvePointObject, "Curve Point Target Settings");
                    CameraTargetUtility.BuildGroundCameraTargets(_curvePointObject.ParentGround);
                }
            }
            EditorGUI.BeginChangeCheck();

            _curvePointObject.LinkedCameraTarget.doUseManualOrthoSize = GUILayout.Toggle(_curvePointObject.LinkedCameraTarget.doUseManualOrthoSize, "Manual Zoom Ortho Size", GUILayout.ExpandWidth(false));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterFullObjectHierarchyUndo(_curvePointObject, "Curve Point Target Settings");
                _curvePointObject.LinkedCameraTarget.manualOrthoSize = _curvePointObject.LinkedCameraTarget.orthoSize;

                if (!_curvePointObject.LinkedCameraTarget.doUseManualOrthoSize)
                {
                    CameraTargetUtility.BuildGroundCameraTargets(_curvePointObject.ParentGround);
                }
            }

            if (_curvePointObject.LinkedCameraTarget.doUseManualOrthoSize)
            {
                EditorGUI.BeginChangeCheck();

                var zoomSize = EditorGUILayout.FloatField("Zoom Ortho Size", _curvePointObject.LinkedCameraTarget.manualOrthoSize);
                zoomSize = Mathf.Max(zoomSize, CameraTargetUtility.DefaultOrthoSize);
                _curvePointObject.LinkedCameraTarget.manualOrthoSize = zoomSize;

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterFullObjectHierarchyUndo(_curvePointObject, "Curve Point Target Settings");
                    CameraTargetUtility.BuildGroundCameraTargets(_curvePointObject.ParentGround);
                }
            }
        }

        #endregion

        #region Start/Finish Options
        GUILayout.Space(10);
        GUILayout.Label("Set Start/Finish", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.lightGreen;
        if (GUILayout.Button("Start", GUILayout.ExpandWidth(true)))
        {
            Debug.Log("Startpoint set to " + _curvePointObject.name);
            _groundManager.StartLine.SetStartLine(_curvePointObject.CurvePoint);
        }

        GUI.backgroundColor = Color.orange;
        if (GUILayout.Button("Finish", GUILayout.ExpandWidth(true)))
        {
            _groundManager.FinishLine.SetFlagPoint(_curvePointObject.CurvePoint);
            Debug.Log("Flagpoint set to " + _curvePointObject.name);

            var nextCurvePoint = NextCurvePoint(_curvePointObject);
            if (nextCurvePoint != null)
            {
                _groundManager.FinishLine.SetBackstopPoint(nextCurvePoint.CurvePoint);
                Debug.Log("Backstop point set to " + nextCurvePoint.name);
            }
            else
            {
                Debug.LogWarning("No next curve point found to set as backstop.");
            }
        }

        GUI.backgroundColor = Color.softYellow;
        if (GUILayout.Button("Flag", GUILayout.ExpandWidth(true)))
        {
            _groundManager.FinishLine.SetFlagPoint(_curvePointObject.CurvePoint);
            Debug.Log("Flagpoint set to " + _curvePointObject.name);
        }

        if (GUILayout.Button("Backstop", GUILayout.ExpandWidth(true)))
        {
            _groundManager.FinishLine.SetBackstopPoint(_curvePointObject.CurvePoint);
            Debug.Log("Backstop point set to " + _curvePointObject.name);
        }

        GUI.backgroundColor = originalColor;

        GUILayout.EndHorizontal();
        #endregion

        #region Floor Options
        if (_curvePointObject.ParentGround.FloorType == FloorType.Segmented)
        {
            GUILayout.Space(10);
            GUILayout.Label("Floor Options", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            int floorHeight = _curvePointObject.CurvePoint.FloorHeight;
            int floorAngle = _curvePointObject.CurvePoint.FloorAngle;

            var hasFloor = GUILayout.Toggle(_curvePointObject.CurvePoint.FloorPointType == FloorPointType.Set, "Has Floor Point");

            if (hasFloor) {
                floorHeight = EditorGUILayout.IntField("Floor Height", floorHeight);
                floorAngle = EditorGUILayout.IntField("Floor Angle", floorAngle);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_curvePointObject, "Change height and angle.");

                if (hasFloor)
                {
                    _curvePointObject.CurvePoint.FloorPointType = FloorPointType.Set;
                } else
                {
                    _curvePointObject.CurvePoint.FloorPointType = FloorPointType.None;
                }

                _curvePointObject.CurvePoint.FloorHeight = floorHeight;
                _curvePointObject.CurvePoint.FloorAngle = floorAngle;

                RefreshGround();
            }
        }
        #endregion

        #region Segment Breaks

        GUILayout.Space(10);
        GUILayout.Label("Segment Options", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();

        var forceSegment = GUILayout.Toggle(_curvePointObject.CurvePoint.ForceNewSegment, "Force New Segment", GUILayout.ExpandWidth(true));

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(_curvePointObject, "Curve Point Segment Settings");
            _curvePointObject.CurvePoint.ForceNewSegment = forceSegment;

            // If forcing a new segment, also ensure not blocking segments
            if (forceSegment)
            {
                _curvePointObject.CurvePoint.BlockNewSegment = false;
            }

            _editManager.UpdateEditorLevel();
        }

        EditorGUI.BeginChangeCheck();

        var blockSegment = GUILayout.Toggle(_curvePointObject.CurvePoint.BlockNewSegment, "Block New Segment", GUILayout.ExpandWidth(false));

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(_curvePointObject, "Curve Point Segment Settings");
            _curvePointObject.CurvePoint.BlockNewSegment = blockSegment;

            // If blocking segments, also ensure not forcing a new segment
            if (blockSegment)
            {
                _curvePointObject.CurvePoint.ForceNewSegment = false;
            }
            _editManager.UpdateEditorLevel();
        }

        EditorGUILayout.EndHorizontal();

        #endregion

    }


    #region Scene GUI
    public void OnSceneGUI()
    {
        //Set key held bools

        if (Event.current.control)
        {
            _controlHeld = true;
        } else if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.LeftControl)
        {
            _controlHeld = false;
        }
        if (Event.current.alt)
        {
            _altHeld = true;
        }
        else if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.LeftAlt)
        {
            _altHeld = false;
        }

        if (Event.current.keyCode == KeyCode.A && Event.current.type == EventType.KeyDown)
        {
            _aHeld = true;
        }
        else if (Event.current.keyCode == KeyCode.A && Event.current.type == EventType.KeyUp)
        {
            _aHeld = false;
        }

        var curvePointObject = (CurvePointEditObject)target;

        var startPos = curvePointObject.transform.position;


        if (!_aHeld && DrawCurvePointHandles(curvePointObject, _altHeld))
        {
            if (_editManager.editType == EditType.Shift || _controlHeld)
            {
                _editManager.ShiftCurvePoints(curvePointObject, curvePointObject.transform.position - startPos);
            }
            RefreshGround();
        }

        if (curvePointObject.LinkedCameraTarget.doLowTarget)
        {
            if (_aHeld)
            {
                EditManagerInspector.DrawCamTargetOptions(_editManager, curvePointObject);
            }

            if(curvePointObject.LinkedCameraTarget.PrevTarget != null)
            {
                DrawTargetInfo(curvePointObject.LinkedCameraTarget.PrevTarget);
            }

            DrawTargetInfo(curvePointObject.LinkedCameraTarget);

            if(curvePointObject.LinkedCameraTarget.NextTarget != null)
            {
                DrawTargetInfo(curvePointObject.LinkedCameraTarget.NextTarget);
            }
        }
    }



    public static bool DrawCurvePointHandles(CurvePointEditObject curvePointObject, bool altHeld = false)
    {
        var objectPosition = curvePointObject.transform.position;
        var objectRotation = curvePointObject.transform.rotation;
        var handleScale = HandleUtility.GetHandleSize(objectPosition) * .4f;
        var groundPosition = curvePointObject.ParentGround.transform.position;

        var handlesChanged = false;
        //Position handle

        if (curvePointObject.DoTargetLow)
        {
            Handles.color = Color.blue;
        }
        else
        {
            Handles.color = Color.white;
        }

        EditorGUI.BeginChangeCheck();

        var handlePosition = Handles.PositionHandle(
            objectPosition,
            objectRotation);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curvePointObject, "Curve Point Edit");
            curvePointObject.PositionChanged(handlePosition);
            handlesChanged = true;
        }

        //Floor handle
        if (curvePointObject.ParentGround.FloorType == FloorType.Segmented && curvePointObject.CurvePoint.FloorPointType == FloorPointType.Set)
        {
            Handles.color = Color.cyan;
            EditorGUI.BeginChangeCheck();
            var floorHandle = Handles.FreeMoveHandle(
                curvePointObject.CurvePoint.FloorPosition + groundPosition,
                handleScale,
                Vector3.zero,
                Handles.ArrowHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(curvePointObject, "Curve Point Edit");
                GroundSplineUtility.GetAngleAndMagFromPosition(
                    curvePointObject.transform.position,
                    floorHandle,
                    out var angle,
                    out var magnitude);
                curvePointObject.CurvePoint.FloorAngle = (int)angle;
                curvePointObject.CurvePoint.FloorHeight = (int)magnitude;
                handlesChanged = true;
            }
        }

        //Don't draw tangents if linear
        if (curvePointObject.CurvePoint.TangentMode == ShapeTangentMode.Linear)
        {
            return handlesChanged;
        }

        Handles.color = Color.red;

        Handles.DrawLine(objectPosition, curvePointObject.CurvePoint.LeftTangentPosition + groundPosition);
        Handles.DrawLine(objectPosition, curvePointObject.CurvePoint.RightTangentPosition + groundPosition);

        Handles.color = Color.yellow;

        //Left tangent handle
        EditorGUI.BeginChangeCheck();

        var leftTangentHandle = Handles.FreeMoveHandle(
            curvePointObject.CurvePoint.LeftTangentPosition + groundPosition,
            handleScale,
            Vector3.zero,
            Handles.ArrowHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curvePointObject, "Curve Point Edit");

            //If alt is held, lock tangent angle and only change magnitude
            if (altHeld)
            {
                leftTangentHandle = BezierMath.GetPerpendicularIntersection(
                    curvePointObject.CurvePoint.Position,
                    curvePointObject.CurvePoint.LeftTangentPosition,
                    leftTangentHandle);
            }

            curvePointObject.LeftTangentChanged(leftTangentHandle);
            handlesChanged = true;
        }


        //Right tangent handle
        EditorGUI.BeginChangeCheck();

        var rightTangentHandle = Handles.FreeMoveHandle(
            curvePointObject.CurvePoint.RightTangentPosition + groundPosition,
            handleScale,
            Vector3.zero,
            Handles.ArrowHandleCap);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(curvePointObject, "Curve Point Edit");

            //If alt is held, lock tangent angle and only change magnitude
            if (altHeld)
            {
                rightTangentHandle = BezierMath.GetPerpendicularIntersection(
                    curvePointObject.CurvePoint.Position,
                    curvePointObject.CurvePoint.RightTangentPosition,
                    rightTangentHandle);
            }

            curvePointObject.RightTangentChanged(rightTangentHandle);
            handlesChanged = true;
        }



        return handlesChanged;
    }

    public static void DrawCamTargetButtons(CurvePointEditObject currentCPObj, CurvePointEditObject targetCPObj)
    {
        var targetObj = targetCPObj.gameObject;

        if (targetCPObj == null)
        {
            return;
        }

        var objPos = targetObj.transform.position;

        var rect = HandleUtility.WorldPointToSizedRect(objPos, rightTargetButton, buttonStyle);
        float xMod = targetCPObj.DoTargetLow ? 0.5f : 0;
        rect.position = new Vector2(rect.position.x - rect.width * xMod, rect.position.y + rect.height);

        Handles.BeginGUI();

        if (xMod != 0)
        {
            rect.position = new Vector2(rect.position.x + rect.width * 1.1f, rect.position.y);
        }

        Handles.EndGUI();
    }

    public static void DrawCamBottomIntercept(CurvePointEditObject cpObj)
    {
        var camBottom = CameraTargetUtility.GetCamBottomIntercept((float)cpObj.CurvePoint.Position.x, cpObj.ParentGround);

        Handles.color = Color.orange;
        Handles.SphereHandleCap(0, camBottom, Quaternion.identity, 1f, EventType.Repaint);
        Handles.DrawLine(camBottom, (Vector3)cpObj.CurvePoint.Position);
    }

    public static void DrawTargetInfo(LinkedCameraTarget target)
    {
        if (target == null || !target.doLowTarget)
        {
            return;
        }
        var camCenterX = target.Position.x - (CameraManager.minXOffset/2);

        var camBottomY = target.CamBottomPosition.y;
        var camTopY = camBottomY + (2 * target.orthoSize);
        var camLeftX = camCenterX - target.orthoSize * CameraTargetUtility.DefaultAspectRatio;
        var camRightX = camCenterX + target.orthoSize * CameraTargetUtility.DefaultAspectRatio;

        var camTopLeft = new Vector3(camLeftX, camTopY);
        var camTopRight = new Vector3(camRightX, camTopY);
        var camBottomLeft = new Vector3(camLeftX, camBottomY);
        var camBottomRight = new Vector3(camRightX, camBottomY);

        //Draw camera box
        if(target.doUseManualOffset || target.doUseManualOrthoSize)
        {
            Handles.color = Color.yellow;
        }
        else
        {
            Handles.color = Color.yellowGreen;
        }
        Handles.DrawLine(camTopLeft, camTopRight);
        Handles.DrawLine(camTopRight, camBottomRight);
        Handles.DrawLine(camBottomRight, camBottomLeft);
        Handles.DrawLine(camBottomLeft, camTopLeft);

        //Draw to prev/next targets with offset size
        Handles.color = Color.magenta;
        if (target.PrevTarget != null)
        {
            Handles.DrawLine(target.CamBottomPosition, target.PrevTarget.CamBottomPosition);
        }

        Handles.color = Color.cyan;
        if (target.NextTarget != null)
        {
            Handles.DrawLine(target.CamBottomPosition, target.NextTarget.CamBottomPosition);
        }

        //Draw unoffset target positions

        Handles.color = Color.beige;

        if (target.NextTarget != null)
        {
            Handles.DrawLine(target.Position, target.NextTarget.Position);
        }


        //Draw offset target positions
        Handles.color = Color.yellow;
        Handles.SphereHandleCap(0, target.CamBottomPosition, Quaternion.identity, 1f, EventType.Repaint);
    }

    #endregion

    #region Utilities
    private static CurvePointEditObject NextCurvePoint(CurvePointEditObject currentCurvePoint)
    {
        var curvePointObjects = currentCurvePoint.ParentGround.CurvePointObjects;
        var index = currentCurvePoint.transform.GetSiblingIndex();

        if (index < curvePointObjects.Length - 1)
        {
            return curvePointObjects[index + 1];
        }

        return null;
    }

    private static CurvePointEditObject PreviousCurvePoint(CurvePointEditObject currentCurvePoint)
    {
        var curvePointObjects = currentCurvePoint.ParentGround.CurvePointObjects;
        var index = currentCurvePoint.transform.GetSiblingIndex();
        if (index > 0)
        {
            return curvePointObjects[index - 1];
        }
        return null;
    }

    private void RefreshGround()
    {
        Undo.RecordObject(_curvePointObject.gameObject, "Refresh ground.");
        _editManager.RefreshSerializable(_curvePointObject.ParentGround);
    }
    #endregion
}