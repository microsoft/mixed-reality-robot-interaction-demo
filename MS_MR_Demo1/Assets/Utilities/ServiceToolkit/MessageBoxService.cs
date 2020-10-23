using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageBoxService : MonoBehaviour, IEventOfferer, IServiceConsumer<IServiceMessage>
{
    public void Start()
    {
        Toolkit.singleton.RegisterEvent("message_box_service", this);

        if (UseActiveCameraAsMRParent)
            Toolkit.singleton.RegisterServiceConsumer(this, CameraServiceName);
    }

    /// <summary>
    /// A prefab for 3D mixed Reality message Boxes
    /// </summary>
    [SerializeField]
    private MsgBoxController MRMessageBox;
    /// <summary>
    /// A prefab for actual UI Canvas components
    /// </summary>
    [SerializeField]
    private MsgBoxController UIMessageBox;
    /// <summary>
    /// The UI Canvas to be used as a parent for UI components (unused on MR devices & in Editor Mode)
    /// </summary>
    [SerializeField]
    private GameObject UICanvas;
    /// <summary>
    /// The parent GameObject where the MR messageBox will be placed into
    /// </summary>
    [HideInInspector]
    public GameObject MRParent;
    /// <summary>
    /// if true, the MR Parent will be set to the current camera
    /// </summary>
    [HideInInspector]
    public bool UseActiveCameraAsMRParent;
    /// <summary>
    /// The name of the camera service to retrieve the active camera
    /// </summary>
    [HideInInspector]
    public string CameraServiceName;
    /// <summary>
    /// if true, the UI component will be used disregarding the platform
    /// </summary>
    [SerializeField]
    public bool ForceUIMessageBox;



    public void HandleEvent(string eventName, IServiceMessage EventArgs)
    {
        //ignore the eventName since we only publish one event from this offerer
        ShowMessageBoxForSeconds((MessageBoxContent)EventArgs);
    }


    private MsgBoxController GetPrefab()
    {
#if WINDOWS_UWP || UNITY_EDITOR
        if (ForceUIMessageBox)
            return UIMessageBox;
        else
            return MRMessageBox;
#else
        return UIMessageBox;
#endif
    }
    private GameObject GetParent()
    {
#if WINDOWS_UWP || UNITY_EDITOR
        if (ForceUIMessageBox)
            return UICanvas;
        else
        {
            return MRParent;
        }
#else
        return UICanvas;
#endif
    }


    private void ShowMessageBoxForSeconds(MessageBoxContent msgBoxContent)
    {
        if(GetParent() == null)
        {
            Debug.LogWarning("Message Box Parent was null. MessageBox will not be shown!");
            return;
        }

        MsgBoxController msgBox = Instantiate(GetPrefab());
        msgBox.SetContent(msgBoxContent);
        msgBox.transform.SetParent(GetParent().transform, false);
        StartCoroutine(WaitForSecondsAndThenKill(msgBoxContent.ShowForSeconds, msgBox.gameObject));
    }

    private IEnumerator WaitForSecondsAndThenKill(float seconds, GameObject objectToKill)
    {
        yield return new WaitForSeconds(seconds);
        GameObject.Destroy(objectToKill);
    }

    public void ConsumeServiceItem(IServiceMessage item, string serviceName)
    {
        if (UseActiveCameraAsMRParent)
        {
            var msg = ((CameraServiceMessage)item);
            if (!msg.IsCameraNull)
            {
                MRParent = msg.ActiveCamera.gameObject;
            }
        }
    }
}