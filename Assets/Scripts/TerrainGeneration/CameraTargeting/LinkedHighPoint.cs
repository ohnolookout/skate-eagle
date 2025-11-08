using System;
using UnityEngine;

[Serializable]
public class LinkedHighPoint
{
    #region Declarations
    [SerializeReference] public LinkedHighPoint _previous;
    [SerializeReference] public LinkedHighPoint _next;
    public Vector3 position;
    public LinkedHighPoint Previous 
    {
        get => _previous;
        set
        {
            _previous = value;
        }
    }
    public LinkedHighPoint Next
    {
        get => _next;
        set
        {
            _next = value;
        }
    }
    public string UID { get; set; }

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
        this.Previous = previous;
        this.Next = next;
        this.position = position;
    }



    #endregion
}
