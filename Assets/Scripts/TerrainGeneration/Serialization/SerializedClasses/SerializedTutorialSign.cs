using System;
using UnityEngine;

[Serializable]
public class SerializedTutorialSign : IDeserializable
{
    [SerializeField] private string _text;
    [SerializeField] private SignType _type;
    [SerializeField] private Vector2 _position;
    [SerializeField] private string _name;
    [SerializeField] private bool _isSquare = false;
    [SerializeField] private float _imgRotation = 0;
    
    public SignType Type { get => _type;}
    public SerializedTutorialSign(string name, SignType type, string signText, Vector2 position, float imgRotation)
    {
        _name = name;
        _type = type;
        _text = signText;
        _position = position;
        _imgRotation = imgRotation;

    }
    public ISerializable Deserialize(GameObject targetObject, GameObject contextObject)
    {
        targetObject.name = _name;
        TutorialSign tutorialSign = targetObject.GetComponent<TutorialSign>();
        tutorialSign.SignText.text = _text;
        targetObject.transform.position = new(_position.x, _position.y);
        tutorialSign.ImageTransform.rotation = Quaternion.Euler(0, 0, _imgRotation);
        return tutorialSign;
    }
}

