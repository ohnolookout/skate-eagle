using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

public class PositionalMinMaxTest
{
    #region Variables
    private GameObject _trackingObj = new();
    private int _defaultDistance = 10;
    private List<PositionObject<string>> _positionStrings = new()
    {
        new("-20", new(-20, -20, 0)),
        new("-10", new(-10, -10, 0)),
        new("0", new(0, 0, 0)),
        new("10", new(10, 10, 0)),
        new("20", new(20, 50, 0)),
        new("30", new(30, 30, 0)),
        new("40", new(40, 40, 0)),
    };
    private Vector3 NEW_MIN = new(-35, -30, 0), NEW_MAX = new(55, 55, 0), DEFAULT_MIN = new(-20, -20, 0), DEFAULT_MAX = new(20, 50, 0);

    private PositionalMinMax<PositionObject<string>> _positionalMinMax;
    private SinglePositionalList<PositionObject<string>> DefaultPositionalList()
    {
        return PositionalListFactory<PositionObject<string>>.TransformTracker(_positionStrings, _trackingObj.transform, _defaultDistance, _defaultDistance);
    }

    #endregion

    #region Tests
    [Test]
    public void InitializationMax()
    {
        _trackingObj = new();
        var positionalList = DefaultPositionalList();
        _positionalMinMax = new(positionalList, ComparisonType.Greatest);
        Assert.AreEqual(new Vector3(10, 10, 0), _positionalMinMax.MinMax.CurrentPoint);
    }

    [Test]
    public void InitializationMin()
    {
        _trackingObj = new();
        var positionalList = DefaultPositionalList();
        _positionalMinMax = new(positionalList, ComparisonType.Least);
        Assert.AreEqual(new Vector3(-10, -10, 0), _positionalMinMax.MinMax.CurrentPoint);
    }

    [Test]
    public void MoveToNewMax()
    {
        _trackingObj = new();
        var positionalList = DefaultPositionalList();
        _positionalMinMax = new(positionalList, ComparisonType.Greatest);
        _trackingObj.transform.Translate(new(10, 0, 0));
        _positionalMinMax.Update();
        Assert.AreEqual(new Vector3(20, 50, 0), _positionalMinMax.MinMax.CurrentPoint);
    }

    [Test]
    public void MoveToNewMin()
    {
        _trackingObj = new();
        var positionalList = DefaultPositionalList();
        _positionalMinMax = new(positionalList, ComparisonType.Least);
        _trackingObj.transform.Translate(new(-10, 0, 0));
        _positionalMinMax.Update();
        Assert.AreEqual(new Vector3(-20, -20, 0), _positionalMinMax.MinMax.CurrentPoint);
    }
    #endregion

}
