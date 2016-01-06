using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Vector4f))]
public class Vector4fPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        Rect xRect = new Rect(position.x + 0, position.y, 75, position.height);
        Rect yRect = new Rect(position.x + 75, position.y, 75, position.height);
        Rect zRect = new Rect(position.x + 150, position.y, 75, position.height);
        Rect wRect = new Rect(position.x + 225, position.y, 75, position.height);

        EditorGUI.BeginProperty(position, new GUIContent(""), property);
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(xRect, property.FindPropertyRelative("X"), GUIContent.none);
        EditorGUI.PropertyField(yRect, property.FindPropertyRelative("Y"), GUIContent.none);
        EditorGUI.PropertyField(zRect, property.FindPropertyRelative("Z"), GUIContent.none);
        EditorGUI.PropertyField(wRect, property.FindPropertyRelative("W"), GUIContent.none);
        EditorGUI.EndChangeCheck();
        EditorGUI.EndProperty();
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}