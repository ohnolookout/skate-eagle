using System;
using UnityEngine;

[Serializable]
public class SerializedTutorialSign : IDeserializable
{
    [SerializeField] private string _signText;
    [SerializeField] private Vector2 _position;
    [SerializeField] private string _name;
    [SerializeField] private bool _isSquare = false;
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

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    public bool IsSquare
    {
        get => _isSquare;
        set => _isSquare = value;
    }

    public SerializedTutorialSign(string name, string signText, Vector2 position, bool isSquare)
    {
        _name = name;
        _signText = signText;
        _position = position;
        _isSquare = isSquare;
    }
    public ISerializable Deserialize(GameObject targetObject, GameObject contextObject)
    {
        targetObject.name = _name;
        TutorialSign tutorialSign = targetObject.GetComponent<TutorialSign>();
        tutorialSign.SignText.text = _signText;
        targetObject.transform.position = new Vector3(_position.x, 0, _position.y);
        return tutorialSign;
    }
}

