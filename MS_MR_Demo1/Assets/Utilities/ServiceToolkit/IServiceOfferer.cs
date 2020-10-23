using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IServiceOfferer<T> where T : IServiceMessage
{
    /// <summary>
    /// Update method invoked by the Toolkit after all ServiceOfferes were queried
    /// </summary>
    void ReportMessageBroadcasted();

    /// <summary>
    /// Priority of service used for deterministic update cycles
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Returns the name of this service
    /// </summary>
    /// <returns></returns>
    string GetServiceName();

    /// <summary>
    /// Method to retrieve the current service object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    T RetrieveServiceItem();

    /// <summary>
    /// True if the service has a new message since the last update
    /// If False, the service's consumer will not receive updates
    /// </summary>
    /// <returns></returns>
    bool HasMessage();

    /// <summary>
    /// If true, all new subscribers will be sent a message of this service offerer
    /// </summary>
    bool SendMessageToNewSubscribers { get; }
}

public interface IServiceMessage
{

}
