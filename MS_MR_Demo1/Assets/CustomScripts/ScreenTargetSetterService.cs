using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScreenTargetSetterService : MonoBehaviour, IServiceOfferer<IServiceMessage>, IServiceConsumer<IServiceMessage>
{
    public void Start()
    {
        //Register to services
        Toolkit.singleton.RegisterServiceConsumer(this, "active_camera_retrieval_service");
        Toolkit.singleton.RegisterServiceConsumer(this, "asa_y_level_publisher");
        Toolkit.singleton.RegisterServiceOfferer(this);
    }

    private float yLevel;
    private bool hasNewTap = false;
    private Camera currentCamera;

    private Vector3 currentRayOrigin;
    private Vector3 currentRayDirection;

    public int Priority => 5;

    public bool SendMessageToNewSubscribers => false;

    public void ConsumeServiceItem(IServiceMessage item, string serviceName)
    {
        if (item is Vector3ServiceMessage)
        {
            yLevel = ((Vector3ServiceMessage)item).Vector.y;
        }
        else if (item is CameraServiceMessage)
        {
            CameraServiceMessage msg = (CameraServiceMessage)item;
            if (!msg.IsCameraNull)
            {
                currentCamera = msg.ActiveCamera;
            }
        }
    }

    public string GetServiceName() => nameof(ScreenTargetSetterService);

    public bool HasMessage() => hasNewTap;

    public void ReportMessageBroadcasted()
    {
        hasNewTap = false;
    }

    public IServiceMessage RetrieveServiceItem()
    {
        return new RayServiceMessage()
        {
            RayOrigin = currentRayOrigin,
            RayDirection = currentRayDirection
        };
    }

    /// <summary>
    /// Queries the EventData to see if the pointer is above a UI Elememnt or not. 
    /// If the inputCoords are not given, it gets them.
    /// </summary>
    /// <param name="inputCoordinates"></param>
    /// <returns></returns>
    private bool IsPointerOverUIObject(Vector2? inputCoordinates = null)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        
        Vector2 input;
        if (inputCoordinates == null)
        {
            var tmp = GetInputTouch();
            if (tmp == null) return false;
            input = (Vector2)tmp;
        }
        else
        {
            input = (Vector2)inputCoordinates;
        }

        eventDataCurrentPosition.position = input;
        List < RaycastResult > results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentCamera != null)
        {
            Vector2? clickPoint = GetInputTouch();

            if (clickPoint != null && !IsPointerOverUIObject(clickPoint))
            {
                Ray ray = currentCamera.ScreenPointToRay((Vector2)clickPoint);

                Vector3 v = ray.direction;
                Vector3 o = ray.origin;

                currentRayOrigin = o;
                currentRayDirection = v;
                hasNewTap = true;
            }
        }
    }

    /// <summary>
    /// Returns the tap/click coordinates
    /// </summary>
    /// <returns></returns>
    private Vector2? GetInputTouch()
    {
        Vector2? clickPoint = null;
        if (Input.GetMouseButton(0))
        {
            //on PC
            clickPoint = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
        else if (Input.touchCount > 0)
        {
            //on smartphones
            Touch touch = Input.GetTouch(0);
            clickPoint = touch.position;
        }
        return clickPoint;
    }

}
