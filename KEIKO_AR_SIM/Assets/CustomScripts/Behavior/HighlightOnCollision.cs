using System;
using System.Collections;
using UnityEngine;

public class HighlightOnCollision : MonoBehaviour
{
    /// <summary>
    /// The component will trigger the configured highlighting if if collides with a gameobject
    /// which is tagged with this name.
    /// </summary>
    [SerializeField]
    private string highlightOnCollisionWithTagName = "Robot";
    /// <summary>
    /// The duration of the highligh animation
    /// </summary>
    [SerializeField]
    private float highlightForSeconds = 5;
    [SerializeField]
    private HighLightMethod HighlightMethod;
    [SerializeField]
    private Transform targetTransform;
    /// <summary>
    /// A value indicating how heavy the highlight should be.
    /// Depending on HighLightMethod might for example be movement amplitude
    /// </summary>
    [SerializeField]
    private float weight = 1;
    /// <summary>
    /// A value used to multiply with the deltatime. Used for time dependent 
    /// highlight methods. (e.g. double the speed of a sinus)
    /// </summary>
    [SerializeField]
    private float speed = 1;
    [SerializeField]
    private float ignoreCollisionsBufferTime = 5;
    [SerializeField]
    private bool collisionDetectionEnabled = true;
    /// <summary>
    /// If this is set, the movement effect will be blended into the movebase target's effect.
    /// </summary>
    [SerializeField]
    private MoveBaseTarget moveBaseTarget;

    /// <summary>
    /// Bounce: The gameobject will bounce in a squared Sin function.
    /// <para>...</para>
    /// </summary>
    public enum HighLightMethod { 
        Bounce 
    }

    // Update is called once per frame
    void Update()
    {
        if (highlightActive)
        {
            highLightUpdate();
        }
    }

    [SerializeField]
    public bool IsHighlightActive => highlightActive;

    private bool highlightActive = false;

    /// <summary>
    /// An action, which is used to update the current highlight in the Update method.
    /// Is assigned depending on the highlight type
    /// </summary>
    private Action highLightUpdate;
    private float highlightStartTime;
    private Vector3 highlightStartPosition;
    /// <summary>
    /// The time when the last highlight finished. Is used to implemenet a buffer time
    /// where all collisions are ignored.
    /// </summary>
    private float highlightEndTime;

    private void OnCollisionEnter(Collision collision)
    {
        // This is how we handle double collisions. Just ignore everything as long as one highlight is still running.
        if (!collisionDetectionEnabled || highlightActive || Time.realtimeSinceStartup - highlightEndTime < ignoreCollisionsBufferTime) return;

        if (collision.gameObject.tag == highlightOnCollisionWithTagName)
        {
            Debug.Log($"Detected collision with {highlightOnCollisionWithTagName}");

            highLightUpdate = GetAction(HighlightMethod);
            highlightStartTime = Time.realtimeSinceStartup;
            highlightStartPosition = TargetTransform.localPosition;
            if (moveBaseTarget != null)
            {
                moveBaseTarget.DisableYFixing();
            }

            highlightActive = true;
            StartCoroutine(StopHighlightAfter(highlightForSeconds));
        }
    }

    private IEnumerator StopHighlightAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        highlightActive = false;
        highlightEndTime = Time.realtimeSinceStartup;
        if(moveBaseTarget != null)
        {
            moveBaseTarget.EnableYFixing();
        }
    }

    /// <summary>
    /// Returns a delegate used to update the highlight.
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    private Action GetAction(HighLightMethod method)
    {
        switch (method)
        {
            case HighLightMethod.Bounce:
                return BounceHighlight;
            default:
                throw new Exception($"Highlightmethod {method} not implemented"); 
        }
    }

    #region Highlight method implementations
    private void BounceHighlight()
    { 
        float deltaTime = (Time.realtimeSinceStartup - highlightStartTime)*speed;
        var newPos = new Vector3(
            TargetTransform.localPosition.x,
            highlightStartPosition.y + Mathf.Pow(Mathf.Sin(deltaTime) * weight, 2),
            TargetTransform.localPosition.z);
        TargetTransform.localPosition = newPos;
    }
    #endregion

    /// <summary>
    /// The Transform used for the Highlight animation
    /// </summary>
    private Transform TargetTransform
    {
        get
        {
            return targetTransform != null ? targetTransform : this.transform;
        }
    }

    /// <summary>
    /// Disables the detection and stops the current highlight process
    /// </summary>
    public void DisableDetection()
    {
        collisionDetectionEnabled = false;
        StartCoroutine(StopHighlightAfter(0));
    }

    /// <summary>
    /// Enables the collision detection
    /// </summary>
    public void EnableDetection()
    {
        collisionDetectionEnabled = true;
    }

}
