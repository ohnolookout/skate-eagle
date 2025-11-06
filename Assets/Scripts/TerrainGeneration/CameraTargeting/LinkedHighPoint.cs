using System;
using UnityEngine;

[Serializable]
public class LinkedHighPoint: IResyncable
{
    #region Declarations
    [SerializeReference] public LinkedHighPoint _previous;
    [SerializeReference] public LinkedHighPoint _next;
    private ResyncRef<LinkedHighPoint> _previousRef = new();
    private ResyncRef<LinkedHighPoint> _nextRef = new();
    public Vector3 position;
    public LinkedHighPoint Previous 
    {
        get => _previous;
        set
        {
            _previous = value;
            _previousRef.Value = value;
        }
    }
    public LinkedHighPoint Next
    {
        get => _next;
        set
        {
            _next = value;
            _nextRef.Value = value;
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
        _previousRef.Value = previous;
        _nextRef.Value = next;
        this.position = position;
    }

    public void RegisterResync()
    {
        LevelManager.ResyncHub.RegisterResync(this);
    }


    #endregion
}
