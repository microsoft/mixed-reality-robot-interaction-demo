using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SceneSystem;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfigScene : MonoBehaviour
{
    [HideInInspector]
    public bool enableMRInput;
    [HideInInspector]
    public bool enableUIInput;

    [HideInInspector]
    public IpInputHandler MrIpInput;
    [HideInInspector]
    public IpInputHandler UiIpInput;
    [HideInInspector]
    public GameObject MrIpInputParent;
    [HideInInspector]
    public GameObject UiIpInputParent;
    [HideInInspector]
    public bool ForceUiInput;

    // Start is called before the first frame update
    void Start()
    {
        if (String.IsNullOrEmpty(PlayerPrefs.GetString("server_address")))
        {
            PlayerPrefs.SetString("server_address", "ws://192.168.1.121:9090");
        }

        //Setup input handlers
        IpInputHandler handler = GetPrefab();
        var ipInput = GameObject.Instantiate(handler);
        ipInput.transform.SetParent(GetParent().transform, false);

        //define the callback and default value which should be pre-filed.
        ipInput.Init(
            OkButtonPressed, 
            new[] { 
                PlayerPrefs.GetString("server_address"), 
                PlayerPrefs.GetString("asa_account_id"), 
                PlayerPrefs.GetString("asa_account_key"), 
                PlayerPrefs.GetString("asa_account_domain"), 
                PlayerPrefs.GetString("current_asa_anchor_id") 
            });
    }

    private string IPInput = "";
    private string[] azureInput;
    public void OkButtonPressed(string ip, string azureId, string azureKey, string azureDomain)
    {
        //store the last submitted input in case the user presses the messageBoxButton witch opts-in into ignoring the validation message
        //this input is then used at the user's own "risk".
        IPInput = ip;
        azureInput = new string[] { azureId, azureKey, azureDomain };

        //Validate input
        //using the Callback of the messageBoxes the use can ignore the validation message and continue.

        //Should be a websocket address
        if (!ip.StartsWith("ws://"))
        {
            Toolkit.singleton.TriggerEvent("message_box_service", new MessageBoxContent(6, "URI is not valid", $"The uri must be a websocket starting with ws://", AcceptLastSubmittedInput, "Ignore & Cont."));
            return;
        }

        //Should be a valid URI
        Uri unusedUri;
        if(!Uri.TryCreate(ip, UriKind.Absolute, out unusedUri))
        {
            Toolkit.singleton.TriggerEvent("message_box_service", new MessageBoxContent(6, "URI is not valid", $"The URI {ip} could not be converted into a valid aboslute URI. The typical IP is ws://192.168.1.X:9090 or even localhost - depending on your setup.", AcceptLastSubmittedInput, "Ignore & Cont."));
            return;
        }

        AcceptInput(ip, azureId, azureKey, azureDomain);
    }

    private void AcceptLastSubmittedInput()
    {
        AcceptInput(IPInput, azureInput[0], azureInput[1], azureInput[2]);
    }

    private async void AcceptInput(string ip, string azureId, string azureKey, string azureDomain)
    {
#if WINDOWS_UWP
        if(String.IsNullOrEmpty(azureId) || String.IsNullOrEmpty(azureKey) || String.IsNullOrEmpty(azureDomain))
        {
            //Load from document.
            string filename = "asa_account_config.json";
            string path = Application.persistentDataPath;
            string filePath = Path.Combine(path, filename);

            if (!File.Exists(filePath))
            {
                Toolkit.singleton.TriggerEvent("message_box_service", 
                    new MessageBoxContent(120, "Config file not found!", $"The path {filePath} does not lead to an existing file.\nYour input was:\nAure Id: {azureId}\nAure Key: {azureKey}\nAzure Domain: {azureDomain}", AcceptLastSubmittedInput, "Try again"));
                return;
            }

            string json = File.ReadAllText(filePath);

            try
            {
                AzureConfig config = JsonConvert.DeserializeObject<AzureConfig>(json);
                azureId = config.AccountId;
                azureKey = config.AccountKey;
                azureDomain = config.AccountDomain;

                if (!String.IsNullOrEmpty(config.AzureAnchorId))
                {
                    PlayerPrefs.SetString("current_asa_anchor_id", config.AzureAnchorId);
                }
            }
            catch(Exception ex)
            {
                Toolkit.singleton.TriggerEvent("message_box_service",
                    new MessageBoxContent(6, "Deserialiation failed!", $"An error occurred", AcceptLastSubmittedInput, "Try again"));
                return;
            }
        }
#endif

        PlayerPrefs.SetString("server_address", ip);
        PlayerPrefs.SetString("asa_account_id", azureId);
        PlayerPrefs.SetString("asa_account_key", azureKey);
        PlayerPrefs.SetString("asa_account_domain", azureDomain);

        PlayerPrefs.Save();

        IMixedRealitySceneSystem sceneSystem = MixedRealityToolkit.Instance.GetService<IMixedRealitySceneSystem>();
        await sceneSystem.LoadContent("Demo2Scene", LoadSceneMode.Single);
    }




    /// <summary>
    /// Returns the current platform's correct IpInputHandler. 
    /// </summary>
    /// <returns></returns>
    private IpInputHandler GetPrefab()
    {
#if WINDOWS_UWP || UNITY_EDITOR
        if (ForceUiInput)
            return UiIpInput;
        else
            return MrIpInput;
#else
        return UiIpInput;
#endif
    }

    /// <summary>
    /// Returns the correct parent for the current platform.
    /// </summary>
    /// <returns></returns>
    private GameObject GetParent()
    {
#if WINDOWS_UWP || UNITY_EDITOR
        if (ForceUiInput)
            return UiIpInputParent;
        else
            return MrIpInputParent;
#else
        return UiIpInputParent;
#endif
    }


}


[Serializable]
public class AzureConfig
{
    public string AccountId { get; set; }
    public string AccountKey { get; set; }
    public string AccountDomain { get; set; }
    public string AzureAnchorId { get; set; }

    public bool IsAccountConfigValid()
    {
        return !(String.IsNullOrEmpty(AccountId) || String.IsNullOrEmpty(AccountKey) || String.IsNullOrEmpty(AccountDomain));
    }

    public void WriteToPlayerPrefs()
    {
        PlayerPrefs.SetString("asa_account_id", AccountId);
        PlayerPrefs.SetString("asa_account_key", AccountKey);
        PlayerPrefs.SetString("asa_account_domain", AccountDomain);
        if (!String.IsNullOrEmpty(AzureAnchorId))
        {
            PlayerPrefs.SetString("current_asa_anchor_id", AzureAnchorId);
        }
    }

    public void ReadFromPlayerPrefs()
    {
        AccountId = PlayerPrefs.GetString("asa_account_id");
        AccountKey = PlayerPrefs.GetString("asa_account_key");
        AccountDomain = PlayerPrefs.GetString("asa_account_domain");
        AzureAnchorId = PlayerPrefs.GetString("current_asa_anchor_id");
    }


    public void WriteToFile()
    {
        string filename = "asa_account_config.json";
        string path = Application.persistentDataPath;
        string filePath = Path.Combine(path, filename);

        try
        {
            string json = JsonConvert.SerializeObject(this);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Toolkit.singleton.TriggerEvent("message_box_service",
                new MessageBoxContent(6, "Serialiation failed!", $"An error occurred: " + ex.Message));
            return;
        }
    }
}