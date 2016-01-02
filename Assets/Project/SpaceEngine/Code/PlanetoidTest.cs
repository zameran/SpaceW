using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class PlanetoidTest : MonoBehaviour
{
    public float PlanetRadius = 1024;

    public bool DebugEnabled = false;

    public List<QuadTest> Quads = new List<QuadTest>();

    public Shader ColorShader;
    public ComputeShader HeightShader;

    [ContextMenu("Dispatch")]
    public void Dispatch()
    {
        if (Quads.Count == 0)
            return;

        for (int i = 0; i < Quads.Count; i++)
        {
            if (Quads[i] != null)
                Quads[i].Dispatch();
        }
    }

    [ContextMenu("DestroyQuads")]
    public void DestroyQuads()
    {
        for (int i = 0; i < Quads.Count; i++)
        {
            if(Quads[i] != null)
                DestroyImmediate(Quads[i].gameObject);
        }

        Quads.Clear();
    }

    [ContextMenu("SetupQuads")]
    public void SetupQuads()
    {
        if (Quads.Count > 0)
            return;

        SetupQuad(QuadPostion.Top);
        SetupQuad(QuadPostion.Bottom);
        SetupQuad(QuadPostion.Left);
        SetupQuad(QuadPostion.Right);
        SetupQuad(QuadPostion.Front);
        SetupQuad(QuadPostion.Back);
    }

    public void SetupQuad(QuadPostion quadPosition)
    {
        GameObject go = new GameObject("Quad" + "_" + quadPosition.ToString());
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.parent = this.transform;

        MeshFilter mf = go.AddComponent<MeshFilter>();

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        mr.receiveShadows = true;
        mr.sharedMaterial = new Material(ColorShader);
        mr.sharedMaterial.name += "_" + quadPosition.ToString() + "(Instance)";

        NoiseParametersSetter nps = go.AddComponent<NoiseParametersSetter>();
        nps.ComputeShaderToUpdate = HeightShader;
        nps.MaterialToUpdate = mr.sharedMaterial;

        QuadTest quadComponent = go.AddComponent<QuadTest>();
        quadComponent.Setter = nps;
        quadComponent.HeightShader = HeightShader;
        quadComponent.Planetoid = this;

        GenerationConstants gc = GenerationConstants.Init();
        gc.planetRadius = PlanetRadius;

        switch (quadPosition)
        {
            case QuadPostion.Top:
                gc.cubeFaceEastDirection = new Vector3(0, 0, -PlanetRadius);
                gc.cubeFaceNorthDirection = new Vector3(PlanetRadius, 0, 0);
                gc.patchCubeCenter = new Vector3(0, PlanetRadius, 0);
                break;
            case QuadPostion.Bottom:
                gc.cubeFaceEastDirection = new Vector3(0, 0, -PlanetRadius);
                gc.cubeFaceNorthDirection = new Vector3(-PlanetRadius, 0, 0);
                gc.patchCubeCenter = new Vector3(0, -PlanetRadius, 0);
                break;
            case QuadPostion.Left:
                gc.cubeFaceEastDirection = new Vector3(0, -PlanetRadius, 0);
                gc.cubeFaceNorthDirection = new Vector3(0, 0, -PlanetRadius);
                gc.patchCubeCenter = new Vector3(-PlanetRadius, 0, 0);
                break;
            case QuadPostion.Right:
                gc.cubeFaceEastDirection = new Vector3(0, -PlanetRadius, 0);
                gc.cubeFaceNorthDirection = new Vector3(0, 0, PlanetRadius);
                gc.patchCubeCenter = new Vector3(PlanetRadius, 0, 0);
                break;
            case QuadPostion.Front:
                gc.cubeFaceEastDirection = new Vector3(PlanetRadius, 0, 0);
                gc.cubeFaceNorthDirection = new Vector3(0, -PlanetRadius, 0);
                gc.patchCubeCenter = new Vector3(0, 0, PlanetRadius);
                break;
            case QuadPostion.Back:
                gc.cubeFaceEastDirection = new Vector3(-PlanetRadius, 0, 0);
                gc.cubeFaceNorthDirection = new Vector3(0, -PlanetRadius, 0);
                gc.patchCubeCenter = new Vector3(0, 0, -PlanetRadius);
                break;
        }

        quadComponent.generationConstants = new GenerationConstants[] { gc };
        quadComponent.Planetoid = this;

        mf.sharedMesh = MeshFactory.SetupQuadMesh();
        mf.sharedMesh.bounds = new Bounds(Vector3.zero, new Vector3(PlanetRadius * 2, PlanetRadius * 2, PlanetRadius * 2));

        quadComponent.Dispatch();

        Quads.Add(quadComponent);
    }

    private void Log(string msg)
    {
        if (DebugEnabled)
            Debug.Log(msg);
    }
}

public enum QuadPostion
{
    Top,
    Bottom,
    Left,
    Right,
    Front,
    Back
}