using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Ring : MonoBehaviour
{
	public List<Light> Lights = new List<Light>();
	
	public List<Shadow> Shadows = new List<Shadow>();
	
	public Texture MainTex;
	
	public Color Color = Color.white;
	
	public float Brightness = 1.0f;
	
	public EngineRenderQueue RenderQueue = EngineRenderQueue.Transparent;
	
	public int RenderQueueOffset;
	
	public float InnerRadius = 1.0f;
	
	public float OuterRadius = 2.0f;
	
	public int SegmentCount = 8;
	
	public int SegmentDetail = 10;
	
	[Range(-1.0f, 1.0f)]
	public float LightingBias = 0.5f;
	
	[Range(0.0f, 1.0f)]
	public float LightingSharpness = 0.5f;
	
	public bool Scattering;
	
	[Range(0.0f, 5.0f)]
	public float MieSharpness = 2.0f;
	
	[Range(0.0f, 10.0f)]
	public float MieStrength = 1.0f;
	
	public float BoundsShift;
	
	public Mesh mesh;
	
	public Material material;
	
	[SerializeField]
	protected List<RingSegment> segments = new List<RingSegment>();
	
	protected static List<string> keywords = new List<string>();

	public Shader RingShader;
	
	public void UpdateState()
	{
		UpdateMaterial();
		UpdateSegments();
	}
	
	public static Ring CreateRing(Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
	{
		var gameObject = Helper.CreateGameObject("Ring", parent, localPosition, localRotation, localScale);
		var ring       = gameObject.AddComponent<Ring>();
		
		return ring;
	}
	
	protected virtual void OnEnable()
	{	
		for (var i = segments.Count - 1; i >= 0; i--)
		{
			var segment = segments[i];
			
			if (segment != null)
			{
				segment.gameObject.SetActive(true);
			}
		}
	}
	
	protected virtual void OnDisable()
	{	
		for (var i = segments.Count - 1; i >= 0; i--)
		{
			var segment = segments[i];
			
			if (segment != null)
			{
				segment.gameObject.SetActive(false);
			}
		}
	}
	
	protected virtual void OnDestroy()
	{
		Helper.Destroy(material);
		
		for (var i = segments.Count - 1; i >= 0; i--)
		{
			DestroyImmediate(segments[i]);
		}
		
		segments.Clear();
	}
	
	protected virtual void Update()
	{
		UpdateState();
	}
	
#if UNITY_EDITOR
	protected virtual void OnDrawGizmosSelected()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		
		Helper.DrawCircle(Vector3.zero, Vector3.up, InnerRadius);
		Helper.DrawCircle(Vector3.zero, Vector3.up, OuterRadius);
	}
#endif
	
	protected virtual void UpdateMaterial()
	{
		if (material == null) material = MaterialHelper.CreateTemp(RingShader, "Ring");
		
		var color       = Helper.Brighten(Color, Brightness);
		var renderQueue = (int)RenderQueue + RenderQueueOffset;
		var lightCount  = Helper.WriteLights(Lights, 2, transform.position, null, null, material);
		var shadowCount = Helper.WriteShadows(Shadows, 2, material);
		
		material.renderQueue = renderQueue;
		material.SetTexture("_MainTex", MainTex);
		material.SetColor("_Color", color);
		material.SetFloat("_LightingBias", LightingBias);
		material.SetFloat("_LightingSharpness", LightingSharpness);
		
		if (Scattering == true)
		{
			keywords.Add("SGT_A");
			
			Helper.WriteMie(MieSharpness, MieStrength, material);
		}
		
		Helper.WriteLightKeywords(Lights.Count > 0, lightCount, keywords);
		Helper.WriteShadowKeywords(shadowCount, keywords);
		
		Helper.SetKeywords(material, keywords); keywords.Clear();
	}
	
	protected void UpdateSegments()
	{
		segments.RemoveAll(m => m == null);
		
		if (SegmentCount != segments.Count)
		{
			Helper.ResizeArrayTo(ref segments, SegmentCount, i => RingSegment.Create(this), s => RingSegment.Pool(s));
		}
		
		var angleStep = Helper.Divide(360.0f, SegmentCount);
		
		for (var i = SegmentCount - 1; i >= 0; i--)
		{
			var angle    = angleStep * i;
			var rotation = Quaternion.Euler(0.0f, angle, 0.0f);
			
			segments[i].ManualUpdate(mesh, material, rotation);
		}
	}
	
	[ContextMenu("RegenerateMesh")]
	protected virtual void RegenerateMesh()
	{
		DestroyImmediate(mesh);

		mesh = ObjectPool<Mesh>.Add(mesh, m => m.Clear());

		ProceduralMesh.Clear();
		{
			if (SegmentDetail >= 3)
			{
				var angleTotal = Helper.Divide(Mathf.PI * 2.0f, SegmentCount);
				var angleStep  = Helper.Divide(angleTotal, SegmentDetail);
				var coordStep  = Helper.Reciprocal(SegmentDetail);
				
				for (var i = 0; i <= SegmentDetail; i++)
				{
					var coord = coordStep * i;
					var angle = angleStep * i;
					var sin   = Mathf.Sin(angle);
					var cos   = Mathf.Cos(angle);
					
					ProceduralMesh.PushPosition(sin * InnerRadius, 0.0f, cos * InnerRadius);
					ProceduralMesh.PushPosition(sin * OuterRadius, 0.0f, cos * OuterRadius);
					
					ProceduralMesh.PushNormal(Vector3.up);
					ProceduralMesh.PushNormal(Vector3.up);
					
					ProceduralMesh.PushCoord1(0.0f, coord);
					ProceduralMesh.PushCoord1(1.0f, coord);
				}
			}
		}
		ProceduralMesh.SplitStrip(HideFlags.DontSave);
		
		mesh = ProceduralMesh.Pop(); ProceduralMesh.Discard();
		
		if (mesh != null)
		{
			var bounds = mesh.bounds;
			
			mesh.bounds = Helper.NewBoundsCenter(bounds, bounds.center + bounds.center.normalized * BoundsShift);
		}
	}
}