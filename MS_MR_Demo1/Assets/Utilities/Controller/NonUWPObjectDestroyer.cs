using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonUWPObjectDestroyer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if !WINDOWS_UWP
        GameObject.Destroy(this.gameObject);
#endif
    }
}
