using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SinglePositionalList<T> : PositionalList<T> where T: IPosition
{
    public SinglePositionalList(List<T> allObjects, Func<float> updateTrailing, Func<float> updateLeading):
        base(allObjects, updateTrailing, updateLeading)
    {
    }

    public override Vector3 CurrentLeadingPosition()
    {
        return _allObjects[CurrentLeadingIndex].Position;
    }

    public override Vector3 CurrentTrailingPosition()
    {
        return _allObjects[CurrentTrailingIndex].Position;
    }

    public override Vector3 NextLeadingPosition()
    {
        return _allObjects[NextLeadingIndex].Position;
    }

    public override Vector3 NextTrailingPosition()
    {
        return _allObjects[NextTrailingIndex].Position;
    }

}
