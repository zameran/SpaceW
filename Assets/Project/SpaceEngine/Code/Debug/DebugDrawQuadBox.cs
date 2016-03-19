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
        #if UNITY_EDITOR
        if (UnityEditor.SceneView.currentDrawingSceneView != null) return; //Do not draw at Scene tab in editor.
        #endif

        for (int i = 0; i < Planet.Quads.Count; i++)
        {
            Quad q = Planet.Quads[i];

            if (q.Generated && q.ShouldDraw)
            {
                Color lineColor = Color.blue;

                int[,] ORDER = new int[,] { { 1, 0 }, { 2, 3 }, { 0, 2 }, { 3, 1 } };

                Vector3[] verts = q.GetVolumeBox(q.Planetoid.TerrainMaxHeight * 3);

                GL.PushMatrix();
                GL.LoadIdentity();
                GL.MultMatrix(Camera.main.worldToCameraMatrix * q.Planetoid.transform.localToWorldMatrix);
                GL.LoadProjectionMatrix(Camera.main.projectionMatrix);

                lineMaterial.renderQueue = 5000;
                lineMaterial.SetPass(0);

                GL.Begin(GL.LINES);
                GL.Color(lineColor);

                for (int j = 0; j < 4; j++)
                {
                    //Draw bottom quad
                    GL.Vertex3(verts[ORDER[j, 0]].x, verts[ORDER[j, 0]].y, verts[ORDER[j, 0]].z);
                    GL.Vertex3(verts[ORDER[j, 1]].x, verts[ORDER[j, 1]].y, verts[ORDER[j, 1]].z);

                    //Draw top quad
                    GL.Vertex3(verts[ORDER[j, 0] + 4].x, verts[ORDER[j, 0] + 4].y, verts[ORDER[j, 0] + 4].z);
                    GL.Vertex3(verts[ORDER[j, 1] + 4].x, verts[ORDER[j, 1] + 4].y, verts[ORDER[j, 1] + 4].z);

                    //Draw verticals
                    GL.Vertex3(verts[ORDER[j, 0]].x, verts[ORDER[j, 0]].y, verts[ORDER[j, 0]].z);
                    GL.Vertex3(verts[ORDER[j, 0] + 4].x, verts[ORDER[j, 0] + 4].y, verts[ORDER[j, 0] + 4].z);
                }

                GL.End();
                GL.PopMatrix();
            }
        }
    }
}