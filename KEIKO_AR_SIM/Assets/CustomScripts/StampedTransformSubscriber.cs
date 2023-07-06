using RosSharp;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Geometry;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StampedTransformSubscriber : UnitySubscriber<RosSharp.RosBridgeClient.MessageTypes.Geometry.TransformStamped>
{
    protected override void Start()
    {
        base.Start();
    }


    bool receivedMsg = false;
    TransformStamped lastMsg;

    public UnityEngine.Transform transformToApply;

    public bool useLocalTransform = true;

    public bool useRos2Unity = false;

    // Update is called once per frame
    void Update()
    {
        if (receivedMsg)
        {
            receivedMsg = false;
            RosSharp.RosBridgeClient.MessageTypes.Geometry.Vector3 newRosSharpPos = lastMsg.transform.translation;
            RosSharp.RosBridgeClient.MessageTypes.Geometry.Quaternion newRosSharpRot = lastMsg.transform.rotation;

            UnityEngine.Vector3 newPos = new UnityEngine.Vector3((float)newRosSharpPos.x, (float)newRosSharpPos.y, (float)newRosSharpPos.z);
            UnityEngine.Quaternion newRot = new UnityEngine.Quaternion((float)newRosSharpRot.x, (float)newRosSharpRot.y, (float)newRosSharpRot.z, (float)newRosSharpRot.w);

            if (useRos2Unity)
            {
                newPos = newPos.Ros2Unity(); 
                newRot = newRot.Ros2Unity();
            }

            if (useLocalTransform)
            {
                transformToApply.localPosition = newPos;
                transformToApply.localRotation = newRot;
            }
            else
            {
                transformToApply.position = newPos;
                transformToApply.rotation = newRot;
            }
        }
    }

    protected override void ReceiveMessage(TransformStamped message)
    {
        lastMsg = message;
        receivedMsg = true;
    }
}
