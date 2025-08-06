using UnityEngine;

public interface ISerializable
{
    IDeserializable Serialize();
    GameObject GameObject { get; }
}
