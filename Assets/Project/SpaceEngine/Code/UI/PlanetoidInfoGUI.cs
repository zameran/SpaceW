using UnityEngine;

public class PlanetoidInfoGUI : DebugGUI
{
    public Planetoid Planetoid;

    private void OnGUI()
    {
        if (Planetoid != null)
        {
            GUI.depth = -100;

            GUILayout.BeginArea(debugInfoBounds);

            GUILayoutExtensions.LabelWithSpace((Planetoid.gameObject.name + ": " + (Planetoid.Working ? "Generating..." : "Idle...")), -10);

            GUILayout.EndArea();
        }
    }
}