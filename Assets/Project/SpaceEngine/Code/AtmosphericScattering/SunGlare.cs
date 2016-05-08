using UnityEngine;

using System;
using System.Collections.Generic;

public sealed class SunGlare : MonoBehaviour
{
    public Atmosphere Atmosphere;
    public AtmosphereSun Sun;

    public Shader sunglareShader;
    private Material sunglareMaterial;

    public Texture2D sunSpikes;
    public Texture2D sunFlare;
    public Texture2D sunGhost1;
    public Texture2D sunGhost2;
    public Texture2D sunGhost3;

    public int RenderQueue = 3000;

    private bool eclipse = false;

    private Vector3 sunViewPortPos = Vector3.zero;

    private float sunGlareScale = 1;
    private float sunGlareFade = 1;

    public float sunGlareFadeDistance = 1000000;

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

        sunglareMaterial = new Material(sunglareShader);
        sunglareMaterial.renderQueue = RenderQueue;

        screenMesh = MeshFactory.MakePlane(2, 2, MeshFactory.PLANE.XY, false, false, false);
        screenMesh.bounds = new Bounds(Vector4.zero, new Vector3(9e37f, 9e37f, 9e37f));

        for (int i = 0; i < ghost1SettingsList.Count; i++)
            ghost1Settings.SetRow(i, ghost1SettingsList[i]);

        for (int i = 0; i < ghost2SettingsList.Count; i++)
            ghost2Settings.SetRow(i, ghost2SettingsList[i]);

        for (int i = 0; i < ghost3SettingsList.Count; i++)
            ghost3Settings.SetRow(i, ghost3SettingsList[i]);

        InitUniforms(sunglareMaterial);
    }

    public void InitSetAtmosphereUniforms()
    {
        InitUniforms();
        SetUniforms();
    }

    public void InitUniforms()
    {
        InitUniforms(sunglareMaterial);
    }

    public void InitUniforms(Material mat)
    {
        if (mat == null) return;

        sunglareMaterial.SetTexture("sunSpikes", sunSpikes);
        sunglareMaterial.SetTexture("sunFlare", sunFlare);
        sunglareMaterial.SetTexture("sunGhost1", sunGhost1);
        sunglareMaterial.SetTexture("sunGhost2", sunGhost2);
        sunglareMaterial.SetTexture("sunGhost3", sunGhost3);

        sunglareMaterial.SetVector("flareSettings", flareSettings);
        sunglareMaterial.SetVector("spikesSettings", spikesSettings);
        sunglareMaterial.SetMatrix("ghost1Settings", ghost1Settings);
        sunglareMaterial.SetMatrix("ghost2Settings", ghost2Settings);
        sunglareMaterial.SetMatrix("ghost3Settings", ghost2Settings);

        if (Atmosphere != null) Atmosphere.InitUniforms(sunglareMaterial);
    }

    public void SetUniforms()
    {
        SetUniforms(sunglareMaterial);
    }

    public void SetUniforms(Material mat)
    {
        if (mat == null) return;

        sunglareMaterial.SetVector("sunViewPortPos", sunViewPortPos);

        sunglareMaterial.SetFloat("aspectRatio", Camera.main.aspect);
        sunglareMaterial.SetFloat("sunGlareScale", sunGlareScale);
        sunglareMaterial.SetFloat("sunGlareFade", sunGlareFade);
        sunglareMaterial.SetFloat("useTransmittance", 1.0f);
        sunglareMaterial.SetFloat("eclipse", eclipse ? 1.0f : 0.0f);

        Atmosphere.SetUniforms(sunglareMaterial);
    }

    public void UpdateNode()
    {
        if (Atmosphere == null || Sun == null) return;

        float dist = (Camera.main.transform.position - Sun.transform.position).magnitude;

        RaycastHit hit;

        sunViewPortPos = Camera.main.WorldToViewportPoint(Sun.transform.position);
        sunGlareScale = dist / 2266660f;
        sunGlareFade = Mathf.SmoothStep(0.0f, 1.0f, (dist / sunGlareFadeDistance) - 0.25f);
        
        eclipse = false;
        eclipse = Physics.Raycast(Camera.main.transform.position, (Sun.transform.position - Camera.main.transform.position).normalized, out hit, Mathf.Infinity);

        if (!eclipse)
            eclipse = Physics.Raycast(Camera.main.transform.position, (Sun.transform.position - Camera.main.transform.position).normalized, out hit, Mathf.Infinity);

        SetUniforms(sunglareMaterial);
    }

    public void Update()
    {
        UpdateNode();

        if (sunViewPortPos.z > 0)
        {
            Graphics.DrawMesh(screenMesh, Vector3.zero, Quaternion.identity, sunglareMaterial, 10, Camera.main, 0, null, false, false);
        }
    }
}