using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayListUtility<T>
{
    public static bool IndexInRange(T[] array, int index)
    {
        return index >= 0 && index < array.Length;
    }

    public static bool IndexInRange(List<T> list, int index)
    {
        return index >= 0 && index < list.Count;
    }

}
