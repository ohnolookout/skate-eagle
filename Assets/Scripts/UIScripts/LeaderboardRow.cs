using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardRow : MonoBehaviour
{
    [SerializeField] private TMP_Text _rank, _name, _time;
    [SerializeField] private GameObject _panel;
    [SerializeField] private Image _panelImage;
    public TMP_Text Rank => _rank;
    public TMP_Text Name => _name;
    public TMP_Text Time => _time;
    private static Color _panelColorLight = new(183 / 255f, 183 / 255f, 183 / 255f, 65 / 255f);
    private static Color _panelColorDark = new(138 / 255f, 142 / 255f, 144 / 255f, 65 / 255f);
    private static Color _panelColorHighlighted = new(83 / 255f, 240 / 255f, 146 / 255f, 65 / 255f);

    public void SetValues(int rank, string name, float timeInMilliseconds)
    {
        _rank.text = rank.ToString();
        _name.text = name;
        var timerChars = new char[8];
        Timer.SecondsToCharArray(timeInMilliseconds/1000, timerChars);
        _time.SetCharArray(timerChars);
    }

    public void PanelIsActive(bool doActivate)
    {
        _panel.SetActive(doActivate);
    }

    public void Highlight()
    {
        _panelImage.color = _panelColorHighlighted;
    }
    public void Unhighlight(int rowIndex)
    {
        if(rowIndex % 2 == 0)
        {
            _panelImage.color = _panelColorDark;
        }
        else
        {
            _panelImage.color = _panelColorLight;
        }
    }
}
