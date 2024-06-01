using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NewPlayerPanel: MonoBehaviour
{
    [SerializeField] private MainMenu _menu;
    [SerializeField] private TMP_InputField _playerNameInputField;
    [SerializeField] GameObject _errorText;
    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = GameManager.Instance;
    }

    public async void SetPlayerName()
    {
        var nameResponse = await _gameManager.SetPlayerName(_playerNameInputField.text);
        if (nameResponse.success)
        {
            Debug.Log("Name set successfully");
            _errorText.SetActive(false);
            gameObject.SetActive(false);
            return;
        }
        else
        {
            Debug.Log("Set name failed");
            _errorText.SetActive(true);
        }
    }

}
