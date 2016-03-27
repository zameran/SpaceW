using UnityEngine;
using System.Collections;

public sealed class DebugDrawFrustumView : DebugDraw
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

        Vector3[] nearCorners = new Vector3[4]; //Approx'd nearplane corners
        Vector3[] farCorners = new Vector3[4]; //Approx'd farplane corners
        Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main); //get planes from matrix

        Plane temp = camPlanes[1]; camPlanes[1] = camPlanes[2]; camPlanes[2] = temp; //swap [1] and [2] so the order is better for the loop

        GL.PushMatrix();
        GL.LoadIdentity();
        GL.MultMatrix(Camera.main.worldToCameraMatrix);
        GL.LoadProjectionMatrix(Camera.main.projectionMatrix);

        lineMaterial.renderQueue = 5000;
        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);

        for (int i = 0; i < 4; i++)
        {
            nearCorners[i] = VectorHelper.Plane3Intersect(camPlanes[4], camPlanes[i], camPlanes[(i + 1) % 4]); //near corners on the created projection matrix
            farCorners[i] = VectorHelper.Plane3Intersect(camPlanes[5], camPlanes[i], camPlanes[(i + 1) % 4]); //far corners on the created projection matrix
        }

        for (int i = 0; i < 4; i++)
        {
            GL.Color(Color.red);
            GL.Vertex3(nearCorners[i].x, nearCorners[i].y, nearCorners[i].z);
            GL.Vertex3(nearCorners[(i + 1) % 4].x, nearCorners[(i + 1) % 4].y, nearCorners[(i + 1) % 4].z);

            GL.Color(Color.blue);
            GL.Vertex3(farCorners[i].x, farCorners[i].y, farCorners[i].z);
            GL.Vertex3(farCorners[(i + 1) % 4].x, farCorners[(i + 1) % 4].y, farCorners[(i + 1) % 4].z);

            GL.Color(Color.green);
            GL.Vertex3(nearCorners[i].x, nearCorners[i].y, nearCorners[i].z);
            GL.Vertex3(farCorners[i].x, farCorners[i].y, farCorners[i].z);
        }

        GL.End();
        GL.PopMatrix();
    }
}