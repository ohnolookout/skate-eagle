using System;
using UnityEngine;

[Serializable]
public class SerializedTutorialSign : IDeserializable
{
    [SerializeField] private string _signText;
    [SerializeField] private Vector2 _position;
    public string SignText
    {
        get => _signText;
        set => _signText = value;
    }

    public Vector2 Position
    {
        get => _position;
        set => _position = value;
    }
    public SerializedTutorialSign(string signText, Vector2 position)
    {
        _signText = signText;
        _position = position;
    }
    public ISerializable Deserialize(GameObject targetObject, GameObject contextObject)
    {
        TutorialSign tutorialSign = targetObject.AddComponent<TutorialSign>();
        tutorialSign.SignText.text = _signText;
        targetObject.transform.position = new Vector3(_position.x, 0, _position.y);
        return tutorialSign;
    }
}

