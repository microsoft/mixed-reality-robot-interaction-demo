using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(MessageBoxService))]
public class MessageBoxSerivceEditor : Editor
{
    MessageBoxService script;
    SerializedObject serializedScript;

    private void OnEnable()
    {
        script = (MessageBoxService)target;
        serializedScript = new UnityEditor.SerializedObject(script);
    }

    private static GameObject MRParent;
    private static string CameraServiceName;
    private static bool UseActiveCamera;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.FindProperty("UseActiveCameraAsMRParent").boolValue = 
            EditorGUILayout.Toggle("Use camera from camera service as MR parent?", serializedObject.FindProperty("UseActiveCameraAsMRParent").boolValue);

        EditorGUI.indentLevel++;
        
        if (serializedObject.FindProperty("UseActiveCameraAsMRParent").boolValue)
        {
            if(String.IsNullOrEmpty(serializedObject.FindProperty("CameraServiceName").stringValue))
            {
                serializedObject.FindProperty("CameraServiceName").stringValue = "active_camera_retrieval_service";
            }

            EditorGUILayout.LabelField("Camera Service will be used");
            serializedObject.FindProperty("CameraServiceName").stringValue = EditorGUILayout.TextField("Camera Service Name", serializedObject.FindProperty("CameraServiceName").stringValue);
        }
        else
        {
            EditorGUILayout.LabelField("Please select your target transform for the MR message box");
            serializedObject.FindProperty("MRParent").objectReferenceValue = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("MR Parent", "The object used for MR MessageBoxes. The messageBox will move and behave relative to this parent"),
                (GameObject)serializedObject.FindProperty("MRParent").objectReferenceValue, 
                typeof(GameObject), 
                true) as GameObject;
        }

        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }
}
