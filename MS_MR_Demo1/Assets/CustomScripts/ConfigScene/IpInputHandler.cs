using RosSharp.RosBridgeClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// Handles the IP configuration in the ConfigScene.
/// Can be used by either a UI GameObject or by 3D (MRTK) GameObjects
/// </summary>
public class IpInputHandler : MonoBehaviour
{

    private string prefill;
    [SerializeField]
    private bool acceptsAureConfig = false;

    public void Init(Action<string, string, string, string> confirmClick, string[] inputPrefills)
    {
        Input.text = inputPrefills[0];
        prefill = inputPrefills[0];
        _confirmClick = confirmClick;

        if (acceptsAureConfig)
        {
            AzureIdInput.text = inputPrefills[1];
            AzureKeyInput.text = inputPrefills[2];
            AzureDomain = inputPrefills[3];
            AzureAnchorId.text = inputPrefills[4];

            var t = AzureDomainInput.options.FirstOrDefault(x => x.text == AzureDomain);
            if (t != null)
                AzureDomainInput.value = AzureDomainInput.options.IndexOf(t);
        }
    }


    public TMP_InputField Input;
    public TMP_InputField AzureKeyInput;
    public TMP_InputField AzureIdInput;
    public TMP_Dropdown AzureDomainInput;
    public TMP_InputField AzureAnchorId;
    private string AzureDomain;

    public void OnEditClicked()
    {
        OpenSystemKeyboard();
    }

    public void DomainSelectionChanged(int index)
    {
        AzureDomain = AzureDomainInput.options[AzureDomainInput.value].text;
    }

    public void OpenSystemKeyboard()
    {
        TouchScreenKeyboard.Open(prefill, TouchScreenKeyboardType.URL, false, false, false, false);
        Input.ActivateInputField();
    }

    private Action<string, string, string, string> _confirmClick;
    public void ConfirmButtonClicked()
    {

        if (acceptsAureConfig)
        {
            if (AzureAnchorId != null && !String.IsNullOrEmpty(AzureAnchorId.text))
            {
                PlayerPrefs.SetString("current_asa_anchor_id", AzureAnchorId.text);
            }
            _confirmClick?.Invoke(Input.text, AzureIdInput.text, AzureKeyInput.text, AzureDomain);
        }
        else
        {
            _confirmClick?.Invoke(Input.text, "", "", "");
        }
    }
}
