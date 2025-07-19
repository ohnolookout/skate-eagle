using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
[CustomEditor(typeof(LevelDatabase))]

public class LevelDBInspector : Editor
{
    private SerializedObject _so;
    private Dictionary<string, bool> _doPublishDictionary;
    private SerializedProperty _levelOrder;
    private LevelDatabase _levelDB;
    private CopyLevelWindow _copyLevelWindow;

    private void OnEnable()
    {
        _so = new SerializedObject(target);
        _levelOrder = _so.FindProperty("_levelOrder");
        _levelDB = (LevelDatabase)target;
        BuildDoPublishDict();
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Label("Level Order", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(_levelOrder, true);

        if(EditorGUI.EndChangeCheck())
        {
            _so.ApplyModifiedProperties();
            _levelDB.LevelOrderIsDirty = true;
            BuildDoPublishDict();
        }

        GUILayout.Space(20);

        GUILayout.Label("Levels To Publish", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        var levelNames = _levelDB.NameToUIDDictionary.Keys.ToList();
        foreach (var name in levelNames)
        {
            _doPublishDictionary[name] = EditorGUILayout.Toggle(name, _doPublishDictionary[name]);
        }

        if (EditorGUI.EndChangeCheck())
        {
            _so.ApplyModifiedProperties();
            UpdateLevelOrder();
        }

        GUILayout.Space(20);

        GUILayout.Label("Utilities", EditorStyles.boldLabel);


        if (GUILayout.Button("Clean Up DB", GUILayout.ExpandWidth(false)))
        {
            CleanUpDicts();
            BuildDoPublishDict();
            UpdateLevelOrder();
        }

        if (GUILayout.Button("Copy Level", GUILayout.ExpandWidth(false)))
        {
            _copyLevelWindow = EditorWindow.GetWindow<CopyLevelWindow>();
            _copyLevelWindow.Init(_levelDB);
        }

        //Reserialization utility for old level format

        //if (GUILayout.Button("Update Serialization Format", GUILayout.ExpandWidth(false)))
        //{
        //    UpdateSerializationFormat();
        //}


        //Curve points utility for old ground system
        if (GUILayout.Button("Populate Curve Points", GUILayout.ExpandWidth(false)))
        {
            PopulateGroundCurvePoints();
        }

        if (GUILayout.Button("Populate Segment Curve Points", GUILayout.ExpandWidth(false)))
        {
            foreach (var level in _levelDB.LevelDictionary.Values)
            {
                level.PopulateSegmentCurvePoints();
            }
            EditorUtility.SetDirty(_levelDB);
        }

        if (GUILayout.Button("Fix Medal Defaults", GUILayout.ExpandWidth(false)))
        {
            FixMedalDefaults();
        }

        _levelDB.LevelOrderIsDirty = EditorGUILayout.Toggle("Level Order Dirty", _levelDB.LevelOrderIsDirty);


    }

    private void BuildDoPublishDict()
    {
        _doPublishDictionary = new();
        foreach (var levelName in _levelDB.NameToUIDDictionary.Keys)
        {
            var doPublish = _levelDB.LevelOrder.Contains(levelName);
            _doPublishDictionary[levelName] = doPublish;
        }
    }

    private void UpdateLevelOrder()
    {
        foreach (var levelName in _doPublishDictionary.Keys)
        {
            if (_doPublishDictionary[levelName])
            {
                if (!_levelDB.LevelOrder.Contains(levelName))
                {
                    _levelDB.LevelOrder.Add(levelName);
                }
            }
            else
            {
                _levelDB.LevelOrder.Remove(levelName);
            }
        }

        _so = new SerializedObject(target);
        _levelOrder = _so.FindProperty("_levelOrder");

        _levelDB.LevelOrderIsDirty = true;
    }
    public void CleanUpDicts()
    {
        var activeUIDs = _levelDB.LevelDictionary.Keys.ToList();
        List<string> activeNames = new();
        foreach (var UID in activeUIDs)
        {
            var level = _levelDB.LevelDictionary[UID];

            if (level == null || level.Name == null)
            {
                _levelDB.LevelDictionary.Remove(UID);
                _levelDB.UIDToNameDictionary.Remove(UID);
                EditorUtility.SetDirty(_levelDB);
                continue;
            }

            if (level.Name != null)
            {
                activeNames.Add(level.Name);
                EditorUtility.SetDirty(_levelDB);
            }

            if (!_levelDB.NameToUIDDictionary.ContainsKey(level.Name))
            {
                _levelDB.NameToUIDDictionary[level.Name] = UID;
                EditorUtility.SetDirty(_levelDB);
            }

            if (!_levelDB.UIDToNameDictionary.ContainsKey(UID))
            {
                _levelDB.UIDToNameDictionary[UID] = level.Name;
                EditorUtility.SetDirty(_levelDB);
            }
        }

        var namesDictNames = _levelDB.NameToUIDDictionary.Keys.ToList();

        foreach (var name in namesDictNames)
        {
            if (!activeNames.Contains(name))
            {
                _levelDB.UIDToNameDictionary.Remove(_levelDB.NameToUIDDictionary[name]);
                _levelDB.NameToUIDDictionary.Remove(name);
                EditorUtility.SetDirty(_levelDB);
            }
        }

        var UIDsDictUIDs = _levelDB.UIDToNameDictionary.Keys.ToList();

        foreach (var uid in UIDsDictUIDs)
        {
            if (!_levelDB.LevelDictionary.ContainsKey(uid))
            {
                _levelDB.NameToUIDDictionary.Remove(_levelDB.UIDToNameDictionary[uid]);
                _levelDB.UIDToNameDictionary.Remove(uid);
                EditorUtility.SetDirty(_levelDB);
            }
        }

        var orderNames = _levelDB.LevelOrder.ToList();

        foreach (var name in orderNames)
        {
            if (!activeNames.Contains(name))
            {
                _levelDB.LevelOrder.Remove(name);
                EditorUtility.SetDirty(_levelDB);
            }
        }
        _so = new SerializedObject(target);
        _levelOrder = _so.FindProperty("_levelOrder");
    }

    private void FixMedalDefaults()
    {
        int levelUpdatedCount = 0;
        foreach (var level in _levelDB.LevelDictionary.Values)
        {
            float bronzeTime = level.MedalTimes.Bronze;
            float silverTime = level.MedalTimes.Silver;
            float goldTime = level.MedalTimes.Gold;
            float blueTime = level.MedalTimes.Blue;
            float redTime = level.MedalTimes.Red;

            bool changedMedal = false;
            if (redTime <= 0)
            {
                changedMedal = true;
                redTime = 6;
            }
            if (level.MedalTimes.Blue <= 0)
            {
                changedMedal = true;
                blueTime = 8;
            }
            if (level.MedalTimes.Gold <= 0)
            {
                changedMedal = true;
                goldTime = 10;
            }
            if (level.MedalTimes.Silver <= 0)
            {
                changedMedal = true;
                silverTime = 14;
            }
            if (level.MedalTimes.Bronze <= 0)
            {
                changedMedal = true;
                bronzeTime = 20;
            }            

            if (changedMedal)
            {
                level.MedalTimes = new MedalTimes(bronzeTime, silverTime, goldTime, blueTime, redTime);

                levelUpdatedCount++;
            }
        }

        if (levelUpdatedCount > 0)
        {
            EditorUtility.SetDirty(_levelDB);
        }            

        Debug.Log("Updated medals in " + levelUpdatedCount + " levels.");
    }

    //private void UpdateSerializationFormat()
    //{
    //    var levels = _levelDB.LevelDictionary.Values.ToList();
    //    foreach (var level in levels)
    //    {
    //        level.Reserialize();
    //    }
    //    EditorUtility.SetDirty(_levelDB);
    //}

    private void PopulateGroundCurvePoints()
    {
        var levels = _levelDB.LevelDictionary.Values.ToList();
        foreach (var level in levels)
        {
            level.PopulateGroundCurvePoints();
        }
        EditorUtility.SetDirty(_levelDB);
    }

}

