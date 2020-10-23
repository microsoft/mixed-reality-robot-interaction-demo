using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3ServiceOfferer : MonoBehaviour, IServiceOfferer<IServiceMessage>
{
    private void Start()
    {
        if (InitOnStart)
        {
            Toolkit.singleton.RegisterServiceOfferer(this);
        }
    }

    public enum Vector3PublisherType { Transform, Vector3 }
    public enum TransformComponent { position, eulerAngles }

    [HideInInspector]
    public Vector3PublisherType VectorSourceType;
    [HideInInspector]
    public Transform transformToPublish;
    [HideInInspector]
    public TransformComponent ComponentToPublish;
    [HideInInspector]
    private Vector3 Vector;
    [HideInInspector]
    public Vector3 InspectorVector;

    public bool InitOnStart = true;
    public string ServiceName = "";
    public int Priority { get; set; } = 5;


    private Vector3 NewlySetVector;
    private bool hasNewVector = false;

    public bool SendMessageToNewSubscribers { get; set; } = true;

    public string GetServiceName() => ServiceName;

    public bool HasMessage() => hasNewVector;

    public void ReportMessageBroadcasted()
    {
        hasNewVector = false;
        Vector = NewlySetVector;
    }

    public IServiceMessage RetrieveServiceItem()
    {
        return new Vector3ServiceMessage(NewlySetVector);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3? newVector = null;

        if (VectorSourceType == Vector3PublisherType.Transform)
        {
            if (ComponentToPublish == TransformComponent.eulerAngles)
            {
                newVector = transformToPublish.eulerAngles;
            }
            else if (ComponentToPublish == TransformComponent.position)
            {
                newVector = transformToPublish.position;
            }
        }
        else
        {
            newVector = InspectorVector;
        }


        if (newVector != Vector && newVector != null)
        {
            hasNewVector = true;
            NewlySetVector = (Vector3)newVector;
        }
    }



    public void SetNewVector(Vector3 vector)
    {
        if (VectorSourceType != Vector3PublisherType.Vector3)
        {
            Debug.LogError("Cannot manually set vector on transform publisher");
            return;
        }

        NewlySetVector = vector;
    }

}


public class Vector3ServiceMessage : IServiceMessage
{
    public Vector3 Vector { get; set; }

    public Vector3ServiceMessage(Vector3 vector)
    {
        Vector = vector;
    }
}