using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FloatingJoystick))]
public class FloatingJoystickEditor : JoystickEditor
{
    private SerializedProperty m_IsFixed;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_IsFixed = serializedObject.FindProperty("m_IsFixed");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (m_Background != null)
        {
            RectTransform backgroundRect = (RectTransform)m_Background.objectReferenceValue;
            backgroundRect.anchorMax = Vector2.zero;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.pivot = center;
        }
    }

    protected override void DrawValues()
    {
        EditorGUILayout.PropertyField(m_IsFixed, new GUIContent("Is Fixed", ""));

        base.DrawValues();
    }
}