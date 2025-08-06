using TMPro;
using UnityEngine;
using System;

public class TutorialSign: MonoBehaviour, ISerializable
{
    [SerializeField] private TMP_Text _signText;
    [SerializeField] private bool _isSquare = false;
    [SerializeField] private Transform _imageTransform;
    public GameObject GameObject => gameObject;

    public TMP_Text SignText
    {
        get => _signText;
        set => _signText = value;
    }

    public Transform ImageTransform
    {
        get => _imageTransform;
        set => _imageTransform = value;
    }

    public IDeserializable Serialize()
    {

        return new SerializedTutorialSign(gameObject.name, _signText.text, new Vector2(transform.position.x, transform.position.y), _imageTransform.rotation.eulerAngles.z, _isSquare);
    }

    public void Clear()
    {
        _signText.text = string.Empty;
        _isSquare = false;
        _imageTransform.rotation = Quaternion.Euler(0, 0, 0);
    }
}