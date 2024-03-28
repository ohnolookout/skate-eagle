using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using System;

public class MinMaxCacheEditTest
{
    #region Variables
    private Vector3 NEG_TWO = new(0, -2, 0), NEG_ONE = new(0, -1, 0), ZERO = new(0, 0, 0), ONE = new(0, 1, 0), TWO = new(0, 2, 0), 
        THREE = new(0, 3, 0), FOUR = new(0, 4, 0), FIVE = new(0, 5, 0);

    private Func<float, float, bool> _lessThan = (a, b) => a <= b,
        _greaterThan = (a, b) => a >= b;

    private Vector3 _defaultVector = new();
    #endregion

    #region Initialization
    [Test]
    public void InitializeMax()
    {
        bool doPrint = false;
        var testLists = AllTestLists();
        foreach(var listName in testLists.Keys)
        {
            var list = testLists[listName];
            var listCopy = DuplicateList(list);
            MinMaxCache cache = new(ComparisonType.Greatest, list);
            if (doPrint)
            {
                Debug.Log($"Checking {listName} for InitializeMax");
                //PrintList(list);
            }
            CheckExpected(cache, listCopy, _greaterThan);
            if (doPrint)
            {
                Debug.Log($"{listName} passed for InitializeMax");
            }
        }
    }

    [Test]
    public void InitializeMin()
    {
        bool doPrint = false;
        var testLists = AllTestLists();
        foreach (var listName in testLists.Keys)
        {
            var list = testLists[listName];
            var listCopy = DuplicateList(list);
            MinMaxCache cache = new(ComparisonType.Least, list);
            if (doPrint)
            {
                Debug.Log($"Checking {listName} for InitializeMin");
                //PrintList(list);
            }
            CheckExpected(cache, listCopy, _lessThan);
            if (doPrint)
            {
                Debug.Log($"{listName} passed for InitializeMin");
            }
        }
    }
    #endregion

    #region AddLeading
    [Test]
    public void AddLeadingLeast()
    {
        bool doPrint = false;
        var testLists = AllTestLists();
        foreach (var listName in testLists.Keys)
        {
            var list = testLists[listName];
            var listCopy = DuplicateList(list);
            MinMaxCache cache = new(ComparisonType.Least, list);
            cache.AddLeading(NEG_TWO);
            listCopy.Add(NEG_TWO);
            if (doPrint)
            {
                Debug.Log($"Checking {listName} for AddLeadingLeast");
                //PrintList(list);
            }
            CheckExpected(cache, listCopy, _lessThan);
            if (doPrint)
            {
                Debug.Log($"{listName} passed for AddLeadingLeast");
            }
        }
    }
    [Test]
    public void AddLeadingGreatest()
    {
        bool doPrint = false;
        var testLists = AllTestLists();
        foreach (var listName in testLists.Keys)
        {
            var list = testLists[listName];
            var listCopy = DuplicateList(list);
            MinMaxCache cache = new(ComparisonType.Greatest, list);
            cache.AddLeading(FIVE);
            listCopy.Add(FIVE);
            if (doPrint)
            {
                Debug.Log($"Checking {listName} for AddLeadingGreatest");
                //PrintList(list);
            }
            CheckExpected(cache, listCopy, _greaterThan);
            if (doPrint)
            {
                Debug.Log($"{listName} passed for AddLeadingGreatest");
            }
        }
    }
    [Test]
    public void AddLeadingIrrelevant()
    {
        bool doPrint = false;
        var testLists = AllTestLists();
        foreach (var listName in testLists.Keys)
        {
            var list = testLists[listName];
            var listCopy = DuplicateList(list);
            MinMaxCache cache = new(ComparisonType.Greatest, list);
            //Debug.Log($"{listName} initialized with current index at {cache.CurrentIndex}");
            cache.AddLeading(TWO);
            //Debug.Log($"FIVE added. Current index at {cache.CurrentIndex}");
            listCopy.Add(TWO);
            if (doPrint)
            {
                Debug.Log($"Checking {listName} for AddLeadingIrrelevant");
                //PrintList(list);
            }
            CheckExpected(cache, listCopy, _greaterThan);
            if (doPrint)
            {
                Debug.Log($"{listName} passed for AddLeadingIrrelevant");
            }
        }
    }
    #endregion

    #region AddTrailing
    [Test]
    public void AddTrailingLeast()
    {
        bool doPrint = false;
        var testLists = AllTestLists();
        foreach (var listName in testLists.Keys)
        {
            var list = testLists[listName];
            var listCopy = DuplicateList(list);
            MinMaxCache cache = new(ComparisonType.Least, list);
            cache.AddTrailing(NEG_TWO);
            listCopy.Add(NEG_TWO);
            if (doPrint)
            {
                Debug.Log($"Checking {listName} for AddTrailingLeast");
                //PrintList(list);
            }
            CheckExpected(cache, listCopy, _lessThan);
            if (doPrint)
            {
                Debug.Log($"{listName} passed for AddTrailingLeast");
            }
        }
    }
    [Test]
    public void AddTrailingGreatest()
    {
        bool doPrint = false;
        var testLists = AllTestLists();
        foreach (var listName in testLists.Keys)
        {
            var list = testLists[listName];
            var listCopy = DuplicateList(list);
            MinMaxCache cache = new(ComparisonType.Greatest, list);
            cache.AddTrailing(FIVE);
            listCopy.Add(FIVE);
            if (doPrint)
            {
                Debug.Log($"Checking {listName} for AddTrailingGreatest");
                //PrintList(list);
            }
            CheckExpected(cache, listCopy, _greaterThan);
            if (doPrint)
            {
                Debug.Log($"{listName} passed for AddTrailingGreatest");
            }
        }
    }
    [Test]
    public void AddTrailingIrrelevant()
    {
        bool doPrint = false;
        var testLists = AllTestLists();
        foreach (var listName in testLists.Keys)
        {
            var list = testLists[listName];
            var listCopy = DuplicateList(list);
            MinMaxCache cache = new(ComparisonType.Greatest, list);
            cache.AddTrailing(TWO);
            listCopy.Add(TWO);
            if (doPrint)
            {
                Debug.Log($"Checking {listName} for AddTrailingIrrelevant");
                //PrintList(list);
            }
            CheckExpected(cache, listCopy, _greaterThan);
            if (doPrint)
            {
                Debug.Log($"{listName} passed for AddTrailingIrrelevant");
            }
        }
    }
    #endregion

