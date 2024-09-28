using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DoublePositionalList<T> : PositionalList<T> where T : IDoublePosition, IPosition
{
    public DoublePositionalList(List<T> allObjects, Func<float> updateTrailing, Func<float> updateLeading, Action<T, ListSection> onObjectAdded = null, Action<T, ListSection> onObjectRemoved = null) :
        base(allObjects, updateTrailing, updateLeading, onObjectAdded, onObjectRemoved)
    {
    }

    public override Vector3 CurrentLeadingPosition()
    {
        return _allObjects[CurrentLeadingIndex].StartPosition;
    }

    public override Vector3 CurrentTrailingPosition()
    {
        return _allObjects[CurrentTrailingIndex].EndPosition;
    }

    public override Vector3 NextLeadingPosition()
    {
        return _allObjects[NextLeadingIndex].StartPosition;
    }

    public override Vector3 NextTrailingPosition()
    {
        return _allObjects[NextTrailingIndex].EndPosition;
    }

}
