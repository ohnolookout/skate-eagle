using UnityEngine;

public interface ISerializable
{
    IDeserializable Serialize();
    void Clear();
    GameObject GameObject { get; }
}
