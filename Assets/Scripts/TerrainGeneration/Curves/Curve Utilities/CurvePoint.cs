using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[Serializable]
public class CurvePoint
{
    [SerializeField] private Vector3 position, leftTangent, rightTangent; //Tangents are relative to the position
    [SerializeField] private Vector3 _serializedWorldPosition;
    [SerializeField] private ShapeTangentMode _mode;
    [SerializeField] private bool _isSymmetrical = true;
    [SerializeField] private int _floorHeight;
    [SerializeField] private int _floorAngle;
    [SerializeField] private bool _forceNewSection;
    [SerializeField] private bool _blockNewSection;
    [SerializeField] private CurvePointObject _object;
    [SerializeField] private LinkedCameraTarget _linkedCameraTarget;

    public CurvePointObject Object { get => _object; set => _object = value; }
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
    public Vector3 LeftTangent { get => leftTangent; set => leftTangent = value; }
    public Vector3 RightTangent { get => rightTangent; set => rightTangent = value; }
    public Vector3 LeftTangentPosition => position + leftTangent;
    public Vector3 RightTangentPosition => position + rightTangent;
    public ShapeTangentMode Mode { get => _mode; set => _mode = value; }
    public bool IsSymmetrical { get => _isSymmetrical; set => _isSymmetrical = value; }
    public bool ForceNewSection { get => _forceNewSection; set => _forceNewSection = value; }
    public bool BlockNewSection { get => _blockNewSection; set => _blockNewSection = value; }
    public int FloorHeight { get => _floorHeight; set => _floorHeight = value; }
    public int FloorAngle { get => _floorAngle; set => _floorAngle = value; }

    public CurvePoint()
    {
        position = Vector3.zero;
        leftTangent = new(-1, 0);
        rightTangent = new(1, 0);
        _mode = ShapeTangentMode.Continuous;
        _isSymmetrical = false;
        _floorHeight = 100;
        _floorAngle = 0;
        _forceNewSection = false;
        _blockNewSection = false;
    }
    public CurvePoint(Vector3 control, ShapeTangentMode mode = ShapeTangentMode.Continuous, bool isSymmetrical = false, bool isCorner = false)
    {
        position = control;
        leftTangent = new(0, 0);
        rightTangent = new(0, 0);
        _mode = mode;
        _isSymmetrical = isSymmetrical;
        _floorHeight = 100;
        _floorAngle = 0;
        _forceNewSection = false;
        _blockNewSection = false;

    }

    public CurvePoint(Vector3 control, Vector3 lTang, Vector3 rTang, ShapeTangentMode mode = ShapeTangentMode.Continuous, bool isSymmetrical = false, bool isCorner = false)
    {
        position = control;
        leftTangent = lTang;
        rightTangent = rTang;
        _mode = mode;
        _isSymmetrical = isSymmetrical;
        _floorHeight = 100;
        _floorAngle = 0;
        _forceNewSection = false;
        _blockNewSection = false;
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

    public CurvePoint DeepCopy()
    {
        CurvePoint copy = new CurvePoint();
        copy.Position = position;
        copy.LeftTangent = leftTangent;
        copy.RightTangent = rightTangent;
        copy.Mode = _mode;
        copy.IsSymmetrical = _isSymmetrical;
        copy.FloorHeight = _floorHeight;
        copy.FloorAngle = _floorAngle;
        copy.ForceNewSection = _forceNewSection;
        copy.BlockNewSection = _blockNewSection;

        return copy;


    }
}
