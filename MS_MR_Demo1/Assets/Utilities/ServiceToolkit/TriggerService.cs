using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Service is capable of sending triggers to all of its subscribers.
/// There is no actual content in the messages. The fact that a message
/// was sent is the message.
/// </summary>
public abstract class TriggerService : MonoBehaviour, IServiceOfferer<IServiceMessage>
{
    protected void Register(string name) {
        ServiceName = name;
        Toolkit.singleton.RegisterServiceOfferer(this);
    }

    private bool HasMessageFlag = false;

    private string ServiceName;

    public int Priority => 10;

    public string GetServiceName() => ServiceName;

    public bool HasMessage() => HasMessageFlag;

    public void ReportMessageBroadcasted()
    {
        HasMessageFlag = false;
    }

    public IServiceMessage RetrieveServiceItem()
    {
        return new ServiceTriggerMessage();
    }

    /// <summary>
    /// We do not want to immediatelly trigger all new subscribers
    /// </summary>
    public bool SendMessageToNewSubscribers => false;

    /// <summary>
    /// Trigger a message via the toolkit
    /// </summary>
    protected void TriggerMessage()
    {
        HasMessageFlag = true;
    }
}
