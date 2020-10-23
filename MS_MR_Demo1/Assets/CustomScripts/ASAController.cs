using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// The controller used to interact with the ASA library
/// </summary>
public class ASAController : MonoBehaviour
{
    SpatialAnchorManager CloudManager;
    AzureConfig Config;
    AnchorLocateCriteria anchorLocateCriteria;
    AsaStatus status;

    /// <summary>
    /// Logs the string to the ROS# String Publisher.
    /// If the publisher is not yet publishing, messages are stacked and published later.
    /// </summary>
    /// <param name="msg">Message to log</param>
    /// <param name="showUser">If true, the message will be logged and shown to the user</param>
    private void Log(string msg, bool showUser = false)
    {
        StringPublisher.PublishDebug(msg);
        if (showUser)
        {
            Debug.Log(msg);
        }
    }

    public void Start()
    {
        Log("Start", true);

        Config = GetConfig();
        if (!IsAccessConfigFullyFilled(Config))
        {
            Log("Access configuration is not fully configured", true);
        }
        ConfigureReferences();

        InitializeSession();

        Log("Start end", true);
    }

    public void Update()
    {
        lock (actionsToExecuteOnMainThread)
        {
            if (actionsToExecuteOnMainThread.Count > 0)
            {
                for (int i = actionsToExecuteOnMainThread.Count - 1; i >= 0; i--)
                {
                    actionsToExecuteOnMainThread[i]();
                }
            }
            actionsToExecuteOnMainThread.Clear();
        }

        //Other than dispatching items to the UI thread, Update only checks for materials and input interactions
    }

    public void OnDestroy()
    {
        Log($"{nameof(OnDestroy)} Stopping ASA Controller");
        CloudManager.StopSession();
    }

    List<Action> actionsToExecuteOnMainThread = new List<Action>();

    /// <summary>
    /// Queues an action which will be executed on the main thread using the Update method in the same order as they are inserted here.
    /// First come first serve
    /// </summary>
    /// <param name="action"></param>
    private void ExecuteOnMainThread(Action action)
    {
        actionsToExecuteOnMainThread.Insert(0, action);
    }

    #region Config
    /// <summary>
    /// Sets up all required references to create and find anchors
    /// </summary>
    private void ConfigureReferences()
    {
        Log(nameof(ConfigureReferences));
        CloudManager = GetComponent<SpatialAnchorManager>();
        CloudManager.SessionUpdated += CloudManager_SessionUpdated;
        CloudManager.AnchorLocated += CloudManager_AnchorLocated;
        CloudManager.LocateAnchorsCompleted += CloudManager_LocateAnchorsCompleted;
        CloudManager.LogDebug += CloudManager_LogDebug;
        CloudManager.Error += CloudManager_Error;

        CloudManager.SpatialAnchorsAccountKey = Config.AccountKey;
        CloudManager.SpatialAnchorsAccountId = Config.AccountId;
        CloudManager.SpatialAnchorsAccountDomain = Config.AccountDomain;
        Log($"ASA Id {Config.AccountId}", true);
        Log($"ASA Key {Config.AccountKey}", true);
        Log($"ASA Domain {Config.AccountDomain}", true);

        anchorLocateCriteria = new AnchorLocateCriteria();
        status = new AsaStatus();

        //info: DemoScript also checks a reference for an input manager here.
    }

    /// <summary>
    /// Loads a config object from the player prefs
    /// </summary>
    /// <returns></returns>
    private AzureConfig GetConfig()
    {
        AzureConfig config = new AzureConfig();
        config.ReadFromPlayerPrefs();
        return config;
    }

