using UnityEngine;

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using ZFramework.Math;
using ZFramework.Unity.Common.PerfomanceMonitor;

[Serializable]
public struct QuadGenerationConstants
{
	public float planetRadius; //4
	public float spacing; //4
	public float spacingreal;
	public float spacingsub;
	public float terrainMaxHeight; //4
	public float LODLevel; //4
	public float orientation;

	public Vector3 cubeFaceEastDirection; //12
	public Vector3 cubeFaceNorthDirection; //12
	public Vector3 patchCubeCenter; //12

	//12 * 3 + 4 * 3 = 36 + 12 = 48

	public static QuadGenerationConstants Init()
	{
		QuadGenerationConstants temp = new QuadGenerationConstants();

		temp.spacing = QS.nSpacing;
		temp.spacingreal = QS.nSpacingReal;
		temp.spacingsub = QS.nSpacingSub;
		temp.terrainMaxHeight = 64.0f;

		return temp;
	}

	public static QuadGenerationConstants Init(float terrainMaxHeight)
	{
		QuadGenerationConstants temp = new QuadGenerationConstants();

		temp.spacing = QS.nSpacing;
		temp.spacingreal = QS.nSpacingReal;
		temp.spacingsub = QS.nSpacingSub;
		temp.terrainMaxHeight = terrainMaxHeight;

		return temp;
	}
}

[Serializable]
public struct OutputStruct
{
	public float noise;

	public Vector3 patchCenter;

	public Vector4 vcolor;
	public Vector4 pos;
	public Vector4 cpos;
}

public class Quad : MonoBehaviour
{
	[Serializable]
	public class Id
	{
		public int LODLevel;
		public int ID;
		public int Position;

		public Id(int LODLevel, int ID, int Position)
		{
			this.LODLevel = LODLevel;
			this.ID = ID;
			this.Position = Position;
		}

		public int Compare(Id id)
		{
			return LODLevel.CompareTo(id.LODLevel);
		}

		public bool Equals(Id id)
		{
			if (ReferenceEquals(this, id))
			{
				return true;
			}

			if ((this == null) || (id == null))
			{
				return false;
			}

			return (this.LODLevel == id.LODLevel && this.ID == id.ID && this.Position == id.Position);
		}

		public override int GetHashCode()
		{
			int code = LODLevel ^ ID ^ Position;
			return code.GetHashCode();
		}

		public override string ToString()
		{
			return LODLevel.ToString() + "," + ID.ToString() + "," + Position.ToString();
		}
	}

	public class EqualityComparerID : IEqualityComparer<Id>
	{
		public bool Equals(Id t1, Id t2)
		{
			return t1.Equals(t2);
		}

		public int GetHashCode(Id t)
		{
			return t.GetHashCode();
		}
	}

	public QuadPostion Position;
	public QuadID ID;

	public Planetoid Planetoid;

	public ComputeShader CoreShader;

	public Mesh QuadMesh;
	public Material QuadMaterial;

	public ComputeBuffer QuadGenerationConstantsBuffer;
	public ComputeBuffer PreOutDataBuffer;
	public ComputeBuffer PreOutDataSubBuffer;
	public ComputeBuffer OutDataBuffer;

	public RenderTexture HeightTexture;
	public RenderTexture NormalTexture;

	public QuadGenerationConstants generationConstants;

	public Quad Parent;

	public List<Quad> Subquads = new List<Quad>();

	public int LODLevel = -1;

	public bool HaveSubQuads = false;
	public bool Generated = false;
	public bool ShouldDraw = false;
	public bool ReadyForDispatch = false;
	public bool Splitting = false;
	public bool Unsplitted = false;
	public bool Visible = false;

	public float lodUpdateInterval = 0.25f;
	public float lastLodUpdateTime = 0.00f;

	public Vector3 topLeftCorner;
	public Vector3 bottomRightCorner;
	public Vector3 topRightCorner;
	public Vector3 bottomLeftCorner;
	public Vector3 middleNormalized;

	public delegate void QuadDelegate(Quad q);
	public event QuadDelegate DispatchStarted, DispatchReady, GPUGetDataReady;

	public Quad.Id GetId()
	{
		return new Quad.Id(LODLevel, (int)ID, (int)Position);
	}

	private void QuadDispatchStarted(Quad q)
	{
		Log("DispatchStarted event fire!");
	}

	private void QuadDispatchReady(Quad q)
	{
		Log("DispatchReady event fire!");
	}

	private void QuadGPUGetDataReady(Quad q)
	{
		Log("GPUGetDataReady event fire!");
	}

	public Quad()
	{
		DispatchStarted += QuadDispatchStarted;
		DispatchReady += QuadDispatchReady;
		GPUGetDataReady += QuadGPUGetDataReady;
	}

	private void Awake()
	{
		QuadGenerationConstantsBuffer = new ComputeBuffer(1, 64);
		PreOutDataBuffer = new ComputeBuffer(QS.nVertsReal, 64);
		PreOutDataSubBuffer = new ComputeBuffer(QS.nRealVertsSub, 64);
		OutDataBuffer = new ComputeBuffer(QS.nVerts, 64);
		
		HeightTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdgeSub, 0);
		NormalTexture = RTExtensions.CreateRTexture(QS.nVertsPerEdgeSub, 0);

