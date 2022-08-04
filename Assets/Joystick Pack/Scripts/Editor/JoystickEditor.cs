using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Joystick), true)]
public class JoystickEditor : Editor
{
    private SerializedProperty m_HandleRange;
    private SerializedProperty m_DeadZone;
    private SerializedProperty m_AxisOptions;
    private SerializedProperty m_SnapX;
    private SerializedProperty m_SnapY;
    protected SerializedProperty m_Background;
    private SerializedProperty m_Handle;

    protected Vector2 center = new Vector2(0.5f, 0.5f);

    protected virtual void OnEnable()
    {
        m_HandleRange = serializedObject.FindProperty("m_HandleRange");
        m_DeadZone = serializedObject.FindProperty("m_DeadZone");
        m_AxisOptions = serializedObject.FindProperty("m_AxisOptions");
        m_SnapX = serializedObject.FindProperty("m_SnapX");
        m_SnapY = serializedObject.FindProperty("m_SnapY");
        m_Background = serializedObject.FindProperty("m_Background");
        m_Handle = serializedObject.FindProperty("m_Handle");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawValues();
        EditorGUILayout.Space();
        DrawComponents();

        serializedObject.ApplyModifiedProperties();

        if (m_Handle != null)
        {
            RectTransform handleRect = (RectTransform)m_Handle.objectReferenceValue;
            handleRect.anchorMax = center;
            handleRect.anchorMin = center;
            handleRect.pivot = center;
            handleRect.anchoredPosition = Vector2.zero;
        }
    }

    protected virtual void DrawValues()
    {
        EditorGUILayout.PropertyField(m_HandleRange,
            new GUIContent("Handle Range", "The distance the visual handle can move from the center of the joystick."));
        EditorGUILayout.PropertyField(m_DeadZone,
            new GUIContent("Dead Zone", "The distance away from the center input has to be before registering."));
        EditorGUILayout.PropertyField(m_AxisOptions, new GUIContent("Axis Options", "Which axes the joystick uses."));
        EditorGUILayout.PropertyField(m_SnapX, new GUIContent("Snap X", "Snap the horizontal input to a whole value."));
        EditorGUILayout.PropertyField(m_SnapY, new GUIContent("Snap Y", "Snap the vertical input to a whole value."));
    }

    protected virtual void DrawComponents()
    {
        EditorGUILayout.ObjectField(m_Background, new GUIContent("Background", "The background's RectTransform component."));
        EditorGUILayout.ObjectField(m_Handle, new GUIContent("Handle", "The handle's RectTransform component."));
    }
}