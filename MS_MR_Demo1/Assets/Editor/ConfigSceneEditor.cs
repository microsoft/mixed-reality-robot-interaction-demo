using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ConfigScene))]
public class ConfigSceneEditor : Editor
{
    SerializedObject obj;

    public void OnEnable()
    {
        obj = new SerializedObject((ConfigScene)target);
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.LabelField("IP input configuration");
        EditorGUI.indentLevel++;

        var enableMRInput = GetProp("enableMRInput");
        var enableUIInput = GetProp("enableUIInput");

        enableMRInput.boolValue = EditorGUILayout.Toggle(new GUIContent("Enable HoloLens builds", "If true the application will require configurations to build for HoloLens. This includes parents for MR inputs (such as IP configuration) and others.") , enableMRInput.boolValue);

        if (enableMRInput.boolValue)
        {
            EditorGUI.indentLevel++;
            var mrIpInput = GetProp("MrIpInput");
            var mrIpInputParent = GetProp("MrIpInputParent");
            mrIpInput.objectReferenceValue = EditorGUILayout.ObjectField("MR IP Input Prefab", mrIpInput.objectReferenceValue, typeof(IpInputHandler), allowSceneObjects: false);
            mrIpInputParent.objectReferenceValue = EditorGUILayout.ObjectField("Parent for MR input", mrIpInputParent.objectReferenceValue, typeof(IpInputHandler), allowSceneObjects: false);
            EditorGUI.indentLevel--;
        }

        enableUIInput.boolValue = EditorGUILayout.Toggle(new GUIContent("Enable UI builds", "If true the application will require configurations for UI components. This is suggested / preferred if the app is deployed to a non-immersive-MR device, such as smartphones or Windows Standalone."), enableUIInput.boolValue);

        if (enableUIInput.boolValue)
        {
            EditorGUI.indentLevel++;
            var mrIpInput = GetProp("UiIpInput");
            var mrIpInputParent = GetProp("UiIpInputParent");
            mrIpInput.objectReferenceValue = EditorGUILayout.ObjectField("MR IP Input Prefab", mrIpInput.objectReferenceValue, typeof(IpInputHandler), allowSceneObjects: false);
            mrIpInputParent.objectReferenceValue = EditorGUILayout.ObjectField("Parent for MR input", mrIpInputParent.objectReferenceValue, typeof(IpInputHandler), allowSceneObjects: false);
            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel--;

        var forceUIinput = GetProp("ForceUiInput");
        forceUIinput.boolValue = EditorGUILayout.Toggle(
            new GUIContent(
                "Force UI mode", 
                "Forces the app to use the UI mode (2D screen UI - non MR UI) instead of the MR input methods disregarding the platform."), 
            forceUIinput.boolValue);
        if (forceUIinput.boolValue)
        {
            enableUIInput.boolValue = true;
        }

        var messageBoxService = ((ConfigScene)target).gameObject.GetComponent<MessageBoxService>();
        if(messageBoxService != null)
        {
            messageBoxService.ForceUIMessageBox = forceUIinput.boolValue;
        }


        obj.ApplyModifiedProperties();

    }


    private SerializedProperty GetProp(string name)
    {
        return obj.FindProperty(name);
    }

}
