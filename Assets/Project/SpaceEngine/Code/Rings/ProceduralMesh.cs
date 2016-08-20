using UnityEngine;
using System.Collections.Generic;

public static class ProceduralMesh
{
	private static List<Vector3> positions = new List<Vector3>();
	
	private static List<Color> colors = new List<Color>();
	
	private static List<Color32> color32s = new List<Color32>();
	
	private static List<Vector2> coords1 = new List<Vector2>();
	
	private static List<Vector2> coords2 = new List<Vector2>();
	
	private static List<Vector3> normals = new List<Vector3>();
	
	private static List<Vector4> tangents = new List<Vector4>();
	
	private static List<int> indices = new List<int>();
	
	private static List<Mesh> meshes = new List<Mesh>();
	
	public static int Count
	{
		get
		{
			return meshes.Count;
		}
	}
	
	public static void Clear()
	{
		ObjectPool<Mesh>.Add(meshes, m => m.Clear());
		
		positions.Clear();
		colors.Clear();
		color32s.Clear();
		coords1.Clear();
		coords2.Clear();
		normals.Clear();
		tangents.Clear();
	}
	
	public static void PushPosition(Vector3 xyz)
	{
		positions.Add(xyz);
	}
	
	public static void PushPosition(Vector3 xyz, int count)
	{
		for (var i = 0; i < count; i++) positions.Add(xyz);
	}
	
	public static void PushPosition(float x, float y, float z)
	{
		positions.Add(new Vector3(x, y, z));
	}
	
	public static void PushPosition(float x, float y, float z, int count)
	{
		var xyz = new Vector3(x, y, z); for (var i = 0; i < count; i++) positions.Add(xyz);
	}
	
	public static void PushColor(Color rgba)
	{
		colors.Add(rgba);
	}
	
	public static void PushColor(Color rgba, int count)
	{
		for (var i = 0; i < count; i++) colors.Add(rgba);
	}
	
	public static void PushColor32(Color32 rgba)
	{
		color32s.Add(rgba);
	}
	
	public static void PushCoord1(Vector2 xy)
	{
		coords1.Add(xy);
	}
	
	public static void PushCoord1(Vector2 xy, int count)
	{
		for (var i = 0; i < count; i++) coords1.Add(xy);
	}
	
	public static void PushCoord1(float x, float y)
	{
		coords1.Add(new Vector2(x, y));
	}
	
	public static void PushCoord1(float x, float y, int count)
	{
		var xy = new Vector2(x, y); for (var i = 0; i < count; i++) coords1.Add(xy);
	}
	
	public static void PushCoord2(Vector2 xy)
	{
		coords2.Add(xy);
	}
	
	public static void PushCoord2(Vector2 xy, int count)
	{
		for (var i = 0; i < count; i++) coords2.Add(xy);
	}
	
	public static void PushCoord2(float x, float y)
	{
		coords2.Add(new Vector2(x, y));
	}
	
	public static void PushCoord2(float x, float y, int count)
	{
		var xy = new Vector2(x, y); for (var i = 0; i < count; i++) coords2.Add(xy);
	}
	
	public static void PushNormal(Vector3 xyz)
	{
		normals.Add(xyz);
	}
	
	public static void PushNormal(Vector3 xyz, int count)
	{
		for (var i = 0; i < count; i++) normals.Add(xyz);
	}
	
	public static void PushNormal(Vector2 xy, float z)
	{
		normals.Add(new Vector3(xy.x, xy.y, z));
	}
	
	public static void PushNormal(Vector2 xy, float z, int count)
	{
		var xyz = new Vector3(xy.x, xy.y, z); for (var i = 0; i < count; i++) normals.Add(xyz);
	}
	
	public static void PushNormal(float x, float y, float z)
	{
		normals.Add(new Vector3(x, y, z));
	}
	
	public static void PushNormal(float x, float y, float z, int count)
	{
		var xyz = new Vector3(x, y, z); for (var i = 0; i < count; i++) normals.Add(xyz);
	}
	
	public static void PushTangent(Vector4 xyzw)
	{
		tangents.Add(xyzw);
	}
	
	public static void PushTangent(Vector4 xyzw, int count)
	{
		for (var i = 0; i < count; i++) tangents.Add(xyzw);
	}
	
	public static void PushTangent(float x, float y, float z, float w)
	{
		var xyzw = new Vector4(x, y, z, w); tangents.Add(xyzw);
	}
	
	public static void PushTangent(float x, float y, float z, float w, int count)
	{
		var xyzw = new Vector4(x, y, z, w); for (var i = 0; i < count; i++) tangents.Add(xyzw);
	}
	
