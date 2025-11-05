using TMPro;
using UnityEngine;
using System;
using UnityEngine.UIElements;

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
    public string UID { get; set; }
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

    public SignType Type => _type;

    public IDeserializable Serialize()
    {

        return new SerializedTutorialSign(this);
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

    public void RegisterResync()
    {
        LevelManager.ResyncHub.RegisterResync(this);
    }
}