using UnityEngine;

using System;
using System.Collections.Generic;

public sealed class SunGlare : MonoBehaviour
{
    public Atmosphere Atmosphere;
    public AtmosphereSun Sun;

    public Shader SunGlareShader;
    private Material SunGlareMaterial;

    public Texture2D SunSpikes;
    public Texture2D SunFlare;
    public Texture2D SunGhost1;
    public Texture2D SunGhost2;
    public Texture2D SunGhost3;

    public int RenderQueue = 3000;

    public bool InitUniformsInUpdate = true;

    private bool Eclipse = false;

    private Vector3 ViewPortPosition = Vector3.zero;

    private float Scale = 1;
    private float Fade = 1;

    public AnimationCurve FadeCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f),
                                                                          new Keyframe(10.0f, 1.0f),
                                                                          new Keyframe(90.0f, 1.0f),
                                                                          new Keyframe(100.0f, 0.0f) });

    private Mesh mesh;

    public Vector3 FlareSettings = new Vector3(0.45f, 1.0f, 0.85f);
    public Vector3 SpikesSettings = new Vector3(0.6f, 1.0f, 1.0f);

    public List<Vector4> Ghost1SettingsList = new List<Vector4> { new Vector4(0.54f, 0.65f, 2.3f, 0.5f), new Vector4(0.54f, 1.0f, 6.0f, 0.7f) };
    public List<Vector4> Ghost2SettingsList = new List<Vector4> { new Vector4(0.135f, 1.0f, 3.0f, 0.9f), new Vector4(0.054f, 1.0f, 8.0f, 1.1f), new Vector4(0.054f, 1.0f, 4.0f, 1.3f), new Vector4(0.054f, 1.0f, 5.0f, 1.5f) };
    public List<Vector4> Ghost3SettingsList = new List<Vector4> { new Vector4(0.135f, 1.0f, 3.0f, 0.9f), new Vector4(0.054f, 1.0f, 8.0f, 1.1f), new Vector4(0.054f, 1.0f, 4.0f, 1.3f), new Vector4(0.054f, 1.0f, 5.0f, 1.5f) };

    private Matrix4x4 Ghost1Settings = Matrix4x4.zero;
    private Matrix4x4 Ghost2Settings = Matrix4x4.zero;
    private Matrix4x4 Ghost3Settings = Matrix4x4.zero;

    public void Start()
    {
        if (Sun == null)
            if (GetComponent<AtmosphereSun>() != null)
                Sun = GetComponent<AtmosphereSun>();

        SunGlareMaterial = MaterialHelper.CreateTemp(SunGlareShader, "Sunglare", RenderQueue);

        mesh = MeshFactory.MakePlane(8, 8, MeshFactory.PLANE.XY, false, false, false);
        mesh.bounds = new Bounds(Vector4.zero, new Vector3(9e37f, 9e37f, 9e37f));

        for (int i = 0; i < Ghost1SettingsList.Count; i++)
            Ghost1Settings.SetRow(i, Ghost1SettingsList[i]);

        for (int i = 0; i < Ghost2SettingsList.Count; i++)
            Ghost2Settings.SetRow(i, Ghost2SettingsList[i]);

        for (int i = 0; i < Ghost3SettingsList.Count; i++)
            Ghost3Settings.SetRow(i, Ghost3SettingsList[i]);

        InitUniforms(SunGlareMaterial);
    }

    public void InitSetAtmosphereUniforms()
    {
        InitUniforms();
        SetUniforms();
    }

    public void InitUniforms()
    {
        InitUniforms(SunGlareMaterial);
    }

    public void InitUniforms(Material mat)
    {
        if (mat == null) return;

        SunGlareMaterial.SetTexture("sunSpikes", SunSpikes);
        SunGlareMaterial.SetTexture("sunFlare", SunFlare);
        SunGlareMaterial.SetTexture("sunGhost1", SunGhost1);
        SunGlareMaterial.SetTexture("sunGhost2", SunGhost2);
        SunGlareMaterial.SetTexture("sunGhost3", SunGhost3);

        SunGlareMaterial.SetVector("flareSettings", FlareSettings);
        SunGlareMaterial.SetVector("spikesSettings", SpikesSettings);
        SunGlareMaterial.SetMatrix("ghost1Settings", Ghost1Settings);
        SunGlareMaterial.SetMatrix("ghost2Settings", Ghost2Settings);
        SunGlareMaterial.SetMatrix("ghost3Settings", Ghost2Settings);

        if (Atmosphere != null) Atmosphere.InitUniforms(null, SunGlareMaterial, false);
    }

    public void SetUniforms()
    {
        SetUniforms(SunGlareMaterial);
    }

    public void SetUniforms(Material mat)
    {
        if (mat == null) return;

        SunGlareMaterial.SetVector("sunViewPortPos", ViewPortPosition);

        SunGlareMaterial.SetFloat("AspectRatio", CameraHelper.Main().aspect);
        SunGlareMaterial.SetFloat("Scale", Scale);
        SunGlareMaterial.SetFloat("Fade", Fade);
        SunGlareMaterial.SetFloat("useAtmosphereColors", 1.0f);
        SunGlareMaterial.SetFloat("Eclipse", Eclipse ? 1.0f : 0.0f);

        if (Atmosphere != null) Atmosphere.SetUniforms(null, SunGlareMaterial, false, false);
    }

    public void UpdateNode()
    {
        if (Atmosphere == null || Sun == null) return;

        float distance = (CameraHelper.Main().transform.position - Sun.transform.position).magnitude;

        RaycastHit hit;

        ViewPortPosition = CameraHelper.Main().WorldToViewportPoint(Sun.transform.position);
        Scale = distance / 2266660f;
        Fade = FadeCurve.Evaluate(Mathf.Clamp(Scale, 0.0f, 100.0f));
        //Fade = FadeCurve.Evaluate(Mathf.Clamp01(VectorHelper.AngularRadius(Sun.transform.position, CameraHelper.Main().transform.position, 250000.0f)));

        Eclipse = false;
        Eclipse = Physics.Raycast(CameraHelper.Main().transform.position, (Sun.transform.position - CameraHelper.Main().transform.position).normalized, out hit, Mathf.Infinity);

        if (!Eclipse)
            Eclipse = Physics.Raycast(CameraHelper.Main().transform.position, (Sun.transform.position - CameraHelper.Main().transform.position).normalized, out hit, Mathf.Infinity);

        if (InitUniformsInUpdate) InitUniforms(SunGlareMaterial);
        SetUniforms(SunGlareMaterial);
    }

    public void Update()
    {
        UpdateNode();

        if (ViewPortPosition.z > 0)
        {
            if (Atmosphere == null) return;

            Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, SunGlareMaterial, 10, CameraHelper.Main(), 0, Atmosphere.planetoid.QuadAtmosphereMPB, false, false);
        }
    }
}