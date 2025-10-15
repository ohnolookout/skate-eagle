using System;
using UnityEngine;

[Serializable]
public class LinkedHighPoint
{
    #region Declarations
    [SerializeReference] public LinkedHighPoint previous;
    [SerializeReference] public LinkedHighPoint next;
    public Vector3 position;

    public LinkedHighPoint()
    {
        position = Vector3.zero;
    }

    public LinkedHighPoint(Vector3 position)
    {
        this.position = position;
    }

    public LinkedHighPoint(Vector3 position, LinkedHighPoint previous, LinkedHighPoint next)
    {
        this.previous = previous;
        this.next = next;
        this.position = position;
    }



    #endregion
}
