using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

/// <summary>
/// A class used to inform the user about the status of the ASAController
/// </summary>
public class ASAStatusController : MonoBehaviour
{
    [SerializeField]
    [Header("ASA Controller (optional)")]
    [Tooltip("The ASA Controller that is currently doing the work. If null, it will look for a component in the scene")]
    public ASAController asaController;

    [SerializeField]
    [Tooltip("The component will be destroyed if the deployed platform is non-uwp, if this option is true")]
    public bool IsUWPController;

    #region Output GameObjects

    public Color32 SuccessColor;
    public Color32 WarningColor;
    public Color32 NormalColor;

    [SerializeField]
    private GameObject UI_StatusParent;
    [SerializeField]
    private TextMeshProUGUI UI_TextMeshPro;

    private bool currentActiveState = false;
    private float lastHideRequest;

    [SerializeField]
    private GameObject MR_StatusParent;
    [SerializeField]
    private TextMeshPro MR_TextMeshPro;

    /// <summary>
    /// Sets the active state of the configured objects
    /// </summary>
    /// <param name="status"></param>
    private void SetParentStatus(bool status)
    {
        if (UI_StatusParent != null) UI_StatusParent.SetActive(status);
        if (MR_StatusParent != null) MR_StatusParent.SetActive(status);

        currentActiveState = status;
    }

    /// <summary>
    /// Sets the text of the textMEshes
    /// </summary>
    /// <param name="text"></param>
    private void SetText(string text, bool isOtherColor = false)
    {
        if (!isOtherColor) SetTextColor(NormalColor);
        //Show the status objects if we want to display text
        if (currentActiveState == false) SetParentStatus(true);

        if (UI_TextMeshPro != null) UI_TextMeshPro.text = text;
        if (MR_TextMeshPro != null) MR_TextMeshPro.text = text;
    }

    /// <summary>
    /// Sets the text of the textMEshes and their color.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="color"></param>
    private void SetText(string text, Color32 color)
    {
        SetText(text, true);
        SetTextColor(color);
    }


    private void SetTextColor(Color32 color)
    {
        if (UI_TextMeshPro != null)
            UI_TextMeshPro.color = color;
        if (MR_TextMeshPro != null)
            MR_TextMeshPro.color = color;
    }

    #endregion

    public void Start()
    {
        EnsureCorrectPlatform();

        InitializeReferences();
    }

    /// <summary>
    /// Destroys the gameobject if the current platform does not match the configuration
    /// </summary>
    private void EnsureCorrectPlatform()
    {
        if (IsUWPController)
        {
            this.gameObject.AddComponent<NonUWPObjectDestroyer>();
        }
        else
        {
            this.gameObject.AddComponent<UWPObjectDestroyer>();
        }
    }

    /// <summary>
    /// Initliazes all references and validates the configuration.
    /// </summary>
    private void InitializeReferences()
    {
        //Look for an asa controller if none is assigned.
        if (asaController == null)
        {
            asaController = FindObjectOfType<ASAController>();
        }

        //If it is still null, no ASAController exists in the scene
        if (asaController == null)
        {
            Debug.Log("No ASA Controller was found. This ASA Status Controller can therefore not do its work");
            return;
        }

        //Hook into the events
        asaController.AsaStatusEventHook += AsaStatusEventHookHandler;
    }

    /// <summary>
    /// Handles the event messages and processes the events arguments
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AsaStatusEventHookHandler(object sender, AsaStatusEventArgs e)
    {
        try
        {

            switch (e.Type)
            {
                case AsaStatusEventType.CreateAnchor_NeedMoreData:
                    SetText($"Move device to capture more data: {e.Status.Percentage}%");
                    break;
                case AsaStatusEventType.CreateAnchor_AttemptUpload:
                    SetText($"Trying to upload anchor...");
                    break;
                case AsaStatusEventType.CreateAnchor_Finished:
                    SetText("Azure anchor created! :)", SuccessColor);
                    StartCoroutine(HideAfterSeconds(3));
                    break;
                case AsaStatusEventType.FindAnchor_NeedMoreData:
                    SetText($"Move device to capture more data: {e.Status.Percentage}%");
                    break;
                case AsaStatusEventType.FindAnchor_Finished:
                    SetText("Azure anchor located! :)", SuccessColor);
                    StartCoroutine(HideAfterSeconds(3));
                    break;
                case AsaStatusEventType.Error:
                    SetText($"Error: {e.Status.Error}", WarningColor);
                    StartCoroutine(HideAfterSeconds(5));
                    break;
                case AsaStatusEventType.FindAnchor_Update:
                    SetText($"Looking for your Anchor...");
                    break;
                case AsaStatusEventType.FindAnchor_CouldNotLocate:
                    SetText($"Your anchor could not be found!", WarningColor);
                    StartCoroutine(HideAfterSeconds(5));
                    break;
                case AsaStatusEventType.FindAnchor_DoesNotExist:
                    SetText($"The anchor does not seem to exist...", WarningColor);
                    StartCoroutine(HideAfterSeconds(5));
                    break;
                case AsaStatusEventType.FindAnchor_AlreadyTracked:
                    SetText($"The anchor is already tracked.");
                    StartCoroutine(HideAfterSeconds(3));
                    break;
                default:
                    SetText($"There was a message - but we forgot what...", WarningColor);
                    StartCoroutine(HideAfterSeconds(5));
                    break;
            }
        }
        catch (Exception ex)
        {
            StringPublisher.PublishException(ex);
        }
    }

    private IEnumerator HideAfterSeconds(int seconds)
    {
        lastHideRequest = Time.realtimeSinceStartup;
        float myTimeStamp = lastHideRequest;

        yield return new WaitForSeconds(seconds);

        //Only execute the action if no other process was started between the init of this one.
        if (myTimeStamp == lastHideRequest)
            SetParentStatus(false);
    }
}
