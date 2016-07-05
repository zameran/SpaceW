#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2016 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using System;

using UnityEngine;

using System.Collections.Generic;

public sealed class Atmosphere : MonoBehaviour, IEventit
{
    private AtmosphereBase atmosphereBase = AtmosphereBase.Earth;
    private AtmosphereBase atmosphereBasePrev = AtmosphereBase.Earth;

    public AtmosphereBase AtmosphereBase
    {
        get { return atmosphereBase; }
        set
        {
            atmosphereBasePrev = atmosphereBase;
            atmosphereBase = value;

            if (atmosphereBasePrev != value)
                if (OnPresetChanged != null)
                    OnPresetChanged(this);
        }
    }

    public AnimationCurve FadeCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f),
                                                                          new Keyframe(0.25f, 1.0f),
                                                                          new Keyframe(1.0f, 1.0f) });

    public delegate void AtmosphereDelegate(Atmosphere a);
    public event AtmosphereDelegate OnPresetChanged, OnBaked;

    public Planetoid planetoid;

    [Range(0.0f, 1.0f)]
    public float Density = 1.0f;

    [Tooltip("1/3 or 1/2 from Planet.TerrainMaxHeight")]
    public float TerrainRadiusHold = 0.0f;
    public float Radius = 2048f;
    public float Height = 100.0f;
    public float Scale = 1.0f;
    public float Fade = 1.0f;
    public float AerialPerspectiveOffset = 2000.0f;
    public float ExtinctionGroundFade = 0.000025f;

    public int AtmosphereMeshResolution = 2;

    public float HDRExposure = 0.2f;

    public Shader SkyShader;
    public Material SkyMaterial;

    [HideInInspector] public AtmosphereHDR HDRMode = AtmosphereHDR.Proland;

    public EngineRenderQueue RenderQueue = EngineRenderQueue.Transparent;
    public int RenderQueueOffset = 0;

    public Mesh AtmosphereMesh;

    public bool LostFocusForceRebake = false;

    public AtmosphereSun Sun_1;
    public AtmosphereSun Sun_2;
    public AtmosphereSun Sun_3;
    public AtmosphereSun Sun_4;

    public List<GameObject> eclipseCasters = new List<GameObject>();
    public List<GameObject> shineCasters = new List<GameObject>();

    private AtmosphereParameters atmosphereParameters;

    public AtmosphereRunTimeBaker artb = null;

    public Vector3 Origin;

    public Color[] shineColors = new Color[4] { XKCDColors.Bluish, XKCDColors.Bluish, XKCDColors.Bluish, XKCDColors.Bluish };

    private Matrix4x4 shineColorsMatrix1;
    private Matrix4x4 shineOccludersMatrix1;
    private Matrix4x4 occludersMatrix1;
    private Matrix4x4 occludersMatrix2;
    private Matrix4x4 sunMatrix1;
    private Matrix4x4 worldToCamera;
    private Matrix4x4 cameraToWorld;
    private Matrix4x4 cameraToScreen;
    private Matrix4x4 screenToCamera;

    private Vector3 worldCameraPos;

    public List<string> Keywords = new List<string>();

    public Matrix4x4 ShineColorsMatrix1
    {
        get { return shineColorsMatrix1; }
        set { shineColorsMatrix1 = value; }
    }

    public Matrix4x4 ShineOccludersMatrix1
    {
        get { return shineColorsMatrix1; }
        set { shineColorsMatrix1 = value; }
    }

    public Matrix4x4 OccludersMatrix1
    {
        get { return occludersMatrix1; }
        set { occludersMatrix1 = value; }
    }

    public Matrix4x4 OccludersMatrix2
    {
        get { return occludersMatrix2; }
        set { occludersMatrix2 = value; }
    }

    public Matrix4x4 SunMatrix1
    {
        get { return sunMatrix1; }
        set { sunMatrix1 = value; }
    }

    public Matrix4x4 WorldToCamera
    {
        get { return worldToCamera; }
        set { worldToCamera = value; }
    }

    public Matrix4x4 CameraToWorld
    {
        get { return cameraToWorld; }
        set { cameraToWorld = value; }
    }

    public Matrix4x4 CameraToScreen
    {
        get { return cameraToScreen; }
        set { cameraToScreen = value; }
    }

    public Matrix4x4 ScreenToCamera
    {
        get { return screenToCamera; }
        set { screenToCamera = value; }
    }

    public Vector3 WorldCameraPos
    {
        get { return worldCameraPos; }
        set { worldCameraPos = value; }
    }

    #region Eventit
    public bool isEventit { get; set; }

    public void Eventit()
    {
        if (isEventit) return;

        OnPresetChanged += AtmosphereOnPresetChanged;
        OnBaked += AtmosphereOnBaked;

        isEventit = true;
    }

    public void UnEventit()
    {
        if (!isEventit) return;

        OnPresetChanged -= AtmosphereOnPresetChanged;
        OnBaked -= AtmosphereOnBaked;

        isEventit = false;
    }
    #endregion

    private void Start()
    {
        Eventit();

        ApplyTestPresset(AtmosphereParameters.Get(atmosphereBase));

        TryBake();

        InitMisc();
        InitMaterials();
        InitMesh();
        InitSuns();

        InitSetAtmosphereUniforms();
    }

    private void ApplyTestPresset(AtmosphereParameters p)
    {
        atmosphereParameters = new AtmosphereParameters(p);

        atmosphereParameters.Rg = Radius - TerrainRadiusHold;
        atmosphereParameters.Rt = (Radius + Height) - TerrainRadiusHold;
        atmosphereParameters.Rl = (Radius + Height * 1.05f) - TerrainRadiusHold;
        atmosphereParameters.SCALE = Scale;
    }

    public void TryBake()
    {
        if (artb != null) artb.Bake(atmosphereParameters);

        if (OnBaked != null) OnBaked(this);
    }

    public List<string> GetKeywords()
    {
        Planetoid planet = transform.parent.GetComponent<Planet>() as Planetoid;

        if(planet != null)
        {
            return planet.GetKeywords();
        }
        else
        {
            Debug.Log("Atmosphere: GetKeywords problem!");

            return null;
        }
    }

    public void SetKeywords(Material m, List<string> keywords, bool checkShaderKeywords = false)
    {
        if (checkShaderKeywords)
        {
            if (m != null && ArraysEqual(m.shaderKeywords, keywords) == false)
            {
                m.shaderKeywords = keywords.ToArray();
            }
        }
        else
        {
            m.shaderKeywords = keywords.ToArray();
        }
    }

    public void CalculateShine(out Matrix4x4 soc1, out Matrix4x4 sc1)
    {
        soc1 = Matrix4x4.zero;
        sc1 = Matrix4x4.zero;

        int index = 0;

        for (int i = 0; i < Mathf.Min(4, shineCasters.Count); i++)
        {
            if (shineCasters[i] == null) { Debug.Log("Atmosphere: Shine problem!"); break; }

            float distance = shineColors[i].a; //TODO : Distance based shine power.

            soc1.SetRow(i, VectorHelper.MakeFrom((shineCasters[i].transform.position - Origin).normalized, 1.0f));

            sc1.SetRow(index, VectorHelper.FromColor(shineColors[i], distance));

            index++;
        }
    }

    public void CalculateEclipses(out Matrix4x4 oc1, out Matrix4x4 oc2, out Matrix4x4 suns)
    {
        oc1 = Matrix4x4.zero;
        oc2 = Matrix4x4.zero;
        suns = Matrix4x4.zero;

        float actualRadius = 250000.0f;

        List<AtmosphereSun> Suns = new List<AtmosphereSun>();

        if (Sun_1 != null) Suns.Add(Sun_1);
        if (Sun_2 != null) Suns.Add(Sun_2);
        if (Sun_3 != null) Suns.Add(Sun_3);
        if (Sun_4 != null) Suns.Add(Sun_4);

        for (int i = 0; i < Mathf.Min(4, Suns.Count); i++)
        {
            suns.SetRow(i, VectorHelper.MakeFrom(Suns[i].transform.position, VectorHelper.AngularRadius(Suns[i].transform.position, Origin, Suns[i].Radius)));
        }

        for (int i = 0; i < Mathf.Min(4, eclipseCasters.Count); i++)
        {
            if (eclipseCasters[i] == null) { Debug.Log("Atmosphere: Eclipses problem!"); break; }

            oc1.SetRow(i, VectorHelper.MakeFrom(eclipseCasters[i].transform.position, actualRadius));
        }

        for (int i = 4; i < Mathf.Min(8, eclipseCasters.Count); i++)
        {
            if (eclipseCasters[i] == null) { Debug.Log("Atmosphere: Eclipses problem!"); break; }

            oc2.SetRow(i - 4, VectorHelper.MakeFrom(eclipseCasters[i].transform.position, actualRadius));
        }
    }

    public void SetShine(Material mat)
    {
        CalculateShine(out shineOccludersMatrix1, out shineColorsMatrix1);

        mat.SetMatrix("_Sky_ShineOccluders_1", shineOccludersMatrix1);
        mat.SetMatrix("_Sky_ShineColors_1", shineColorsMatrix1);
    }

    public void SetShine(MaterialPropertyBlock block)
    {
        CalculateShine(out shineOccludersMatrix1, out shineColorsMatrix1);

        block.SetMatrix("_Sky_ShineOccluders_1", shineOccludersMatrix1);
        block.SetMatrix("_Sky_ShineColors_1", shineColorsMatrix1);
    }

    public void SetEclipses(Material mat)
    {
        CalculateEclipses(out occludersMatrix1, out occludersMatrix2, out sunMatrix1);

        mat.SetMatrix("_Sky_LightOccluders_1", occludersMatrix1);
        mat.SetMatrix("_Sky_LightOccluders_2", occludersMatrix2);
        mat.SetMatrix("_Sun_Positions_1", sunMatrix1);
    }

    public void SetEclipses(MaterialPropertyBlock block)
    {
        CalculateEclipses(out occludersMatrix1, out occludersMatrix2, out sunMatrix1);

        block.SetMatrix("_Sky_LightOccluders_1", occludersMatrix1);
        block.SetMatrix("_Sky_LightOccluders_2", occludersMatrix2);
        block.SetMatrix("_Sun_Positions_1", sunMatrix1);
    }

    public bool ArraysEqual<T>(T[] a, List<T> b)
    {
        if (a != null && b == null) return false;
        if (a == null && b != null) return false;

        if (a != null && b != null)
        {
            if (a.Length != b.Count) return false;

            var comparer = EqualityComparer<T>.Default;

            for (var i = 0; i < a.Length; i++)
            {
                if (comparer.Equals(a[i], b[i]) == false)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void Update()
    {
        if (Sun_1 != null) Sun_1.UpdateNode();
        if (Sun_2 != null) Sun_2.UpdateNode();
        if (Sun_3 != null) Sun_3.UpdateNode();
        if (Sun_4 != null) Sun_4.UpdateNode();

        UpdateNode();
    }

    public void UpdateNode()
    {
        atmosphereParameters.Rg = Radius - TerrainRadiusHold;
        atmosphereParameters.Rt = (Radius + Height) - TerrainRadiusHold;
        atmosphereParameters.Rl = (Radius + Height * 1.05f) - TerrainRadiusHold;
        atmosphereParameters.SCALE = Scale;

        if (Sun_1 != null) Sun_1.Origin = Origin;
        if (Sun_2 != null) Sun_2.Origin = Origin;
        if (Sun_3 != null) Sun_3.Origin = Origin;
        if (Sun_4 != null) Sun_4.Origin = Origin;

        worldToCamera = CameraHelper.Main().GetWorldToCamera();
        cameraToWorld = CameraHelper.Main().GetCameraToWorld();
        cameraToScreen = CameraHelper.Main().GetCameraToScreen();
        screenToCamera = CameraHelper.Main().GetScreenToCamera();
        worldCameraPos = CameraHelper.Main().transform.position;

        var fadeValue = Mathf.Clamp01(VectorHelper.AngularRadius(Origin, planetoid.LODTarget.position, planetoid.PlanetRadius));

        Fade = FadeCurve.Evaluate(float.IsNaN(fadeValue) || float.IsInfinity(fadeValue) ? 1.0f : fadeValue);

        Keywords = GetKeywords();
    }

    public void OnApplicationFocus(bool focusStatus)
    {
        if (focusStatus == true && LostFocusForceRebake == true)
        {
            TryBake();
        }
    }

    public void Render(Vector3 Origin, int drawLayer = 8)
    {
        Render(CameraHelper.Main(), Origin, drawLayer);
    }

    public void Render(Camera camera, Vector3 Origin, int drawLayer = 8)
    {
        this.Origin = Origin;

        SetUniforms(planetoid.QuadAtmosphereMPB, SkyMaterial, true);

        SkyMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

        Graphics.DrawMesh(AtmosphereMesh, transform.localToWorldMatrix, SkyMaterial, drawLayer, camera, 0, planetoid.QuadAtmosphereMPB);
    }

    private void OnDestroy()
    {
        UnEventit();
    }

    private void OnDrawGizmos()
    {
        if (planetoid != null)
        {
            if (planetoid.DrawGizmos == false) return;

            //NOTE : Sun Direction is sun's tranform.forward with sun rotation applyied...

            List<AtmosphereSun> Suns = new List<AtmosphereSun>();

            if (Sun_1 != null) Suns.Add(Sun_1);
            if (Sun_2 != null) Suns.Add(Sun_2);
            if (Sun_3 != null) Suns.Add(Sun_3);
            if (Sun_4 != null) Suns.Add(Sun_4);

            for (int i = 0; i < Mathf.Min(4, Suns.Count); i++)
            {
                float sunRadius = Suns[i].Radius;
                float sunToPlanetDistance = Vector3.Distance(planetoid.Origin, Suns[i].transform.position);
                float umbraLength = CalculateUmbraLength(planetoid.PlanetRadius * 2, sunRadius, sunToPlanetDistance);
                float umbraAngle = CalculateUmbraSubtendedAngle(planetoid.PlanetRadius * 2, umbraLength);

                Vector3 direction = (Suns[i].GetDirection() * umbraLength);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(Suns[i].transform.position, sunRadius);
                Gizmos.DrawRay(Suns[i].transform.position, (direction / umbraLength) * -sunToPlanetDistance);

                Gizmos.color = Color.red;
                Gizmos.DrawRay(planetoid.Origin, direction);

                Gizmos.color = Color.blue;
                Gizmos.DrawRay(planetoid.transform.InverseTransformVector(planetoid.Origin + direction), -(Quaternion.Euler(umbraAngle, 0, 0) * direction));
                Gizmos.DrawRay(planetoid.transform.InverseTransformVector(planetoid.Origin + direction), -(Quaternion.Euler(-umbraAngle, 0, 0) * direction));
                Gizmos.DrawRay(planetoid.transform.InverseTransformVector(planetoid.Origin + direction), -(Quaternion.Euler(0, umbraAngle, 0) * direction));
                Gizmos.DrawRay(planetoid.transform.InverseTransformVector(planetoid.Origin + direction), -(Quaternion.Euler(0, -umbraAngle, 0) * direction));
            }
        }
    }

    private void AtmosphereOnPresetChanged(Atmosphere a)
    {
        //Debug.Log("Atmosphere: AtmosphereOnPresetChanged() - " + a.gameObject.name);

        ApplyTestPresset(AtmosphereParameters.Get(AtmosphereBase));
        TryBake();
    }

    private void AtmosphereOnBaked(Atmosphere a)
    {
        //Debug.Log("Atmosphere: AtmosphereOnBaked() - " + a.gameObject.name);

        //Just make sure that all Origin variables set.
        if (a.transform.parent != null)
        {
            Planetoid owner = a.GetComponentInParent<Planetoid>();

            if (owner != null)
                owner.ReSetupQuads();
        }
    }

    public void InitMaterials()
    {
        if (SkyMaterial == null)
        {
            SkyMaterial = MaterialHelper.CreateTemp(SkyShader, "Sky");
        }
    }

    public void InitMesh()
    {
        AtmosphereMesh = MeshFactory.MakePlane(AtmosphereMeshResolution, AtmosphereMeshResolution, MeshFactory.PLANE.XY, false, false, false);
        AtmosphereMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));
    }

    public void InitMisc()
    {
        worldToCamera = CameraHelper.Main().GetWorldToCamera();
        cameraToWorld = CameraHelper.Main().GetCameraToWorld();
        cameraToScreen = CameraHelper.Main().GetCameraToScreen();
        screenToCamera = CameraHelper.Main().GetScreenToCamera();
        worldCameraPos = CameraHelper.Main().transform.position;

        Keywords = GetKeywords();
    }

    public void InitSuns()
    {
        AtmosphereSun[] suns = FindObjectsOfType<AtmosphereSun>();

        if (suns.Length > 0) if (Sun_1 == null && suns[0] != null) Sun_1 = suns[0];
        if (suns.Length > 1) if (Sun_2 == null && suns[1] != null) Sun_2 = suns[1];
        if (suns.Length > 2) if (Sun_3 == null && suns[2] != null) Sun_3 = suns[2];
        if (suns.Length > 3) if (Sun_4 == null && suns[3] != null) Sun_4 = suns[3];
    }

    public void InitUniforms(MaterialPropertyBlock block, Material mat, bool full = true)
    {
        if (mat != null)
        {
            SetKeywords(mat, Keywords);
        }

        if (full)
        {
            if (block == null) return;

            SetEclipses(block);
            SetShine(block);

            //block.SetTexture("_Sun_Glare", SunGlareTexture);

            block.SetFloat("_Aerial_Perspective_Offset", AerialPerspectiveOffset);

            block.SetFloat("Rg", atmosphereParameters.Rg);
            block.SetFloat("Rt", atmosphereParameters.Rt);
            block.SetFloat("RL", atmosphereParameters.Rl);

            block.SetFloat("TRANSMITTANCE_W", AtmosphereConstants.TRANSMITTANCE_W);
            block.SetFloat("TRANSMITTANCE_H", AtmosphereConstants.TRANSMITTANCE_H);
            block.SetFloat("SKY_W", AtmosphereConstants.SKY_W);
            block.SetFloat("SKY_H", AtmosphereConstants.SKY_H);
            block.SetFloat("RES_R", AtmosphereConstants.RES_R);
            block.SetFloat("RES_MU", AtmosphereConstants.RES_MU);
            block.SetFloat("RES_MU_S", AtmosphereConstants.RES_MU_S);
            block.SetFloat("RES_NU", AtmosphereConstants.RES_NU);
            block.SetFloat("AVERAGE_GROUND_REFLECTANCE", atmosphereParameters.AVERAGE_GROUND_REFLECTANCE);
            block.SetFloat("HR", atmosphereParameters.HR * 1000.0f);
            block.SetFloat("HM", atmosphereParameters.HM * 1000.0f);
        }
    }

    public void InitPlanetoidUniforms(Planetoid planetoid)
    {
        if (planetoid.Atmosphere != null)
        {
            for (int i = 0; i < planetoid.Quads.Count; i++)
            {
                if (planetoid.Quads[i] != null)
                {
                    planetoid.Atmosphere.InitUniforms(null, planetoid.Quads[i].QuadMaterial, false);
                }
            }

            //Just make sure that all mpb parameters are set.
            planetoid.Atmosphere.InitUniforms(planetoid.QuadAtmosphereMPB, null, true);
        }
    }

    public void SetPlanetoidUniforms(Planetoid planetoid)
    {
        if (planetoid.Atmosphere != null)
        {
            for (int i = 0; i < planetoid.Quads.Count; i++)
            {
                if (planetoid.Quads[i] != null)
                {
                    planetoid.Atmosphere.SetUniforms(null, planetoid.Quads[i].QuadMaterial, false, true);
                }
            }

            //Just make sure that all mpb parameters are set.
            planetoid.Atmosphere.SetUniforms(planetoid.QuadAtmosphereMPB, null, true, true);
        }
    }

    public void InitSetPlanetoidUniforms(Planetoid planetoid)
    {
        InitPlanetoidUniforms(planetoid);
        SetPlanetoidUniforms(planetoid);
    }

    public void InitSetAtmosphereUniforms()
    {
        InitUniforms(planetoid.QuadAtmosphereMPB, SkyMaterial, true);
        SetUniforms(planetoid.QuadAtmosphereMPB, SkyMaterial, true);
    }

    public void InitSetAtmosphereUniforms(Atmosphere atmosphere)
    {
        InitUniforms(planetoid.QuadAtmosphereMPB, SkyMaterial, true);
        SetUniforms(planetoid.QuadAtmosphereMPB, SkyMaterial, true);
    }

    public void ReanimateAtmosphereUniforms(Atmosphere atmosphere, Planetoid planetoid)
    {
        if (atmosphere != null && planetoid != null)
        {
            atmosphere.InitSetPlanetoidUniforms(planetoid);
            atmosphere.InitSetAtmosphereUniforms();

            if (Sun_1 != null)
            {
                SunGlare sg_1 = Sun_1.GetComponent<SunGlare>();

                if (sg_1 != null)
                    sg_1.InitSetAtmosphereUniforms();
            }

            if (Sun_2 != null)
            {
                SunGlare sg_2 = Sun_2.GetComponent<SunGlare>();

                if (sg_2 != null)
                    sg_2.InitSetAtmosphereUniforms();
            }

            if (Sun_3 != null)
            {
                SunGlare sg_3 = Sun_3.GetComponent<SunGlare>();

                if (sg_3 != null)
                    sg_3.InitSetAtmosphereUniforms();
            }

            if (Sun_4 != null)
            {
                SunGlare sg_4 = Sun_4.GetComponent<SunGlare>();

                if (sg_4 != null)
                    sg_4.InitSetAtmosphereUniforms();
            }
        }
        else
            Debug.Log("Atmosphere: Reanimation fail!");
    }

    public void SetUniforms(MaterialPropertyBlock block, Material mat, bool full = true, bool forQuad = false)
    {
        if (artb == null) { Debug.Log("Atmosphere: ARTB is null!"); return; }

        if (mat != null)
        {
            SetKeywords(mat, Keywords);
        }

        if (full)
        {
            if (block == null) return;

            SetEclipses(block);
            SetShine(block);

            block.SetFloat("fade", Fade);
            block.SetFloat("density", Density);
            block.SetFloat("scale", atmosphereParameters.SCALE);
            block.SetFloat("Rg", atmosphereParameters.Rg);
            block.SetFloat("Rt", atmosphereParameters.Rt);
            block.SetFloat("RL", atmosphereParameters.Rl);
            block.SetVector("betaR", atmosphereParameters.BETA_R / 1000);
            block.SetVector("betaMSca", atmosphereParameters.BETA_MSca / 1000);
            block.SetVector("betaMEx", atmosphereParameters.BETA_MEx / 1000);
            block.SetFloat("mieG", Mathf.Clamp(atmosphereParameters.MIE_G, 0.0f, 0.99f));

            block.SetFloat("_Aerial_Perspective_Offset", AerialPerspectiveOffset);
            block.SetFloat("_ExtinctionGroundFade", ExtinctionGroundFade);

            block.SetFloat("_Sun_Glare_Scale", 0.1f);

            if (artb.transmittanceT != null) block.SetTexture("_Sky_Transmittance", artb.transmittanceT);
            if (artb.inscatterT_Read != null) block.SetTexture("_Sky_Inscatter", artb.inscatterT_Read);
            if (artb.irradianceT_Read != null) block.SetTexture("_Sky_Irradiance", artb.irradianceT_Read);

            block.SetMatrix("_Globals_WorldToCamera", worldToCamera);
            block.SetMatrix("_Globals_CameraToWorld", cameraToWorld);
            block.SetMatrix("_Globals_CameraToScreen", cameraToScreen);
            block.SetMatrix("_Globals_ScreenToCamera", screenToCamera);
            block.SetVector("_Globals_WorldCameraPos", forQuad == true ? worldCameraPos - Origin : worldCameraPos);

            block.SetVector("_Globals_Origin", -Origin);
            block.SetFloat("_Exposure", HDRExposure);
            block.SetFloat("_HDRMode", (int)HDRMode);

            if (Sun_1 != null) Sun_1.SetUniforms(block);
            if (Sun_2 != null) Sun_2.SetUniforms(block);
            if (Sun_3 != null) Sun_3.SetUniforms(block);
            if (Sun_4 != null) Sun_4.SetUniforms(block);
        }
    }

    #region ExtraAPI

    public float CalculateUmbraLength(float planetDiameter, float sunDiameter, float distance)
    {
        return (planetDiameter * distance) / (sunDiameter - planetDiameter);
    }

    public float CalculateUmbraSubtendedAngle(float planetDiameter, float umbraLength)
    {
        return Mathf.Asin(planetDiameter / (umbraLength * 2.0f)) * Mathf.Rad2Deg;
    }

    #endregion
}