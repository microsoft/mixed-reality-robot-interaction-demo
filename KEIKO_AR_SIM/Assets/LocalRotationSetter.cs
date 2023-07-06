using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalRotationSetter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Target == null)
        {
            //Debug.Log("LocalRotationSetter cannot work since the target is not set. Self Destroy Component");
            //GameObject.Destroy(this);
        }

        var radialView = this.GetComponent<RadialView>();
        if (radialView != null)
        {
            radialView.enabled = false;
        }

    }

    public Transform Target;
    public float[] factors = new float[3];
    public float[] offsets = new float[3];
    public bool isEnabled = true;

    // Update is called once per frame
    void Update()
    {
        if (!isEnabled) return;

        if (Target == null && Camera.current != null)
        {
            Target = Camera.current.transform;
        }
        else if (Target == null)
            return;

        this.transform.LookAt(Target);
        this.transform.localEulerAngles = new Vector3(
            transform.localEulerAngles.x * factors[0] + offsets[0],
            transform.localEulerAngles.y * factors[1] + offsets[1],
            transform.localEulerAngles.z * factors[2] + offsets[2]);
    }

    public void EnableLookAtTransformRotation()
    {
        isEnabled = true;
    }


    public void DisableLookAtTransformRotation()
    {
        isEnabled = false;
    }

    public void SetIsEnabledState(bool enabled)
    {
        isEnabled = enabled;
    }

    public void ToggleIsEnabledState()
    {
        isEnabled = !isEnabled;
    }

}
