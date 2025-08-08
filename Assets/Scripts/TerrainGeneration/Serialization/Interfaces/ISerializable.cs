using UnityEngine;

public interface ISerializable
{
    IDeserializable Serialize();
    void Clear();
    void Refresh(GroundManager groundManager = null);
    GameObject GameObject { get; }
}
