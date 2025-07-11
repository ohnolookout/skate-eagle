using Newtonsoft.Json;
using System;
using UnityEngine;

public interface IDeserializable
{
    ISerializable Deserialize(GameObject targetObject, GameObject contextObject);
}
