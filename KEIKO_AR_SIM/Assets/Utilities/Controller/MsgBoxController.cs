using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class MsgBoxController : MonoBehaviour
{
    [HideInInspector]
    public bool isUiController;

    [HideInInspector]
    public TextMeshPro HeaderTextMesh;
    [HideInInspector]
    public TextMeshPro BodyTextMesh;
    [HideInInspector]
    public TextMeshProUGUI HeaderTextMeshUI;
    [HideInInspector]
    public TextMeshProUGUI BodyTextMeshUI;

    /// <summary>
    /// The right button
    /// </summary>
    [HideInInspector]
    public GameObject ButtonRight;
    [HideInInspector]
    public TextMeshPro ButtonRightTextMesh;
    [HideInInspector]
    public TextMeshProUGUI ButtonRightTextMeshUI;

    private Action ButtonRightClickAction;

    /// <summary>
    /// the left button
    /// </summary>
    [HideInInspector]
    public GameObject ButtonLeft;
    [HideInInspector]
    public TextMeshPro ButtonLeftTextMesh;
    [HideInInspector]
    public TextMeshProUGUI ButtonLeftTextMeshUI;

    private Action ButtonLeftClickAction;

    public void SetContent(MessageBoxContent content)
    {
        switch (content.type)
        {
            case MessageBoxContent.MsgBoxType.NoButtons:
                SetContent(content.Header, content.Body);
                break;
            case MessageBoxContent.MsgBoxType.OneButton:
                SetContent(content.Header, content.Body, content.ButtonRightAction, content.ButtonRightText);
                break;
            case MessageBoxContent.MsgBoxType.TwoButtons:
                SetContent(content.Header, content.Body, content.ButtonRightAction, content.ButtonRightText, content.ButtonLeftAction, content.ButtonLeftText);
                break;
            default:
                break;
        }
    }

    public void SetContent(string header, string body)
    {
        //Default to not showing the button
        if (ButtonRight != null)
        {
            ButtonRight.SetActive(false);
        }
        if (ButtonLeft != null)
        {
            ButtonLeft.SetActive(false);
        }

        if (HeaderTextMesh != null)
            HeaderTextMesh.text = header;
        if (BodyTextMesh != null)
            BodyTextMesh.text = body;
        if (HeaderTextMeshUI != null)
            HeaderTextMeshUI.text = header;
        if (BodyTextMeshUI != null)
            BodyTextMeshUI.text = body;
    }

    public void SetContent(string header, string body, Action buttonRightClickAction, string buttonRightText)
    {
        SetContent(header, body);

        //Set button data
        ButtonRight.SetActive(true);
        if (ButtonRightTextMesh != null)
            ButtonRightTextMesh.text = buttonRightText;
        if (ButtonRightTextMeshUI != null)
            ButtonRightTextMeshUI.text = buttonRightText;
        ButtonRightClickAction = buttonRightClickAction;
    }

    public void SetContent(string header, string body, Action buttonRightClickAction, string buttonRightText, Action buttonLeftClickAction, string buttonLeftText)
    {
        SetContent(header, body, buttonRightClickAction, buttonRightText);

        //Set button left data
        ButtonLeft.SetActive(true);
        if (ButtonLeftTextMesh != null)
            ButtonLeftTextMesh.text = buttonLeftText;
        if (ButtonLeftTextMeshUI != null)
            ButtonLeftTextMeshUI.text = buttonLeftText;
        ButtonLeftClickAction = buttonLeftClickAction;
    }

    public void ButtonRightClick()
    {
        ButtonRightClickAction?.Invoke();
    }
    public void ButtonLeftClick()
    {
        ButtonLeftClickAction?.Invoke();
    }


}
