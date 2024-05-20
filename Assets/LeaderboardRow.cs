using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardRow : MonoBehaviour
{
    [SerializeField] private TMP_Text _rank, _name, _time;
    [SerializeField] private GameObject _panel;
    public TMP_Text Rank => _rank;
    public TMP_Text Name => _name;
    public TMP_Text Time => _time;

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
}
