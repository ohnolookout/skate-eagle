using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

[Serializable]
public class SerializedGround : IDeserializable
{
    public Vector2 position;
    public Quaternion rotation;
    public string name;
    public bool isFloating = false;
    public bool isInverted = false;
    public bool hasShadow = true;
    public SerializedGroundSegment editorSegment; //Single segment for editing
    public List<SerializedGroundSegment> segmentList; //Divided segments for runtime
    public List<CurvePoint> curvePoints;
    //public List<IDeserializable> serializedObjectList;

    public SerializedGround(string name, Vector2 position, List<SerializedGroundSegment> segmentList, SerializedGroundSegment editSegment, List<CurvePoint> curvePointList)
    {
        this.name = name;
        this.position = position;
        this.segmentList = new();
        this.curvePoints = curvePointList;

        foreach (var segment in segmentList)
        {
            this.segmentList.Add(segment as SerializedGroundSegment);
        }
    }

    public ISerializable Deserialize(GameObject targetObject, GameObject contextObject)
    {
        var ground = targetObject.GetComponent<Ground>();
        var groundManager = contextObject.GetComponent<GroundManager>();

        if (ground == null)
        {
            Debug.LogWarning("SerializedGround: Deserialize called on a GameObject that does not have a Ground component.");
            return null;
        }

        if (groundManager == null)
        {
            Debug.LogWarning("SerializedGround: Deserialize called with a context GameObject that does not have a GroundManager component.");
            return null;
        }

        ground.name = name;
        
        if (Application.isPlaying)
        {
            DeserializeRuntimeSegments(groundManager, ground);
        }
        else
        {
            DeserializeEditSegment(groundManager, ground);
        }

        //ground.SegmentList = new();

        //foreach (var serializedSegment in segmentList)
        //{
        //    var segment = groundManager.groundSpawner.AddEmptySegment(ground);
        //    serializedSegment.Deserialize(segment.gameObject, ground.gameObject);

        //    if (segment.NextLeftSegment != null)
        //    {
        //        segment.NextLeftSegment.NextRightSegment = segment;
        //    }
        //    ground.SegmentList.Add(segment);

        //    if (segment.IsStart)
        //    {
        //        groundManager.StartSegment = segment;
        //    }

        //    if (segment.IsFinish)
        //    {
        //        groundManager.FinishSegment = segment;
        //    }


        //    segment.gameObject.SetActive(false);
        //    segment.gameObject.SetActive(true);
        //}

#if UNITY_EDITOR
        //curvePointList = SerializeLevelUtility.GenerateCurvePointListFromGround(this);

        foreach (var curvePoint in curvePoints)
        {
            ground.AddCurvePointEditObject(curvePoint);
        }
#endif

        return ground;
    }

    private void DeserializeEditSegment(GroundManager groundManager, Ground ground)
    {
        var segment = groundManager.groundSpawner.AddEmptySegment(ground);
        editorSegment.Deserialize(segment.gameObject, ground.gameObject);
    }

    private void DeserializeRuntimeSegments(GroundManager groundManager, Ground ground)
    {
        foreach (var serializedSegment in segmentList)
        {
            var segment = groundManager.groundSpawner.AddEmptySegment(ground);
            serializedSegment.Deserialize(segment.gameObject, ground.gameObject);
            segment.gameObject.SetActive(false);
            segment.gameObject.SetActive(true);
        }
    }

}
