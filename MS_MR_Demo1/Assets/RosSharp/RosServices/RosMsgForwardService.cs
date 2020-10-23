using RosSharp.RosBridgeClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RosMsgForwardService : IServiceOfferer<IServiceMessage>
{
    public int Priority => 1;

    /// <summary>
    /// We do not need to update all subscribers with the current message, since the this service's
    /// message lifetime is not very long and changes rapidly. Furthermore, it contains different 
    /// kinds of messages, which in the end might not even make sense.
    /// </summary>
    public bool SendMessageToNewSubscribers => false;

    public string GetServiceName() => nameof(RosMsgForwardService);

    List<RosMsgServiceMsgItem> messages = new List<RosMsgServiceMsgItem>();

    public IServiceMessage RetrieveServiceItem()
    {
        return new RosMsgServiceMsg() { Items = messages };
    }

    public void ReportMessageBroadcasted()
    {
        messages.Clear();
    }

    public void ReportMsg(Message msg, Type type, string name)
    {
        messages.Add(new RosMsgServiceMsgItem() { Msg = msg, Type = type, Name = name });
    }

    public bool HasMessage() => messages.Count > 0;
}


public class RosMsgServiceMsg : IServiceMessage
{
    public List<RosMsgServiceMsgItem> Items { get; set; }
}

public class RosMsgServiceMsgItem
{
    public Message Msg { get; set; }
    public Type Type { get; set; }
    public string Name { get; set; }
}
