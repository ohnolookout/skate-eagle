using System;
using UnityEngine;

[Serializable]
public class SerializedTutorialSign : IDeserializable
{
    [SerializeField] private string _signText;
    [SerializeField] private Vector2 _position;
    [SerializeField] private string _name;
    [SerializeField] private bool _isSquare = false;
    [SerializeField] private float _imgRotation = 0;
    
    public bool IsSquare { get => _isSquare; set => _isSquare = value; }
    public SerializedTutorialSign(string name, string signText, Vector2 position, float imgRotation, bool isSquare)
    {
        _name = name;
        _signText = signText;
        _position = position;
        _imgRotation = imgRotation;
        _isSquare = isSquare;
    }
    public ISerializable Deserialize(GameObject targetObject, GameObject contextObject)
    {
        targetObject.name = _name;
        TutorialSign tutorialSign = targetObject.GetComponent<TutorialSign>();
        tutorialSign.SignText.text = _signText;
        targetObject.transform.position = new(_position.x, _position.y);
        tutorialSign.ImageTransform.rotation = Quaternion.Euler(0, 0, _imgRotation);
        return tutorialSign;
    }
}

