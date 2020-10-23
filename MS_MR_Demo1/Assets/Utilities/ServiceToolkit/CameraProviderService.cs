using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraProviderService : MonoBehaviour, IServiceOfferer<IServiceMessage>
{

    public void Start()
    {
        Toolkit.singleton.RegisterServiceOfferer(this);
    }

    [SerializeField]
    private Camera ActiveCamera;

    /// <summary>
    /// if the last camera sent via the Toolkit was null, this flag is true.
    /// It will trigger an additional Message next update and will continue to do so, until the
    /// Camera is not null.
    /// </summary>
    private bool LastMsgCameraWasNull = false;

    private bool hasMessage = false;

    public int Priority => 5;

    public bool SendMessageToNewSubscribers => true;

    public string GetServiceName() => "active_camera_retrieval_service";

    public bool HasMessage() => hasMessage && ActiveCamera != null;

    public void ReportMessageBroadcasted()
    {
        //Do not send more messages, if the last message was a non-null-message
        hasMessage = !LastMsgCameraWasNull;
    }

    public IServiceMessage RetrieveServiceItem()
    {
        LastMsgCameraWasNull = ActiveCamera == null;
        return new CameraServiceMessage(ActiveCamera);
    }

    public void SetCamera(Camera camera)
    {
        ActiveCamera = camera;
        hasMessage = true;
    }
}

public class CameraServiceMessage : IServiceMessage
{
    public CameraServiceMessage(Camera activeCamera)
    {
        ActiveCamera = activeCamera;
    }

    /// <summary>
    /// Returns true if the camera is not null
    /// (Emphasises the fact, that the camera can be null)
    /// </summary>
    public bool IsCameraNull => ActiveCamera == null;

    /// <summary>
    /// The current Camera.
    /// Might be null
    /// </summary>
    public Camera ActiveCamera { get; set; }
}