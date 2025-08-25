using UnityEngine;

public interface ISerializable
{
    IDeserializable Serialize();
    public void Clear();
    public void Refresh(GroundManager groundManager = null);
    GameObject GameObject { get; }
}
