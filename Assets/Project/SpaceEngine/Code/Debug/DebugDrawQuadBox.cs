using UnityEngine;

public sealed class DebugDrawQuadBox : DebugDraw
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void OnPostRender()
    {
        base.OnPostRender();
    }

    protected override void CreateLineMaterial()
    {
        base.CreateLineMaterial();
    }

    protected override void Draw()
    {
        for (int i = 0; i < Planet.Quads.Count; i++)
        {
            Quad q = Planet.Quads[i];

            if (q.Generated && q.ShouldDraw)
            {
                q.RenderOutline(Planet.LODTarget.GetComponent<Camera>(), lineMaterial);
            }
        }
    }
}