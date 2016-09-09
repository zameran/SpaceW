using UnityEngine;

using System.Collections.Generic;

using ZFramework.Unity.Common.PerfomanceMonitor;

public class Ring : MonoBehaviour
{
    public List<Light> Lights = new List<Light>();
    public List<Shadow> Shadows = new List<Shadow>();

    public Texture MainTex;

    public Color Color = Color.white;

    public float Brightness = 1.0f;
    public float InnerRadius = 1.0f;
    public float OuterRadius = 2.0f;

    public EngineRenderQueue RenderQueue = EngineRenderQueue.Transparent;
    public int RenderQueueOffset;

    public int SegmentCount = 8;
    public int SegmentDetail = 8;

    public float BoundsShift;

    [Range(-1.0f, 1.0f)] public float LightingBias = 0.5f;
    [Range(0.0f, 1.0f)] public float LightingSharpness = 0.5f;
    [Range(0.0f, 5.0f)] public float MieSharpness = 2.0f;
    [Range(0.0f, 10.0f)] public float MieStrength = 1.0f;

    public List<RingSegment> Segments = new List<RingSegment>();

    public static List<string> keywords = new List<string>();

    public Shader RingShader;
    public Material RingMaterial;

    public Mesh RingSegmentMesh;

    private void Start()
    {
        InitMesh();
        InitMaterial();
    }

    private void Update()
    {
        SetUniforms(RingMaterial);

        UpdateNode();
    }

    private void OnEnable()
    {
        for (var i = Segments.Count - 1; i >= 0; i--)
        {
            var segment = Segments[i];

            if (segment != null)
            {
                segment.gameObject.SetActive(true);
            }
        }
    }

    private void OnDisable()
    {
        for (var i = Segments.Count - 1; i >= 0; i--)
        {
            var segment = Segments[i];

            if (segment != null)
            {
                segment.gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        Helper.Destroy(RingMaterial);
        Helper.Destroy(RingSegmentMesh);

        for (var i = Segments.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(Segments[i]);
        }

        Segments.Clear();
    }

    #region Gizmos

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        Helper.DrawCircle(Vector3.zero, Vector3.up, InnerRadius);
        Helper.DrawCircle(Vector3.zero, Vector3.up, OuterRadius);
    }
#endif

    #endregion

    public void Render(Camera camera, int drawLayer = 8)
    {
        if (Segments == null) return;
        if (Segments.Count == 0) return;

        for (int i = 0; i < Segments.Count; i++)
        {
            Segments[i].Render(camera, drawLayer);
        }
    }

    public void InitUniforms(Planet planet)
    {
        if (planet == null) return;

        SetUniforms(RingMaterial);
    }

    public void SetLightsAndShadows(Material mat)
    {
        if (mat == null) return;

        var lightCount = Helper.WriteLights(Lights, 4, transform.position, null, null, mat);
        var shadowCount = Helper.WriteShadows(Shadows, 4, mat);

        Helper.WriteLightKeywords(lightCount, keywords);
        Helper.WriteShadowKeywords(shadowCount, keywords);

        keywords.Add("SCATTERING");

        Helper.SetKeywords(mat, keywords);
    }

    public void SetShadows(Material mat, List<Shadow> shadows)
    {
        if (mat == null) return;

        Helper.WriteShadows(shadows, 4, mat);
    }

    public void SetShadows(MaterialPropertyBlock block, List<Shadow> shadows)
    {
        if (block == null) return;

        Helper.WriteShadows(shadows, 4, block);
    }

    public void SetUniforms(Material mat)
    {
        if (mat == null) return;

        SetLightsAndShadows(mat);

        mat.renderQueue = (int)RenderQueue + RenderQueueOffset;
        mat.SetTexture("_MainTex", MainTex);
        mat.SetColor("_Color", Helper.Brighten(Color, Brightness));
        mat.SetFloat("_LightingBias", LightingBias);
        mat.SetFloat("_LightingSharpness", LightingSharpness);

        Helper.WriteMie(MieSharpness, MieStrength, mat);

        keywords.Clear();
    }

    private void UpdateNode()
    {
        using (new Timer("Ring.UpdateNode()"))
        {
            Segments.RemoveAll(m => m == null);

            if (SegmentCount != Segments.Count)
            {
                Helper.ResizeArrayTo(ref Segments, SegmentCount, i => RingSegment.Create(this), null);
            }

            var angleStep = Helper.Divide(360.0f, SegmentCount);

            for (var i = SegmentCount - 1; i >= 0; i--)
            {
                var angle = angleStep * i;
                var rotation = Quaternion.Euler(0.0f, angle, 0.0f);

                Segments[i].UpdateNode(RingSegmentMesh, RingMaterial, rotation);
            }
        }
    }

    private void InitMaterial()
    {
        RingMaterial = MaterialHelper.CreateTemp(RingShader, "Ring");
    }

    private void InitMesh()
    {
        RingSegmentMesh = MeshFactory.SetupRingSegmentMesh(SegmentCount, SegmentDetail, InnerRadius, OuterRadius, BoundsShift);
    }
}