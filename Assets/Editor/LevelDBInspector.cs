using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
using log4net.Core;
using System.Linq;
[CustomEditor(typeof(LevelDatabase))]

public class LevelDBInspector : Editor
{
    private SerializedObject _so;
    private Dictionary<string, bool> _doPublishDictionary;
    private SerializedProperty _levelOrder;
    private LevelDatabase _levelDatabase;

    private void OnEnable()
    {
        _so = new SerializedObject(target);
        _levelOrder = _so.FindProperty("_levelOrder");
        _levelDatabase = (LevelDatabase)target;
        BuildDoPublishDict();
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        
        EditorGUILayout.PropertyField(_levelOrder, true);

        if(EditorGUI.EndChangeCheck())
        {
            _so.ApplyModifiedProperties();
            BuildDoPublishDict();
        }

        EditorGUI.BeginChangeCheck();

        var levelNames = _levelDatabase.NameToUIDDictionary.Keys.ToList();
        foreach (var name in levelNames)
        {
            _doPublishDictionary[name] = EditorGUILayout.Toggle(name, _doPublishDictionary[name]);
        }

        if (EditorGUI.EndChangeCheck())
        {
            _so.ApplyModifiedProperties();
            UpdateLevelOrder();
        }



    }

    private void BuildDoPublishDict()
    {
        _doPublishDictionary = new();
        foreach (var levelName in _levelDatabase.NameToUIDDictionary.Keys)
        {
            var doPublish = _levelDatabase.LevelOrder.Contains(levelName);
            _doPublishDictionary[levelName] = doPublish;
        }

    }

    private void UpdateLevelOrder()
    {
        foreach (var levelName in _doPublishDictionary.Keys)
        {
            if (_doPublishDictionary[levelName])
            {
                if (!_levelDatabase.LevelOrder.Contains(levelName))
                {
                    _levelDatabase.LevelOrder.Add(levelName);
                }
            }
            else
            {
                _levelDatabase.LevelOrder.Remove(levelName);
            }
        }

        _so = new SerializedObject(target);
        _levelOrder = _so.FindProperty("_levelOrder");
        //_levelDatabase = (LevelDatabase)target;
        //BuildDoPublishDict();
    }
}

