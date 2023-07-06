using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Nav;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationPlanSubscriber : UnitySubscriber<RosSharp.RosBridgeClient.MessageTypes.Nav.Path>
{
    protected override void ReceiveMessage(Path message)
    {
        lastMsg = message;
        didReceiveMessage = true;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    private Path lastMsg;
    private bool didReceiveMessage = false;

    // Update is called once per frame
    void Update()
    {
        if (didReceiveMessage)
        {
            rosMsgForwardService.ReportMsg(lastMsg, typeof(Path), Topic);
            didReceiveMessage = false;
        }
    }
}
