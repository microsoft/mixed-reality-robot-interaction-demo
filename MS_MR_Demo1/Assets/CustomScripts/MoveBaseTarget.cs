using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A component which serves as a target item for the robot.
/// It serves as a communication center for the application 
/// with regards to the goal pose, and also handles the behavior 
/// the goal component should have.
/// </summary>
public class MoveBaseTarget : TriggerService, IServiceConsumer<IServiceMessage>
{
    /// <summary>
    /// If true, then the attached gameObject will be fixed to y = 0;
    /// </summary>
    [SerializeField]
    private bool yFixingEnabled = true;

    /// <summary>
    /// The y level used to fix this target
    /// </summary>
    private float yFixingLevel;

    /// <summary>
    /// The GameObject-Prefab used to rotate the goal pose marker object.
    /// </summary>
    [SerializeField]
    private GameObject RotationObject;

    // Start is called before the first frame update
    void Start()
    {
        base.Register("move_base_goal_publishing_service");
        Toolkit.singleton.RegisterServiceConsumer(this, "asa_y_level_publisher");
        Toolkit.singleton.RegisterServiceConsumer(this, "ScreenTargetSetterService");
    }

    void Update()
    {
        if (yFixingEnabled && !inSmoothingProcess && this.transform.position.y != 0)
        {
            Vector3 v = transform.localPosition;
            transform.localPosition = new Vector3(v.x, yFixingLevel, v.z);
        }
        else if (inSmoothingProcess)
        {
            Vector3 v = transform.localPosition;
            Vector3 target = new Vector3(v.x, yFixingLevel, v.z);
            this.transform.localPosition = v + (target - v) * (Time.realtimeSinceStartup - smoothingTimeStampStart) / 3;
            if ((target - v).magnitude < .001)
            {
                inSmoothingProcess = false;
            }
        }
    }

    /// <summary>
    /// Sends a trigger, to tell the responsible component (attached to the RosConnector)
    /// to publish the goal pose.
    /// </summary>
    public void SendGoalPoseMessage()
    {
        TriggerMessage();
    }

    /// <summary>
    /// Flag indicating whether or not this component is currently smoothing into its fixed y position 
    /// after an animation.
    /// </summary>
    private bool inSmoothingProcess = false;
    /// <summary>
    /// a timestamp stating when the smoothing process started. Used for smoothing-linear-interpolation.
    /// </summary>
    private float smoothingTimeStampStart;

    /// <summary>
    /// Disables the y fixing.
    /// </summary>
    public void DisableYFixing()
    {
        yFixingEnabled = false;
    }

    /// <summary>
    /// Enables the y-Fixing and starts the smoothing animation back to its root-position.
    /// </summary>
    public void EnableYFixing()
    {
        yFixingEnabled = true;
        inSmoothingProcess = true;
        smoothingTimeStampStart = Time.realtimeSinceStartup;
    }

    #region Rotation Mode
    private bool rotationMode;

    public bool RotationMode
    {
        get { return rotationMode; }
        set { rotationMode = value; SetRotationMode(value); }
    }


    public void SetRotationMode(bool mode)
    {
        if (RotationObject != null) RotationObject.SetActive(mode);
    }

    /// <summary>
    /// Toggles the state of the rotation helper of the goal pose indicator
    /// </summary>
    public void ToggleRotationMode()
    {
        if (RotationObject != null)
        {
            RotationObject.SetActive(!RotationObject.activeInHierarchy);
        }
    }
    #endregion

    #region ScreenSetterService
    private bool screenTapToSetGoalPointActive = false;
    public void ToggleScreenTapServiceEnabled()
    {
        screenTapToSetGoalPointActive = !screenTapToSetGoalPointActive;
    }
    #endregion

    public void ConsumeServiceItem(IServiceMessage item, string serviceName)
    {
        if (serviceName == "asa_y_level_publisher")
        {
            yFixingLevel = ((Vector3ServiceMessage)item).Vector.y;
            EnableYFixing();
        }
        else if (serviceName == "ScreenTargetSetterService")
        {
            if (screenTapToSetGoalPointActive)
            {
                var msg = (RayServiceMessage)item;
                Vector3 origin_world = msg.RayOrigin;
                Vector3 direction_world = msg.RayDirection;

                Vector3 o = this.transform.InverseTransformPoint(origin_world); //.InverseTransformPoint(child.parent.position);
                Vector3 v = this.transform.InverseTransformDirection(direction_world);

                float alpha = (yFixingLevel - o.y) / v.y;
                float x = o.x + alpha * v.x;
                float z = o.z + alpha * v.z;

                if (Mathf.Abs(x) > 100) x = 100;
                if (Mathf.Abs(z) > 100) z = 100;

                this.transform.position = this.transform.TransformPoint(new Vector3(x, yFixingLevel, z));

                Debug.DrawRay(o, v * alpha, Color.yellow);
            }
        }
    }


}
