using TMPro;
using UnityEngine;
using System;

public class TutorialSign: MonoBehaviour, ISerializable
{
    [SerializeField] private TMP_Text _signText;
    [SerializeField] private bool _isSquare = false;

    public TMP_Text SignText
    {
        get => _signText;
        set => _signText = value;
    }

    public bool IsSquare
    {
        get => _isSquare;
        set => _isSquare = value;
    }

    public IDeserializable Serialize()
    {
        return new SerializedTutorialSign(gameObject.name, _signText.text, new Vector2(transform.position.x, transform.position.z), _isSquare);
    }
}