    /// <summary>
    /// Performs a quick sanity check
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    private bool IsAccessConfigFullyFilled(AzureConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.AccountId)
            || string.IsNullOrWhiteSpace(config.AccountKey)
            || string.IsNullOrWhiteSpace(config.AccountDomain))
        {
            return false;
        }

        return true;
    }

    #endregion

    #region Event Handler

    private void CloudManager_Error(object sender, Microsoft.Azure.SpatialAnchors.SessionErrorEventArgs args)
    {
        Log(nameof(CloudManager_Error) + args.ErrorCode + ": " + args.ErrorMessage, true);
    }

    private void CloudManager_LogDebug(object sender, Microsoft.Azure.SpatialAnchors.OnLogDebugEventArgs args)
    {
        Log(nameof(CloudManager_LogDebug) + " " + args.Message);
    }

    private void CloudManager_LocateAnchorsCompleted(object sender, Microsoft.Azure.SpatialAnchors.LocateAnchorsCompletedEventArgs args)
    {
        Log(nameof(CloudManager_LocateAnchorsCompleted));

        //Forward the event to the FinAnchor section
        ReportAllAnchorLocationResultsReceived(args);
    }

    private void CloudManager_AnchorLocated(object sender, Microsoft.Azure.SpatialAnchors.AnchorLocatedEventArgs args)
    {
        Log(nameof(CloudManager_AnchorLocated));

        //Forward the event to the FinAnchor section
        ReportAnchorLocationResultReceived(args);
    }

    private void CloudManager_SessionUpdated(object sender, Microsoft.Azure.SpatialAnchors.SessionUpdatedEventArgs args)
    {
        Log(nameof(CloudManager_SessionUpdated) + $"{args.Status.UserFeedback}: {args.Status.ReadyForCreateProgress}/{args.Status.RecommendedForCreateProgress}");

        //ReadyForCreateProgress should be above 1 to create Anchors
        //RecommendedForCreateProgress can/should be above 1 as well

        if (args.Status.ReadyForCreateProgress > 1 && args.Status.RecommendedForCreateProgress > 1 && status.CanCreateAnchor == false)
        {
            status.CanCreateAnchor = true;
            Log($"{nameof(CloudManager_SessionUpdated)}: Can create anchor!", true);
        }
    }
    #endregion

    #region Session Management

    private async Task InitializeSession()
    {
        Log($"{nameof(InitializeSession)}", true);
        await CloudManager.StartSessionAsync();
    }

    #endregion

    #region Creating Anchors

    private GameObject localAnchor;

    /// <summary>
    /// Creates a spatial anchor at the given transform's pose
    /// </summary>
    /// <param name="poseIndicator">An object used to position the wanted anchor location</param>
    private async Task<string> CreateSpatialAnchor(GameObject poseIndicator)
    {
        Log($"{nameof(CreateSpatialAnchor)}", true);

        //Create the temporary object there.
        localAnchor = this.gameObject;

        //update the localAnchors position to be equal to the pose indicator
        //This only works if the poseIndicator is a child of the localAnchor object, as it is in our case.
        localAnchor.transform.position = poseIndicator.transform.position;
        localAnchor.transform.rotation = poseIndicator.transform.rotation;
        poseIndicator.transform.localPosition = new Vector3(0, 0, 0);
        poseIndicator.transform.localRotation = new Quaternion(0, 0, 0, 1);

        localAnchor.CreateNativeAnchor();

        CloudSpatialAnchor cloudAnchor = new CloudSpatialAnchor();
        cloudAnchor.LocalAnchor = localAnchor.FindNativeAnchor().GetPointer();

        // Wait until enough data is accumulated
        while (!CloudManager.IsReadyForCreate)
        {
            await Task.Delay(666);
            float createProgress = CloudManager.SessionStatus.RecommendedForCreateProgress;
            SendUpdateOnMainThread(AsaStatusEventType.CreateAnchor_NeedMoreData, status.SetPercentage(createProgress * 100));
        }

        Log($"Attempting to create anchor", true);
        SendUpdateOnMainThread(AsaStatusEventType.CreateAnchor_AttemptUpload, status);
        try
        {
            await this.CloudManager.CreateAnchorAsync(cloudAnchor);
            SendUpdateOnMainThread(AsaStatusEventType.CreateAnchor_Finished, status);
        }
        catch (Exception ex)
        {
            Log($"Error while creating the anchor: {ex.Message}\nExiting.", true);
            SendUpdateOnMainThread(AsaStatusEventType.Error, status.SetError(ex.Message));
            return null;
        }
        Log($"Created a cloud spatial anchor with ID = {cloudAnchor.Identifier}", true);

        return cloudAnchor.Identifier;
    }

    #endregion

    #region Finding Anchors

    /// <summary>
    /// Locates the specified anchorId
    /// This method was written following the documentation from
    /// https://docs.microsoft.com/en-us/azure/spatial-anchors/how-tos/create-locate-anchors-unity#locate-a-cloud-spatial-anchor
    /// </summary>
    /// <param name="anchorId"></param>
    private void FindSpatialAnchor(string anchorId)
    {
        //A quick validation check to see if the achor ID is valid.
        if (!ValidateAnchorId(anchorId))
        {
            Log($"AnchorId {anchorId} is not valid!", true);
            return;
        }

        anchorLocateCriteria.Identifiers = new string[] { anchorId };

        SendUpdateOnMainThread(AsaStatusEventType.FindAnchor_Update, status);

        //The watcher will automatically invoke the AnchorLocated and LocateAnchorsCompleted events
        CloudManager.Session.CreateWatcher(anchorLocateCriteria);
    }

    /// <summary>
    /// Validates the anchor id by format only.
    /// Does not perform external requests.
    /// </summary>
    /// <param name="anchorId"></param>
    /// <returns></returns>
    private bool ValidateAnchorId(string anchorId)
    {
        return !String.IsNullOrEmpty(anchorId);
    }

    /// <summary>
    /// The watcher created in <see cref="FindSpatialAnchor(string)"/> invokes this event for every anchor requested
    /// This event fires if an anchor is located or also if it cannot be located.
    /// </summary>
    /// <param name="args"></param>
    private void ReportAnchorLocationResultReceived(Microsoft.Azure.SpatialAnchors.AnchorLocatedEventArgs args)
    {
        if (args.Watcher != null)
        {
            args.Watcher.Stop();
        }
        switch (args.Status)
        {
            case LocateAnchorStatus.AlreadyTracked:
                Log($"The requested anchor {args.Identifier} was already tracked.", true);
                SendUpdateOnMainThread(AsaStatusEventType.FindAnchor_AlreadyTracked, status);
                break;
            case LocateAnchorStatus.Located:
                CloudSpatialAnchor foundAnchor = args.Anchor;

                Log($"The requested anchor {args.Identifier} was found!", true);

                if (foundAnchor == null)
                {
                    Log($"Found the requested anchor but the returned anchor is null!", true);
                    return;
                }

                SendUpdateOnMainThread(AsaStatusEventType.FindAnchor_Finished, status);

                ExecuteOnMainThread(() =>
                {
                    Log("Processing the found anchor's position", true);
                    // Notify AnchorFeedbackScript
                    Pose anchorPose = Pose.identity;

#if UNITY_ANDROID || UNITY_IOS
                    anchorPose = foundAnchor.GetPose();
#endif

#if WINDOWS_UWP || UNITY_WSA
                    // HoloLens: The position will be set based on the unityARUserAnchor that was located.

                    // Create a local anchor at the location of the object in question
                    gameObject.CreateNativeAnchor();

                    // On HoloLens, if we do not have a cloudAnchor already, we will have already positioned the
                    // object based on the passed in worldPos/worldRot and attached a new world anchor,
                    // so we are ready to commit the anchor to the cloud if requested.
                    // If we do have a cloudAnchor, we will use it's pointer to setup the world anchor,
                    // which will position the object automatically.
                    if (foundAnchor != null)
                    {
                        Log("Local anchor position successfully set to Azure anchor position", true);

                        var worldAnchor = gameObject.GetComponent<UnityEngine.XR.WSA.WorldAnchor>();
                        if(worldAnchor != null)
                        {
                            worldAnchor.SetNativeSpatialAnchorPtr(foundAnchor.LocalAnchor);
                        }
                        else
                        {
                            Log("WorldAnchorComponent was null", true);
                        }
                    }
#else
                    Log($"Setting object to anchor pose with position '{anchorPose.position}' and rotation '{anchorPose.rotation}'", true);
                    transform.position = anchorPose.position;
                    transform.rotation = anchorPose.rotation;

                    // Create a native anchor at the location of the object in question
                    //gameObject.CreateNativeAnchor();
#endif
                });

                break;
            case LocateAnchorStatus.NotLocatedAnchorDoesNotExist:
                // The anchor was deleted or never existed in the first place
                // Drop it, or show UI to ask user to anchor the content anew
                Log($"The requested anchor {args.Identifier} does not exist.", true);
                SendUpdateOnMainThread(AsaStatusEventType.FindAnchor_DoesNotExist, status);
                break;
            case LocateAnchorStatus.NotLocated:
                // The anchor hasn't been found given the location data
                // The user might in the wrong location, or maybe more data will help
                // Show UI to tell user to keep looking around
                Log($"The requested anchor {args.Identifier} could not be located.", true);
                SendUpdateOnMainThread(AsaStatusEventType.FindAnchor_CouldNotLocate, status);
                break;
        }
    }

    /// <summary>
    /// The watcher created in <see cref="FindSpatialAnchor(string)"/> invokes this event once after all requested achors are processed
    /// </summary>
    /// <param name="args"></param>
    private void ReportAllAnchorLocationResultsReceived(Microsoft.Azure.SpatialAnchors.LocateAnchorsCompletedEventArgs args)
    {
        Log("Received all requested anchors. Stopping the watcher.", true);
        args.Watcher.Stop();
    }

    #endregion

    #region External Calls

    /// <summary>
    /// Creates an anchor at the PoseIndicator's transform and saves the id persistently if everything worked.
    /// </summary>
    /// <param name="PoseIndicator"></param>
    public async void CreateAnchor(GameObject PoseIndicator)
    {
        string anchorID = await CreateSpatialAnchor(PoseIndicator);

        if (!String.IsNullOrEmpty(anchorID))
        {
            Config.AzureAnchorId = anchorID;
            Config.WriteToFile();
            Config.WriteToPlayerPrefs();
        }
    }

    /// <summary>
    /// Attempts to find the currently configured anchor and updates the gameobjects position to the anchor
    /// </summary>
    public void FindAnchor()
    {
        FindSpatialAnchor(Config.AzureAnchorId);
    }

    #endregion

    #region Events
    public EventHandler<AsaStatusEventArgs> AsaStatusEventHook;
    private void SendUpdateOnMainThread(AsaStatusEventType type, AsaStatus status)
    {
        ExecuteOnMainThread(new Action(() => AsaStatusEventHook?.Invoke(this, new AsaStatusEventArgs(type, status))));
    }
    #endregion
}

public class AsaStatusEventArgs : EventArgs
{
    public AsaStatusEventArgs(AsaStatusEventType type, AsaStatus status)
    {
        Type = type;
        Status = status;
    }

    public AsaStatusEventType Type;
    public AsaStatus Status;
}

public enum AsaStatusEventType
{
    CreateAnchor_NeedMoreData,
    CreateAnchor_AttemptUpload,
    CreateAnchor_Finished,
    FindAnchor_NeedMoreData,
    FindAnchor_Finished,
    FindAnchor_AlreadyTracked,
    Error,
    FindAnchor_Update,
    FindAnchor_CouldNotLocate,
    FindAnchor_DoesNotExist,
}

public struct AsaStatus
{
    public bool CanCreateAnchor { get; set; }

    /// <summary>
    /// Is set if the status requires more data to complete the process.
    /// This value only makes sense for the 
    /// </summary>
    public float Percentage { get; private set; }

    public string Error { get; private set; }

    public AsaStatus SetPercentage(float percentage)
    {
        Percentage = Mathf.Round(percentage);
        Error = "";
        return this;
    }

    public AsaStatus SetError(string error)
    {
        Error = error;
        return this;
    }
}