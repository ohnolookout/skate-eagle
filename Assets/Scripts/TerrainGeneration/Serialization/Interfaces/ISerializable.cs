using UnityEngine;

public interface ISerializable: IResyncable
{
    IDeserializable Serialize();
    public void Clear();
    public void Refresh(GroundManager groundManager = null);
    GameObject GameObject { get; }
}
