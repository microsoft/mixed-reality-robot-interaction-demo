using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RosBridgeStatusController : MonoBehaviour, IServiceConsumer<IServiceMessage>
{
    public ToggleStateSetter StateSetter;
    public int TimeOutSecondsThreshold = 2;
    public TextMeshPro LastMsgReceivedTextMesh;
    public TextMeshPro NrMsgReceivedTextMesh;

    private int nrOfMsgReceived = 0;
    private ConnectionStateService connectionSateService;

    public void Start()
    {
        connectionSateService = this.GetComponent<ConnectionStateService>();
        Toolkit.singleton.RegisterServiceConsumer(this, "RosMsgForwardService");
        Toolkit.singleton.RegisterServiceOfferer(connectionSateService);
    }

    public void Update()
    {
        if (receivedClock)
        {
            lastTimeClockReceived = Time.realtimeSinceStartup;
            receivedClock = false;
        }
        float noMessageSinceSeconds = Time.realtimeSinceStartup - lastTimeClockReceived;
        if (noMessageSinceSeconds > TimeOutSecondsThreshold)
        {
            // Did not receive a clock update for at least two seconds
            if (currentConnectionStateVisible)
            {
                StateSetter.SetToggleButtonState(false);
                currentConnectionStateVisible = false;
                connectionSateService.SetCurrentState(false);
            }
        }
        else
        {
            // Last update is newer than 2 seconds
            if (!currentConnectionStateVisible)
            {
                StateSetter.SetToggleButtonState(true);
                currentConnectionStateVisible = true;
                connectionSateService.SetCurrentState(true);
            }
        }

        SetTextMesh(LastMsgReceivedTextMesh, Mathf.RoundToInt(noMessageSinceSeconds) + " sec ago", nameof(LastMsgReceivedTextMesh));
        SetTextMesh(NrMsgReceivedTextMesh, nrOfMsgReceived.ToString(), nameof(NrMsgReceivedTextMesh));
    }

    /// <summary>
    /// sets a textMeshPro's text and logs to the Unity Log if the TextMeshPro object is not set
    /// </summary>
    /// <param name="textMesh"></param>
    /// <param name="content"></param>
    /// <param name="textName"></param>
    private void SetTextMesh(TextMeshPro textMesh, string content, string textName)
    {
        if (textMesh != null)
        {
            textMesh.text = content;
        }
        else
        {
            Debug.Log($"TextMeshPro {textName} is not set");
        }
    }

    bool currentConnectionStateVisible = false;
    float lastTimeClockReceived = -1;
    Clock lastClock;
    bool receivedClock;

    public void ConsumeServiceItem(IServiceMessage item, string serviceName)
    {
        RosMsgServiceMsgItem wantedItem = ((RosMsgServiceMsg)item).Items.FirstOrDefault(x => x.Name == "/clock");
        
        if (wantedItem == null) 
            //No clock msg in this update
            return;
        
        Clock element = (Clock)wantedItem.Msg;
        lastClock = element;
        receivedClock = true;
        nrOfMsgReceived += ((RosMsgServiceMsg)item).Items.Count;
    }
}
