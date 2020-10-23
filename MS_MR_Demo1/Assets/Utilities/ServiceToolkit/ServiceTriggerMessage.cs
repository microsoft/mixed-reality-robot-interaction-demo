using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A message that has no real content and only serves as a trigger.
/// The Time property indicates when the message was created
/// </summary>
public class ServiceTriggerMessage : IServiceMessage
{
    public ServiceTriggerMessage()
    {
        Time = UnityEngine.Time.realtimeSinceStartup;
    }

    /// <summary>
    /// A time parameter stating when this message was created. Is set in the constructor of this class
    /// </summary>
    public float Time;
}
