using UnityEngine;

public static class GUILayoutExtensions
{
    public static void LabelWithSpace(string text, int space)
    {
        GUILayout.Label(text);
        GUILayout.Space(space);
    }

    public static void LabelWithSpace(GUIContent content, int space)
    {
        GUILayout.Label(content);
        GUILayout.Space(space);
    }

    public static void LabelWithSpace(Texture image, int space)
    {
        GUILayout.Label(image);
        GUILayout.Space(space);
    }
}