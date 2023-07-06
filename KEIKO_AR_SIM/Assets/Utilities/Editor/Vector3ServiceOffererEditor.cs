using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Vector3ServiceOfferer))]
public class Vector3ServiceOffererEditor : Editor
{

    SerializedObject obj;

    private void OnEnable()
    {
        obj = new SerializedObject(target);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var type = obj.FindProperty("VectorSourceType");

        Vector3ServiceOfferer.Vector3PublisherType t = (Vector3ServiceOfferer.Vector3PublisherType)type.enumValueIndex;

        t = (Vector3ServiceOfferer.Vector3PublisherType)EditorGUILayout.EnumPopup("Vector3 Type to Publish", t);

        type.enumValueIndex = (int)t;

        EditorGUI.indentLevel++;

        switch (t)
        {
            case Vector3ServiceOfferer.Vector3PublisherType.Transform:
                var component = obj.FindProperty("ComponentToPublish");
                var transform = obj.FindProperty("transformToPublish");

                Vector3ServiceOfferer.TransformComponent c = (Vector3ServiceOfferer.TransformComponent)component.enumValueIndex;
                c = (Vector3ServiceOfferer.TransformComponent)EditorGUILayout.EnumPopup("Transform component:", c);
                component.enumValueIndex = (int)c;
                transform.objectReferenceValue = EditorGUILayout.ObjectField("Transform", transform.objectReferenceValue, typeof(Transform), true);

                break;
            case Vector3ServiceOfferer.Vector3PublisherType.Vector3:
                var vector = obj.FindProperty("InspectorVector");
                vector.vector3Value = EditorGUILayout.Vector3Field("Vector", vector.vector3Value);
                break;
        }

        EditorGUI.indentLevel--;

        obj.ApplyModifiedProperties();
    }
}
