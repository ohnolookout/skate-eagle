using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class PositionalListEmulator
{
    public static List<PositionObject<string>> ExpectedStringsFromPosition(List<PositionObject<string>> possibleValues, Vector3 position, float trailingBuffer, float leadingBuffer, bool doPrint = false)
    {
        List<PositionObject<string>> containedValues = new();
        float trailingX = position.x - trailingBuffer;
        float leadingX = position.x + leadingBuffer;
        int startIndex = 0;
        int i = 0;
        while(i < possibleValues.Count && possibleValues[i].Position.x <= leadingX)
        {
            if(possibleValues[i].Position.x >= trailingX)
            {
                if(containedValues.Count == 0)
                {
                    startIndex = i;
                }
                containedValues.Add(possibleValues[i]);
            }
            i++;
        }
        if (doPrint)
        {
            PositionalListPrinter.PrintExpectedParams(trailingX, leadingX, startIndex, i);
        }

        if (doPrint)
        {
            PositionalListPrinter.PrintExpectedStrings(containedValues);
        }

        return containedValues;
    }

    public static List<DoublePositionObject<string>> ExpectedStringsFromPosition(List<DoublePositionObject<string>> possibleValues, Vector3 position, float trailingBuffer, float leadingBuffer, bool doPrint = false)
    {
        List<DoublePositionObject<string>> containedValues = new();
        float trailingX = position.x - trailingBuffer;
        float leadingX = position.x + leadingBuffer;
        int startIndex = 0;
        int i = 0;
        while (possibleValues[i].StartPosition.x <= leadingX && i < possibleValues.Count)
        {
            if (possibleValues[i].EndPosition.x >= trailingX)
            {
                if (containedValues.Count == 0)
                {
                    startIndex = i;
                }
                containedValues.Add(possibleValues[i]);
            }
        }
        if (doPrint)
        {
            PositionalListPrinter.PrintExpectedParams(trailingX, leadingX, startIndex, i);
        }

        if (doPrint)
        {
            PositionalListPrinter.PrintExpectedStrings(containedValues);
        }

        return containedValues;
    }

    public static List<PositionObject<string>> ExpectedStringsFromPosition(List<PositionObject<string>> possibleValues, GameObject obj, float trailingBuffer, float leadingBuffer, bool doPrint = false)
    {
        return ExpectedStringsFromPosition(possibleValues, obj.transform.position, trailingBuffer, leadingBuffer, doPrint);
    }

}
