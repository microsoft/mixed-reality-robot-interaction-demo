using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Subscribes to gazebo's clock topic
/// </summary>
public class ClockSubscriber : UnitySubscriber<Clock>
{
    private Clock lastMsg;
    private bool msgReceived;

    protected override void Start()
    {
        base.Start();
    }

    protected override void ReceiveMessage(Clock message)
    {
        lastMsg = message;
        msgReceived = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (msgReceived)
        {
            //Forward the received message to the RosMsgForwardingService
            rosMsgForwardService.ReportMsg(lastMsg, typeof(Clock), Topic);
            msgReceived = false;
        }
    }
}

public class Clock : Message
{
    public RosSharp.RosBridgeClient.MessageTypes.Std.Time clock { get; set; }
    public override string RosMessageName => "rosgraph_msgs/Clock";
}