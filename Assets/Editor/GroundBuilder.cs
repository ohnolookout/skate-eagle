using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Codice.Client.Common.GameUI;
using static UnityEngine.Rendering.HableCurve;

public class GroundBuilder : EditorWindow
{
    #region Declarations
    private enum GroundBuilderState
    {
        GroundSelected,
        GroundSegmentSelected,
        NoGroundSelected
    }

    GroundSpawner _spawner;
    GroundManager _manager;
    GroundBuilderState _selectionState;
    GameObject _selectedObject;
    SerializedObject _so;
    SerializedProperty _serializedCurve;
    GroundSegment _segment;
    Ground _ground;


    #endregion
    [MenuItem("Tools/GroundBuilder")]
    public static void ShowWindow()
    {
        GetWindow<GroundBuilder>();
    }

    private void OnEnable()
    {
        _spawner = FindAnyObjectByType<GroundSpawner>();
        _manager = FindAnyObjectByType<GroundManager>();

        _selectedObject = Selection.activeGameObject;
        OnSelectionChanged();

        Selection.selectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        _spawner = null;
        _manager = null;
        _selectedObject = null;
        Selection.selectionChanged -= OnSelectionChanged;
    }

    #region GUI
    private void OnGUI()
    {
        if(_spawner == null)
        {
            EditorGUILayout.HelpBox("No GroundConstructor found in scene. Please add one to continue.", MessageType.Warning);
            return;
        }

        switch(_selectionState)
        {
            case GroundBuilderState.GroundSelected:
                OnGroundSelected();
                break;
            case GroundBuilderState.GroundSegmentSelected:
                OnGroundSegmentSelected();
                break;
            case GroundBuilderState.NoGroundSelected:
                OnNoGroundSelected();
                break;
        }
    }

    private void OnGroundSelected()
    {
        if (_ground == null)
        {
            OnSelectionChanged();
            return;
        }

        if (GUILayout.Button("Add Segment"))
        {
            _spawner.AddSegment(_ground);
        }
        if (GUILayout.Button("Add Segment to Front"))
        {
            _spawner.AddSegmentToFront(_ground);
        }
        if (GUILayout.Button("Remove Segment"))
        {
            _spawner.RemoveSegment(_ground);
        }
        if(GUILayout.Button("Recalculate Segments"))
        {
            _spawner.RecalculateSegments(_ground, 0);
        }
        if (GUILayout.Button("Delete Ground"))
        {
            _spawner.RemoveGround(_selectedObject.GetComponent<Ground>());
        }
        if (GUILayout.Button("Add Start"))
        {
            var segment = _spawner.AddSegmentToFront(_selectedObject.GetComponent<Ground>(), _spawner.DefaultStart());
            _manager.SetStartPoint(segment, 1);
        }
        if (GUILayout.Button("Add Finish"))
        {
            var segment = _spawner.AddSegment(_selectedObject.GetComponent<Ground>(), _spawner.DefaultFinish());
            _manager.SetFinishPoint(segment, 1);
        }
    }

    private void OnGroundSegmentSelected()
    {
        if(_segment == null)
        {
            OnSelectionChanged();
            return;
        }
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_serializedCurve, true);

        _so.ApplyModifiedProperties();
        _so.Update();

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(_segment, "Curve Change");
            _spawner.RefreshCurve(_segment);
            _spawner.RecalculateSegments(_segment);
        }
        if (GUILayout.Button("Duplicate"))
        {
            _spawner.DuplicateSegment(_segment);
        }

        if (GUILayout.Button("Reset"))
        {
            _spawner.ResetSegment(_segment);
        }

        if (GUILayout.Button("Delete"))
        {
            _spawner.RemoveSegment(_segment);
            if (_segment.PreviousSegment != null)
            {
                Selection.activeGameObject = _segment.PreviousSegment.gameObject;
            } else
            {
                Selection.activeGameObject = _segment.parentGround.gameObject;
            }
            return;
        }
    }

    private void OnNoGroundSelected()
    {
        if(GUILayout.Button("Add Ground"))
        {
            Selection.activeGameObject = _spawner.AddGround().gameObject;
        }
    }
    #endregion

    #region State Mgmt

    private void OnSelectionChanged()
    {
        _selectedObject = Selection.activeGameObject;
        _segment = null;
        _ground = null;

        if (_selectedObject == null)
        {
            _selectionState = GroundBuilderState.NoGroundSelected;
        }
        else
        {
            _so = new(_selectedObject);

            if (_selectedObject.GetComponent<Ground>() != null)
            {
                _selectionState = GroundBuilderState.GroundSelected;
                _ground = _selectedObject.GetComponent<Ground>();
                return;
            } 
            else if (_selectedObject.GetComponent<GroundSegment>() != null)
            {
                _segment = _selectedObject.GetComponent<GroundSegment>();
                _selectionState = GroundBuilderState.GroundSegmentSelected;
                _so = new(_segment);
                _serializedCurve = _so.FindProperty("curve");
                return;
            }
            else
            {
                _selectionState = GroundBuilderState.NoGroundSelected;
            }

        }
        Repaint();
    }
    #endregion
}
