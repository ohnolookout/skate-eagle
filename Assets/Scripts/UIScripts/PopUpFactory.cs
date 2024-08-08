using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using PlayFab;
using TMPro;

public class PopUpFactory
{
    MenuPanel _popUpPanel;
    GameManager _gameManager;
    public bool DoingFirstTimeSetup;
    public PopUpFactory(MenuPanel popUpPanel)
    {
        _popUpPanel = popUpPanel; 
        _gameManager = GameManager.Instance;
    }

    #region Set Name
    private void ShowSetNamePopUp(MenuPanel popUpPanel, bool hasBack = false)
    {
        _popUpPanel = popUpPanel;
        _popUpPanel.LoadPanelPreset(
            MenuPanelPresets.SetNamePanel(
                //Primary yes button
                () => SubmitDisplayName(false),
                //Primary neutral button
                _popUpPanel.HidePanel,
                //Secondary error yes button
                _popUpPanel.HidePanel,
                //Secondary error neutral button
                _popUpPanel.HidePanel
                ),
            hasBack, true);
    }
    public void ShowFirstTimeSetNamePopUp(MenuPanel popUpPanel)
    {
        _popUpPanel = popUpPanel;
        var firstTimeNamePreset = MenuPanelPresets.SetNameFirstTimePanel(
            () => SubmitDisplayName(true),
            () => _popUpPanel.ShowSecondaryPanel(0),
            () => ShowEmailLoginPopUp(_popUpPanel),
            GenerateDisplayName,
            _popUpPanel.HidePanel,
            _popUpPanel.HidePanel,
            () => _popUpPanel.ShowSecondaryPanel(0)
            );
        _popUpPanel.LoadPanelPreset(firstTimeNamePreset, false, true);
    }
    private void SubmitDisplayName(bool isFirstTimeUser = false)
    {
        var name = _popUpPanel.InputFields[0].text;
        if (!IsValidUsername(name))
        {
            _popUpPanel.ErrorText.text = "Invalid name. Names must be 3 or more characters, alphanumeric only.";
            _popUpPanel.ErrorText.gameObject.SetActive(true);
            return;
        }

        _popUpPanel.ErrorText.gameObject.SetActive(false);
        _gameManager.OnLoading?.Invoke(true);
        if (isFirstTimeUser)
        {
            _gameManager.PlayFabManager.OnSetNameComplete += OnFirstTimeSetNameComplete;
        }
        else
        {
            _gameManager.PlayFabManager.OnSetNameComplete += OnSetNameComplete;

        }
        _gameManager.PlayFabManager.SetDisplayName(_popUpPanel.InputFields[0].text);
    }
    private void OnFirstTimeSetNameComplete(bool isSuccess, string storedName, PlayFabError error)
    {
        _gameManager.OnLoading?.Invoke(false);
        _gameManager.PlayFabManager.OnSetNameComplete -= OnFirstTimeSetNameComplete;

        if (!isSuccess)
        {
            _popUpPanel.ShowSecondaryPanel(1);
            _popUpPanel.BodyText.text = "Something went wrong: <br><br>" + error.ErrorMessage + ".<br><br>You can try again or skip account setup for now and come back to it later.";
        }
        else
        {
            ShowAddEmailPopUp(_popUpPanel);
        }
    }
    private void OnSetNameComplete(bool isSuccess, string storedName, PlayFabError error)
    {
        _gameManager.OnLoading?.Invoke(false);
        _gameManager.PlayFabManager.OnSetNameComplete -= OnSetNameComplete;

        if (!isSuccess)
        {
            Debug.Log("Set name failed...");
            _popUpPanel.ErrorText.text = error.ErrorMessage + ".";
            _popUpPanel.ErrorText.gameObject.SetActive(true);
        }
        else
        {
            _popUpPanel.HidePanel();
        }
    }
    private static bool IsValidUsername(string name)
    {
        return name.Length >= 3 && Regex.IsMatch(name, @"^[A-Za-z0-9]+$");
    }

    private void GenerateDisplayName()
    {
        _gameManager.OnLoading?.Invoke(true);
        _gameManager.PlayFabManager.OnSetNameComplete += OnFirstTimeSetNameComplete;
        _gameManager.PlayFabManager.GenerateName();
    }
    #endregion