		RTUtility.ClearColor(new RenderTexture[] { HeightTexture, NormalTexture });
	}

	private void Start()
	{

	}

	private void Update()
	{
		if (Planetoid.RenderPerUpdate)
			Render();

		if (Time.time > lastLodUpdateTime + lodUpdateInterval && Planetoid.UseLOD)
		{
			lastLodUpdateTime = Time.time;

			if (LODLevel < Planetoid.LODMaxLevel)
			{
				if (!Planetoid.OneSplittingQuad)
				{
					if (Generated && !HaveSubQuads)
					{
						if (GetDistanceToClosestCorner() < Planetoid.LODDistances[LODLevel + 1] * Planetoid.LODDistanceMultiplier && !Splitting)
							StartCoroutine(Split());
					}
					else
					{
						if (GetDistanceToClosestCorner() > Planetoid.LODDistances[LODLevel + 1] * Planetoid.LODDistanceMultiplier && !Splitting)
							Unsplit();
					}
				}
				else
				{
					if (Generated && !HaveSubQuads && !Planetoid.Working)
					{
						if (GetDistanceToClosestCorner() < Planetoid.LODDistances[LODLevel + 1] * Planetoid.LODDistanceMultiplier && !Splitting)
							StartCoroutine(Split());
					}
					else
					{
						if (GetDistanceToClosestCorner() > Planetoid.LODDistances[LODLevel + 1] * Planetoid.LODDistanceMultiplier && !Splitting)
							Unsplit();
					}
				}
			}
		}
	}

	private void OnDestroy()
	{
		BufferHelper.ReleaseAndDisposeBuffers(QuadGenerationConstantsBuffer, PreOutDataBuffer, PreOutDataSubBuffer, OutDataBuffer);

		if (HeightTexture != null)
			HeightTexture.ReleaseAndDestroy();

		if (NormalTexture != null)
			NormalTexture.ReleaseAndDestroy();

		//if (QuadMesh != null)
		//    DestroyImmediate(QuadMesh);

		if (QuadMaterial != null)
			DestroyImmediate(QuadMaterial);

		if (DispatchStarted != null)
			DispatchStarted -= QuadDispatchStarted;

		if (DispatchReady != null)
			DispatchReady -= QuadDispatchReady;

		if (GPUGetDataReady != null)
			GPUGetDataReady -= QuadGPUGetDataReady;
	}

	private void OnWillRenderObject()
	{

	}

	private void OnRenderObject()
	{
		if (!Planetoid.RenderPerUpdate)
			Render();
	}

	private void OnDrawGizmos()
	{
		//Gizmos.color = Color.blue;
		//Gizmos.DrawWireCube(QuadMesh.bounds.center, QuadMesh.bounds.size);
	}

	public void Render()
	{
		if (ReadyForDispatch)
		{
			if (!Generated)
				Dispatch();
		}

		SetupBounds(this, QuadMesh);

		Planetoid.Atmosphere.SetUniformsForPlanetQuad(QuadMaterial);

		QuadMaterial.SetBuffer("data", OutDataBuffer);
		QuadMaterial.SetBuffer("quadGenerationConstants", QuadGenerationConstantsBuffer);
		QuadMaterial.SetTexture("_HeightTexture", HeightTexture);
		QuadMaterial.SetTexture("_NormalTexture", NormalTexture);
		QuadMaterial.SetFloat("_Wireframe", Planetoid.DrawWireframe ? 1.0f : 0.0f);
		QuadMaterial.SetFloat("_Normale", Planetoid.DrawNormals ? 1.0f : 0.0f);
		QuadMaterial.SetFloat("_Side", (float)Position);
		QuadMaterial.SetVector("_Rotation", Planetoid.QuadsRoot.transform.rotation.eulerAngles * Mathf.Deg2Rad);
		QuadMaterial.SetVector("_Origin", Planetoid.Origin);
		QuadMaterial.renderQueue = Planetoid.RenderQueue;
		QuadMaterial.SetPass(0);

		if (Generated && ShouldDraw)
		{
			Visible = PlaneFrustumCheck(Camera.main);

			if (QuadMesh != null)
			{
				if (Planetoid.RenderPerUpdate)
					Graphics.DrawMesh(QuadMesh, Planetoid.Origin, Planetoid.OriginRotation, QuadMaterial, 0, Camera.main, 0, null, true, true);
				else
				{
					if (Visible)
						Graphics.DrawMeshNow(QuadMesh, Planetoid.Origin, Planetoid.OriginRotation);
				}
			}
		}
	}

	public Vector3[] GetFlatBox(float offset = 0)
	{
		Vector3[] verts = new Vector3[4];

		Vector3 tl = topLeftCorner;
		Vector3 tr = topRightCorner;
		Vector3 bl = bottomLeftCorner;
		Vector3 br = bottomRightCorner;

		verts[0] = tl.NormalizeToRadius(Planetoid.PlanetRadius + offset);
		verts[1] = tr.NormalizeToRadius(Planetoid.PlanetRadius + offset);
		verts[2] = bl.NormalizeToRadius(Planetoid.PlanetRadius + offset);
		verts[3] = br.NormalizeToRadius(Planetoid.PlanetRadius + offset);

		return verts;
	}

	public Vector3[] GetFlatBoxWithMiddle(float offset = 0)
	{
		Vector3[] verts = new Vector3[5];

		Vector3 tl = topLeftCorner;
		Vector3 tr = topRightCorner;
		Vector3 bl = bottomLeftCorner;
		Vector3 br = bottomRightCorner;
		Vector3 mi = middleNormalized;

		verts[0] = tl.NormalizeToRadius(Planetoid.PlanetRadius + offset);
		verts[1] = tr.NormalizeToRadius(Planetoid.PlanetRadius + offset);
		verts[2] = mi;
		verts[3] = bl.NormalizeToRadius(Planetoid.PlanetRadius + offset);
		verts[4] = br.NormalizeToRadius(Planetoid.PlanetRadius + offset);

		return verts;
	}

	public Vector3[] GetVolumeBox(float height, float offset = 0)
	{
		Vector3[] verts = new Vector3[8];

		Vector3 tl = topLeftCorner;
		Vector3 tr = topRightCorner;
		Vector3 bl = bottomLeftCorner;
		Vector3 br = bottomRightCorner;

		verts[0] = tl.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
		verts[1] = tr.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
		verts[2] = bl.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);
		verts[3] = br.NormalizeToRadius(Planetoid.PlanetRadius + height + offset);

		verts[4] = tl.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);
		verts[5] = tr.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);
		verts[6] = bl.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);
		verts[7] = br.NormalizeToRadius(Planetoid.PlanetRadius - height - offset);

		return verts;
	}

	public bool PlaneFrustumCheck(Camera camera)
	{
		if (Parent == null || !Generated || Splitting)
			return true;

		Vector3[] verts0 = GetFlatBoxWithMiddle(Planetoid.TerrainMaxHeight);
		Vector3[] verts1 = GetFlatBox(Planetoid.TerrainMaxHeight / 2);
		Vector3[] verts2 = GetFlatBox(Planetoid.TerrainMaxHeight * 2);

		Vector3[] vertsAll = new Vector3[verts0.Length + verts1.Length + verts2.Length];

		Array.Copy(verts0, vertsAll, verts0.Length);
		Array.Copy(verts1, 0, vertsAll, verts0.Length, verts1.Length);
		Array.Copy(verts2, 0, vertsAll, verts0.Length + verts1.Length, verts2.Length);

		bool[] states = new bool[verts0.Length];

		for (int i = 0; i < states.Length; i++)
		{
			states[i] = BorderFrustumCheck(camera, verts0[i]);
		}

		return states.Contains(true);
	}

	public bool BorderFrustumCheck(Camera camera, Vector3 border)
	{
		float offset = 512.0f;

		bool useOffset = true;

		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

		for (int i = 0; i < planes.Length; i++)
		{
			if (planes[i].GetDistanceToPoint(transform.TransformPoint(border)) < (useOffset ? 0 - offset : 0))
			{
				return false;
			}
		}

		return true;
	}

	public void RenderOutline(Camera camera, Material lineMaterial)
	{
		#if UNITY_EDITOR
		if (UnityEditor.SceneView.currentDrawingSceneView != null) return; //Do not draw at Scene tab in editor.
		#endif

		Color lineColor = Color.blue;

		int[,] ORDER = new int[,] { { 1, 0 }, { 2, 3 }, { 0, 2 }, { 3, 1 } };

		Vector3[] verts = GetVolumeBox(Planetoid.TerrainMaxHeight * 3);

		GL.PushMatrix();
		GL.LoadIdentity();
		GL.MultMatrix(Camera.main.worldToCameraMatrix * Planetoid.transform.localToWorldMatrix);
		GL.LoadProjectionMatrix(Camera.main.projectionMatrix);

		lineMaterial.renderQueue = 5000;
		lineMaterial.SetPass(0);

		GL.Begin(GL.LINES);
		GL.Color(lineColor);

		for (int i = 0; i < 4; i++)
		{
			//Draw bottom quad
			GL.Vertex3(verts[ORDER[i, 0]].x, verts[ORDER[i, 0]].y, verts[ORDER[i, 0]].z);
			GL.Vertex3(verts[ORDER[i, 1]].x, verts[ORDER[i, 1]].y, verts[ORDER[i, 1]].z);

			//Draw top quad
			GL.Vertex3(verts[ORDER[i, 0] + 4].x, verts[ORDER[i, 0] + 4].y, verts[ORDER[i, 0] + 4].z);
			GL.Vertex3(verts[ORDER[i, 1] + 4].x, verts[ORDER[i, 1] + 4].y, verts[ORDER[i, 1] + 4].z);

			//Draw verticals
			GL.Vertex3(verts[ORDER[i, 0]].x, verts[ORDER[i, 0]].y, verts[ORDER[i, 0]].z);
			GL.Vertex3(verts[ORDER[i, 0] + 4].x, verts[ORDER[i, 0] + 4].y, verts[ORDER[i, 0] + 4].z);
		}

		GL.End();
		GL.PopMatrix();
	}

	public void InitCorners(Vector3 topLeft, Vector3 bottmoRight, Vector3 topRight, Vector3 bottomLeft)
	{
		topLeftCorner = topLeft;
		bottomRightCorner = bottmoRight;
		topRightCorner = topRight;
		bottomLeftCorner = bottomLeft;

		middleNormalized = CalculateMiddlePoint(topLeft, bottmoRight, topRight, bottmoRight);
	}

	public IEnumerator Split()
	{
		int id = 0;

		Vector3 size = bottomRightCorner - topLeftCorner;
		Vector3 step = size / 2;

		bool staticX = false, staticY = false, staticZ = false;

		if (step.x == 0)
			staticX = true;
		if (step.y == 0)
			staticY = true;
		if (step.z == 0)
			staticZ = true;

		Planetoid.Working = true;
		HaveSubQuads = true;
		Splitting = true;
		Unsplitted = false;

		for (int sY = 0; sY < 2; sY++)
		{
			for (int sX = 0; sX < 2; sX++, id++)
			{
				Vector3 subTopLeft = Vector3.zero, subBottomRight = Vector3.zero;
				Vector3 subTopRight = Vector3.zero, subBottomLeft = Vector3.zero;

				if (staticX)
				{
					subTopLeft = new Vector3(topLeftCorner.x, topLeftCorner.y + step.y * sY, topLeftCorner.z + step.z * sX);
					subBottomRight = new Vector3(topLeftCorner.x, topLeftCorner.y + step.y * (sY + 1), topLeftCorner.z + step.z * (sX + 1));

					subTopRight = new Vector3(topLeftCorner.x, topLeftCorner.y + step.y * sY, topLeftCorner.z + step.z * (sX + 1));
					subBottomLeft = new Vector3(topLeftCorner.x, topLeftCorner.y + step.y * (sY + 1), topLeftCorner.z + step.z * sX);
				}

				if (staticY)
				{
					subTopLeft = new Vector3(topLeftCorner.x + step.x * sX, topLeftCorner.y, topLeftCorner.z + step.z * sY);
					subBottomRight = new Vector3(topLeftCorner.x + step.x * (sX + 1), topLeftCorner.y, topLeftCorner.z + step.z * (sY + 1));

					subTopRight = new Vector3(topLeftCorner.x + step.x * (sX + 1), topLeftCorner.y, topLeftCorner.z + step.z * sY);
					subBottomLeft = new Vector3(topLeftCorner.x + step.x * sX, topLeftCorner.y, topLeftCorner.z + step.z * (sY + 1));
				}

				if (staticZ)
				{
					subTopLeft = new Vector3(topLeftCorner.x + step.x * sX, topLeftCorner.y + step.y * sY, topLeftCorner.z);
					subBottomRight = new Vector3(topLeftCorner.x + step.x * (sX + 1), topLeftCorner.y + step.y * (sY + 1), topLeftCorner.z);

					subTopRight = new Vector3(topLeftCorner.x + step.x * (sX + 1), topLeftCorner.y + step.y * sY, topLeftCorner.z);
					subBottomLeft = new Vector3(topLeftCorner.x + step.x * sX, topLeftCorner.y + step.y * (sY + 1), topLeftCorner.z);
				}

				Quad quad = Planetoid.SetupSubQuad(Position);
				quad.Splitting = true;
				quad.ShouldDraw = false;
				quad.InitCorners(subTopLeft, subBottomRight, subTopRight, subBottomLeft);
				quad.SetupParent(this);
				quad.SetupLODLevel(quad);
				quad.SetupID(quad, id);
				quad.SetupVectors(quad, id, staticX, staticY, staticZ);

				if (quad.Parent.transform != null)
					quad.transform.parent = quad.Parent.transform;

				quad.gameObject.name += "_ID" + id + "_LOD" + quad.LODLevel;

				quad.ReadyForDispatch = true;

				Subquads.Add(quad);

				for (int wait = 0; wait < Planetoid.DispatchSkipFramesCount; wait++)
				{
					yield return new WaitForEndOfFrame();
				}
			}
		}

		foreach (Quad q in Subquads)
		{
			q.Splitting = false;
			q.ShouldDraw = true;
		}

		ShouldDraw = false;

		Planetoid.Working = false;
		Splitting = false;
	}

	public void Unsplit()
	{
		if (Unsplitted) return;

		StopAllCoroutines();

		for (int i = 0; i < Subquads.Count; i++)
		{
			if (Subquads[i].HaveSubQuads)
			{
				Subquads[i].Unsplit();
			}

			if (Planetoid.Quads.Contains(Subquads[i]))
			{
				Planetoid.Quads.Remove(Subquads[i]);
			}

			if (Subquads[i] != null)
			{
				DestroyImmediate(Subquads[i].gameObject);
			}
		}

		if (HaveSubQuads == true) ShouldDraw = true;

		HaveSubQuads = false;
		Unsplitted = true;
		Subquads.Clear();
	}

	public void Dispatch()
	{
		if (DispatchStarted != null)
			DispatchStarted(this);

		Planetoid.NPS.UpdateUniforms(QuadMaterial, CoreShader);

		generationConstants.LODLevel = (((1 << LODLevel + 2) * (Planetoid.PlanetRadius / (LODLevel + 2)) - ((Planetoid.PlanetRadius / (LODLevel + 2)) / 2)) / Planetoid.PlanetRadius);
		generationConstants.orientation = (float)Position;

		bool cached = Planetoid.Cache.ExistInTexturesCache(this);
		if (cached) Log("Textures founded in cache!"); else Log("Textures not found in cache!");

		QuadGenerationConstants[] quadGenerationConstantsData = new QuadGenerationConstants[] { generationConstants };
		OutputStruct[] preOutputStructData = new OutputStruct[QS.nVertsReal];
		OutputStruct[] preOutputSubStructData = new OutputStruct[QS.nRealVertsSub];
		OutputStruct[] outputStructData = new OutputStruct[QS.nVerts];

		QuadGenerationConstantsBuffer.SetData(quadGenerationConstantsData);
		PreOutDataBuffer.SetData(preOutputStructData);
		PreOutDataSubBuffer.SetData(preOutputSubStructData);
		OutDataBuffer.SetData(outputStructData);

		int kernel1 = CoreShader.FindKernel("HeightMain");
		int kernel2 = CoreShader.FindKernel("Transfer");
		int kernel3 = CoreShader.FindKernel("HeightSub");
		int kernel4 = CoreShader.FindKernel("TexturesSub");

		SetupComputeShader(kernel1, QuadGenerationConstantsBuffer, PreOutDataBuffer, PreOutDataSubBuffer, OutDataBuffer); Log("Buffers for first kernel ready!");

		CoreShader.Dispatch(kernel1,
		QS.THREADGROUP_SIZE_X_REAL,
		QS.THREADGROUP_SIZE_Y_REAL,
		QS.THREADGROUP_SIZE_Z_REAL); Log("First kernel ready!");

		SetupComputeShader(kernel2, QuadGenerationConstantsBuffer, PreOutDataBuffer, PreOutDataSubBuffer, OutDataBuffer); Log("Buffers for second kernel ready!");

		CoreShader.Dispatch(kernel2,
		QS.THREADGROUP_SIZE_X,
		QS.THREADGROUP_SIZE_Y,
		QS.THREADGROUP_SIZE_Z); Log("Second kernel ready!");

		SetupComputeShader(kernel3, QuadGenerationConstantsBuffer, PreOutDataBuffer, PreOutDataSubBuffer, OutDataBuffer); Log("Buffers for third kernel ready!");

		CoreShader.Dispatch(kernel3,
		QS.THREADGROUP_SIZE_X_SUB_REAL,
		QS.THREADGROUP_SIZE_Y_SUB_REAL,
		QS.THREADGROUP_SIZE_Z_SUB_REAL); Log("Third kernel ready!");

		SetupComputeShader(kernel4, QuadGenerationConstantsBuffer, PreOutDataBuffer, PreOutDataSubBuffer, OutDataBuffer); Log("Buffers for fourth kernel ready!");

		CoreShader.Dispatch(kernel4,
		QS.THREADGROUP_SIZE_X_SUB,
		QS.THREADGROUP_SIZE_Y_SUB,
		QS.THREADGROUP_SIZE_Z_SUB); Log("Fourth kernel ready!");

		Generated = true;

		if (DispatchReady != null)
			DispatchReady(this);
	}

	private bool AllSubquadsGenerated()
	{
		if (Subquads.Count != 0)
		{
			var state = true;
			return Subquads.All(s => s.Generated == state);
		}
		else
			return false;
	}

	private void SetupComputeShader(int kernel, ComputeBuffer QuadGenerationConstantsBuffer, ComputeBuffer PreOutDataBuffer, ComputeBuffer PreOutDataSubBuffer, ComputeBuffer OutDataBuffer)
	{
		if (CoreShader == null) return;

		CoreShader.SetBuffer(kernel, "quadGenerationConstants", QuadGenerationConstantsBuffer);
		CoreShader.SetBuffer(kernel, "patchPreOutput", PreOutDataBuffer);
		CoreShader.SetBuffer(kernel, "patchPreOutputSub", PreOutDataSubBuffer);
		CoreShader.SetBuffer(kernel, "patchOutput", OutDataBuffer);

		CoreShader.SetTexture(kernel, "Height", HeightTexture);
		CoreShader.SetTexture(kernel, "Normal", NormalTexture);

		Planetoid.NPS.SetUniforms(CoreShader, kernel);

		if (Planetoid.transform.GetComponentInChildren<TCCommonParametersSetter>() != null)
			Planetoid.transform.GetComponentInChildren<TCCommonParametersSetter>().UpdateUniforms(CoreShader);
	}

	public void SetupVectors(Quad quad, int id, bool staticX, bool staticY, bool staticZ)
	{
		Vector3 cfed = Parent.generationConstants.cubeFaceEastDirection / 2;
		Vector3 cfnd = Parent.generationConstants.cubeFaceNorthDirection / 2;

		quad.generationConstants.cubeFaceEastDirection = cfed;
		quad.generationConstants.cubeFaceNorthDirection = cfnd;
		quad.generationConstants.patchCubeCenter = quad.GetPatchCubeCenterSplitted(quad.Position, id, staticX, staticY, staticZ);
	}

	public void SetupCorners(QuadPostion pos)
	{
		float v = Planetoid.PlanetRadius / 2;

		switch (pos)
		{
			case QuadPostion.Top:
				topLeftCorner = new Vector3(-v, v, v);
				bottomRightCorner = new Vector3(v, v, -v);

				topRightCorner = new Vector3(v, v, v);
				bottomLeftCorner = new Vector3(-v, v, -v);
				break;
			case QuadPostion.Bottom:
				topLeftCorner = new Vector3(-v, -v, -v);
				bottomRightCorner = new Vector3(v, -v, v);

				topRightCorner = new Vector3(v, -v, -v);
				bottomLeftCorner = new Vector3(-v, -v, v);
				break;
			case QuadPostion.Left:
				topLeftCorner = new Vector3(-v, v, v);
				bottomRightCorner = new Vector3(-v, -v, -v);

				topRightCorner = new Vector3(-v, v, -v);
				bottomLeftCorner = new Vector3(-v, -v, v);
				break;
			case QuadPostion.Right:
				topLeftCorner = new Vector3(v, v, -v);
				bottomRightCorner = new Vector3(v, -v, v);

				topRightCorner = new Vector3(v, v, v);
				bottomLeftCorner = new Vector3(v, -v, -v);
				break;
			case QuadPostion.Front:
				topLeftCorner = new Vector3(v, v, v);
				bottomRightCorner = new Vector3(-v, -v, v);

				topRightCorner = new Vector3(-v, v, v);
				bottomLeftCorner = new Vector3(v, -v, v);
				break;
			case QuadPostion.Back:
				topLeftCorner = new Vector3(-v, v, -v);
				bottomRightCorner = new Vector3(v, -v, -v);

				topRightCorner = new Vector3(v, v, -v);
				bottomLeftCorner = new Vector3(-v, -v, -v);
				break;
		}

		middleNormalized = CalculateMiddlePoint(topLeftCorner,
												bottomRightCorner,
												topRightCorner,
												bottomLeftCorner);
	}

	public void SetupParent(Quad parent)
	{
		Parent = parent;
	}

	public void SetupLODLevel(Quad quad)
	{
		quad.LODLevel = quad.Parent.LODLevel + 1;
	}

	public void SetupID(Quad quad, int id)
	{
		quad.ID = (QuadID)id;
	}

	public void SetupBounds(Quad quad, Mesh mesh)
	{
		Vector3 middle = quad.middleNormalized;

		mesh.bounds = new Bounds(middle, GetBoundsSize(quad));
	}

	public float GetDistanceToClosestCorner()
	{
		return Vector3.Distance(Planetoid.LODTarget.position, GetClosestCorner());
	}

	public Vector3 GetClosestCorner()
	{
		float closestDistance = Mathf.Infinity;

		Vector3 closestCorner = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);

		Vector3 tl = transform.TransformPoint(topLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius));
		Vector3 tr = transform.TransformPoint(topRightCorner.NormalizeToRadius(Planetoid.PlanetRadius));
		Vector3 middlePoint = transform.TransformPoint(middleNormalized);
		Vector3 bl = transform.TransformPoint(bottomLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius));
		Vector3 br = transform.TransformPoint(bottomRightCorner.NormalizeToRadius(Planetoid.PlanetRadius));

		float d = Vector3.Distance(Planetoid.LODTarget.position, tl);

		if (d < closestDistance)
		{
			closestCorner = tl;
			closestDistance = d;
		}

		d = Vector3.Distance(Planetoid.LODTarget.position, tr);

		if (d < closestDistance)
		{
			closestCorner = tr;
			closestDistance = d;
		}

		d = Vector3.Distance(Planetoid.LODTarget.position, middlePoint);

		if (d < closestDistance)
		{
			closestCorner = middlePoint;
			closestDistance = d;
		}

		d = Vector3.Distance(Planetoid.LODTarget.position, bl);

		if (d < closestDistance)
		{
			closestCorner = bl;
			closestDistance = d;
		}

		d = Vector3.Distance(Planetoid.LODTarget.position, br);

		if (d < closestDistance)
		{
			closestCorner = br;
			closestDistance = d;
		}

		if (Generated)
			return closestCorner;
		else
			return new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
	}

	public float GetClosestDistance(float offset)
	{
		float closestDistance = Mathf.Infinity;

		Vector3 tl = topLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius);
		Vector3 tr = topRightCorner.NormalizeToRadius(Planetoid.PlanetRadius);
		Vector3 middlePoint = middleNormalized;
		Vector3 bl = bottomLeftCorner.NormalizeToRadius(Planetoid.PlanetRadius);
		Vector3 br = bottomRightCorner.NormalizeToRadius(Planetoid.PlanetRadius);

		float d = Vector3.Distance(Planetoid.LODTarget.position, tl);

		if (d < closestDistance)
		{
			closestDistance = d + offset;
		}

		d = Vector3.Distance(Planetoid.LODTarget.position, tr);

		if (d < closestDistance)
		{
			closestDistance = d + offset;
		}

		d = Vector3.Distance(Planetoid.LODTarget.position, middlePoint);

		if (d < closestDistance)
		{
			closestDistance = d + offset;
		}

		d = Vector3.Distance(Planetoid.LODTarget.position, bl);

		if (d < closestDistance)
		{
			closestDistance = d + offset;
		}

		d = Vector3.Distance(Planetoid.LODTarget.position, br);

		if (d < closestDistance)
		{
			closestDistance = d + offset;
		}

		return closestDistance;
	}

	public Vector3 GetBoundsSize(Quad quad)
	{
		if (Planetoid.UseUnityCulling)
		{
			Vector3 size = bottomRightCorner - topLeftCorner;

			return VectorHelper.Abs(size * 2.0f - VectorHelper.Abs(middleNormalized));
		}
		else
		{
			return new Vector3(9e37f, 9e37f, 9e37f);
		}
	}

	public Vector3 GetCubeFaceEastDirection(QuadPostion quadPosition)
	{
		Vector3 temp = Vector3.zero;

		float r = Planetoid.PlanetRadius;

		switch (quadPosition)
		{
			case QuadPostion.Top:
				temp = new Vector3(0.0f, 0.0f, -r);
				break;
			case QuadPostion.Bottom:
				temp = new Vector3(0.0f, 0.0f, -r);
				break;
			case QuadPostion.Left:
				temp = new Vector3(0.0f, -r, 0.0f);
				break;
			case QuadPostion.Right:
				temp = new Vector3(0.0f, -r, 0.0f);
				break;
			case QuadPostion.Front:
				temp = new Vector3(r, 0.0f, 0.0f);
				break;
			case QuadPostion.Back:
				temp = new Vector3(-r, 0.0f, 0.0f);
				break;
		}

		return temp;
	}

	public Vector3 GetCubeFaceNorthDirection(QuadPostion quadPosition)
	{
		Vector3 temp = Vector3.zero;

		float r = Planetoid.PlanetRadius;

		switch (quadPosition)
		{
			case QuadPostion.Top:
				temp = new Vector3(r, 0.0f, 0.0f);
				break;
			case QuadPostion.Bottom:
				temp = new Vector3(-r, 0.0f, 0.0f);
				break;
			case QuadPostion.Left:
				temp = new Vector3(0.0f, 0.0f, -r);
				break;
			case QuadPostion.Right:
				temp = new Vector3(0.0f, 0.0f, r);
				break;
			case QuadPostion.Front:
				temp = new Vector3(0.0f, -r, 0);
				break;
			case QuadPostion.Back:
				temp = new Vector3(0.0f, -r, 0.0f);
				break;
		}

		return temp;
	}

	public Vector3 GetPatchCubeCenter(QuadPostion quadPosition)
	{
		Vector3 temp = Vector3.zero;

		float r = Planetoid.PlanetRadius;

		switch (quadPosition)
		{
			case QuadPostion.Top:
				temp = new Vector3(0.0f, r, 0.0f);
				break;
			case QuadPostion.Bottom:
				temp = new Vector3(0.0f, -r, 0.0f);
				break;
			case QuadPostion.Left:
				temp = new Vector3(-r, 0.0f, 0.0f);
				break;
			case QuadPostion.Right:
				temp = new Vector3(r, 0.0f, 0.0f);
				break;
			case QuadPostion.Front:
				temp = new Vector3(0.0f, 0.0f, r);
				break;
			case QuadPostion.Back:
				temp = new Vector3(0.0f, 0.0f, -r);
				break;
		}

		return temp;
	}

	public Vector3 GetPatchCubeCenterSplitted(QuadPostion quadPosition, int id, bool staticX, bool staticY, bool staticZ)
	{
		Vector3 temp = Vector3.zero;

		float v = Planetoid.PlanetRadius;

		switch (quadPosition)
		{
			case QuadPostion.Top:
				if (id == 0)
					temp += new Vector3(-v / 2, v, v / 2);
				else if (id == 1)
					temp += new Vector3(v / 2, v, v / 2);
				else if (id == 2)
					temp += new Vector3(-v / 2, v, -v / 2);
				else if (id == 3)
					temp += new Vector3(v / 2, v, -v / 2);
				break;
			case QuadPostion.Bottom:
				if (id == 0)
					temp += new Vector3(-v / 2, -v, -v / 2);
				else if (id == 1)
					temp += new Vector3(v / 2, -v, -v / 2);
				else if (id == 2)
					temp += new Vector3(-v / 2, -v, v / 2);
				else if (id == 3)
					temp += new Vector3(v / 2, -v, v / 2);
				break;
			case QuadPostion.Left:
				if (id == 0)
					temp += new Vector3(-v, v / 2, v / 2);
				else if (id == 1)
					temp += new Vector3(-v, v / 2, -v / 2);
				else if (id == 2)
					temp += new Vector3(-v, -v / 2, v / 2);
				else if (id == 3)
					temp += new Vector3(-v, -v / 2, -v / 2);
				break;
			case QuadPostion.Right:
				if (id == 0)
					temp += new Vector3(v, v / 2, -v / 2);
				else if (id == 1)
					temp += new Vector3(v, v / 2, v / 2);
				else if (id == 2)
					temp += new Vector3(v, -v / 2, -v / 2);
				else if (id == 3)
					temp += new Vector3(v, -v / 2, v / 2);
				break;
			case QuadPostion.Front:
				if (id == 0)
					temp += new Vector3(v / 2, v / 2, v);
				else if (id == 1)
					temp += new Vector3(-v / 2, v / 2, v);
				else if (id == 2)
					temp += new Vector3(v / 2, -v / 2, v);
				else if (id == 3)
					temp += new Vector3(-v / 2, -v / 2, v);
				break;
			case QuadPostion.Back:
				if (id == 0)
					temp += new Vector3(-v / 2, v / 2, -v);
				else if (id == 1)
					temp += new Vector3(v / 2, v / 2, -v);
				else if (id == 2)
					temp += new Vector3(-v / 2, -v / 2, -v);
				else if (id == 3)
					temp += new Vector3(v / 2, -v / 2, -v);
				break;
		}

		float tempStatic = 0;

		if (staticX)
			tempStatic = temp.x;
		if (staticY)
			tempStatic = temp.y;
		if (staticZ)
			tempStatic = temp.z;

		//TODO : Make a formula!
		//So. We have exponential modifier... WTF!?
		//Fuck dat shit. 7 LOD level more than i need. fuck. dat.

		//WARNING!!! Magic! Ya, it works...
		if (LODLevel >= 1)
		{
			if (LODLevel == 1)
				temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 7.5f), 0.5f); //0.5f
			else if (LODLevel == 2)
				temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 11.25f), 0.75f); //0.5f + 0.5f / 2.0f
			else if (LODLevel == 3)
				temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 13.125f), 0.875f); //0.75f + ((0.5f / 2.0f) / 2.0f)
			else if (LODLevel == 4)
				temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 14.0625f), 0.9375f); //0.875f + (((0.5f / 2.0f) / 2.0f) / 2.0f)
			else if (LODLevel == 5)
				temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 14.53125f), 0.96875f); //0.9375f + ((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f)
			else if (LODLevel == 6)
				temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 14.765625f), 0.984375f); //0.96875f + (((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
			else if (LODLevel == 7) //Experimental! Maybe float precision have place on small planet radius!
				temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 14.8828125f), 0.9921875f); //0.984375f + ((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
			else if (LODLevel == 8) //Experimental! Maybe float precision have place on small planet radius!
				temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 14.94140625f), 0.99609375f); //0.9921875f + (((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
			else if (LODLevel == 9) //Experimental! Maybe float precision have place on small planet radius!
				temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 14.970703100f), 0.998046875f); //0.99609375f + ((((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
			else if (LODLevel == 10) //Sooooo deep... what i'am doing?
				temp = Vector3.Lerp(temp, Parent.generationConstants.patchCubeCenter * (15.0f / 14.9853515000f), 0.999023438f); //0.998046875f + (((((((((0.5f / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f) / 2.0f)
			//OMG...
		}
		//End of magic here.

		if (staticX)
			temp.x = tempStatic;
		if (staticY)
			temp.y = tempStatic;
		if (staticZ)
			temp.z = tempStatic;

		temp = new Vector3(Mathf.RoundToInt(temp.x), Mathf.RoundToInt(temp.y), Mathf.RoundToInt(temp.z)); //Just make sure that our values is rounded...

		return temp;
	}

	public Vector3 CalculateMiddlePoint(Vector3 topLeft, Vector3 bottmoRight, Vector3 topRight, Vector3 bottomLeft)
	{
		Vector3 size = bottomLeft - topLeft;
		Vector3 middle = Vector3.zero;

		bool staticX = false, staticY = false, staticZ = false;

		if (size.x == 0)
			staticX = true;
		else if (size.y == 0)
			staticY = true;
		else if (size.z == 0)
			staticZ = true;

		float tempStatic = 0;

		middle = (topLeft + bottmoRight) * (1 / Mathf.Abs(LODLevel));
		middle = middle.NormalizeToRadius(Planetoid.PlanetRadius);

		if (staticX)
			tempStatic = middle.x;
		if (staticY)
			tempStatic = middle.y;
		if (staticZ)
			tempStatic = middle.z;

		if (staticX)
			middle.x = tempStatic;
		if (staticY)
			middle.y = tempStatic;
		if (staticZ)
			middle.z = tempStatic;

		return middle;
	}

	public Matrix4x4 GetTangentFrame(Quad q)
	{
		Matrix4x4d worldToTangentFrame = new Matrix4x4d();

		Quad mq = Planetoid.GetMainQuad(q.Position);

		double D = mq.generationConstants.spacing * QS.nVertsPerEdge;
		double R = D / 2.0;

		double tx = mq.generationConstants.patchCubeCenter.x;
		double ty = mq.generationConstants.patchCubeCenter.y;
		double tz = mq.generationConstants.patchCubeCenter.z;

		double x0 = (tx) * D - R;
		double x1 = (tx + 1) * D - R;
		double y0 = (ty) * D - R;
		double y1 = (ty + 1) * D - R;
		double z0 = (tz) * D - R;
		double z1 = (tz + 1) * D - R;

		Vector3d pc = new Vector3d((x0 + x1) * 0.5, (y0 + y1) * 0.5, (z0 + z1) * 0.5);

		Vector3d uz = pc.Normalized();
		Vector3d ux = (new Vector3d(0, 1, 0)).Cross(uz).Normalized();
		Vector3d uy = uz.Cross(ux);

		worldToTangentFrame = new Matrix4x4d(ux.x, ux.y, ux.z, 0.0,
											 uy.x, uy.y, uy.z, 0.0,
											 uz.x, uz.y, uz.z, 0.0,
											 0.0, 0.0, 0.0, 0.0);

		return worldToTangentFrame.ToMatrix4x4();
	}

	private void Log(string msg)
	{
		if (Planetoid.DebugEnabled)
			Debug.Log(msg);
	}

	private void Log(string msg, bool state)
	{
		if (state)
			Debug.Log(msg);
	}
}