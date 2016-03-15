using UnityEngine;

public class PlanetoidInfoGUI : DebugGUI
{
    public Planetoid Planetoid;

    private void OnGUI()
    {
        if (Planetoid != null)
        {
            GUI.depth = -100;

            int quadsCount = Planetoid.Quads.Count;
            int quadsCulledCount = Planetoid.GetCulledQuadsCount();
            int vertsRendered = (quadsCount - quadsCulledCount) * QS.nVerts;

            GUILayout.BeginArea(debugInfoBounds);

            GUILayoutExtensions.LabelWithSpace((Planetoid.gameObject.name + ": " + (Planetoid.Working ? "Generating..." : "Idle...")), -10);
            GUILayoutExtensions.LabelWithSpace("Quads count: " + quadsCount, -10);
            GUILayoutExtensions.LabelWithSpace("Quads culled count: " + quadsCulledCount, -10);
            GUILayoutExtensions.LabelWithSpace("Verts rendered per frame (Only Quads): " + vertsRendered, -10);

            GUILayout.EndArea();
        }
    }
}