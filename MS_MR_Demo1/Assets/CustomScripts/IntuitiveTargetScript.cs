using Microsoft.Azure.SpatialAnchors.Unity;
using Microsoft.MixedReality.Toolkit.Physics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using UnityEngine;

public class IntuitiveTargetScript : MonoBehaviour
{
    public GameObject RobotOrigin;
    public GameObject RobotTarget;
    public GameObject JackalOrigin;
    public GameObject JackalTarget;
    public GameObject IntuitiveGoal;

    public GameObject[] NormalControlObjects;
    public GameObject[] IntuitiveControlObjects;

    public Pose lastPose;
    public float lastGoalSentTime;

    public Vector3 oldHandPos;
    public Quaternion oldHandRot;


    bool targetDidMove = false;

    public void Update()
    {
        if (normalControlMode) return;
        bool targetStoppedMoving = false;

        Transform target = IntuitiveGoal.transform;

        if (oldHandPos == target.localPosition && oldHandRot == target.localRotation)
        {
            //we have to use local rot&pos for that, sicne the standard gittering of the ros messages will make this guy move globally constantly.
            if (targetDidMove)
            {
                targetStoppedMoving = true;
            }
            targetDidMove = false;
        }
        else
        {
            targetDidMove = true;
            oldHandPos = target.localPosition;
            oldHandRot = target.localRotation;
        }

        Vector3 worldCoords = transform.TransformPoint(target.localPosition);

        Vector3 RobBaseToTarget_World = worldCoords - RobotOrigin.transform.position;
        Vector2 xyTarget_World = new Vector2(RobBaseToTarget_World.x, RobBaseToTarget_World.z);

        float xyDistance = xyTarget_World.magnitude;

        if (xyDistance > .7f)
        {
            Vector3 newJackalTargetPos = new Vector3(target.transform.position.x, JackalTarget.transform.position.y, target.transform.position.z);
            Vector3 currentJackalTargetPos = JackalTarget.transform.position;
            Vector3 new2 = newJackalTargetPos - (newJackalTargetPos - currentJackalTargetPos).normalized * 0.2f;

            JackalTarget.transform.position = new2;
            JackalTarget.transform.LookAt(newJackalTargetPos);

            if (targetStoppedMoving || (Time.realtimeSinceStartup - lastGoalSentTime > 2 && targetDidMove))
            {
                JackalTarget.GetComponent<MoveBaseTarget>().SendGoalPoseMessage();
                lastGoalSentTime = Time.realtimeSinceStartup;
            }
        }
        else
        {
            lastGoalSentTime = 0;
            Vector3 IndustrialRobotTarget = RobotOrigin.transform.InverseTransformPoint(worldCoords);
            RobotTarget.transform.localPosition = IndustrialRobotTarget;
            RobotTarget.transform.rotation = target.rotation;
        }
    }

    bool normalControlMode = true;
    public void ToggleControlMode()
    {
        normalControlMode = !normalControlMode;

        SetEnabled(NormalControlObjects, normalControlMode);
        SetEnabled(IntuitiveControlObjects, !normalControlMode);
    }
    private void SetEnabled(GameObject[] list, bool state)
    {
        foreach (var transform in list)
        {
            transform.SetActive(state);
        }
    }
}
