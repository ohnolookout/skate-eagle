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

    GroundSpawner _groundConstructor;
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
        _groundConstructor = FindAnyObjectByType<GroundSpawner>();

        _selectedObject = Selection.activeGameObject;
        OnSelectionChanged();

        Selection.selectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        _groundConstructor = null;
        Selection.selectionChanged -= OnSelectionChanged;
    }

    #region GUI
    private void OnGUI()
    {
        if(_groundConstructor == null)
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
            _groundConstructor.AddSegment(_ground);
        }
        if (GUILayout.Button("Add Segment to Front"))
        {
            _groundConstructor.AddSegmentToFront(_ground);
        }
        if (GUILayout.Button("Remove Segment"))
        {
            _groundConstructor.RemoveSegment(_ground);
        }
        if(GUILayout.Button("Recalculate Segments"))
        {
            _groundConstructor.RecalculateSegments(_ground, 0);
        }
        if (GUILayout.Button("Delete Ground"))
        {
            _groundConstructor.RemoveGround(_selectedObject.GetComponent<Ground>());
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
            _groundConstructor.RefreshCurve(_segment);
            _groundConstructor.RecalculateSegments(_segment);
        }
        if (GUILayout.Button("Duplicate"))
        {
            _groundConstructor.DuplicateSegment(_segment);
        }

        if (GUILayout.Button("Reset"))
        {
            _groundConstructor.ResetSegment(_segment);
        }

        if (GUILayout.Button("Delete"))
        {
            _groundConstructor.RemoveSegment(_segment);
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
            Selection.activeGameObject = _groundConstructor.AddGround().gameObject;
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