    #region Remove
    [Test]
    public void RemoveTrailing()
    {
        bool doPrint = false;
        var testLists = AllTestLists();
        foreach (var listName in testLists.Keys)
        {
            var list = testLists[listName];
            var listCopy = DuplicateList(list);
            MinMaxCache cache = new(ComparisonType.Least, list);
            cache.RemoveTrailing();

            if (listCopy.Count > 0)
            {
                listCopy.RemoveAt(0);
            }

            if (doPrint)
            {
                Debug.Log($"Checking {listName} for RemoveTrailingLeast");
                //PrintList(list);
            }
            CheckExpected(cache, listCopy, _lessThan);
            if (doPrint)
            {
                Debug.Log($"{listName} passed for RemoveTrailingLeast");
            }
        }
    }

    [Test]
    public void RemoveLeading()
    {
        bool doPrint = false;
        var testLists = AllTestLists();
        foreach (var listName in testLists.Keys)
        {
            var list = testLists[listName];
            var listCopy = DuplicateList(list);
            MinMaxCache cache = new(ComparisonType.Greatest, list);
            //Debug.Log($"{listName} initialized with current index at {cache.CurrentIndex}");
            cache.RemoveLeading();

            if (listCopy.Count > 0)
            {
                listCopy.RemoveAt(listCopy.Count - 1);
            }

            if (doPrint)
            {
                Debug.Log($"Checking {listName} for AddTrailingGreatest");
                //PrintList(list);
            }
            CheckExpected(cache, listCopy, _greaterThan);
            if (doPrint)
            {
                Debug.Log($"{listName} passed for AddTrailingGreatest");
            }
        }
    }

    #endregion

    #region Multistep
    [Test]
    public void ThreeSteps()
    {
        bool doPrint = false;
        var testLists = AllTestLists();
        foreach (var listName in testLists.Keys)
        {
            var list = testLists[listName];
            var listCopy = DuplicateList(list);
            MinMaxCache cache = new(ComparisonType.Greatest, list);
            //Debug.Log($"{listName} initialized with current index at {cache.CurrentIndex}");
            cache.AddLeading(TWO);
            listCopy.Add(TWO);
            cache.AddLeading(FIVE);
            listCopy.Add(FIVE);
            cache.RemoveTrailing();
            listCopy.RemoveAt(0);
            if (doPrint)
            {
                Debug.Log($"Checking {listName} for AddTrailingIrrelevant");
                //PrintList(list);
            }
            CheckExpected(cache, listCopy, _greaterThan);
            if (doPrint)
            {
                Debug.Log($"{listName} passed for AddTrailingIrrelevant");
            }
        }
    }

    #endregion

    #region TestUtilities
    private void CheckExpected(MinMaxCache minMax, List<Vector3> mockList, Func<float, float, bool> comparison)
    {
        Assert.AreEqual(MinMaxFromList(mockList, comparison), minMax.CurrentPoint);
    }

    private Vector3 MinMaxFromList(List<Vector3> inputList, Func<float, float, bool> comparison)
    {
        if(inputList.Count < 1)
        {
            return _defaultVector;
        }
        Vector3 minMaxY = Vector3.negativeInfinity;

        if(comparison == _lessThan)
        {
            minMaxY = Vector3.positiveInfinity;
        }

        foreach(var vector in inputList)
        {
            if(comparison(vector.y, minMaxY.y))
            {
                minMaxY = vector;
            }
        }

        return minMaxY;
    }

    private List<Vector3> DuplicateList(List<Vector3> toDuplicate)
    {
        List<Vector3> duplicate = new();
        foreach (var item in toDuplicate)
        {
            duplicate.Add(item);
        }

        return duplicate;
    }
    #endregion

    #region ListUtilities
    private Dictionary<string, List<Vector3>> AllTestLists()
    {
        Dictionary<string, List<Vector3>> lists = new()
        {
            {
                "Ascending",
                new()
                {
                    NEG_ONE,
                    ZERO,
                    ONE,
                    TWO,
                    THREE,
                    FOUR
                }
            },
            {
                "Descending",
                new()
                {
                    FOUR,
                    THREE,
                    TWO,
                    ONE,
                    ZERO,
                    NEG_ONE
                }
            },
            {
                "Random",
                new()
                {
                    TWO,
                    ZERO,
                    FOUR,
                    THREE,
                    ONE
                }
            },
            {
                "All Zero",
                new()
                {
                    ZERO,
                    ZERO,
                    ZERO
                }
            },
            {
                "Zeroes and Ones",
                new()
                {
                    ZERO,
                    ONE,
                    ZERO,
                    ONE
                }
            },
            {
                "One Element",
                new()
                {
                    ZERO
                }
            },
            {
                "Empty",
                new()
                {                
                }
            },

        };



        return lists;
    }

    private void PrintList(List<Vector3> list)
    {
        Debug.Log("List contents:");
        foreach(var item in list)
        {
            Debug.Log(item);
        }
    }
    #endregion
}
