using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using UnityEngine.UI;
using PlayFab;
using System.Text.RegularExpressions;

public class MainMenu : MonoBehaviour
{
    public Button MainPlayButton;
    public Button MainQuitButton;
    public Button MainNewGameButton;
    public Button MainPlayerSettingsButton;
    public Button LevelScreenBackButton;
    public Button LevelScreenPlayButton;
    public PopUpFactory _popUpFactory;
    public MenuPanel PopUpPanel;
    public MenuPanel SettingsPanel;
    public TMP_Text UsernameDisplay;
    public LevelMenu LevelMenu;
    public GameObject levelScreen, titleScreen;
    public GameObject loadingScreen;
    public bool DoingFirstTimeSetup = false;

    private GameManager _gameManager;

    void Awake()
    {
        _gameManager = GameManager.Instance;
        _gameManager.PlayFabManager.OnInitializationComplete += HandleStartUp;
        _gameManager.OnMenuLoaded += OnMenuLoaded;
        _gameManager.PlayFabManager.OnUpdateStoredName += (displayName, formattedDisplayName) =>
        {
            UsernameDisplay.text = formattedDisplayName;
        };

        _popUpFactory = new(PopUpPanel);
        
        //Main screen buttons
        MainPlayButton.onClick.AddListener(GoToLevelScreen);
        MainQuitButton.onClick.AddListener(Application.Quit);
        MainNewGameButton.onClick.AddListener( () => _popUpFactory.ShowNewGamePanel(PopUpPanel));
        MainPlayerSettingsButton.onClick.AddListener(() => _popUpFactory.ShowSettingsPanel(PopUpPanel));

    }
    //Show notification depending on whether it's a new player or not
    private void HandleStartUp(InitializationResult result)
    {
        _gameManager.PlayFabManager.OnInitializationComplete -= HandleStartUp;
        if (result.isFirstTime)
        {
            DoingFirstTimeSetup = true;
            _popUpFactory.ShowFirstTimeSetNamePopUp(PopUpPanel);
            return;
        }

        UsernameDisplay.text = PlayerPrefs.GetString(PlayFabManager.FormattedDisplayNameKey, "");

        if (_gameManager.InitializationResult.doAskEmail)
        {
            _popUpFactory.ShowAddEmailPopUp(PopUpPanel);
        }

    }

    void Start()
    {

    }

    #region Navigation
    private void OnMenuLoaded(bool goToLevelMenu)
    {
        if (goToLevelMenu)
        {
            GoToLevelScreen();
        }
    }

    public void LoadLevel(Level level)
    {
        _gameManager.LoadLevel(level);
    }

    public void GoToLevelScreen()
    {
        titleScreen.SetActive(false);
        levelScreen.SetActive(true);
    }

