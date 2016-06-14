using UnityEngine;

using System;
using System.Collections.Generic;

public sealed class SunGlare : MonoBehaviour
{
    public Atmosphere Atmosphere;
    public AtmosphereSun Sun;

    public Shader sunGlareShader;
    private Material sunGlareMaterial;

    public Texture2D sunSpikes;
    public Texture2D sunFlare;
    public Texture2D sunGhost1;
    public Texture2D sunGhost2;
    public Texture2D sunGhost3;

    public int RenderQueue = 3000;

    public bool InitUniformsInUpdate = true;

    private bool eclipse = false;

    private Vector3 sunViewPortPosition = Vector3.zero;

    private float sunGlareScale = 1;
    private float sunGlareFade = 1;

    public AnimationCurve sunGlareFadeCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f),
                                                                                  new Keyframe(1.0f, 1.0f),
                                                                                  new Keyframe(9.0f, 1.0f),
                                                                                  new Keyframe(10.0f, 0.0f) });

    Mesh screenMesh;

    public Vector3 flareSettings = new Vector3(0.45f, 1.0f, 0.85f);
    public Vector3 spikesSettings = new Vector3(0.6f, 1.0f, 1.0f);

    public List<Vector4> ghost1SettingsList = new List<Vector4> { new Vector4(0.54f, 0.65f, 2.3f, 0.5f), new Vector4(0.54f, 1.0f, 6.0f, 0.7f) };
    public List<Vector4> ghost2SettingsList = new List<Vector4> { new Vector4(0.135f, 1.0f, 3.0f, 0.9f), new Vector4(0.054f, 1.0f, 8.0f, 1.1f), new Vector4(0.054f, 1.0f, 4.0f, 1.3f), new Vector4(0.054f, 1.0f, 5.0f, 1.5f) };
    public List<Vector4> ghost3SettingsList = new List<Vector4> { new Vector4(0.135f, 1.0f, 3.0f, 0.9f), new Vector4(0.054f, 1.0f, 8.0f, 1.1f), new Vector4(0.054f, 1.0f, 4.0f, 1.3f), new Vector4(0.054f, 1.0f, 5.0f, 1.5f) };

    private Matrix4x4 ghost1Settings = Matrix4x4.zero;
    private Matrix4x4 ghost2Settings = Matrix4x4.zero;
    private Matrix4x4 ghost3Settings = Matrix4x4.zero;

    public void Start()
    {
        if (Sun == null)
            if (GetComponent<AtmosphereSun>() != null)
                Sun = GetComponent<AtmosphereSun>();

        sunGlareMaterial = new Material(sunGlareShader);
        sunGlareMaterial.renderQueue = RenderQueue;

        screenMesh = MeshFactory.MakePlane(8, 8, MeshFactory.PLANE.XY, false, false, false);
        screenMesh.bounds = new Bounds(Vector4.zero, new Vector3(9e37f, 9e37f, 9e37f));

        for (int i = 0; i < ghost1SettingsList.Count; i++)
            ghost1Settings.SetRow(i, ghost1SettingsList[i]);

        for (int i = 0; i < ghost2SettingsList.Count; i++)
            ghost2Settings.SetRow(i, ghost2SettingsList[i]);

        for (int i = 0; i < ghost3SettingsList.Count; i++)
            ghost3Settings.SetRow(i, ghost3SettingsList[i]);

        InitUniforms(sunGlareMaterial);
    }

    public void InitSetAtmosphereUniforms()
    {
        InitUniforms();
        SetUniforms();
    }

    public void InitUniforms()
    {
        InitUniforms(sunGlareMaterial);
    }

    public void InitUniforms(Material mat)
    {
        if (mat == null) return;

        sunGlareMaterial.SetTexture("sunSpikes", sunSpikes);
        sunGlareMaterial.SetTexture("sunFlare", sunFlare);
        sunGlareMaterial.SetTexture("sunGhost1", sunGhost1);
        sunGlareMaterial.SetTexture("sunGhost2", sunGhost2);
        sunGlareMaterial.SetTexture("sunGhost3", sunGhost3);

        sunGlareMaterial.SetVector("flareSettings", flareSettings);
        sunGlareMaterial.SetVector("spikesSettings", spikesSettings);
        sunGlareMaterial.SetMatrix("ghost1Settings", ghost1Settings);
        sunGlareMaterial.SetMatrix("ghost2Settings", ghost2Settings);
        sunGlareMaterial.SetMatrix("ghost3Settings", ghost2Settings);

        if (Atmosphere != null) Atmosphere.InitUniforms(sunGlareMaterial);
    }

    public void SetUniforms()
    {
        SetUniforms(sunGlareMaterial);
    }

    public void SetUniforms(Material mat)
    {
        if (mat == null) return;

        sunGlareMaterial.SetVector("sunViewPortPos", sunViewPortPosition);

        sunGlareMaterial.SetFloat("aspectRatio", CameraHelper.Main().aspect);
        sunGlareMaterial.SetFloat("sunGlareScale", sunGlareScale);
        sunGlareMaterial.SetFloat("sunGlareFade", sunGlareFade);
        sunGlareMaterial.SetFloat("useAtmosphereColors", 1.0f);
        sunGlareMaterial.SetFloat("eclipse", eclipse ? 1.0f : 0.0f);

        Atmosphere.SetUniforms(sunGlareMaterial);
    }

    public void UpdateNode()
    {
        if (Atmosphere == null || Sun == null) return;

        float dist = (CameraHelper.Main().transform.position - Sun.transform.position).magnitude;

        RaycastHit hit;

        sunViewPortPosition = CameraHelper.Main().WorldToViewportPoint(Sun.transform.position);
        sunGlareScale = dist / 2266660f;
        sunGlareFade = sunGlareFadeCurve.Evaluate(sunGlareScale);

        eclipse = false;
        eclipse = Physics.Raycast(CameraHelper.Main().transform.position, (Sun.transform.position - CameraHelper.Main().transform.position).normalized, out hit, Mathf.Infinity);

        if (!eclipse)
            eclipse = Physics.Raycast(CameraHelper.Main().transform.position, (Sun.transform.position - CameraHelper.Main().transform.position).normalized, out hit, Mathf.Infinity);

        if (InitUniformsInUpdate) InitUniforms(sunGlareMaterial);
        SetUniforms(sunGlareMaterial);
    }

    public void Update()
    {
        UpdateNode();

        if (sunViewPortPosition.z > 0)
        {
            Graphics.DrawMesh(screenMesh, Vector3.zero, Quaternion.identity, sunGlareMaterial, 10, CameraHelper.Main(), 0, null, false, false);
        }
    }
}