using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using TMPro;

public class MenuPanel : MonoBehaviour
{
    public MenuPanelPreset CurrentPreset;
    public List<MenuPanelPreset> SecondaryPanelPresets;
    public bool HasBack;

    public TMP_Text TitleText;
    public TMP_Text BodyText;

    public GameObject InputFieldsBlock;
    public TMP_InputField[] InputFields;
    public TMP_Text[] InputFieldsPlaceholderText;

    public GameObject ToggleBlock;
    public Toggle Toggle;
    public TMP_Text ToggleText;

    public GameObject VerticalButtonBlock;
    public GameObject HorizontalButtonBlock;
    public OverlayButton[] VerticalButtons;
    public OverlayButton[] HorizontalButtons;
    public GameObject CloseButtonBlock;
    public OverlayButton CloseButton;

    public Button ClickableTextButton;
    public TMP_Text ClickableText;

    public TMP_Text ErrorText;

    private List<MenuPanelPreset> _presetHistory = new(); //Add most recent panel to history. Reset history when panel is loaded that can't go back.
    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }

    public void HidePanel()
    {

        //Go back if panel has previous presets to go back to
        if(HasBack && _presetHistory.Count > 0)
        {
            var lastPreset = _presetHistory[^1];
            _presetHistory.RemoveAt(_presetHistory.Count - 1);
            LoadPanelPreset(lastPreset, _presetHistory.Count > 0, true);
            return;
        }
        //Otherwise turn off gameobject and reset alterable fields.
        Close();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        ErrorText.gameObject.SetActive(false);

        foreach (var input in InputFields)
        {
            input.text = "";
            input.contentType = TMP_InputField.ContentType.Standard;
        }

        CloseButton.Button.onClick.RemoveAllListeners();
        Toggle.onValueChanged.RemoveAllListeners();

        Toggle.isOn = false;

    }
    private void ResetFields()
    {

    }
    public void ShowSecondaryPanel(int i)
    {
        _presetHistory.Add(CurrentPreset);
        LoadPanelPreset(SecondaryPanelPresets[i], true, true);
    }

    public void ShowSecondaryPanel(UnityAction<MenuPanel, bool> showPanelAction)
    {
        _presetHistory.Add(CurrentPreset);
        showPanelAction(this, true);
    }

    public void ShowSecondaryPanel(UnityAction<MenuPanel> showPanelAction)
    {
        _presetHistory.Add(CurrentPreset);
        showPanelAction(this);
    }

    #region Loading Presets
    public void LoadPanelPreset(MenuPanelPreset preset, bool hasBack, bool activatePanel)
    {
        CurrentPreset = preset;
        HasBack = hasBack;
        if (!HasBack)
        {
            _presetHistory = new();
        }

        //Set title
        ApplyTextPreset(TitleText, preset.TitleText);

        //Set body
        ApplyTextPreset(BodyText, preset.BodyText);

        //Set buttons
        //Vertical buttons
        if (preset.VerticalButtonDefs.Count > 0)
        {
            ApplyDefinitionsToButtons(VerticalButtons, preset.VerticalButtonDefs);
            VerticalButtonBlock.SetActive(true);
        }
        else
        {
            VerticalButtonBlock.SetActive(false);
        }

        //Horizontal buttons
        if (preset.HorizontalButtonDefs.Count > 0)
        {
            ApplyDefinitionsToButtons(HorizontalButtons, preset.HorizontalButtonDefs);
            HorizontalButtonBlock.SetActive(true);
        }
        else
        {
            HorizontalButtonBlock.SetActive(false);
        }

        //Close button
        if (preset.CloseButtonDef != null)
        {
            CloseButtonBlock.SetActive(true);
            CloseButton.ApplyDefinition(preset.CloseButtonDef);
        }
        else
        {
            CloseButtonBlock.SetActive(false);
        }

        //Set input fields
        if (preset.InputPlaceholders.Count > 0)
        {
            InputFieldsBlock.SetActive(true);
            ApplyInputFieldSettings(InputFields, InputFieldsPlaceholderText, preset.InputPlaceholders);
        }
        else
        {
            InputFieldsBlock.SetActive(false);
        }

        //Set toggle
        if (!String.IsNullOrEmpty(preset.ToggleText))
        {
            ToggleText.text = preset.ToggleText;
            Toggle.isOn = false;
            Toggle.onValueChanged.RemoveAllListeners();
            Toggle.onValueChanged.AddListener(preset.OnToggleChanged);
            ToggleBlock.SetActive(true);
        }
        else
        {
            ToggleBlock.SetActive(false);
        }

        //Set text button block
        if (!String.IsNullOrEmpty(preset.TextBlockButtonText))
        {
            ClickableText.text = preset.TextBlockButtonText;
            ClickableTextButton.onClick.RemoveAllListeners();
            ClickableTextButton.onClick.AddListener(preset.OnTextButtonClicked);
            ClickableTextButton.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            ClickableTextButton.transform.parent.gameObject.SetActive(false);
        }

        //Error text
        ErrorText.gameObject.SetActive(false);

        if (preset.OnLoadPanel != null)
        {
            preset.OnLoadPanel();
        }

        //Secondary panel presets
        SecondaryPanelPresets = preset.SecondaryPanelPresets;

        //Activate or deactivate panel depending on method call
        gameObject.SetActive(activatePanel);
    }

    private static void ApplyDefinitionsToButtons(OverlayButton[] buttons, List<ButtonDefinition> definitions)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < definitions.Count)
            {
                buttons[i].ApplyDefinition(definitions[i]);
                buttons[i].gameObject.SetActive(true);
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }
    }

    private static void ApplyInputFieldSettings(TMP_InputField[] inputFields, TMP_Text[] inputPlaceholderText, List<string> inputPlaceholders)
    {        
        for (int i = 0; i < inputFields.Length; i++)
        {
            if (i < inputPlaceholders.Count)
            {
                inputFields[i].text = "";
                inputPlaceholderText[i].text = inputPlaceholders[i];
                inputFields[i].gameObject.SetActive(true);
            }
            else
            {
                inputFields[i].gameObject.SetActive(false);
            }
        }
    }

    private static void ApplyTextPreset(TMP_Text text, string preset)
    {
        if(preset == "")
        {
            text.gameObject.SetActive(false);
        }
        else
        {
            text.text = preset;
            text.gameObject.SetActive(true);
        }
    }
    #endregion

}

