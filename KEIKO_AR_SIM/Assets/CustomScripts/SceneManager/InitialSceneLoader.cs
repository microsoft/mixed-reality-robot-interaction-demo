using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SceneSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialSceneLoader : MonoBehaviour
{

    [SerializeField]
    private string StartSceneName = "";

    [SerializeField]
    private bool doNotChangeAnything = false;


    // Start is called before the first frame update
    void Start()
    {
#if !UNITY_EDITOR
        doNotChangeAnything = false;
#endif
        if (doNotChangeAnything) return;

        if (!string.IsNullOrEmpty(StartSceneName))
        {
            Load();
        }
    }

    private async void Load()
    {
        IMixedRealitySceneSystem sceneSystem = MixedRealityToolkit.Instance.GetService<IMixedRealitySceneSystem>();
        if (!sceneSystem.IsContentLoaded(StartSceneName))
            await sceneSystem.LoadContent(StartSceneName, LoadSceneMode.Single);
    }
}
