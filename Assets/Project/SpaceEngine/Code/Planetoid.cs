using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class Planetoid : MonoBehaviour
{
    public float PlanetRadius = 1024;

    public bool DebugEnabled = false;

    public List<Quad> Quads = new List<Quad>();

    public Shader ColorShader;
    public ComputeShader HeightShader;

    [ContextMenu("Dispatch")]
    public void Dispatch()
    {
        float time = Time.realtimeSinceStartup;

        if (Quads.Count == 0)
            return;

        for (int i = 0; i < Quads.Count; i++)
        {
            if (Quads[i] != null)
                if (Quads[i].Subquads.Count == 0)
                {
                    Quads[i].Dispatch();
                }
        }

        Log("Planet dispatched in " + (Time.realtimeSinceStartup - time).ToString() + "ms");
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

        SetupMainQuad(QuadPostion.Top);
        SetupMainQuad(QuadPostion.Bottom);
        SetupMainQuad(QuadPostion.Left);
        SetupMainQuad(QuadPostion.Right);
        SetupMainQuad(QuadPostion.Front);
        SetupMainQuad(QuadPostion.Back);
    }

    public void SetupMainQuad(QuadPostion quadPosition)
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

        Quad quadComponent = go.AddComponent<Quad>();
        quadComponent.Setter = nps;
        quadComponent.HeightShader = HeightShader;
        quadComponent.Planetoid = this;

        QuadGenerationConstants gc = QuadGenerationConstants.Init();
        gc.planetRadius = PlanetRadius;

        gc.cubeFaceEastDirection = quadComponent.GetCubeFaceEastDirection(quadPosition);
        gc.cubeFaceNorthDirection = quadComponent.GetCubeFaceNorthDirection(quadPosition);
        gc.patchCubeCenter = quadComponent.GetPatchCubeCenter(quadPosition);

        quadComponent.Position = quadPosition;
        quadComponent.quadGenerationConstants = gc;
        quadComponent.Planetoid = this;

        mf.sharedMesh = MeshFactory.SetupQuadMesh();
        mf.sharedMesh.bounds = new Bounds(Vector3.zero, new Vector3(PlanetRadius * 2, PlanetRadius * 2, PlanetRadius * 2));

        quadComponent.Dispatch();

        Quads.Add(quadComponent);
    }

    public Quad SetupSubQuad(QuadPostion quadPosition)
    {
        GameObject go = new GameObject("Quad" + "_" + quadPosition.ToString());
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;

        MeshFilter mf = go.AddComponent<MeshFilter>();

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        mr.receiveShadows = true;
        mr.sharedMaterial = new Material(ColorShader);
        mr.sharedMaterial.name += "_" + quadPosition.ToString() + "(Instance)";

        NoiseParametersSetter nps = go.AddComponent<NoiseParametersSetter>();
        nps.ComputeShaderToUpdate = HeightShader;
        nps.MaterialToUpdate = mr.sharedMaterial;

        Quad quadComponent = go.AddComponent<Quad>();
        quadComponent.Setter = nps;
        quadComponent.HeightShader = HeightShader;
        quadComponent.Planetoid = this;

        QuadGenerationConstants gc = QuadGenerationConstants.Init();
        gc.planetRadius = PlanetRadius;

        quadComponent.Position = quadPosition;
        quadComponent.quadGenerationConstants = gc;
        quadComponent.Planetoid = this;

        mf.sharedMesh = MeshFactory.SetupQuadMesh();
        mf.sharedMesh.bounds = new Bounds(Vector3.zero, new Vector3(PlanetRadius * 2, PlanetRadius * 2, PlanetRadius * 2));

        quadComponent.Dispatch();

        Quads.Add(quadComponent);

        return quadComponent;
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