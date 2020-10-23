using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEventTriggerer
{
    void TriggerEvent(string eventName, IServiceMessage EventArgs);
}