    public void StartScreen()
    {
        titleScreen.SetActive(true);
        levelScreen.SetActive(false);
    }
    #endregion
    /*

    #region Settings Panel
    public void ShowSettingsPanel()
    {
        DoSettingsPanelSetup();
        SettingsPanel.gameObject.SetActive(true);
    }
    public void DoSettingsPanelSetup()
    {

        SettingsPanel.GrayOutButton.onClick.AddListener(() => SettingsPanel.gameObject.SetActive(false));
        SettingsPanel.VerticalButtons[4].onClick.AddListener(() => SettingsPanel.gameObject.SetActive(false));

        if (!_gameManager.InitializationResult.hasEmail)
        {
            SettingsPanel.VerticalButtons[1].gameObject.SetActive(true);
            SettingsPanel.VerticalButtons[2].gameObject.SetActive(true);
            SettingsPanel.VerticalButtons[3].gameObject.SetActive(false);
        } else
        {
            SettingsPanel.VerticalButtons[1].gameObject.SetActive(false);
            SettingsPanel.VerticalButtons[2].gameObject.SetActive(false);
            SettingsPanel.VerticalButtons[3].gameObject.SetActive(true);
        }

    }
    #endregion
    

    #region Add Email
    private void ShowAddEmailPopUp()
    {
        PopUpPanel.LoadPanelPreset(MenuPanelPresets.AddEmailPanel(
        //OnYesButton
        () =>
        {
            //Validate email
            if (!IsValidEmail(PopUpPanel.InputFields[0].text))
            {
                PopUpPanel.ErrorText.text = "Invalid email.";
                PopUpPanel.ErrorText.gameObject.SetActive(true);
                return;
            }

            //Validate passwords
            if (PopUpPanel.InputFields[1].text.Length < 8)
            {
                PopUpPanel.ErrorText.text = "Password must be at least 8 characters.";
                PopUpPanel.ErrorText.gameObject.SetActive(true);
                return;
            }

            if (PopUpPanel.InputFields[1].text != PopUpPanel.InputFields[2].text)
            {
                PopUpPanel.ErrorText.text = "Passwords don't match.";
                PopUpPanel.ErrorText.gameObject.SetActive(true);
                return;
            }

            PopUpPanel.ErrorText.gameObject.SetActive(false);
            _gameManager.PlayFabManager.OnAddEmailComplete += OnAddEmailComplete;
            _gameManager.PlayFabManager.AddEmail(PopUpPanel.InputFields[0].text, PopUpPanel.InputFields[1].text);
        },
        //OnNeutralButton
        () => {
            PopUpPanel.gameObject.SetActive(false);
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
            PopUpPanel.gameObject.SetActive(false);
        }
        ), 
        true);
    }
    private void OnAddEmailComplete(bool isSuccess, PlayFabError error = null)
    {
        if (isSuccess)
        {
            PopUpPanel.ErrorText.gameObject.SetActive(false);
            PopUpPanel.ShowSecondaryPanel(0);
            var newInitializationResult = _gameManager.InitializationResult;
            newInitializationResult.hasEmail = true;
            _gameManager.InitializationResult = newInitializationResult;
        }
        else
        {
            PopUpPanel.ErrorText.text = error.ErrorMessage;
            PopUpPanel.ErrorText.gameObject.SetActive(true);
        }
    }

    private bool IsValidEmail(string emailAddress)
    {
        var pattern = @"^[a-zA-Z0-9.!#$%&'*+-/=?^_`{|}~]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$";

        var regex = new Regex(pattern);
        return regex.IsMatch(emailAddress);
    }



    private void ShowEmailLoginPopUp()
    {
        var emailLoginPanel = MenuPanelPresets.EmailLoginPanel(
                //OnYesButton
                () => PopUpPanel.ShowSecondaryPanel(0),
                //OnNeutralButton
                () =>
                {
                    if (DoingFirstTimeSetup)
                    {
                        ShowFirstTimeSetNamePopUp();
                    }
                    else
                    {
                        PopUpPanel.gameObject.SetActive(false);
                    }
                },
                //OnSecondaryConfirmationYesButton
                () =>
                {
                    var email = PopUpPanel.InputFields[0].text;
                    var password = PopUpPanel.InputFields[1].text;

                    if (!IsValidEmail(email))
                    {
                        PopUpPanel.ErrorText.text = "Invalid email.";
                        PopUpPanel.ErrorText.gameObject.SetActive(true);
                        return;
                    }

                    PopUpPanel.ErrorText.gameObject.SetActive(false);

                    _gameManager.PlayFabManager.OnEmailLoginComplete += OnEmailLoginComplete;
                    _gameManager.PlayFabManager.EmailLogin(email, password);
                },
                //OnSecondaryConfirmationNoButton
                PopUpPanel.HideSecondaryPanel,
                //OnSecondaryErrorNeutralButton
                PopUpPanel.HideSecondaryPanel,
                //OnSecondarySuccessNeutralButton
                () => PopUpPanel.gameObject.SetActive(false)
                );

        PopUpPanel.LoadPanelPreset( emailLoginPanel, true);
    }


    private void OnEmailLoginComplete(bool isSuccess, PlayFabError error)
    {
        if (isSuccess)
        {
            _gameManager.PlayFabManager.LoadFromCloud();
            PopUpPanel.ShowSecondaryPanel(2);
        }
        else
        {
            PopUpPanel.ShowSecondaryPanel(1);
            PopUpPanel.SecondaryPanel.ErrorText.text = "Something went wrong:<br><br>" + error.ErrorMessage;
        }
    }
    #endregion
    private void SkipAccountSetup()
    {

    }

    #region Set Name
    private void ShowFirstTimeSetNamePopUp()
    {
        var firstTimeNamePreset = MenuPanelPresets.SetNameFirstTimePanel(
            () => SubmitDisplayName(true),
            () => PopUpPanel.ShowSecondaryPanel(0),
            ShowEmailLoginPopUp,
            GenerateDisplayName,
            PopUpPanel.HideSecondaryPanel,
            PopUpPanel.HideSecondaryPanel,
            () => PopUpPanel.ShowSecondaryPanel(0)
            );
        firstTimeNamePreset.CanExitWithBackground = false;
        PopUpPanel.LoadPanelPreset(firstTimeNamePreset, true);
    }

    private void ShowSetNamePopUp()
    {
        PopUpPanel.LoadPanelPreset(
            MenuPanelPresets.SetNamePanel(
                () => SubmitDisplayName(false),
                () => PopUpPanel.gameObject.SetActive(false),
                ShowEmailLoginPopUp,
                GenerateDisplayName,
                PopUpPanel.HideSecondaryPanel,
                PopUpPanel.HideSecondaryPanel,
                () => PopUpPanel.ShowSecondaryPanel(0)
                ),
            true);
    }
    private void SubmitDisplayName(bool isFirstTimeUser = false)
    {
        var name = PopUpPanel.InputFields[0].text;
        if (!IsValidUsername(name))
        {
            PopUpPanel.ErrorText.text = "Invalid name. Names must be 3 or more characters, alphanumeric only.";
            PopUpPanel.ErrorText.gameObject.SetActive(true);
            return;
        }

        PopUpPanel.ErrorText.gameObject.SetActive(false);
        loadingScreen.SetActive(true);
        if (isFirstTimeUser){
            _gameManager.PlayFabManager.OnSetNameComplete += OnFirstTimeSetNameComplete;
        }
        else
        {
            _gameManager.PlayFabManager.OnSetNameComplete += OnSetNameComplete;

        }
        _gameManager.PlayFabManager.SetDisplayName(PopUpPanel.InputFields[0].text);
    }
    private bool IsValidUsername(string name)
    {
        return name.Length >= 3 && Regex.IsMatch(name, @"^[A-Za-z0-9]+$");
    }
    private void GenerateDisplayName()
    {
        loadingScreen.SetActive(true);
        _gameManager.PlayFabManager.OnSetNameComplete += OnFirstTimeSetNameComplete;
        _gameManager.PlayFabManager.GenerateName();
    }

    private void OnFirstTimeSetNameComplete(bool isSuccess, string storedName, PlayFabError error)
    {
        loadingScreen.SetActive(false);
        _gameManager.PlayFabManager.OnSetNameComplete -= OnFirstTimeSetNameComplete;

        if (!isSuccess)
        {
            PopUpPanel.ShowSecondaryPanel(1);
            PopUpPanel.SecondaryPanel.BodyText.text = "Something went wrong: <br><br>" + error.ErrorMessage + ".<br><br>You can try again or skip account setup for now and come back to it later.";
        }
        else
        {
            UsernameDisplay.text = PlayFabManager.FormatDisplayName(storedName);
            ShowAddEmailPopUp();
        }
    }

    private void OnSetNameComplete(bool isSuccess, string storedName, PlayFabError error)
    {
        _gameManager.PlayFabManager.OnSetNameComplete -= OnSetNameComplete;
        loadingScreen.SetActive(false);

        if (!isSuccess)
        {
            PopUpPanel.ErrorText.text = error.ErrorMessage;
        } else
        {
            UsernameDisplay.text = storedName;
            PopUpPanel.gameObject.SetActive(false);
        }
    }
    #endregion
    private void OnEmailSignInComplete()
    {

    }

    */
}
