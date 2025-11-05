using UnityEngine;
public interface ICameraTargetable : IResyncable
{
    void AddObjectToTarget();
    
    //Use LinkedCameraTarget for actual targeting, but only build from GameObjects on level save
    LinkedCameraTarget LinkedCameraTarget { get; set; }
    GameObject Object { get; }
    bool DoTargetLow { get; set; }
}


