using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public enum  FloorPointType
{
    None,
    Set,
    Auto
}

[Serializable]
public class CurvePoint: IResyncable
{
    public string name = "CP";
    [SerializeField] private Vector3 position, leftTangent, rightTangent; //Tangents are relative to the position
    [SerializeField] private Vector3 _serializedWorldPosition;
    [SerializeField] private ShapeTangentMode _mode;
    [SerializeField] private FloorPointType _floorPointType = FloorPointType.None;
    [SerializeField] private bool _isSymmetrical = true;
    [SerializeField] private int _floorHeight;
    [SerializeField] private int _floorAngle;
    [SerializeField] private Vector3 _floorPosition;
    [SerializeField] private bool _forceNewSegment;
    [SerializeField] private bool _blockNewSegment;
    [SerializeField] private ResyncRef<CurvePointEditObject> _cpObjectRef = new();
    [SerializeField] private GameObject _object;
    [SerializeField] private LinkedCameraTarget _linkedCameraTarget;
    public string UID {get; set;}

    public GameObject Object
    {
        get => _object; 
        set
        {
            _object = value;
            _cpObjectRef.Value = value.GetComponent<CurvePointEditObject>();
        }
    }
    public LinkedCameraTarget LinkedCameraTarget { get => _linkedCameraTarget; set => _linkedCameraTarget = value; }
    public Vector3 Position { get => position; set => position = value; }
    public Vector3 WorldPosition
    {
        get
        {
            if(_object != null)
            {
                return _object.transform.position;
            }

            return _serializedWorldPosition;
        }
        set { _serializedWorldPosition = value; }
    }
    public Vector3 LeftTangent 
    { 
        get 
        {
            if(_mode == ShapeTangentMode.Linear)
            {
                return new(0, 0);
            }
            return leftTangent;
        }
        set => leftTangent = value; 
    }
    public Vector3 RightTangent 
    {
        get
        {
            if (_mode == ShapeTangentMode.Linear)
            {
                return new(0, 0);
            }
            return rightTangent;
        }
        set => rightTangent = value; 
    }
    public Vector3 LeftTangentPosition => position + leftTangent;
    public Vector3 RightTangentPosition => position + rightTangent;
    public ShapeTangentMode TangentMode { get => _mode; set => _mode = value; }
    public bool IsSymmetrical { get => _isSymmetrical; set => _isSymmetrical = value; }
    public bool ForceNewSegment { get => _forceNewSegment; set => _forceNewSegment = value; }
    public bool BlockNewSegment { get => _blockNewSegment; set => _blockNewSegment = value; }
    public int FloorHeight { get => _floorHeight; set => _floorHeight = value; }
    public int FloorAngle { get => _floorAngle; set => _floorAngle = value; }
    public Vector3 FloorPosition { get => _floorPosition; set => _floorPosition = value; }
    public FloorPointType FloorPointType { get => _floorPointType; set => _floorPointType = value; }
    public ResyncRef<CurvePointEditObject> CPObjRef { get => _cpObjectRef; }

    public CurvePoint()
    {
        position = Vector3.zero;
        leftTangent = new(-1, 0);
        rightTangent = new(1, 0);
        _mode = ShapeTangentMode.Continuous;
        _isSymmetrical = false;
        _floorHeight = 100;
        _floorAngle = 0;
        _forceNewSegment = false;
        _blockNewSegment = false;
        _linkedCameraTarget = new();
    }
    public CurvePoint(Vector3 control, ShapeTangentMode mode = ShapeTangentMode.Continuous, bool isSymmetrical = false)
    {
        position = control;
        leftTangent = new(0, 0);
        rightTangent = new(0, 0);
        _mode = mode;
        _isSymmetrical = isSymmetrical;
        _floorHeight = 100;
        _floorAngle = 0;
        _forceNewSegment = false;
        _blockNewSegment = false;
        _linkedCameraTarget = new();

    }

