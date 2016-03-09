using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class Planetoid : MonoBehaviour
{
	public bool DrawWireframe = false;

	public bool Working = false;

	public Transform LODTarget = null;

	public float PlanetRadius = 1024;

	public bool DebugEnabled = false;
	public bool DebugExtra = false;

	public List<Quad> MainQuads = new List<Quad>();
	public List<Quad> Quads = new List<Quad>();

	public Shader ColorShader;
	public ComputeShader CoreShader;

	public int DispatchSkipFramesCount = 8;

    public int LODDistanceMultiplier = 1;
	public int LODMaxLevel = 8;
	public int[] LODDistances = new int[9] { 2048, 1024, 512, 256, 128, 64, 32, 16, 8 };

	public Mesh PrototypeMesh;

	public QuadStorage Cache = null;
	public NoiseParametersSetter NPS = null;

	public bool UseUnityCulling = true;
	public bool UseLOD = true;
	public bool RenderPerUpdate = false;

	public float TerrainMaxHeight = 64.0f;

	private void Start()
	{
		ThreadScheduler.Initialize();

		if (Cache == null)
			if (this.gameObject.GetComponentInChildren<QuadStorage>() != null)
				Cache = this.gameObject.GetComponentInChildren<QuadStorage>();

		if (NPS != null)
			NPS.LoadAndInit();

		if (PrototypeMesh == null)
			PrototypeMesh = MeshFactory.SetupQuadMesh(QS.nVertsPerEdge);
	}

	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.F1))
		{
			DrawWireframe = !DrawWireframe;
		}

		CheckCutoff();
	}

	private void OnGUI()
	{

	}

	private void OnRenderObject()
	{

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

		DestroyImmediate(PrototypeMesh);
	}

	[ContextMenu("SetupQuads")]
	public void SetupQuads()
	{
		if (Quads.Count > 0)
			return;

		if (PrototypeMesh == null)
			PrototypeMesh = MeshFactory.SetupQuadMesh(QS.nVertsPerEdge);

		SetupMainQuad(QuadPostion.Top);
		SetupMainQuad(QuadPostion.Bottom);
		SetupMainQuad(QuadPostion.Left);
		SetupMainQuad(QuadPostion.Right);
		SetupMainQuad(QuadPostion.Front);
		SetupMainQuad(QuadPostion.Back);

		if (NPS != null)
			NPS.LoadAndInit();
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

		Mesh mesh = PrototypeMesh;
		mesh.bounds = new Bounds(Vector3.zero, new Vector3(PlanetRadius * 2, PlanetRadius * 2, PlanetRadius * 2));

		Material material = new Material(ColorShader);
		material.name += "_" + quadPosition.ToString() + "(Instance)";

		Quad quadComponent = go.AddComponent<Quad>();
		quadComponent.CoreShader = CoreShader;
		quadComponent.Planetoid = this;
		quadComponent.QuadMesh = mesh;
		quadComponent.QuadMaterial = material;

		QuadGenerationConstants gc = QuadGenerationConstants.Init(TerrainMaxHeight);
		gc.planetRadius = PlanetRadius;

		gc.cubeFaceEastDirection = quadComponent.GetCubeFaceEastDirection(quadPosition);
		gc.cubeFaceNorthDirection = quadComponent.GetCubeFaceNorthDirection(quadPosition);
		gc.patchCubeCenter = quadComponent.GetPatchCubeCenter(quadPosition);
		
		quadComponent.Position = quadPosition;
		quadComponent.ID = QuadID.One;
		quadComponent.generationConstants = gc;
		quadComponent.Planetoid = this;
		quadComponent.SetupCorners(quadPosition);
		quadComponent.ShouldDraw = true;
		quadComponent.ReadyForDispatch = true;

		Quads.Add(quadComponent);
		MainQuads.Add(quadComponent);
	}

	public Quad SetupSubQuad(QuadPostion quadPosition)
	{
		GameObject go = new GameObject("Quad" + "_" + quadPosition.ToString());
		go.transform.position = Vector3.zero;
		go.transform.rotation = Quaternion.identity;

		Mesh mesh = PrototypeMesh;
		mesh.bounds = new Bounds(Vector3.zero, new Vector3(PlanetRadius * 2, PlanetRadius * 2, PlanetRadius * 2));

		Material material = new Material(ColorShader);
		material.name += "_" + quadPosition.ToString() + "(Instance)";

		Quad quadComponent = go.AddComponent<Quad>();
		quadComponent.CoreShader = CoreShader;
		quadComponent.Planetoid = this;
		quadComponent.QuadMesh = mesh;
		quadComponent.QuadMaterial = material;
		quadComponent.SetupCorners(quadPosition);

		QuadGenerationConstants gc = QuadGenerationConstants.Init(TerrainMaxHeight);
		gc.planetRadius = PlanetRadius;

		quadComponent.Position = quadPosition;
		quadComponent.generationConstants = gc;
		quadComponent.Planetoid = this;
		quadComponent.ShouldDraw = false;

		Quads.Add(quadComponent);

		return quadComponent;
	}

	public void CheckCutoff()
	{
		//Prevent fast jumping of lod distances check and working state.
		if(Vector3.Distance(LODTarget.transform.position, this.transform.position) > this.PlanetRadius * 2 + LODDistances[0])
		{
			for (int i = 0; i < Quads.Count; i++)
			{
				Quads[i].StopAllCoroutines();
			}

			this.Working = false;
		}
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