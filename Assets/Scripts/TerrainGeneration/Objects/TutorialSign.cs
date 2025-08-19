using TMPro;
using UnityEngine;
using System;

public enum SignType
{
    ArrowSquare,
    ArrowText,
    RotateSquare,
    RotateText
}
public class TutorialSign: MonoBehaviour, ISerializable
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private SignType _type = SignType.ArrowSquare;
    [SerializeField] private Transform _imageTransform;
    public GameObject GameObject => gameObject;

    public TMP_Text SignText
    {
        get => _text;
        set => _text = value;
    }

    public Transform ImageTransform
    {
        get => _imageTransform;
        set => _imageTransform = value;
    }

    public IDeserializable Serialize()
    {

        return new SerializedTutorialSign(gameObject.name, _type, _text.text, new Vector2(transform.position.x, transform.position.y), _imageTransform.rotation.eulerAngles.z);
    }

    public void Clear()
    {
        _text.text = string.Empty;
        _imageTransform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void Refresh(GroundManager _)
    {
        return;
    }
}