using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PositionalListPrinter
{
    public static void PrintCurrentStrings(SinglePositionalList<PositionObject<string>> positionalList, GameObject trackingObj)
    {
        var strings = StringsFromPositionalList(positionalList);
        PrintStatus(trackingObj.transform.position, strings, positionalList.TrailingX, positionalList.LeadingX, positionalList.CurrentTrailingIndex, positionalList.CurrentLeadingIndex);
    }

    public static void PrintCurrentStrings(DoublePositionalList<DoublePositionObject<string>> positionalList, GameObject trackingObj)
    {
        var strings = StringsFromPositionalList(positionalList);
        PrintStatus(trackingObj.transform.position, strings, positionalList.TrailingX, positionalList.LeadingX, positionalList.CurrentTrailingIndex, positionalList.CurrentLeadingIndex);
    }

    private static void PrintStatus(Vector3 objPosition, List<string> stringValues, float trailingX, float leadingX, int trailingIndex, int leadingIndex)
    {
        Debug.Log("");
        Debug.Log("-----------------------");
        Debug.Log($"Current strings for object at {objPosition}: ");
        foreach (var str in stringValues)
        {
            Debug.Log(str);
        }
        Debug.Log("");
        Debug.Log("Trailing X: " + trailingX);
        Debug.Log("Leading X: " + leadingX);
        Debug.Log("Trailing index: " + trailingIndex);
        Debug.Log("Leading index: " + leadingIndex);
        Debug.Log("");
    }

    private static List<string> StringsFromPositionalList(DoublePositionalList<DoublePositionObject<string>> positionalList)
    {
        List<string> strings = new();
        foreach (var strObj in positionalList.CurrentObjects)
        {
            strings.Add(strObj.Value);
        }
        return strings;
    }

    private static List<string> StringsFromPositionalList(SinglePositionalList<PositionObject<string>> positionalList)
    {
        List<string> strings = new();
        foreach (var strObj in positionalList.CurrentObjects)
        {
            strings.Add(strObj.Value);
        }
        return strings;
    }

    public static void PrintExpectedStrings(List<PositionObject<string>> positionalStrings)
    {
        Debug.Log($"Expected strings:");
        foreach (var stringObj in positionalStrings)
        {
            Debug.Log(stringObj.Value);
        }
        Debug.Log("");
    }

    public static void PrintExpectedStrings(List<DoublePositionObject<string>> positionalStrings)
    {
        Debug.Log($"Expected strings:");
        foreach (var stringObj in positionalStrings)
        {
            Debug.Log(stringObj.Value);
        }
        Debug.Log("");
    }

    public static void PrintExpectedParams(float trailingX, float leadingX, int startIndex, int endIndex)
    {
        Debug.Log($"Expected trailing X: {trailingX}");
        Debug.Log($"Expected leading X: {leadingX}");
        Debug.Log($"Expected start index (inclusive): {startIndex}");
        Debug.Log($"Expected end index (exclusive): {endIndex}");
        Debug.Log("");
    }
}
