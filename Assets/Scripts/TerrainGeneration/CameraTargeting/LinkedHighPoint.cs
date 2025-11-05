using System;
using UnityEngine;

[Serializable]
public class LinkedHighPoint: IResyncable
{
    #region Declarations
    [SerializeReference] public LinkedHighPoint previous;
    [SerializeReference] public LinkedHighPoint next;
    private ResyncRef<LinkedHighPoint> _previousRef = new();
    private ResyncRef<LinkedHighPoint> _nextRef = new();
    public Vector3 position;
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
        this.previous = previous;
        this.next = next;
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
