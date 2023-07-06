using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the debug logger.
/// </summary>
public class DebugLoggerController : MonoBehaviour
{

    public static DebugLoggerController inistance;

    public int maxLogCount = 10;
    public GameObject LogItemPrefab;
    public Transform LogContainer;
    public ScrollRect LogScrollRect;

    // Start is called before the first frame update
    void Start()
    {
        inistance = this;
        Application.logMessageReceivedThreaded += Application_logMessageReceivedThreaded;
    }

    private List<string> Logs = new List<string>();
    int counter = 0;
    int nullRefCounter = 0;
    private void Application_logMessageReceivedThreaded(string condition, string stackTrace, LogType type)
    {
        string txt = counter++ + " " + type.ToString() + ": " + condition;
        if (txt.Contains("Exception: NullReferenceException") && nullRefCounter < 50)
        {
            nullRefCounter++;
            return;
        }
        if(nullRefCounter >= 50)
        {
            nullRefCounter = 0;
        }
        Logs.Add(txt);
    }

    public void ManualLog(string txt)
    {
        if (txt.StartsWith("Exception: NullReferenceException")) return;

        Logs.Add(counter++ + " " + txt);
    }

    List<GameObject> GameObjects = new List<GameObject>();

    string lastText = "";
    public void Update()
    {
        //First copy the list to avoid the original list being changed during the foreach loop,
        //which throws an Exception
        var Logs_ = new List<string>(Logs);
        Logs.Clear();
        foreach (var logText in Logs_)
        {
            StringPublisher.PublishDebug(logText);

            if (logText.Contains("Move your device to capture more environment") && lastText.Contains("Move your device to capture more environment"))
            {
                if (GameObjects.Count > 0)
                {
                    GameObjects[0].GetComponentInChildren<TextMeshProUGUI>().text = logText;
                }
            }
            else
            {
                GameObject log = Instantiate(LogItemPrefab);
                log.GetComponentInChildren<TextMeshProUGUI>().text = logText;
                log.transform.SetParent(LogContainer, false);
                GameObjects.Insert(0, log);
            }

            lastText = logText;
        }

        //Destroy the oldest GameObjects.
        //In Exception cases there can be hundreds of Exceptions per Second.
        //HoloLens does not like this many (useless) GameObjects.
        while (GameObjects.Count > maxLogCount)
        {
            GameObject.Destroy(GameObjects.Last());
            GameObjects.RemoveAt(GameObjects.Count - 1);
        }

        LogScrollRect.ScrollToBottom();
    }
}

public static class ScrollRectExtensions
{
    public static void ScrollToTop(this ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, 1);
    }
    public static void ScrollToBottom(this ScrollRect scrollRect)
    {
        scrollRect.normalizedPosition = new Vector2(0, 0);
    }
}