using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TMPro;

[CustomEditor(typeof(MsgBoxController))]
public class MsgBoxControllerEditor : Editor
{
    SerializedObject obj;

    public void OnEnable()
    {
        obj = new SerializedObject((MsgBoxController)target);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var isUiContorller = obj.FindProperty("isUiController");

        isUiContorller.boolValue = EditorGUILayout.Toggle("Is UI Controller", isUiContorller.boolValue);
        EditorGUI.indentLevel++;

        bool anyIsNull = false;

        if (!isUiContorller.boolValue)
        {
            anyIsNull |= AddReferenceEditor<TextMeshPro>("HeaderTextMesh", "Header Text Mesh Pro");
            anyIsNull |= AddReferenceEditor<TextMeshPro>("BodyTextMesh", "Body Text Mesh Pro");
            anyIsNull |= AddReferenceEditor<GameObject>("ButtonRight", "Right Button");
            anyIsNull |= AddReferenceEditor<TextMeshPro>("ButtonRightTextMesh", "right button Text Mesh Pro");
            anyIsNull |= AddReferenceEditor<GameObject>("ButtonLeft", "Left Button");
            anyIsNull |= AddReferenceEditor<TextMeshPro>("ButtonLeftTextMesh", "left button Text Mesh Pro");
        }
        else
        {
            anyIsNull |= AddReferenceEditor<TextMeshProUGUI>("HeaderTextMeshUI", "Header Text Mesh Pro UGUI");
            anyIsNull |= AddReferenceEditor<TextMeshProUGUI>("BodyTextMeshUI", "Body Text Mesh Pro UGUI");

            anyIsNull |= AddReferenceEditor<GameObject>("ButtonRight", "Right Button");
            anyIsNull |= AddReferenceEditor<TextMeshProUGUI>("ButtonRightTextMeshUI", "right button Text Mesh Pro UGUI");
            anyIsNull |= AddReferenceEditor<GameObject>("ButtonLeft", "Left Button");
            anyIsNull |= AddReferenceEditor<TextMeshProUGUI>("ButtonLeftTextMeshUI", "left button Text Mesh Pro UGUI");
        }

        EditorGUI.indentLevel--;
        
        if(anyIsNull)
        {
            EditorGUILayout.LabelField("All values should be set!");
        }
        
        obj.ApplyModifiedProperties();
    }


    private bool AddReferenceEditor<T>(string name, string description, string tooltip = "")
    {
        var property = obj.FindProperty(name);

        property.objectReferenceValue = EditorGUILayout.ObjectField(
            new GUIContent(description, tooltip),
            property.objectReferenceValue,
            typeof(T),
            false);

        return property.objectReferenceValue == null;
    }

}
