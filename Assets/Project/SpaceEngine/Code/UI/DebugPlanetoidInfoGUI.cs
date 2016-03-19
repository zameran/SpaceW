using UnityEngine;

public class DebugPlanetoidInfoGUI : DebugGUI
{
    public Planetoid Planetoid;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnGUI()
    {
        base.OnGUI();

        if (Planetoid != null)
        {
            int quadsCount = Planetoid.Quads.Count;
            int quadsCulledCount = Planetoid.GetCulledQuadsCount();
            int vertsRendered = (quadsCount - quadsCulledCount) * QS.nVerts;

            GUILayout.BeginArea(debugInfoBounds);

            GUILayoutExtensions.LabelWithSpace((Planetoid.gameObject.name + ": " + (Planetoid.Working ? "Generating..." : "Idle...")), -10);
            GUILayoutExtensions.LabelWithSpace("Quads count: " + quadsCount, -10);
            GUILayoutExtensions.LabelWithSpace("Quads culled count: " + quadsCulledCount, -10);
            GUILayoutExtensions.LabelWithSpace("Quads culling machine: " + (Planetoid.UseUnityCulling ? "Unity Culling" : "Custom Culling"), -10);
            GUILayoutExtensions.LabelWithSpace("Verts rendered per frame (Only Quads): " + vertsRendered, -10);

            GUILayout.EndArea();
        }
    }
}