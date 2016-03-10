using UnityEngine;

public class PlanetoidInfoGUI : MonoBehaviour
{
    public Planetoid Planetoid;

    public Rect debugInfoBounds = new Rect(10, 10, 500, 500);

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