    #region Add Email
    public void ShowAddEmailPopUp(MenuPanel popUpPanel, bool hasBack = false)
    {
        _popUpPanel = popUpPanel;
        _popUpPanel.LoadPanelPreset(MenuPanelPresets.AddEmailPanel(
        //OnYesButton
        () =>
        {
            //Validate email
            if (!IsValidEmail(_popUpPanel.InputFields[0].text))
            {
                _popUpPanel.ErrorText.text = "Invalid email.";
                _popUpPanel.ErrorText.gameObject.SetActive(true);
                return;
            }

            //Validate passwords
            if (_popUpPanel.InputFields[1].text.Length < 8)
            {
                _popUpPanel.ErrorText.text = "Password must be at least 8 characters.";
                _popUpPanel.ErrorText.gameObject.SetActive(true);
                return;
            }

            if (_popUpPanel.InputFields[1].text != _popUpPanel.InputFields[2].text)
            {
                _popUpPanel.ErrorText.text = "Passwords don't match.";
                _popUpPanel.ErrorText.gameObject.SetActive(true);
                return;
            }

            _popUpPanel.ErrorText.gameObject.SetActive(false);
            _gameManager.PlayFabManager.OnAddEmailComplete += OnAddEmailComplete;
            _gameManager.PlayFabManager.AddEmail(_popUpPanel.InputFields[0].text, _popUpPanel.InputFields[1].text);
        },
        //OnNeutralButton
        () => {
            _popUpPanel.gameObject.SetActive(false);
        },
        //OnToggleChanged
        (toggle) => {
            if (toggle)
            {
                PlayerPrefs.SetInt(PlayFabManager.DontAskEmailKey, 1);
            }
            else
            {
                PlayerPrefs.SetInt(PlayFabManager.DontAskEmailKey, 0);
            }
        },
        //OnSecondaryConfirmNeutralButton
        () => {
            _popUpPanel.gameObject.SetActive(false);
        }
        ),
        hasBack, true);
        _popUpPanel.InputFields[1].contentType = TMP_InputField.ContentType.Password;
        _popUpPanel.InputFields[2].contentType = TMP_InputField.ContentType.Password;
    }
    private void OnAddEmailComplete(bool isSuccess, PlayFabError error = null)
    {
        if (isSuccess)
        {
            _popUpPanel.ErrorText.gameObject.SetActive(false);
            _popUpPanel.ShowSecondaryPanel(0);
            var newInitializationResult = _gameManager.InitializationResult;
            newInitializationResult.hasEmail = true;
            _gameManager.InitializationResult = newInitializationResult;
        }
        else
        {
            _popUpPanel.ErrorText.text = error.ErrorMessage;
            _popUpPanel.ErrorText.gameObject.SetActive(true);
        }
    }

    private bool IsValidEmail(string emailAddress)
    {
        var pattern = @"^[a-zA-Z0-9.!#$%&'*+-/=?^_`{|}~]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$";

        var regex = new Regex(pattern);
        return regex.IsMatch(emailAddress);
    }
    #endregion

    #region Email Login
    private void ShowEmailLoginPopUp(MenuPanel popUpPanel, bool hasBack = false)
    {
        _popUpPanel = popUpPanel;
        var emailLoginPanel = MenuPanelPresets.EmailLoginPanel(
                //OnYesButton
                () => _popUpPanel.ShowSecondaryPanel(0),
                //OnNeutralButton
                _popUpPanel.HidePanel,
                //OnTextBlockButton
                () => ShowAddEmailPopUp(_popUpPanel, true),
                //OnSecondaryConfirmationYesButton
                () =>
                {
                    var email = _popUpPanel.InputFields[0].text;
                    var password = _popUpPanel.InputFields[1].text;

                    if (!IsValidEmail(email))
                    {
                        _popUpPanel.ErrorText.text = "Invalid email.";
                        _popUpPanel.ErrorText.gameObject.SetActive(true);
                        return;
                    }

                    _popUpPanel.ErrorText.gameObject.SetActive(false);

                    _gameManager.PlayFabManager.OnEmailLoginComplete += OnEmailLoginComplete;
                    _gameManager.PlayFabManager.EmailLogin(email, password);
                },
                //OnSecondaryConfirmationNoButton
                _popUpPanel.HidePanel,
                //OnSecondaryErrorNeutralButton
                _popUpPanel.HidePanel,
                //OnSecondarySuccessNeutralButton
                _popUpPanel.HidePanel
                );

        _popUpPanel.LoadPanelPreset(emailLoginPanel, hasBack, true);
        _popUpPanel.InputFields[1].contentType = TMP_InputField.ContentType.Password;
    }

