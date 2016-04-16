#region License
/* Procedural planet generator.
 *
 * Copyright (C) 2015-2016 Denis Ovchinnikov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using UnityEngine;

using System.Collections.Generic;

public sealed class Planetoid : MonoBehaviour
{
	public Atmosphere Atmosphere;

	public bool DrawWireframe = false;
	public bool DrawNormals = false;

	public bool Working = false;

	public Transform LODTarget = null;

	public float PlanetRadius = 1024;

	public bool DebugEnabled = false;

	public List<Quad> MainQuads = new List<Quad>();
	public List<Quad> Quads = new List<Quad>();

	public Shader ColorShader;
	public ComputeShader CoreShader;

	public int RenderQueue = 2000;

	public int DispatchSkipFramesCount = 8;

	public int LODDistanceMultiplier = 1;
	public int LODMaxLevel = 8;
	public int[] LODDistances = new int[11] { 2048, 1024, 512, 256, 128, 64, 32, 16, 8, 4, 2 };

	public Mesh PrototypeMesh;

	public GameObject QuadsRoot = null;
	public QuadStorage Cache = null;
	public NoiseParametersSetter NPS = null;

	public bool UseUnityCulling = true;
	public bool UseLOD = true;
	public bool RenderPerUpdate = false;
	public bool OneSplittingQuad = true;
	public bool ExternalRendering = false;

	public float TerrainMaxHeight = 64.0f;

	public Vector3 Origin = Vector3.zero;
	public Quaternion OriginRotation = Quaternion.identity;

	public QuadDistanceToClosestCornerComparer qdtccc;

	public class QuadDistanceToClosestCornerComparer : IComparer<Quad>
	{
		public int Compare(Quad x, Quad y)
		{
			if (x.DistanceToClosestCorner > y.DistanceToClosestCorner)
				return 1;
			if (x.DistanceToClosestCorner < y.DistanceToClosestCorner)
				return -1;
			else
				return 0;
		}
	}

	private void Awake()
	{
		Origin = transform.position;
		OriginRotation = QuadsRoot.transform.rotation;

		if (Atmosphere != null) Atmosphere.Origin = Origin;
	}

	private void Start()
	{
		ThreadScheduler.Initialize();

		if (Cache == null)
			if (this.gameObject.GetComponentInChildren<QuadStorage>() != null)
				Cache = this.gameObject.GetComponentInChildren<QuadStorage>();

		if (NPS != null)
			NPS.LoadAndInit();

		if (Atmosphere != null)
		{
			foreach (Quad q in MainQuads)
			{
				Atmosphere.InitUniforms(q.QuadMaterial);
			}
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			DrawWireframe = !DrawWireframe;
		}

		if (Input.GetKeyDown(KeyCode.F2))
		{
			DrawNormals = !DrawNormals;
		}

		if (Input.GetKeyDown(KeyCode.F3))
		{
			if (Atmosphere != null) Atmosphere.TryBake();
		}

		CheckCutoff();

		Origin = transform.position;
		OriginRotation = QuadsRoot.transform.rotation;

		if (Atmosphere != null)
		{
			Atmosphere.Origin = Origin;
			Atmosphere.Sun.UpdateNode();
			Atmosphere.UpdateNode();
			Atmosphere.Render(false);
		}

		if (ExternalRendering && RenderPerUpdate)
		{
			Render();
		}
	}

	private void LateUpdate()
	{

	}

	private void OnRenderObject()
	{
		if (ExternalRendering && !RenderPerUpdate)
		{
			Render();
		}
	}

	public void Render()
	{
		for (int i = 0; i < Quads.Count; i++)
		{
			Quads[i].Render();
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
		MainQuads.Clear();

		if (QuadsRoot != null) DestroyImmediate(QuadsRoot);

		if (PrototypeMesh != null) DestroyImmediate(PrototypeMesh);
	}

	[ContextMenu("SetupMeshes")]
	public void SetupMeshes()
	{
		if (PrototypeMesh != null) DestroyImmediate(PrototypeMesh);

		PrototypeMesh = MeshFactory.SetupQuadMesh();
	}

	public int GetCulledQuadsCount()
	{
		int count = 0;

		for (int i = 0; i < Quads.Count; i++)
		{
			if (!Quads[i].Visible)
				count++;
		}

		return count;
	}

	public Quad GetMainQuad(QuadPosition position)
	{
		foreach (Quad q in MainQuads)
		{
			if (q.Position == position)
				return q;
		}

		return null;
	}

	public Mesh GetMesh(QuadPosition position)
	{
		return PrototypeMesh;
	}

	[ContextMenu("SetupQuads")]
	public void SetupQuads()
	{
		if (Quads.Count > 0)
			return;

		SetupMeshes();
		SetupRoot();

		SetupMainQuad(QuadPosition.Top);
		SetupMainQuad(QuadPosition.Bottom);
		SetupMainQuad(QuadPosition.Left);
		SetupMainQuad(QuadPosition.Right);
		SetupMainQuad(QuadPosition.Front);
		SetupMainQuad(QuadPosition.Back);

		if (NPS != null)
			NPS.LoadAndInit();
	}

	[ContextMenu("ReSetupQuads")]
	public void ReSetupQuads()
	{
		DestroyQuads();
		SetupQuads();
	}

	public void SetupRoot()
	{
		if (QuadsRoot == null)
		{
			QuadsRoot = new GameObject("Quads_Root");
			QuadsRoot.transform.position = transform.position;
			QuadsRoot.transform.rotation = transform.rotation;
			QuadsRoot.transform.parent = transform;
		}
		else
		{
			return;
		}
	}

	public void SetupMainQuad(QuadPosition quadPosition)
	{
		GameObject go = new GameObject("Quad" + "_" + quadPosition.ToString());
		go.transform.position = Vector3.zero;
		go.transform.rotation = Quaternion.identity;
		go.transform.parent = QuadsRoot.transform;

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

	public Quad SetupSubQuad(QuadPosition quadPosition)
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

		if(Atmosphere != null) Atmosphere.InitUniforms(quadComponent.QuadMaterial);

		QuadGenerationConstants gc = QuadGenerationConstants.Init(TerrainMaxHeight);
		gc.planetRadius = PlanetRadius;

		quadComponent.Position = quadPosition;
		quadComponent.generationConstants = gc;
		quadComponent.Planetoid = this;
		quadComponent.ShouldDraw = false;

		if (qdtccc == null)
			qdtccc = new QuadDistanceToClosestCornerComparer();

		Quads.Add(quadComponent);
		Quads.Sort(qdtccc);

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
}