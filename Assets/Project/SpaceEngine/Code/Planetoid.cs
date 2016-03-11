using UnityEngine;

using System.Collections.Generic;

public class Planetoid : MonoBehaviour
{
	public Proland.Manager manager;

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

	public int RenderQueue = 2000;

	public int DispatchSkipFramesCount = 8;

	public int LODDistanceMultiplier = 1;
	public int LODMaxLevel = 8;
	public int[] LODDistances = new int[11] { 2048, 1024, 512, 256, 128, 64, 32, 16, 8, 4, 2 };

	public Mesh TopPrototypeMesh;
	public Mesh BottomPrototypeMesh;
	public Mesh LeftPrototypeMesh;
	public Mesh RightPrototypeMesh;
	public Mesh FrontPrototypeMesh;
	public Mesh BackPrototypeMesh;

	public QuadStorage Cache = null;
	public NoiseParametersSetter NPS = null;

	public bool UseUnityCulling = true;
	public bool UseLOD = true;
	public bool RenderPerUpdate = false;
	public bool OneSplittingQuad = true;

	public float TerrainMaxHeight = 64.0f;

	public Vector3 Origin = Vector3.zero;

	private void Awake()
	{
		Origin = transform.position;
		if (manager != null) manager.origin = Origin;
	}

	private void Start()
	{
		ThreadScheduler.Initialize();

		if (Cache == null)
			if (this.gameObject.GetComponentInChildren<QuadStorage>() != null)
				Cache = this.gameObject.GetComponentInChildren<QuadStorage>();

		if (NPS != null)
			NPS.LoadAndInit();
	}

	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.F1))
		{
			DrawWireframe = !DrawWireframe;
		}

		CheckCutoff();

		Origin = transform.position;
		if (manager != null) manager.origin = Origin;
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

		if (TopPrototypeMesh != null) DestroyImmediate(TopPrototypeMesh);
		if (BottomPrototypeMesh != null) DestroyImmediate(BottomPrototypeMesh);
		if (LeftPrototypeMesh != null) DestroyImmediate(LeftPrototypeMesh);
		if (RightPrototypeMesh != null) DestroyImmediate(RightPrototypeMesh);
		if (FrontPrototypeMesh != null) DestroyImmediate(FrontPrototypeMesh);
		if (BackPrototypeMesh != null) DestroyImmediate(BackPrototypeMesh);
	}

	[ContextMenu("SetupMeshes")]
	public void SetupMeshes()
	{
		if (TopPrototypeMesh != null) DestroyImmediate(TopPrototypeMesh);
		if (BottomPrototypeMesh != null) DestroyImmediate(BottomPrototypeMesh);
		if (LeftPrototypeMesh != null) DestroyImmediate(LeftPrototypeMesh);
		if (RightPrototypeMesh != null) DestroyImmediate(RightPrototypeMesh);
		if (FrontPrototypeMesh != null) DestroyImmediate(FrontPrototypeMesh);
		if (BackPrototypeMesh != null) DestroyImmediate(BackPrototypeMesh);

		//bool uv_01 = true;
		bool invert = false;

		TopPrototypeMesh = MeshFactory.SetupQuadMesh(QS.nVertsPerEdge, MeshFactory.PLANE.XZ, !invert); //MeshFactory.MakePlane(QS.nVertsPerEdge, QS.nVertsPerEdge, MeshFactory.PLANE.XZ, uv_01, false, !invert);

		BottomPrototypeMesh = MeshFactory.SetupQuadMesh(QS.nVertsPerEdge, MeshFactory.PLANE.XZ, invert); //MeshFactory.MakePlane(QS.nVertsPerEdge, QS.nVertsPerEdge, MeshFactory.PLANE.XZ, uv_01, false, invert);

		LeftPrototypeMesh = MeshFactory.SetupQuadMesh(QS.nVertsPerEdge, MeshFactory.PLANE.YZ, !invert); //MeshFactory.MakePlane(QS.nVertsPerEdge, QS.nVertsPerEdge, MeshFactory.PLANE.YZ, uv_01, false, !invert);

		RightPrototypeMesh = MeshFactory.SetupQuadMesh(QS.nVertsPerEdge, MeshFactory.PLANE.YZ, invert); //MeshFactory.MakePlane(QS.nVertsPerEdge, QS.nVertsPerEdge, MeshFactory.PLANE.YZ, uv_01, false, invert);

		FrontPrototypeMesh = MeshFactory.SetupQuadMesh(QS.nVertsPerEdge, MeshFactory.PLANE.XY, !invert); //MeshFactory.MakePlane(QS.nVertsPerEdge, QS.nVertsPerEdge, MeshFactory.PLANE.XY, uv_01, false, !invert);

		BackPrototypeMesh = MeshFactory.SetupQuadMesh(QS.nVertsPerEdge, MeshFactory.PLANE.XY, invert); //MeshFactory.MakePlane(QS.nVertsPerEdge, QS.nVertsPerEdge, MeshFactory.PLANE.XY, uv_01, false, invert);
	}

	public Mesh GetMesh(QuadPostion position)
	{
		Mesh temp = null;

		switch(position)
		{
			case QuadPostion.Top:
				temp = TopPrototypeMesh;
				break;
			case QuadPostion.Bottom:
				temp = BottomPrototypeMesh;
				break;
			case QuadPostion.Left:
				temp = LeftPrototypeMesh;
				break;
			case QuadPostion.Right:
				temp = RightPrototypeMesh;
				break;
			case QuadPostion.Front:
				temp = FrontPrototypeMesh;
				break;
			case QuadPostion.Back:
				temp = BackPrototypeMesh;
				break;
		}

		return temp;
	}

	[ContextMenu("SetupQuads")]
	public void SetupQuads()
	{
		if (Quads.Count > 0)
			return;

		SetupMeshes();

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

		Mesh mesh = GetMesh(quadPosition);
		mesh.bounds = new Bounds(Vector3.zero, new Vector3(PlanetRadius * 2, PlanetRadius * 2, PlanetRadius * 2));

		Material material = new Material(ColorShader);
		material.name += "_" + quadPosition.ToString() + "(Instance)" + "_" + Random.Range(float.MinValue, float.MaxValue);

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

		Mesh mesh = GetMesh(quadPosition);
		mesh.bounds = new Bounds(Vector3.zero, new Vector3(PlanetRadius * 2, PlanetRadius * 2, PlanetRadius * 2));

		Material material = new Material(ColorShader);
		material.name += "_" + quadPosition.ToString() + "(Instance)" + "_" + Random.Range(float.MinValue, float.MaxValue);

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