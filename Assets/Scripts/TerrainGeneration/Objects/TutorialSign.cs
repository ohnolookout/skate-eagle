using TMPro;
using UnityEngine;
using System;

public class TutorialSign: MonoBehaviour
{
    [SerializeField] private TextMeshPro _signText;

    public TextMeshPro SignText
    {
        get => _signText;
        set => _signText = value;
    }
}


[Serializable]
public class SerializedTutorialSignData
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
    public SerializedTutorialSignData(string signText, Vector2 position)
    {
        _signText = signText;
        _position = position;
    }
}
