using RosSharp.RosBridgeClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringPublisher : UnityPublisher<RosSharp.RosBridgeClient.MessageTypes.Std.String>
{
    private static List<string> stack = new List<string>();
    private static List<Exception> stack_e = new List<Exception>();

    /// <summary>
    /// A method which publishes the str if the publisher is advertised already, otherwise stacks it and publishes as soon as possible
    /// </summary>
    /// <param name="str"></param>
    public static void PublishDebug(string str)
    {
        if (str == null) return;

        str = DateTime.Now.TimeOfDay.ToString() + " - " + str;

        if (DebugLogger == null || !DebugLogger.canPublish)
        {
            stack.Add(str);
        }
        else
        {
            DebugLogger.PublishStringInternal(str);
        }
    }
    /// <summary>
    /// A method which publishes the str if the publisher is advertised already, otherwise stacks it and publishes as soon as possible
    /// </summary>
    /// <param name="str"></param>
    public static void PublishException(Exception ex)
    {
        if (DebugLogger == null || !DebugLogger.canPublish)
        {
            stack_e.Add(ex);
        }
        else
        {
            DebugLogger.PublishExceptionInternal(ex);
        }
    }

    public static StringPublisher DebugLogger;
    protected override void Start()
    {
        base.Start();

        if (DebugLogger == null)
        {
            DebugLogger = this;
        }

        Debug.Log("I came online");
    }

    public void Update()
    {
        if(stack.Count > 0 && canPublish)
        {
            foreach (var str in stack)
            {
                PublishStringInternal(str);
            }
            stack.Clear();
        }
        if(stack_e.Count > 0 && canPublish)
        {
            foreach (var ex in stack_e)
            {
                PublishExceptionInternal(ex);
            }
            stack_e.Clear();
        }
    }

    private void PublishStringInternal(string str)
    {
        if (str == null) return;
        if (str.Contains("Warning: Publication id was null.")) return;

        try
        {
            Publish(new RosSharp.RosBridgeClient.MessageTypes.Std.String(str));
        }
        catch
        {

        }
    }
    private void PublishExceptionInternal(Exception ex)
    {
        try
        {
            Publish(new RosSharp.RosBridgeClient.MessageTypes.Std.String($"Stacktrace: {ex.StackTrace}, Message: {ex.Message}, Target: {ex.TargetSite}, Source: {ex.Source}"));
        }
        catch
        {

        }
    }
}
