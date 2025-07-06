using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using UnityEngine.UI;
using PlayFab;
using TMPro;

public class PopUpFactory
{
    MenuPanel _popUpPanel;
    GameManager _gameManager;
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
    public void ShowFirstTimeSetNamePopUp(MenuPanel popUpPanel, bool isTrueFirstTime)
    {
        _popUpPanel = popUpPanel;
        var firstTimeNamePreset = MenuPanelPresets.SetNameFirstTimePanel(
            //OnPrimaryYesButton
            () => SubmitDisplayName(isTrueFirstTime),
            //OnPrimaryNeutralButton
            () => _popUpPanel.ShowSecondaryPanel(0),
            //OnPrimaryTextBlockButton
            () => _popUpPanel.ShowSecondaryPanel(ShowFirstTimeEmailLoginPopUp),
            //OnSecondarySkipYesButton
            () => GenerateDisplayName(isTrueFirstTime),
            //OnSecondarySkipNeutralButton
            _popUpPanel.HidePanel,
            //OnSecondaryErrorYesButton
            _popUpPanel.HidePanel,
            //OnSecondaryErrorNeutralButton
            () => _popUpPanel.ShowSecondaryPanel(0)
            );
        if (!isTrueFirstTime)
        {
            firstTimeNamePreset.TextBlockButtonText = "";
            firstTimeNamePreset.OnTextButtonClicked = null;
        }
        _popUpPanel.LoadPanelPreset(firstTimeNamePreset, false, true);
    }
    private void SubmitDisplayName(bool isFirstTimeUser)
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
            _gameManager.PlayFabManager.OnSetNameComplete += OnChangeNameComplete;

        }
        _gameManager.PlayFabManager.SetDisplayName(_popUpPanel.InputFields[0].text);
    }


    private void OnFirstTimeSetNameComplete(bool isSuccess, string submittedName, string storedName, PlayFabError error)
    {
        _gameManager.OnLoading?.Invoke(false);
        _gameManager.PlayFabManager.OnSetNameComplete -= OnFirstTimeSetNameComplete;

        if (!isSuccess)
        {
            _popUpPanel.ShowSecondaryPanel(1);
            _popUpPanel.BodyText.text = "Something went wrong: <br><br>" + error.ErrorMessage + ".<br><br>You can try again or skip account setup for now and come back to it later.";
            return;
        }

        if(submittedName == "SkateEagle")
        {
            ShowFirstTimeAddEmailPopUp(_popUpPanel);
            return;
        }

        //Show confirmation that goes to add email panel
        var confirmationPopUp = MenuPanelPresets.ChangeNameSuccessPanel(() => ShowFirstTimeAddEmailPopUp(_popUpPanel), submittedName);
        _popUpPanel.LoadPanelPreset(confirmationPopUp, false, true);
    }
    private void OnChangeNameComplete(bool isSuccess, string submittedName, string storedName, PlayFabError error)
    {
        _gameManager.OnLoading?.Invoke(false);
        _gameManager.PlayFabManager.OnSetNameComplete -= OnChangeNameComplete;

        if (!isSuccess)
        {
            Debug.Log("Set name failed...");
            _popUpPanel.ErrorText.text = error.ErrorMessage + ".";
            _popUpPanel.ErrorText.gameObject.SetActive(true);
        }
        else
        {
            var confirmationPopUp = MenuPanelPresets.ChangeNameSuccessPanel(_popUpPanel.HidePanel, submittedName);
            _popUpPanel.LoadPanelPreset(confirmationPopUp, false, true);
        }
    }

    private static bool IsValidUsername(string name)
    {
        return name.Length >= 3 && Regex.IsMatch(name, @"^[A-Za-z0-9]+$");
    }

    private void GenerateDisplayName(bool isTrueFirstTime)
    {
        _gameManager.OnLoading?.Invoke(true);
        if (isTrueFirstTime)
        {
            _gameManager.PlayFabManager.OnSetNameComplete += OnFirstTimeSetNameComplete;
        }
        else
        {
            _gameManager.PlayFabManager.OnSetNameComplete += OnChangeNameComplete;

        }
        _gameManager.PlayFabManager.GenerateName();
    }
    #endregion

    #region Add Email
    public void ShowFirstTimeAddEmailPopUp(MenuPanel popUpPanel)
    {
        _popUpPanel = popUpPanel;
        _popUpPanel.LoadPanelPreset(MenuPanelPresets.AddEmailFirstTimePanel(
        //OnYesButton
            ValidateAndAddEmail,
            //OnNeutralButton
            _popUpPanel.HidePanel,
            //OnToggleChanged
            (toggle) => {
                if (toggle)
                {
                    Debug.Log("Setting DontAskEmailKey to 1");
                    PlayerPrefs.SetInt(PlayFabManager.DontAskEmailKey, 1);
                }
                else
                {
                    Debug.Log("Setting DontAskEmailKey to 0");
                    PlayerPrefs.SetInt(PlayFabManager.DontAskEmailKey, 0);
                }
            },
            //OnSecondaryConfirmNeutralButton
            _popUpPanel.Close
            ),
            false, true);
        _popUpPanel.InputFields[1].contentType = TMP_InputField.ContentType.Password;
        _popUpPanel.InputFields[2].contentType = TMP_InputField.ContentType.Password;
    }
    public void ShowAddEmailPopUp(MenuPanel popUpPanel, bool hasBack = false)
    {
        _popUpPanel = popUpPanel;
        _popUpPanel.LoadPanelPreset(MenuPanelPresets.AddEmailPanel(
        //OnYesButton
            ValidateAndAddEmail,
            //OnNeutralButton
            _popUpPanel.HidePanel,
            //OnSecondaryConfirmNeutralButton
            _popUpPanel.Close
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

    private void ValidateAndAddEmail ()
    {
        if (ValidateAddEmailAndPasswords())
        {
            _popUpPanel.ErrorText.gameObject.SetActive(false);
            _gameManager.PlayFabManager.OnAddEmailComplete += OnAddEmailComplete;
            _gameManager.PlayFabManager.AddEmail(_popUpPanel.InputFields[0].text, _popUpPanel.InputFields[1].text);
        }
    }

    private bool ValidateAddEmailAndPasswords()
    {
        //Validate email
        if (!IsValidEmail(_popUpPanel.InputFields[0].text))
        {
            _popUpPanel.ErrorText.text = "Invalid email.";
            _popUpPanel.ErrorText.gameObject.SetActive(true);
            return false;
        }

        //Validate passwords
        if (_popUpPanel.InputFields[1].text.Length < 8)
        {
            _popUpPanel.ErrorText.text = "Password must be at least 8 characters.";
            _popUpPanel.ErrorText.gameObject.SetActive(true);
            return false;
        }

        if (_popUpPanel.InputFields[1].text != _popUpPanel.InputFields[2].text)
        {
            _popUpPanel.ErrorText.text = "Passwords don't match.";
            _popUpPanel.ErrorText.gameObject.SetActive(true);
            return false;
        }

        return true;
    }

    private bool IsValidEmail(string emailAddress)
    {
        var pattern = @"^[a-zA-Z0-9.!#$%&'*+-/=?^_`{|}~]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$";

        var regex = new Regex(pattern);
        return regex.IsMatch(emailAddress);
    }

    #endregion

    #region Email Login
    private void ShowFirstTimeEmailLoginPopUp(MenuPanel popUpPanel)
    {
        _popUpPanel = popUpPanel;
        var emailLoginPanel = MenuPanelPresets.FirstTimeEmailLoginPanel(
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

        _popUpPanel.LoadPanelPreset(emailLoginPanel, true, true);
        _popUpPanel.InputFields[1].contentType = TMP_InputField.ContentType.Password;
    }

    private void ShowEmailLoginPopUp(MenuPanel popUpPanel, bool hasBack = false)
    {
        _popUpPanel = popUpPanel;
        var emailLoginPanel = MenuPanelPresets.EmailLoginPanel(
                //OnYesButton
                () => _popUpPanel.ShowSecondaryPanel(0),
                //OnNeutralButton
                _popUpPanel.HidePanel,
                //OnTextBlockButton
                () => _popUpPanel.ShowSecondaryPanel(ShowAddEmailPopUp),
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
            _popUpPanel.BodyText.text = "Something went wrong:<br><br>" + error.ErrorMessage;
        }
    }

    #endregion

    #region Main Menu Panels

    public void ShowNewGamePanel(MenuPanel popUpPanel)
    {
        _popUpPanel = popUpPanel;

        var newGamePanel = MenuPanelPresets.ConfirmNewGamePanel(
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
            () => _popUpPanel.ShowSecondaryPanel(ShowEmailLoginPopUp),
            //Register email button
            () => _popUpPanel.ShowSecondaryPanel(ShowAddEmailPopUp),
            //Switch account button
            () => _popUpPanel.ShowSecondaryPanel(ShowSwitchEmailPopUp),
            //Delete account button
            //() => _gameManager.PlayFabManager.DeletePlayerAccount(_gameManager),
            () => _popUpPanel.ShowSecondaryPanel(ShowConfirmResetAccountPopUp),
            //Close button
            _popUpPanel.Close,
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

    private void ShowConfirmResetAccountPopUp(MenuPanel popUpPanel, bool hasBack = true)
    {
        _popUpPanel = popUpPanel;
        var confirmPopUp = MenuPanelPresets.ConfirmResetAccountPanel(
            //OnYesButton
            () => _gameManager.PlayFabManager.DeletePlayerAccount(_gameManager),
            //OnNoButton
            _popUpPanel.HidePanel
        );

        _popUpPanel.LoadPanelPreset(confirmPopUp, hasBack, true);
    }

    public void ShowResetAccountSuccessPopUp(MenuPanel popUpPanel)
    {
        _popUpPanel = popUpPanel;
        _popUpPanel.LoadPanelPreset(MenuPanelPresets.ResetAccountSuccessPanel(
            () => ShowFirstTimeSetNamePopUp(_popUpPanel, false)
            ), 
            false, true);
    }

    #endregion
}