	public static void SplitQuads(HideFlags hideFlags = HideFlags.None)
	{
		Split(4, hideFlags);
		
		for (var i = 0; i < meshes.Count; i++)
		{
			var mesh  = meshes[i];
			var steps = mesh.vertexCount / 4;
			
			for (var j = 0; j < steps; j++)
			{
				var vertexOff = j * 4;
				
				indices.Add(vertexOff + 0);
				indices.Add(vertexOff + 1);
				indices.Add(vertexOff + 2);
				indices.Add(vertexOff + 3);
				indices.Add(vertexOff + 2);
				indices.Add(vertexOff + 1);
			}
			
			mesh.triangles = indices.ToArray(); indices.Clear();
		}
	}
	
	public static void SplitStrip(HideFlags hideFlags = HideFlags.None)
	{
		Split(2, hideFlags);
		
		for (var i = 0; i < meshes.Count; i++)
		{
			var mesh  = meshes[i];
			var steps = mesh.vertexCount / 2 - 1;
			
			for (var j = 0; j < steps; j++)
			{
				var vertexOff = j * 2;
				
				indices.Add(vertexOff + 0);
				indices.Add(vertexOff + 1);
				indices.Add(vertexOff + 2);
				indices.Add(vertexOff + 3);
				indices.Add(vertexOff + 2);
				indices.Add(vertexOff + 1);
			}
			
			mesh.triangles = indices.ToArray(); indices.Clear();
		}
	}
	
	public static void Split(int verticesStep, HideFlags hideFlags)
	{
		if (positions.Count > 0)
		{
			var stepCount       = positions.Count / verticesStep;
			var stepsPerMesh    = 65000 / verticesStep;
			var verticesPerMesh = stepsPerMesh * verticesStep;
			var meshCount       = (positions.Count + verticesPerMesh - 1) / verticesPerMesh;
			
			for (var i = 0; i < meshCount; i++)
			{
				var stepA = i * stepsPerMesh;
				var stepB = Mathf.Min(stepA + stepsPerMesh, stepCount);
				var stepC = stepB - stepA;
				var vertA = stepA * verticesStep;
				var vertC = stepC * verticesStep;
				var mesh  = ObjectPool<Mesh>.Pop() ?? new Mesh(); meshes.Add(mesh);
				
				mesh.Clear(false);
				
				mesh.hideFlags = hideFlags;
				mesh.vertices  = GetRange(positions, vertA, vertC);
				
				if (colors.Count > 0)
				{
					mesh.colors = GetRange(colors, vertA, vertC);
				}
				
				if (color32s.Count > 0)
				{
					mesh.colors32 = GetRange(color32s, vertA, vertC);
				}
				
				if (coords1.Count > 0)
				{
					mesh.uv = GetRange(coords1, vertA, vertC);
				}
				
				if (coords2.Count > 0)
				{
					mesh.uv2 = GetRange(coords2, vertA, vertC);
				}
				
				if (normals.Count > 0)
				{
					mesh.normals = GetRange(normals, vertA, vertC);
				}
				
				if (tangents.Count > 0)
				{
					mesh.tangents = GetRange(tangents, vertA, vertC);
				}
			}
		}
	}
	
	public static Mesh Pop()
	{
		if (meshes.Count > 0)
		{
			var index = meshes.Count - 1;
			var mesh  = meshes[index];
			
			meshes.RemoveAt(index);
			
			return mesh;
		}
		
		return null;
	}
	
	public static void Discard(bool warning = true)
	{
		if (meshes.Count > 0)
		{
			if (warning == true)
			{
				Debug.LogWarning("SgtProceduralMesh generated too many meshes!");
			}
			
			ObjectPool<Mesh>.Add(meshes);
		}
	}
	
	private static T[] GetRange<T>(List<T> list, int first, int count)
	{
		var array = new T[count];
		
		for (var i = 0; i < count; i++)
		{
			array[i] = list[first + i];
		}
		
		return array;
	}
	
	private static int[] CalculateIndices(int[] indexStructure, int verticesStep, int stepC)
	{
		var indices = new int[stepC * indexStructure.Length];
		
		for (var j = 0; j < stepC; j++)
		{
			var indexOff  = j * indexStructure.Length;
			var vertexOff = j * verticesStep;
			
			for (var k = 0; k < indexStructure.Length; k++)
			{
				indices[indexOff + k] = indexStructure[k] + vertexOff;
			}
		}
		
		return indices;
	}
}