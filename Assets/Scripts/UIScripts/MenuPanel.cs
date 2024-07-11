using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using TMPro;

public class MenuPanel : MonoBehaviour
{
    public GameObject MainPanel;
    public GameObject ConfirmationPanel;
    public Image MainGrayOut;
    public Button MainGrayOutButton;
    public Button MainYesButton;
    public Button MainNoButton;
    public Button ConfirmYesButton;
    public Button ConfirmNoButton;
    public TMP_Text ConfirmationText;
    public TMP_Text MainErrorText;
    public TMP_InputField[] InputFields;
    public UnityAction OnMainYesButtonClicked;
    public UnityAction OnMainNoButtonClicked;
    public UnityAction OnConfirmYesButtonClicked;
    public UnityAction OnConfirmNoButtonClicked;


    void Start()
    {
        MainGrayOutButton.onClick.AddListener(OnMainGrayOutButtonClicked);
        MainYesButton.onClick.AddListener(OnMainYesButtonClicked);
        MainNoButton.onClick.AddListener(OnMainNoButtonClicked);

        if (ConfirmationPanel != null)
        {
            ConfirmYesButton.onClick.AddListener(OnConfirmYesButtonClicked);
            ConfirmNoButton.onClick.AddListener(OnConfirmNoButtonClicked);
        }
    }

    private void OnMainGrayOutButtonClicked()
    {
        gameObject.SetActive(false);
    }

    public void ShowConfirmation()
    {
        if (ConfirmationPanel == null)
        {
            return;
        }

        ConfirmationPanel.SetActive(true);
        MainGrayOut.enabled = false;

    }

    public void CloseConfirmation()
    {
        if (ConfirmationPanel == null)
        {
            return;
        }

        ConfirmationPanel.SetActive(false);
        MainGrayOut.enabled = true;
    }

}
