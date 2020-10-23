using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using UnityEngine;

/// <summary>
/// A singleton class which manages services.
/// There are two types:
/// - one to many service
///   One service offerer has multiple service consumers. Communication happens via objects implementing the IServiceMessage interface
/// - many to one events
///   Many event-consumers can trigger one event-offerer's event.
/// </summary>
public class Toolkit : MonoBehaviour
{
    private void Awake()
    {
        if (singleton == null)
            singleton = this;
    }

    public static Toolkit singleton;

    /// <summary>
    /// Standard Unity Update life cylcle method
    /// Processes all services
    /// </summary>
    void Update()
    {
        //Query all services and forward messages to consumers
        foreach (var service in services.Where(x => x.HasMessage()))
        {
            var obj = service.RetrieveServiceItem();
            var currentName = service.GetServiceName();
            foreach (var serviceConsumer in serviceTable[service])
            {
                serviceConsumer.ConsumeServiceItem(obj, currentName);
            }

            service.ReportMessageBroadcasted();
        }
    }

    /// <summary>
    /// Registers a service offerer if not exists already
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public bool RegisterServiceOfferer(IServiceOfferer<IServiceMessage> service)
    {
        if (!serviceNameTable.ContainsKey(service.GetServiceName()))
        {
            services.Add(service);
            services = services.OrderBy(x => x.Priority).ToList();
            serviceTable[service] = new List<IServiceConsumer<IServiceMessage>>();
            serviceNameTable[service.GetServiceName()] = service;

            //post-register all services that are in the specific stack
            if (consumerStack.ContainsKey(service.GetServiceName()))
            {
                serviceTable[service].AddRange(consumerStack[service.GetServiceName()]);
                consumerStack.Remove(service.GetServiceName());

                if (service.SendMessageToNewSubscribers)
                {
                    var serviceItem = service.RetrieveServiceItem();
                    foreach (var consumer in serviceTable[service])
                    {
                        consumer.ConsumeServiceItem(serviceItem, service.GetServiceName());
                    }
                }
            }

            return true;
        }
        else
        {
            //Debug.Log("Service was added twice");
            return false;
        }
    }

    /// <summary>
    /// Unregisters a service offerer if it exists
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    public bool UnregisterServiceOfferer(IServiceOfferer<IServiceMessage> service)
    {
        if (serviceTable.ContainsKey(service))
        {
            services.Remove(service);
            serviceTable.Remove(service);
            serviceNameTable.Remove(service.GetServiceName());
            return true;
        }
        else
        {
            Debug.Log("Service was not found in service table");
            return false;
        }
    }

    /// <summary>
    /// Registers a Service Consumer if the service exists
    /// </summary>
    /// <param name="consumer"></param>
    /// <param name="consumedServiceName"></param>
    /// <param name="stackIfServiceDoesNotExist">if true, the consumer will be stacked and registered, as soon as the correct service registeres</param>
    /// <returns></returns>
    public bool RegisterServiceConsumer(IServiceConsumer<IServiceMessage> consumer, string consumedServiceName, bool stackIfServiceDoesNotExist = true)
    {
        //Check if service exists
        if (serviceNameTable.ContainsKey(consumedServiceName))
        {
            var service = serviceNameTable[consumedServiceName];
            serviceTable[service].Add(consumer);

            if (service.SendMessageToNewSubscribers)
            {
                consumer.ConsumeServiceItem(service.RetrieveServiceItem(), service.GetServiceName());
            }

            return true;
        }
        else
        {
            if (stackIfServiceDoesNotExist)
            {
                //Add consumer to stack
                if (consumerStack.ContainsKey(consumedServiceName))
                {
                    consumerStack[consumedServiceName].Add(consumer);
                }
                else
                {
                    consumerStack[consumedServiceName] = new List<IServiceConsumer<IServiceMessage>>()
                    {
                        consumer
                    };
                }
                //Debug.Log($"Service with the name {consumedServiceName} does not exist, consumer was stacked");
                return true;
            }

            //Debug.Log($"Service with the name {consumedServiceName} does not exist");
            return false;
        }
    }

    /// <summary>
    /// Unregisters a service consumer if exists
    /// </summary>
    /// <param name="consumer"></param>
    /// <param name="consumedServiceName"></param>
    /// <returns></returns>
    public bool UnregisterServiceConsumer(IServiceConsumer<IServiceMessage> consumer, string consumedServiceName)
    {
        //Remove consumer from stack if it exists
        if (consumerStack.ContainsKey(consumedServiceName))
        {
            consumerStack[consumedServiceName].Remove(consumer);
            if (consumerStack[consumedServiceName].Count == 0)
            {
                consumerStack.Remove(consumedServiceName);
            }
        }

        if (serviceNameTable.ContainsKey(consumedServiceName))
        {
            serviceTable[serviceNameTable[consumedServiceName]].Remove(consumer);
            return true;
        }
        else
        {
            //Debug.Log($"Service with the name {consumedServiceName} does not exist");
            return false;
        }
    }

    /// <summary>
    /// Returns the service registered under the given name
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public IServiceOfferer<IServiceMessage> GetService(string serviceName)
    {
        if (serviceNameTable.ContainsKey(serviceName))
        {
            return serviceNameTable[serviceName];
        }
        else
        {
            //Debug.Log($"Service with the name {serviceName} does not exist");
            return null;
        }
    }

    private Dictionary<IServiceOfferer<IServiceMessage>, List<IServiceConsumer<IServiceMessage>>> serviceTable
        = new Dictionary<IServiceOfferer<IServiceMessage>, List<IServiceConsumer<IServiceMessage>>>();

    private Dictionary<string, IServiceOfferer<IServiceMessage>> serviceNameTable
        = new Dictionary<string, IServiceOfferer<IServiceMessage>>();

    private List<IServiceOfferer<IServiceMessage>> services
        = new List<IServiceOfferer<IServiceMessage>>();

    /// <summary>
    /// Used to temporarily stack consumers if their service does not exist yet.
    /// </summary>
    private Dictionary<string, List<IServiceConsumer<IServiceMessage>>> consumerStack
        = new Dictionary<string, List<IServiceConsumer<IServiceMessage>>>();





    #region Events

    Dictionary<string, IEventOfferer> eventOfferers = new Dictionary<string, IEventOfferer>();

    public void RegisterEvent(string eventName, IEventOfferer offerer)
    {
        if (!eventOfferers.ContainsKey(eventName))
        {
            eventOfferers[eventName] = offerer;
        }
        else
        {
            Debug.Log($"Event with name {eventName} already registered");
        }
    }

    public void TriggerEvent(string eventName, IServiceMessage EventArgs)
    {
        if (eventOfferers.ContainsKey(eventName))
        {
            eventOfferers[eventName].HandleEvent(eventName, EventArgs);
        }
        else
        {
            Debug.Log($"No event with name {eventName} is registered");
        }
    }


    #endregion


}
