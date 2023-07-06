using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleStateSetter : MonoBehaviour
{
    public GameObject OnStateGameObject;

    public void SetToggleButtonState(bool stateToSet)
    {
        if(OnStateGameObject != null)
        {
            OnStateGameObject.SetActive(stateToSet);
        }
        else
        {
            Debug.Log($"{nameof(OnStateGameObject)} is not set");
        }
    }
}
