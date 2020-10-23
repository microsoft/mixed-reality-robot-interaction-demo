using RosSharp;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The executable StampedPosePublisher listens to the Serivce "move_base_goal_publishing_service"
/// and publishes its assigned Transform every time when the service triggers it.
/// </summary>
public class ExecutableStampedPosePublisher : PoseStampedPublisher, IServiceConsumer<IServiceMessage>
{
    [SerializeField]
    private string TriggerServiceName = "move_base_goal_publishing_service";

    protected override void Start()
    {
        base.Start();

        //The move_base_goal_publishing_service sends trigger messages every time when 
        //a new goal pose should be published. This consumer registeres to it and enables publishing whenever
        //it is told to do so / triggered.
        Toolkit.singleton.RegisterServiceConsumer(this, TriggerServiceName);
    }

    public bool ShouldPublishOnce = false;

    public void ConsumeServiceItem(IServiceMessage item, string serviceName)
    {
        if (serviceName == TriggerServiceName)
            ShouldPublishOnce = true;
    }

    protected override void UpdateMessage()
    {
        if (ShouldPublishOnce && canPublish)
        {
            message.header.Update();
            GetGeometryPoint(PublishedTransform.localPosition.Unity2Ros(), message.pose.position);
            GetGeometryQuaternion(PublishedTransform.localRotation.Unity2Ros(), message.pose.orientation);

            Publish(message);
            Debug.Log("Published move_base_goal point!");
            ShouldPublishOnce = false;
        }
    }
}
