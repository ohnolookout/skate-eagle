using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;

public class PositionalListEditTest
{
    #region Variables
    private List<PositionObject<string>> _positionStrings = new()
    {
        new("0", new(0, 0, 0)),
        new("10", new(10, 0, 0)),
        new("20", new(20, 0, 0)),
        new("30", new(30, 0, 0)),
        new("40", new(40, 0, 0)),
    };
    private int _defaultDistance = 10;
    private GameObject _trackingObj = new();
    private List<float> _allDistances = new() { -45, -20, -10, -5, 0, 5, 10, 20, 45 };

    //STARTING POSITIONS
    private Vector3 START = new(0, 0, 0), MID_WHOLE = new(20, 0, 0), MID_HALF = new(25, 0, 0), END = new(40, 0, 0), 
        OB_START_SINGLE = new( -10, 0, 0), OB_START_BOTH = new( -20, 0, 0), OB_END_SINGLE = new(50, 0, 0), OB_END_BOTH = new(60, 0, 0),
        OB_START_SINGLE_HALF = new(-5, 0, 0), OB_START_BOTH_HALF = new(-25, 0, 0), OB_END_SINGLE_HALF = new(45, 0, 0), OB_END_BOTH_HALF = new(65, 0, 0);

    private Dictionary<string, Vector3> DefaultPositionsDict()
    {
        Dictionary<string, Vector3> positionsDict = new()
        {
            { "START", START },
            { "MID_WHOLE", MID_WHOLE },
            { "MID_HALF", MID_HALF },
            { "END", END },
            { "OB_START_SINGLE", OB_START_SINGLE },
            { "OB_START_BOTH", OB_START_BOTH },
            { "OB_END_SINGLE", OB_END_SINGLE },
            { "OB_END_BOTH", OB_END_BOTH },
            { "OB_START_SINGLE_HALF", OB_START_SINGLE_HALF },
            { "OB_START_BOTH_HALF", OB_START_BOTH_HALF },
            { "OB_END_SINGLE_HALF", OB_END_SINGLE_HALF },
            { "OB_END_BOTH_HALF", OB_END_BOTH_HALF }
        };

        return positionsDict;
    }
    #endregion

    #region Tests
    [Test]
    public void Initialization()
    {
        bool doPrint = false;
        var allPositions = DefaultPositionsDict();

        foreach (var name in allPositions.Keys)
        {
            if (doPrint)
            {
                Debug.Log($"Testing initialization at {name}");
            }

            _trackingObj = new();
            _trackingObj.transform.position = allPositions[name];
            var positionalList = DefaultPositionalList();
            CheckExpected(positionalList);

            if (doPrint)
            {
                Debug.Log($"Initialization at {name} passed.");
            }
        }
    }
    

    [Test]
    public void SingleIncrement()
    {
        bool doPrint = false;
        foreach(var distance in _allDistances)
        {
            MoveAndTestAllPositions(distance, doPrint);
        }
    }

    [Test]
    public void DownUpIncrement()
    {
        bool doPrint = false;
        List<float[]> movements = new();
        for(int i = 0; i<4; i++)
        {
            float[] movement = new float[2] { _allDistances[i], _allDistances[^(i + 1)] };
            movements.Add(movement);
        }

        foreach(var movement in movements)
        {
            MoveAndTestAllPositions(movement, doPrint);
        }
    }

    [Test]
    public void UpDownIncrement()
    {
        bool doPrint = false;
        List<float[]> movements = new();
        for (int i = 0; i < 4; i++)
        {
            float[] movement = new float[2] { _allDistances[^(i + 1)], _allDistances[i] };
            movements.Add(movement);
        }

        foreach (var movement in movements)
        {
            MoveAndTestAllPositions(movement, doPrint);
        }
    }

    [Test]
    public void RandomThreeSteps()
    {
        var random = new System.Random();
        bool doPrint = false;
        List<float[]> movements = new();
        while(movements.Count < 10)
        {
            float[] movement = new float[3];
            for(int i = 0; i < 3; i++)
            {
                movement[i] = _allDistances[random.Next(_allDistances.Count)];
            }
            movements.Add(movement);
        }

        foreach (var movement in movements)
        {
            MoveAndTestAllPositions(movement, doPrint);
        }
    }
    #endregion

    #region Test Iterators
    private void MoveAndTestAllPositions(float distance, bool doPrint = false)
    { 
        float[] distances = new float[1] { distance };
        MoveAndTestAllPositions(distances, doPrint);
    }

    private void MoveAndTestAllPositions(float[] distances, bool doPrint = false)
    {
        var allPositions = DefaultPositionsDict();
        foreach (var name in allPositions.Keys)
        {
            

            _trackingObj = new();
            _trackingObj.transform.position = allPositions[name];
            var positionalList = DefaultPositionalList();
            if (doPrint)
            {
                Debug.Log($"Testing starting position {name}.");
            }
            foreach (var distance in distances)
            {
                if (doPrint)
                {
                    Debug.Log($"Testing increment of {distance}.");
                }
               
                TestSinglePosition(positionalList, allPositions[name], distance);
                
                if (doPrint)
                {
                    Debug.Log($"Increment of {distance} passed.");
                }
            }
            if (doPrint)
            {
                Debug.Log($"Starting position {name} passed.");
            }
        }
    }

    private void TestSinglePosition(SinglePositionalList<PositionObject<string>> positionalList, Vector3 startPosition, float distance, bool doPrint = false)
    {
        MoveObjAndUpdateList(positionalList, distance);
        CheckExpected(positionalList, doPrint);
    }

    private void MoveObjAndUpdateList(SinglePositionalList<PositionObject<string>> list, float distance)
    {
        int moveModifier = 1;
        if (distance < 0)
        {
            moveModifier = -1;
        }
        float moveDistance = 5 * moveModifier;
        int moveCount = (int)Math.Floor(distance / moveDistance);
        float remainingDistance = distance - (moveCount * moveDistance);
        for (int i = 0; i < moveCount; i++)
        {
            _trackingObj.transform.Translate(new Vector3(moveDistance, 0, 0));
            list.Update();
        }
        if (Mathf.Abs(remainingDistance) > 0)
        {
            _trackingObj.transform.Translate(new Vector3(remainingDistance, 0, 0));
            list.Update();
        }
    }
    #endregion

    #region Expected String Utilities
    private void CheckExpected(SinglePositionalList<PositionObject<string>> positionalList, bool doPrint = false)
    {
        if (doPrint)
        {
            PositionalListPrinter.PrintCurrentStrings(positionalList, _trackingObj);
        }
        Assert.AreEqual(ExpectedStrings(doPrint), positionalList.CurrentObjects);
    }
    private SinglePositionalList<PositionObject<string>> DefaultPositionalList()
    {
        return PositionalListFactory<PositionObject<string>>.TransformTracker(_positionStrings, _trackingObj.transform, _defaultDistance, _defaultDistance);
    }


    private List<PositionObject<string>> ExpectedStrings(bool doPrint = false)
    {
        return PositionalListEmulator.ExpectedStringsFromPosition(_positionStrings, _trackingObj, _defaultDistance, _defaultDistance, doPrint);
    }
    #endregion    
}
