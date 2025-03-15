using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using System;
/*
public class PositionalMinMaxTest
{
    #region Variables
    private GameObject _trackingObj = new();
    private int _defaultDistance = 10;
    private List<SortablePositionObject<string>> _positionStrings = new()
    {
        new("-20", new(-20, -20, 0), -20),
        new("-10", new(-10, -10, 0), -10),
        new("0", new(0, 0, 0), 0),
        new("10", new(10, 10, 0), 10),
        new("20", new(20, 50, 0), 50),
        new("30", new(30, 30, 0), 30),
        new("40", new(40, 40, 0), 40),
    };


    private PositionalMinMax<SortablePositionObject<string>> _positionalMinMax;
    private SinglePositionalList<SortablePositionObject<string>> DefaultPositionalList()
    {
        Func<float> updateTrailing = () => _trackingObj.transform.position.x - _defaultDistance;
        Func<float> updateLeading = () => _trackingObj.transform.position.x + _defaultDistance;
        return new(_positionStrings, updateTrailing, updateLeading);
    }

    #endregion

    #region Tests
    [Test]
    public void InitializationMax()
    {
        _trackingObj = new();
        var positionalList = DefaultPositionalList();
        _positionalMinMax = new(positionalList, ComparisonType.Greatest);
        Assert.AreEqual(new Vector3(10, 10, 0), _positionalMinMax.MinMax.CurrentValue.Position);
    }

    [Test]
    public void InitializationMin()
    {
        _trackingObj = new();
        var positionalList = DefaultPositionalList();
        _positionalMinMax = new(positionalList, ComparisonType.Least);
        Assert.AreEqual(new Vector3(-10, -10, 0), _positionalMinMax.MinMax.CurrentValue.Position);
    }

    [Test]
    public void MoveToNewMax()
    {
        _trackingObj = new();
        var positionalList = DefaultPositionalList();
        _positionalMinMax = new(positionalList, ComparisonType.Greatest);
        _trackingObj.transform.Translate(new(10, 0, 0));
        _positionalMinMax.Update();
        Assert.AreEqual(new Vector3(20, 50, 0), _positionalMinMax.MinMax.CurrentValue.Position);
    }

    [Test]
    public void MoveToNewMin()
    {
        _trackingObj = new();
        var positionalList = DefaultPositionalList();
        _positionalMinMax = new(positionalList, ComparisonType.Least);
        _trackingObj.transform.Translate(new(-10, 0, 0));
        _positionalMinMax.Update();
        Assert.AreEqual(new Vector3(-20, -20, 0), _positionalMinMax.MinMax.CurrentValue.Position);
    }
    #endregion

}
*/