    public CurvePoint(Vector3 control, Vector3 lTang, Vector3 rTang, ShapeTangentMode mode = ShapeTangentMode.Continuous, bool isSymmetrical = false)
    {
        position = control;
        leftTangent = lTang;
        rightTangent = rTang;
        _mode = mode;
        _isSymmetrical = isSymmetrical;
        _floorHeight = 100;
        _floorAngle = 0;
        _forceNewSegment = false;
        _blockNewSegment = false;
        _linkedCameraTarget = new();
    }


    public void SetTangents(float slope, float velocity)
    {
        leftTangent = new Vector3(-velocity, -velocity * slope);
        rightTangent = new Vector3(velocity, velocity * slope);
    }

    public void SetTangents(Vector3 tangent)
    {
        leftTangent = -tangent;
        rightTangent = tangent;
    }

    public void SetTangents(Vector3 leftTangent, Vector3 rightTangent)
    {
        this.leftTangent = leftTangent;
        this.rightTangent = rightTangent;
    }

    public void SetLeftTangentAngle(float angle, Vector2 endPoint, float? magnitude = null)
    {
        var mag = magnitude ?? leftTangent.magnitude;
        leftTangent = BezierMath.GetTangentFromAngle(Position, endPoint, angle, mag);
    }

    public void SetRightTangentAngle(float angle, Vector2 endPoint, float? magnitude = null)
    {
        var mag = magnitude ?? rightTangent.magnitude;
        rightTangent = BezierMath.GetTangentFromAngle(Position, endPoint, angle, mag);
    }

    public void SetTangentAngles(float angle, Vector2 endPoint, bool doReverse, float? magnitude = null)
    {

        if (doReverse)
        {
            angle = -angle;
            SetRightTangentAngle(angle + 180, endPoint, magnitude);
            SetLeftTangentAngle(angle, endPoint, magnitude);
            return;
        }

        SetLeftTangentAngle(angle + 180, endPoint, magnitude);
        SetRightTangentAngle(angle, endPoint, magnitude);
    }

    public CurvePoint Move(Vector3 move)
    {
        return new(position + move, leftTangent, rightTangent);
    }

    public void Log()
    {
        Debug.Log("~~~Curve Point~~~");
        Debug.Log($"Control Point: {position}");
        Debug.Log($"Left Tangent: {leftTangent}");
        Debug.Log($"Right Tangent: {rightTangent}");

    }

    //Returns the tangent that is adjusted relative to the given tangent
    public static Vector2 AdjustedTangent(Vector2 tangent, Vector2 baseline)
    {
        float m1 = tangent.y / tangent.x;
        float m2 = baseline.y / baseline.x;

        float magnitude = tangent.magnitude;

        // Handle perpendicular case
        if (1 + m1 * m2 == 0)
        {
            return new Vector2(-tangent.y, tangent.x); // Rotate 90 degrees
        }

        float angleSlope = (m2 - m1) / (1 + m1 * m2);
        Vector2 direction = new Vector2(1, angleSlope).normalized;

        return direction * magnitude;
    }

    public void SaveWorldPosition()
    {
        if (_object != null)
        {
            _serializedWorldPosition = _object.transform.position;
        }
    }

    public CurvePoint DeepCopy()
    {
        CurvePoint copy = new CurvePoint();
        copy.Position = position;
        copy.LeftTangent = leftTangent;
        copy.RightTangent = rightTangent;
        copy.TangentMode = _mode;
        copy.IsSymmetrical = _isSymmetrical;
        copy.FloorHeight = _floorHeight;
        copy.FloorAngle = _floorAngle;
        copy.FloorPointType = _floorPointType;
        copy.FloorPosition = _floorPosition;
        copy.ForceNewSegment = _forceNewSegment;
        copy.BlockNewSegment = _blockNewSegment;
        copy.UID = UID;

        return copy;


    }

    public void RegisterResync()
    {
        if (_object != null)
        {
            _cpObjectRef.Value = _object.GetComponent<CurvePointEditObject>();
        }

        if (_linkedCameraTarget != null)
        {
            _linkedCameraTarget.RegisterResync();
        }
        LevelManager.ResyncHub.RegisterResync(this);
    }
}
