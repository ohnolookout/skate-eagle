using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI.ProceduralImage;

public class OverlayButton : MonoBehaviour
{
    public TMP_Text ButtonText;
    public ProceduralImage BackgroundImage;
    public ProceduralImage BorderImage;
    public Button Button;
    public RectTransform Rect;
    public static Dictionary<OverlayButtonColor, Color> DarkColorDict = new()
    {
        { OverlayButtonColor.White, new(255/255f, 255 / 255f, 255 / 255f, 150 /255f) },
        { OverlayButtonColor.Green, new(81 / 255f, 128 / 255f, 87 / 255f, 150 / 255f) },
        { OverlayButtonColor.Orange, new(154 / 255f, 114 / 255f, 78 / 255f, 150 / 255f) }
    };

    public static Dictionary<OverlayButtonColor, Color> BrightColorDict = new()
    {
        { OverlayButtonColor.White, new(255 / 255f, 255 / 255f, 255 / 255f, 1) },
        { OverlayButtonColor.Green, new(106 / 255f, 168 / 255f, 114 / 255f, 1) },
        { OverlayButtonColor.Orange, new(210 / 255f, 156 / 255f, 106 / 255f, 1) }
    };

    public void ApplyDefinition(ButtonDefinition definition)
    {
        Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(definition.Action);
        ButtonText.text = definition.Text;

        //Set color
        Color darkColor = DarkColorDict[definition.Color];
        Color brightColor = BrightColorDict[definition.Color];
        BackgroundImage.color = darkColor;
        ButtonText.color = brightColor;
        BorderImage.color = brightColor;
        Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, definition.Width);
    }
}
public class ButtonDefinition
{
    public string Text;
    public int Width;
    public OverlayButtonColor Color;
    public UnityAction Action;
    public const int DefaultWidth = 325;
    public ButtonDefinition(string text, OverlayButtonColor color, UnityAction action, int width = DefaultWidth)
    {
        Text = text;
        Color = color;
        Action = action;
        Width = width;
    }

}
public enum OverlayButtonColor { White, Orange, Green };
