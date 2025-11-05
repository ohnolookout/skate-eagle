using System;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[Serializable]
public class SerializedTutorialSign : IDeserializable
{
    [SerializeField] private string _text;
    [SerializeField] private SignType _type;
    [SerializeField] private Vector2 _position;
    [SerializeField] private string _name;
    [SerializeField] private float _imgRotation = 0;
    [SerializeField] private string _uid;
    
    public SignType Type { get => _type;}

    public SerializedTutorialSign(TutorialSign sign)
    {
        _name = sign.gameObject.name;
        _type = sign.Type;
        _text = sign.SignText.text;
        _position = new Vector2(sign.gameObject.transform.position.x, sign.gameObject.transform.position.y);
        _imgRotation = sign.ImageTransform.rotation.eulerAngles.z;
        _uid = sign.UID;
    }
    public ISerializable Deserialize(GameObject targetObject, GameObject contextObject)
    {
        targetObject.name = _name;
        TutorialSign tutorialSign = targetObject.GetComponent<TutorialSign>();
        tutorialSign.SignText.text = _text;
        targetObject.transform.position = new(_position.x, _position.y);
        tutorialSign.ImageTransform.rotation = Quaternion.Euler(0, 0, _imgRotation);
        tutorialSign.UID = _uid;
        return tutorialSign;
    }
}

