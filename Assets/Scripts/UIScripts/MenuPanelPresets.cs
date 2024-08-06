using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public static class MenuPanelPresets
{
    #region Set Name
    public static MenuPanelPreset SetNameFirstTimePanel(UnityAction OnPrimaryYesButton, UnityAction OnPrimaryNeutralButton, UnityAction OnPrimaryTextBlockButton, 
        UnityAction OnSecondarySkipYesButton, UnityAction OnSecondarySkipNeutralButton, UnityAction OnSecondaryErrorYesButton, UnityAction OnSecondaryErrorNeutralButton)
    {
        MenuPanelPreset preset = new();

        //Title and body
        preset.TitleText = "New Here?";
        preset.BodyText = "You need a name. Don't stress, you can change it anytime.";

        //Buttons        
        ButtonDefinition yesButton = new("Enter", OverlayButtonColor.Green, OnPrimaryYesButton);
        ButtonDefinition neutralButton = new("Skip It", OverlayButtonColor.White, OnPrimaryNeutralButton);
        preset.HorizontalButtonDefs = new() { yesButton, neutralButton };

        //Text block and toggle
        preset.TextBlockButtonText = "Already have an account? <br>Tap here to log in.";
        preset.OnTextButtonClicked = OnPrimaryTextBlockButton;

        //Text inputs
        preset.InputPlaceholders = new() { "Keep it clean..." };

        //Secondary panels
        preset.SecondaryPanelPresets = new(){ SetNameSecondarySkipPanel(OnSecondarySkipYesButton, OnSecondarySkipNeutralButton), SetNameSecondaryErrorPanel(OnSecondaryErrorYesButton, OnSecondaryErrorNeutralButton) };

        return preset;
    }
    public static MenuPanelPreset SetNameSecondarySkipPanel(UnityAction OnYesButton, UnityAction OnNeutralButton)
    {
        MenuPanelPreset preset = new();

        //Title and body
        preset.TitleText = "That's Cool";
        preset.BodyText = "We'll just call you Skate Eagle. You can always change it later.";

        //Buttons        
        ButtonDefinition yesButton = new("OK", OverlayButtonColor.Green, OnYesButton);
        ButtonDefinition neutralButton = new("Back", OverlayButtonColor.White, OnNeutralButton);
        preset.HorizontalButtonDefs = new() { yesButton, neutralButton };

        return preset;
    }

    public static MenuPanelPreset SetNameSecondaryErrorPanel(UnityAction OnYesButton, UnityAction OnNeutralButton)
    {
        MenuPanelPreset preset = new();

        //Title and body
        preset.TitleText = "Oops";
        preset.BodyText = "Something went wrong. You can try again or skip account setup for now and come back to it later.";

        //Buttons        
        ButtonDefinition yesButton = new("Try Again", OverlayButtonColor.Green, OnYesButton);
        ButtonDefinition neutralButton = new("Skip It", OverlayButtonColor.White, OnNeutralButton);
        preset.HorizontalButtonDefs = new() { yesButton, neutralButton };

        return preset;
    }

    public static MenuPanelPreset SetNamePanel(UnityAction OnPrimaryYesButton, UnityAction OnPrimaryNeutralButton, UnityAction OnSecondaryErrorYesButton, UnityAction OnSecondaryErrorNeutralButton)
    {
        MenuPanelPreset preset = new();

        //Title and body
        preset.TitleText = "Set Name";
        preset.BodyText = "New name:";

        //Buttons        
        ButtonDefinition yesButton = new("Enter", OverlayButtonColor.Green, OnPrimaryYesButton);
        ButtonDefinition neutralButton = new("Back", OverlayButtonColor.White, OnPrimaryNeutralButton);
        preset.HorizontalButtonDefs = new() { yesButton, neutralButton };

        //Text inputs
        preset.InputPlaceholders = new() { "Keep it clean..." };

        //Secondary panels
        preset.SecondaryPanelPresets = new() { SetNameSecondaryErrorPanel(OnSecondaryErrorYesButton, OnSecondaryErrorNeutralButton) };

        return preset;
    }
    #endregion

    #region Add Email
    public static MenuPanelPreset AddEmailPanel(UnityAction OnYesButton, UnityAction OnNeutralButton, UnityAction<bool> OnToggleChanged, UnityAction OnSecondaryConfirmNeutralButton)
    {
        MenuPanelPreset preset = new();

        //Title and body
        preset.TitleText = "Add Email?";
        preset.BodyText = "Adding an email secures your account and lets you sync across devices. It's totally optional.";

        //Buttons        
        ButtonDefinition yesButton = new("Submit", OverlayButtonColor.Green, OnYesButton);
        ButtonDefinition neutralButton = new("Skip", OverlayButtonColor.White, OnNeutralButton);
        preset.HorizontalButtonDefs = new() { yesButton, neutralButton };

        //Text inputs
        preset.InputPlaceholders = new() { "Email", "Password", "Confirm Password" };

        //Toggle
        preset.ToggleText = "Don't ask again";
        preset.OnToggleChanged = OnToggleChanged;

        //Secondary confrim panels
        var errorPanel = OneButtonSecondaryPanel(OnSecondaryConfirmNeutralButton);
        errorPanel.TitleText = "Nice";
        errorPanel.TitleText = "Email added successfully. Now your account is secure and you can access it from other devices.";

        preset.SecondaryPanelPresets = new() { errorPanel };

        return preset;
    }
    #endregion

    #region Email Login
    public static MenuPanelPreset EmailLoginPanel(UnityAction OnYesButton, UnityAction OnNeutralButton, UnityAction OnTextButtonClicked,
        UnityAction OnSecondaryConfirmYesButton, UnityAction OnSecondaryConfirmNoButton, UnityAction OnSecondaryErrorNeutralButton, UnityAction OnSecondarySuccessNeutralButton)
    {
        MenuPanelPreset preset = new();

        //Title and body
        preset.TitleText = "Log In to Account";
        preset.BodyText = "If you already have an account, download your save data.";

        //Buttons        
        ButtonDefinition yesButton = new("Log In", OverlayButtonColor.Green, OnYesButton);
        ButtonDefinition noButton = new("Nevermind", OverlayButtonColor.White, OnNeutralButton);
        preset.HorizontalButtonDefs = new() { yesButton, noButton };

        //Text inputs
        preset.InputPlaceholders = new() { "Email", "Password"};

        //Text block button
        preset.TextBlockButtonText = "Or create a new account.";
        preset.OnTextButtonClicked = OnTextButtonClicked;

        //Secondary panels
        var errorPanel = OneButtonSecondaryPanel(OnSecondaryErrorNeutralButton);
        errorPanel.TitleText = "Oops";
        errorPanel.BodyText = "Something went wrong.";

        var successPanel = OneButtonSecondaryPanel(OnSecondarySuccessNeutralButton);
        errorPanel.TitleText = "Nice";
        errorPanel.BodyText = "Successfully logged in to your account.";

        preset.SecondaryPanelPresets = new(){ EmailLoginSecondaryConfirmPanel(OnSecondaryConfirmYesButton, OnSecondaryConfirmNoButton), errorPanel, successPanel };

        return preset;
    }

    public static MenuPanelPreset SwitchEmailLoginPanel(UnityAction OnYesButton, UnityAction OnNeutralButton,
        UnityAction OnSecondaryErrorNeutralButton, UnityAction OnSecondarySuccessNeutralButton)
    {
        MenuPanelPreset preset = new();

        //Title and body
        preset.TitleText = "Switch Account";
        preset.BodyText = "This will change your local data to the new account. You can always switch back to your current account.";

        //Buttons        
        ButtonDefinition yesButton = new("Switch", OverlayButtonColor.Green, OnYesButton);
        ButtonDefinition noButton = new("Nevermind", OverlayButtonColor.White, OnNeutralButton);
        preset.HorizontalButtonDefs = new() { yesButton, noButton };

        //Text inputs
        preset.InputPlaceholders = new() { "Email", "Password"};

        //Secondary panels
        var errorPanel = OneButtonSecondaryPanel(OnSecondaryErrorNeutralButton);
        errorPanel.TitleText = "Oops";
        errorPanel.BodyText = "Something went wrong.";

        var successPanel = OneButtonSecondaryPanel(OnSecondarySuccessNeutralButton);
        errorPanel.TitleText = "Nice";
        errorPanel.BodyText = "Successfully logged in.";

        preset.SecondaryPanelPresets = new(){ errorPanel, successPanel };

        return preset;
    }

    public static MenuPanelPreset EmailLoginSecondaryConfirmPanel(UnityAction OnYesButton, UnityAction OnNoButton)
    {
        MenuPanelPreset preset = new();

        //Title and body
        preset.TitleText = "Confirm Login";
        preset.BodyText = "Warning: This will replace your locally stored game. You won't be able to recover your local save.<br><br>Replace local save with cloud save?";

        //Buttons        
        ButtonDefinition yesButton = new("Yes", OverlayButtonColor.Green, OnYesButton);
        ButtonDefinition noButton = new("No", OverlayButtonColor.Orange, OnNoButton);
        preset.HorizontalButtonDefs = new() { yesButton, noButton };

        return preset;
    }
    #endregion

    #region Main Menu
    public static MenuPanelPreset PlayerSettingsPanel(UnityAction OnChangeNameButton, UnityAction OnSignInButton, UnityAction OnRegisterEmailButton, 
        UnityAction OnSwitchAccountButton, UnityAction OnCloseButton, UnityAction OnLoadPanel)
    {
        MenuPanelPreset preset = new();

        //Title and body
        preset.TitleText = "Settings";
        preset.BodyText = "Player";

        //Buttons        
        ButtonDefinition changeName = new("Change Name", OverlayButtonColor.White, OnChangeNameButton, 450);
        ButtonDefinition signIn = new("Sign In", OverlayButtonColor.White, OnSignInButton, 450);
        ButtonDefinition register = new("Register Email", OverlayButtonColor.White, OnRegisterEmailButton, 450);
        ButtonDefinition switchAccount = new("Switch Account", OverlayButtonColor.White, OnSwitchAccountButton, 450);
        preset.VerticalButtonDefs = new() { changeName, signIn, register, switchAccount };

        preset.CloseButtonDef = new("Close", OverlayButtonColor.Orange, OnCloseButton);

        //On load action
        preset.OnLoadPanel = OnLoadPanel;

        return preset;
    }
    public static MenuPanelPreset ConfirmNewGamePanel(UnityAction OnYesButton, UnityAction OnNoButton)
    {
        MenuPanelPreset preset = TwoButtonSecondaryPanel(OnYesButton, OnNoButton);
        //Title and body
        preset.TitleText = "You Sure?";
        preset.BodyText = "Starting a new game will delete all your records. There's no going back.";

        //Buttons
        preset.HorizontalButtonDefs[0].Text = "Do It";
        preset.HorizontalButtonDefs[1].Text = "Don't It";

        return preset;
    }

    #endregion

    #region Generic
    public static MenuPanelPreset OneButtonSecondaryPanel(UnityAction OnNeutralButton)
    {
        MenuPanelPreset preset = new();

        //Title and body
        preset.TitleText = "Title";
        preset.BodyText = "Body";

        //Buttons
        ButtonDefinition noButton = new("OK", OverlayButtonColor.White, OnNeutralButton);
        preset.HorizontalButtonDefs = new() { noButton };

        return preset;
    }
    public static MenuPanelPreset TwoButtonSecondaryPanel(UnityAction OnYesButton, UnityAction OnNoButton)
    {
        MenuPanelPreset preset = new();

        //Title and body
        preset.TitleText = "Title";
        preset.BodyText = "Body";

        //Buttons
        ButtonDefinition yesButton = new("Yes", OverlayButtonColor.Green, OnYesButton);
        ButtonDefinition noButton = new("No", OverlayButtonColor.Orange, OnNoButton);

        preset.HorizontalButtonDefs = new() { yesButton, noButton };

        return preset;
    }
    #endregion
}

public class MenuPanelPreset
{

    //Title and body
    public string TitleText = "";
    public string BodyText = "";

    //Buttons
    public List<ButtonDefinition> VerticalButtonDefs = new();
    public List<ButtonDefinition> HorizontalButtonDefs = new();
    public ButtonDefinition CloseButtonDef = null;

    //Text inputs
    public List<string> InputPlaceholders = new();

    //Text block
    public string TextBlockButtonText = "";
    public UnityAction OnTextButtonClicked = null;

    //Toggle
    public string ToggleText = "";
    public UnityAction<bool> OnToggleChanged = null;

    //Secondary panels
    public List<MenuPanelPreset> SecondaryPanelPresets = new();

    //Action to execute on load
    public UnityAction OnLoadPanel = null;

}