    private void ShowSwitchEmailPopUp(MenuPanel popUpPanel, bool hasBack = false)
    {
        _popUpPanel = popUpPanel;
        var emailLoginPanel = MenuPanelPresets.SwitchEmailLoginPanel(
                //OnYesButton
                () =>
                {
                    var email = _popUpPanel.InputFields[0].text;
                    var password = _popUpPanel.InputFields[1].text;

                    if (!IsValidEmail(email))
                    {
                        _popUpPanel.ErrorText.text = "Invalid email.";
                        _popUpPanel.ErrorText.gameObject.SetActive(true);
                        return;
                    }

                    _popUpPanel.ErrorText.gameObject.SetActive(false);

                    _gameManager.PlayFabManager.OnEmailLoginComplete += OnEmailLoginComplete;
                    _gameManager.PlayFabManager.EmailLogin(email, password);
                },
                //OnNeutralButton
                _popUpPanel.HidePanel,
                //OnSecondaryErrorNeutralButton
                _popUpPanel.HidePanel,
                //OnSecondarySuccessNeutralButton
                _popUpPanel.HidePanel
                );

        _popUpPanel.LoadPanelPreset(emailLoginPanel, hasBack, true);
        _popUpPanel.InputFields[1].contentType = TMP_InputField.ContentType.Password;
    }


    private void OnEmailLoginComplete(bool isSuccess, PlayFabError error)
    {
        if (isSuccess)
        {
            _gameManager.PlayFabManager.LoadFromCloud();
            _popUpPanel.ShowSecondaryPanel(1);
        }
        else
        {
            _popUpPanel.ShowSecondaryPanel(0);
            _popUpPanel.ErrorText.text = "Something went wrong:<br><br>" + error.ErrorMessage;
        }
    }

    #endregion

    #region Main Menu Panels

    public void ShowNewGamePanel(MenuPanel popUpPanel)
    {
        _popUpPanel = popUpPanel; 

        MenuPanelPreset newGamePanel = MenuPanelPresets.ConfirmNewGamePanel(
            () =>
            {
                _gameManager.ResetSaveData();
                _popUpPanel.gameObject.SetActive(false);
            },
            () => _popUpPanel.gameObject.SetActive(false)
            );

        _popUpPanel.LoadPanelPreset(newGamePanel, true, true);
    }

    public void ShowSettingsPanel(MenuPanel popUpPanel)
    {
        _popUpPanel = popUpPanel;


        var settingsPanel = MenuPanelPresets.PlayerSettingsPanel(
            //Change name button
            () => _popUpPanel.ShowSecondaryPanel(ShowSetNamePopUp),
            //Sign in button
            () => _popUpPanel.ShowSecondaryPanel(ShowAddEmailPopUp),
            //Register email button
            () => _popUpPanel.ShowSecondaryPanel(ShowEmailLoginPopUp),
            //Switch account button
            () => _popUpPanel.ShowSecondaryPanel(ShowSwitchEmailPopUp),
            //Delete account button
            () => _gameManager.PlayFabManager.DeletePlayerAccount(),
            //Close button
            _popUpPanel.HidePanel,
            //On load panel
            () => {
                _popUpPanel.BodyText.text = PlayerPrefs.GetString(PlayFabManager.FormattedDisplayNameKey, "Skate Eagle");

                if (!_gameManager.InitializationResult.hasEmail)
                {
                    _popUpPanel.VerticalButtons[1].gameObject.SetActive(true);
                    _popUpPanel.VerticalButtons[2].gameObject.SetActive(true);
                    _popUpPanel.VerticalButtons[3].gameObject.SetActive(false);
                    _popUpPanel.VerticalButtons[4].gameObject.SetActive(true);
                }
                else
                {
                    _popUpPanel.VerticalButtons[1].gameObject.SetActive(false);
                    _popUpPanel.VerticalButtons[2].gameObject.SetActive(false);
                    _popUpPanel.VerticalButtons[3].gameObject.SetActive(true);
                    _popUpPanel.VerticalButtons[4].gameObject.SetActive(true);
                }
            }
        );

        _popUpPanel.LoadPanelPreset(settingsPanel, false, true);
    }

    #endregion
}
