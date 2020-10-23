using RosSharp.RosBridgeClient.MessageTypes.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RobotTrajectoryController : MonoBehaviour, IServiceConsumer<IServiceMessage>
{
    [SerializeField]
    private string topicMsgConsumed = "/move_base/TrajectoryPlannerROS/global_plan";
    [SerializeField]
    private float simplifyToleracne = .01f;

    public void ConsumeServiceItem(IServiceMessage item, string serviceName)
    {
        if (serviceName == "RosMsgForwardService")
        {
            var msg = ((RosMsgServiceMsg)item);
            var planItem = msg.Items.LastOrDefault(x => x.Name == topicMsgConsumed && x.Type == typeof(RosSharp.RosBridgeClient.MessageTypes.Nav.Path));

            if (planItem == null) return;

            var plan = (RosSharp.RosBridgeClient.MessageTypes.Nav.Path)planItem.Msg;
            newPositions = plan.poses;
            newPositionsReceived = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Toolkit.singleton.RegisterServiceConsumer(this, "RosMsgForwardService");
        if (renderer == null)
        {
            renderer = this.GetComponent<LineRenderer>();
        }
    }

    private bool newPositionsReceived = false;
    PoseStamped[] newPositions;
    LineRenderer renderer;

    // Update is called once per frame
    void Update()
    {
        if (newPositionsReceived)
        {
            newPositionsReceived = false;
            renderer.positionCount = newPositions.Length;
            renderer.SetPositions(newPositions.Select(x => x.pose.position.ToLocalVector(this.transform, true)).ToArray());
            renderer.Simplify(simplifyToleracne);
        }
    }


}

public static class PointExtensions
{
    public static UnityEngine.Vector3 ToLocalVector(this Point p, UnityEngine.Transform relativeTransform, bool toWorldCoords)
    {
        var vector_world = RosSharp.TransformExtensions.Ros2Unity(new UnityEngine.Vector3((float)p.x, (float)p.y, (float)p.z));
        if (toWorldCoords) 
            return vector_world;
        else
            return relativeTransform.InverseTransformPoint(vector_world);
    }
}