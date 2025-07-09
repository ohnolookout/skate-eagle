using TMPro;
using UnityEngine;
using System;

public class TutorialSign: MonoBehaviour, ISerializable
{
    [SerializeField] private TextMeshPro _signText;

    public TextMeshPro SignText
    {
        get => _signText;
        set => _signText = value;
    }

    public IDeserializable Serialize()
    {
        return new SerializedTutorialSign(_signText.text, new Vector2(transform.position.x, transform.position.z));
    }
}