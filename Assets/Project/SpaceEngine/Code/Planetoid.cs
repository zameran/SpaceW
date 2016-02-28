using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class Planetoid : MonoBehaviour
{
    [HideInInspector]
    public bool Working = false;

    public bool DrawGizmos = false;

    public Transform LODTarget = null;

    public float PlanetRadius = 1024;

    public bool DebugEnabled = false;
    public bool DebugExtra = false;

    public List<Quad> MainQuads = new List<Quad>();
    public List<Quad> Quads = new List<Quad>();
    public List<Quad> LODQueue = new List<Quad>();

    public Shader ColorShader;
    public ComputeShader CoreShader;

    public int LODMaxLevel = 8;
    public int[] LODDistances = new int[9] { 2048, 1024, 512, 256, 128, 64, 32, 16, 8 };

    private void Start()
    {
        ThreadScheduler.Initialize();
    }

    private void Update()
    {
        if (LODQueue.Count > 6)
            Working = true;
        else
            Working = false;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 250, 50), Working ? "Generating " + (LODQueue.Count - MainQuads.Count) + " more..." : "Ready!");
    }

    [ContextMenu("Dispatch")]
    public void Dispatch()
    {
        float time = Time.realtimeSinceStartup;

        if (Quads.Count == 0 || MainQuads.Count == 0)
            return;

        if (this.GetComponentInChildren<TCCommonParametersSetter>() != null)
            GetComponentInChildren<TCCommonParametersSetter>().UpdateUniforms(false);
        else
            Log("TCCCommon prmeters setter not found in children!");

        for (int i = 0; i < Quads.Count; i++)
        {
            if (Quads[i] != null)
                if (!Quads[i].HaveSubQuads)
                    Quads[i].Dispatch(null);
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
        MainQuads.Clear();
        LODQueue.Clear();
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

    [ContextMenu("ReSetupQuads")]
    public void ReSetupQuads()
    {
        DestroyQuads();
        SetupQuads();
    }

    public void SetupMainQuad(QuadPostion quadPosition)
    {
        GameObject go = new GameObject("Quad" + "_" + quadPosition.ToString());
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.parent = this.transform;

        Mesh mesh = MeshFactory.SetupQuadMesh(QS.nVertsPerEdge);
        //Mesh mesh = MeshFactory.MakePlane(QS.nVertsPerEdge, QS.nVertsPerEdge, MeshFactory.PLANE.XY, true, true);
        mesh.bounds = new Bounds(Vector3.zero, new Vector3(PlanetRadius * 2, PlanetRadius * 2, PlanetRadius * 2));

        Material material = new Material(ColorShader);
        material.name += "_" + quadPosition.ToString() + "(Instance)";

        //MeshFilter mf = go.AddComponent<MeshFilter>();
        //mf.sharedMesh = mesh;

        //MeshRenderer mr = go.AddComponent<MeshRenderer>();
        //mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        //mr.receiveShadows = true;
        //mr.sharedMaterial = material;

        NoiseParametersSetter nps = go.AddComponent<NoiseParametersSetter>();
        nps.ComputeShaderToUpdate = CoreShader;
        nps.MaterialToUpdate = material;

        Quad quadComponent = go.AddComponent<Quad>();
        quadComponent.Setter = nps;
        quadComponent.CoreShader = CoreShader;
        quadComponent.Planetoid = this;
        quadComponent.QuadMesh = mesh;
        quadComponent.QuadMaterial = material;

        QuadGenerationConstants gc = QuadGenerationConstants.Init();
        gc.planetRadius = PlanetRadius;

        gc.cubeFaceEastDirection = quadComponent.GetCubeFaceEastDirection(quadPosition);
        gc.cubeFaceNorthDirection = quadComponent.GetCubeFaceNorthDirection(quadPosition);
        gc.patchCubeCenter = quadComponent.GetPatchCubeCenter(quadPosition);
		
        quadComponent.Position = quadPosition;
        quadComponent.ID = QuadID.One;
        quadComponent.generationConstants = gc;
        quadComponent.Planetoid = this;
        quadComponent.SetupCorners(quadPosition);

        quadComponent.Dispatch(null);

        Quads.Add(quadComponent);
        LODQueue.Add(quadComponent);
        MainQuads.Add(quadComponent);
    }

    public Quad SetupSubQuad(QuadPostion quadPosition)
    {
        GameObject go = new GameObject("Quad" + "_" + quadPosition.ToString());
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;

        Mesh mesh = MeshFactory.SetupQuadMesh(QS.nVertsPerEdge);
        //Mesh mesh = MeshFactory.MakePlane(QS.nVertsPerEdge, QS.nVertsPerEdge, MeshFactory.PLANE.XY, true, true);
        mesh.bounds = new Bounds(Vector3.zero, new Vector3(PlanetRadius * 2, PlanetRadius * 2, PlanetRadius * 2));

        Material material = new Material(ColorShader);
        material.name += "_" + quadPosition.ToString() + "(Instance)";

        //MeshFilter mf = go.AddComponent<MeshFilter>();
        //mf.sharedMesh = mesh;

        //MeshRenderer mr = go.AddComponent<MeshRenderer>();
        //mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        //mr.receiveShadows = true;
        //mr.sharedMaterial = material;

        NoiseParametersSetter nps = go.AddComponent<NoiseParametersSetter>();
        nps.ComputeShaderToUpdate = CoreShader;
        nps.MaterialToUpdate = material;

        Quad quadComponent = go.AddComponent<Quad>();
        quadComponent.Setter = nps;
        quadComponent.CoreShader = CoreShader;
        quadComponent.Planetoid = this;
        quadComponent.QuadMesh = mesh;
        quadComponent.QuadMaterial = material;
        quadComponent.SetupCorners(quadPosition);

        QuadGenerationConstants gc = QuadGenerationConstants.Init();
        gc.planetRadius = PlanetRadius;

        quadComponent.Position = quadPosition;
        quadComponent.generationConstants = gc;
        quadComponent.Planetoid = this;

        //quadComponent.Dispatch();

        Quads.Add(quadComponent);
        LODQueue.Add(quadComponent);

        return quadComponent;
    }

    private void Log(string msg)
    {
        if (DebugEnabled)
            Debug.Log(msg);
    }

    private void Log(string msg, bool state)
    {
        if (state)
            Debug.Log(msg);
    }
}