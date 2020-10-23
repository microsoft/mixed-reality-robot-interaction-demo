using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEventOfferer
{
    void HandleEvent(string eventName, IServiceMessage EventArgs);
}
