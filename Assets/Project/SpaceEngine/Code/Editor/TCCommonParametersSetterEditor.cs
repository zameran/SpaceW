#region License
/* Procedural planet generator.
 *
 * Copyright (C) 2015-2017 Denis Ovchinnikov
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

using System;

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TCCommonParametersSetter))]
public sealed class TCCommonParametersSetterEditor : Editor
{
    private enum Tab
    {
        Main, Noise, Texturing, Clouds, Nature, Montes, Dunes, Hills, Canyons, Rivers, Cracks, Craters, Volcanoes, Mare, Venus
    }

    private Tab currentTab = Tab.Main;
    private Tab prevTab = Tab.Main;

    private void DrawGUIForMain(TCCommonParametersSetter setter)
    {
        EditorGUILayout.LabelField("Auto update: ", EditorStyles.boldLabel);
        setter.AutoUpdate = EditorGUILayout.Toggle(setter.AutoUpdate);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Planetoid to setup: ", EditorStyles.boldLabel);
        setter.Planet = EditorGUILayout.ObjectField(setter.Planet, typeof(Planetoid), true) as Planetoid;

        EditorGUILayout.LabelField("Patche Sphere to setup: ", EditorStyles.boldLabel);
        setter.PatchSphere = EditorGUILayout.ObjectField(setter.PatchSphere, typeof(PatchSphere), true) as PatchSphere;

        EditorGUILayout.Space();
    }

    private void DrawGUIForNoise(TCCommonParametersSetter setter)
    {
        setter.Lacunarity = EditorGUILayout.Slider("Lacunarity ", setter.Lacunarity, 0.25f, 8.0f);
        setter.H = EditorGUILayout.Slider("H ", setter.H, 0.0f, 1.0f);
        setter.Offset = EditorGUILayout.Slider("Offset ", setter.Offset, 0.0f, 1.0f);
        setter.RidgeSmooth = EditorGUILayout.Slider("Ridge Smooth ", setter.RidgeSmooth, 0.0000001f, 0.01f);
        EditorGUILayout.Separator();

        setter.scaleParams.w = EditorGUILayout.Slider("tidalLock ", setter.scaleParams.w, -1.0f, 1.0f);

        setter.Randomize.x = EditorGUILayout.Slider("Randomize.x ", setter.Randomize.x, -2.0f, 2.0f);
        setter.Randomize.y = EditorGUILayout.Slider("Randomize.y ", setter.Randomize.y, -2.0f, 2.0f);
        setter.Randomize.z = EditorGUILayout.Slider("Randomize.z ", setter.Randomize.z, -2.0f, 2.0f);

        setter.mainParams.x = EditorGUILayout.Slider("mainFreq ", setter.mainParams.x, 0.0f, 1000.0f);

        setter.canyonsParams.w = EditorGUILayout.Slider("erosion", setter.canyonsParams.w, 0.0f, 1.0f);

        setter.mainParams.y = EditorGUILayout.Slider("terraceProb ", setter.mainParams.y, 0.0f, 1.0f);
    }

    private void DrawGUIForTexturing(TCCommonParametersSetter setter)
    {
        setter.PlanetGlobalColor = EditorGUILayout.ColorField("Global Color ", setter.PlanetGlobalColor);

        setter.InvSize.x = EditorGUILayout.Slider("InvSize x ", setter.InvSize.x, 0.0f, 64.0f);
        setter.InvSize.y = EditorGUILayout.Slider("InvSize y ", setter.InvSize.y, 0.0f, 64.0f);

        setter.texturingUVAtlasOffset.x = EditorGUILayout.Slider("UV offset x ", setter.texturingUVAtlasOffset.x, 0.0f, 2.0f);
        setter.texturingUVAtlasOffset.y = EditorGUILayout.Slider("UV offset y ", setter.texturingUVAtlasOffset.y, 0.0f, 2.0f);

        setter.faceParams.x = EditorGUILayout.Slider("x0 ", setter.faceParams.x, -10.0f, 10.0f);
        setter.faceParams.y = EditorGUILayout.Slider("y0 ", setter.faceParams.y, -10.0f, 10.0f);
        setter.faceParams.z = EditorGUILayout.Slider("size ", setter.faceParams.z, 0.0f, 4096.0f);
        setter.faceParams.w = EditorGUILayout.Slider("face ", setter.faceParams.w, 0.0f, 6.0f);

        setter.textureParams.x = EditorGUILayout.Slider("texScale ", setter.textureParams.x, 0.0f, 4096.0f);
        setter.textureParams.y = EditorGUILayout.Slider("texColorConv ", setter.textureParams.y, -1.0f, 1.0f);

        setter.texturingHeightOffset = EditorGUILayout.Slider("texturingHeightOffset ", setter.texturingHeightOffset, -1.0f, 1.0f);
        setter.texturingSlopeOffset = EditorGUILayout.Slider("texturingSlopeOffset ", setter.texturingSlopeOffset, -1.0f, 1.0f);

        setter.colorParams.x = EditorGUILayout.Slider("colorDistMagn ", setter.colorParams.x, 0.0f, 100.0f);
        setter.colorParams.y = EditorGUILayout.Slider("colorDistFreq ", setter.colorParams.y, 0.0f, 10000.0f);

        setter.lavaParams.x = EditorGUILayout.Slider("lavaCoverage ", setter.lavaParams.x, -10.0f, 1.0f);
        setter.lavaParams.y = EditorGUILayout.Slider("lavaTemperature ", setter.lavaParams.y, 0.0f, 373.0f);
        setter.lavaParams.z = EditorGUILayout.Slider("surfTemperature ", setter.lavaParams.z, 0.0f, 373.0f);
        setter.lavaParams.w = EditorGUILayout.Slider("heightTempGrad ", setter.lavaParams.w, 0.0f, 1.0f);

        setter.dunesParams.w = EditorGUILayout.Slider("drivenDarkening ", setter.dunesParams.w, -1.0f, 1.0f);
    }

    private void DrawGUIForClouds(TCCommonParametersSetter setter)
    {
        setter.cloudsParams2.x = EditorGUILayout.Slider("cloudsLayer ", setter.cloudsParams2.x, 0.0f, 1.0f);
        setter.cloudsParams2.y = EditorGUILayout.Slider("cloudsNLayers ", setter.cloudsParams2.y, 0.0f, 4.0f);
        setter.cloudsParams2.z = EditorGUILayout.Slider("cloudsCoverage ", setter.cloudsParams2.z, 0.0f, 1.0f);
        setter.cloudsParams2.w = EditorGUILayout.Slider("stripeFluct ", setter.cloudsParams2.w, -1.0f, 1.0f);


        setter.cloudsParams1.x = EditorGUILayout.Slider("cloudsFreq ", setter.cloudsParams1.x, 0.0f, 1000.0f);
        setter.cloudsParams1.y = (float)Mathf.RoundToInt(EditorGUILayout.Slider("cloudsOctaves ", setter.cloudsParams1.y, 0.0f, 6.0f));
        setter.cloudsParams1.z = EditorGUILayout.Slider("stripeZones ", setter.cloudsParams1.z, 0.0f, 10.0f);
        setter.cloudsParams1.w = EditorGUILayout.Slider("stripeMagn ", setter.cloudsParams1.w, 0.0f, 100.0f);

        setter.cycloneParams.x = EditorGUILayout.Slider("cycloneMagn ", setter.cycloneParams.x, 0.0f, 100.0f);
        setter.cycloneParams.y = EditorGUILayout.Slider("cycloneFreq ", setter.cycloneParams.y, 0.0f, 1000.0f);
        setter.cycloneParams.z = EditorGUILayout.Slider("cycloneDensity ", setter.cycloneParams.z, 0.0f, 1.0f);
        setter.cycloneParams.w = (float)Mathf.RoundToInt(EditorGUILayout.Slider("cycloneOctaves ", setter.cycloneParams.w, 0.0f, 6.0f));
    }

    private void DrawGUIForNature(TCCommonParametersSetter setter)
    {
        setter.mareParams.x = EditorGUILayout.Slider("seaLevel ", setter.mareParams.x, -1.0f, 1.0f);
        setter.mainParams.w = EditorGUILayout.Slider("snowLevel ", setter.mainParams.w, 0.0f, 10.0f);

        setter.mainParams.z = EditorGUILayout.Slider("surfType ", setter.mainParams.z, 0.0f, 6.0f);

        setter.climateParams.x = EditorGUILayout.Slider("climatePole ", setter.climateParams.x, 0.0f, 1.0f);
        setter.climateParams.y = EditorGUILayout.Slider("climateTropic ", setter.climateParams.y, 0.0f, 1.0f);
        setter.climateParams.z = EditorGUILayout.Slider("climateEquator ", setter.climateParams.z, 0.0f, 1.0f);

        setter.climateParams.w = EditorGUILayout.Slider("tropicWidth ", setter.climateParams.w, 0.0f, 1.0f);

        setter.colorParams.w = EditorGUILayout.Slider("latTropic ", setter.colorParams.w, 0.0f, 1.0f);
        setter.colorParams.z = EditorGUILayout.Slider("latIceCaps ", setter.colorParams.z, 0.0f, 1.0f);

        setter.mareParams.w = EditorGUILayout.Slider("icecapHeight ", setter.mareParams.w, 0.0f, 1.0f);
    }

    private void DrawGUIForMontes(TCCommonParametersSetter setter)
    {
        setter.montesParams.x = EditorGUILayout.Slider("montesMagn ", setter.montesParams.x, 0.0f, 100.0f);
        setter.montesParams.y = EditorGUILayout.Slider("montesFreq ", setter.montesParams.y, 0.0f, 1000.0f);
        setter.montesParams.z = EditorGUILayout.Slider("montesFraction ", setter.montesParams.z, 0.0f, 1.0f);
        setter.montesParams.w = EditorGUILayout.Slider("montesSpiky ", setter.montesParams.w, 0.0f, 1.0f);
    }

    private void DrawGUIForDunes(TCCommonParametersSetter setter)
    {
        setter.dunesParams.x = EditorGUILayout.Slider("dunesMagn ", setter.dunesParams.x, 0.0f, 100.0f);
        setter.dunesParams.y = EditorGUILayout.Slider("dunesFreq ", setter.dunesParams.y, 0.0f, 1000.0f);
        setter.dunesParams.z = EditorGUILayout.Slider("dunesFraction ", setter.dunesParams.z, 0.0f, 1.0f);
    }

    private void DrawGUIForHills(TCCommonParametersSetter setter)
    {
        setter.hillsParams.x = EditorGUILayout.Slider("hillsMagn ", setter.hillsParams.x, 0.0f, 100.0f);
        setter.hillsParams.y = EditorGUILayout.Slider("hillsFreq ", setter.hillsParams.y, 0.0f, 1000.0f);
        setter.hillsParams.z = EditorGUILayout.Slider("hillsFraction ", setter.hillsParams.z, 0.0f, 1.0f);
        setter.hillsParams.w = EditorGUILayout.Slider("hills2Fraction ", setter.hillsParams.w, 0.0f, 1.0f);
    }

    private void DrawGUIForCanyons(TCCommonParametersSetter setter)
    {
        setter.canyonsParams.x = EditorGUILayout.Slider("canyonsMagn ", setter.canyonsParams.x, 0.0f, 100.0f);
        setter.canyonsParams.y = EditorGUILayout.Slider("canyonsFreq ", setter.canyonsParams.y, 0.0f, 1000.0f);
        setter.canyonsParams.z = EditorGUILayout.Slider("canyonsFraction ", setter.canyonsParams.z, 0.0f, 1.0f);
    }

    private void DrawGUIForRivers(TCCommonParametersSetter setter)
    {
        setter.riversParams.x = EditorGUILayout.Slider("riversMagn ", setter.riversParams.x, 0.0f, 100.0f);
        setter.riversParams.y = EditorGUILayout.Slider("riversFreq ", setter.riversParams.y, 0.0f, 1000.0f);
        setter.riversParams.z = EditorGUILayout.Slider("riversSin ", setter.riversParams.z, 0.0f, 10.0f);
        setter.riversParams.w = (float)Mathf.RoundToInt(EditorGUILayout.Slider("riversOctaves ", setter.riversParams.w, 0.0f, 5.0f));
    }

    private void DrawGUIForCracks(TCCommonParametersSetter setter)
    {
        setter.cracksParams.x = EditorGUILayout.Slider("cracksMagn ", setter.cracksParams.x, 0.0f, 100.0f);
        setter.cracksParams.y = EditorGUILayout.Slider("cracksFreq ", setter.cracksParams.y, 0.0f, 1000.0f);
        setter.cracksParams.z = (float)Mathf.RoundToInt(EditorGUILayout.Slider("cracksOctaves ", setter.cracksParams.z, 0.0f, 15.0f));
    }

    private void DrawGUIForCraters(TCCommonParametersSetter setter)
    {
        setter.craterParams.x = EditorGUILayout.Slider("craterMagn ", setter.craterParams.x, 0.0f, 10000.0f);
        setter.craterParams.y = EditorGUILayout.Slider("craterFreq ", setter.craterParams.y, 0.0f, 1000.0f);
        setter.craterParams.z = EditorGUILayout.Slider("craterDensity ", setter.craterParams.z, 0.0f, 2.0f);
        setter.craterParams.w = (float)Mathf.RoundToInt(EditorGUILayout.Slider("craterOctaves ", setter.craterParams.w, 0.0f, 20.0f));
        setter.cracksParams.w = EditorGUILayout.Slider("craterRayedFactor ", setter.cracksParams.w, 0.0f, 1.0f);
    }

    private void DrawGUIForVolcanoes(TCCommonParametersSetter setter)
    {
        setter.volcanoParams1.x = EditorGUILayout.Slider("volcanoMagn ", setter.volcanoParams1.x, 0.0f, 100.0f);
        setter.volcanoParams1.y = EditorGUILayout.Slider("volcanoFreq ", setter.volcanoParams1.y, 0.0f, 1000.0f);
        setter.volcanoParams1.z = EditorGUILayout.Slider("volcanoDensity ", setter.volcanoParams1.z, 0.0f, 1.0f);
        setter.volcanoParams1.w = (float)Mathf.RoundToInt(EditorGUILayout.Slider("volcanoOctaves ", setter.volcanoParams1.w, 0.0f, 5.0f));

        setter.volcanoParams2.z = EditorGUILayout.Slider("volcanoRadius ", setter.volcanoParams2.z, 0.0f, 5.0f);
        setter.volcanoParams2.y = EditorGUILayout.Slider("volcanoFlows ", setter.volcanoParams2.y, -1.0f, 1.0f);
        setter.volcanoParams2.w = EditorGUILayout.Slider("volcanoTemp ", setter.volcanoParams2.w, 0.0f, 3000.0f);
        setter.volcanoParams2.x = EditorGUILayout.Slider("volcanoActivity ", setter.volcanoParams2.x, 0.0f, 2.0f);
    }

    private void DrawGUIForMare(TCCommonParametersSetter setter)
    {
        setter.mareParams.y = EditorGUILayout.Slider("mareFreq ", setter.mareParams.y, 0.001f, 1000.0f);
        setter.mareParams.z = EditorGUILayout.Slider("mareDensity ", setter.mareParams.z, 0.0f, 1.0f);
    }

    private void DrawGUIForVenus(TCCommonParametersSetter setter)
    {
        setter.textureParams.z = EditorGUILayout.Slider("venusMagn ", setter.textureParams.z, -1.0f, 1.0f);
        setter.textureParams.w = EditorGUILayout.Slider("venusFreq ", setter.textureParams.w, 0.0f, 1000.0f);
    }

    private void ResetupSphere(TCCommonParametersSetter setter)
    {
        if (setter.PatchSphere != null)
        {
            setter.PatchSphere.Rebuild();
            setter.PatchSphere.CallUpdate();
        }
    }

    private void ResetupPlanetoid(TCCommonParametersSetter setter)
    {
        if (Application.isPlaying)
        {
            if (setter.Planet != null)
            {
                setter.Planet.ReSetupQuads();

                if (setter.Planet.Atmosphere != null)
                {
                    setter.Planet.Atmosphere.ReanimateAtmosphereUniforms(setter.Planet.Atmosphere, setter.Planet);
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        var setter = (TCCommonParametersSetter)target;

        EditorGUILayout.Space();

        if (GUILayout.Button("Update"))
        {
            ResetupPlanetoid(setter);
            ResetupSphere(setter);
        }

        if (GUI.changed && setter.AutoUpdate)
        {
            ResetupPlanetoid(setter);
            ResetupSphere(setter);
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        currentTab = (Tab)GUILayout.SelectionGrid((int)currentTab, Enum.GetNames(typeof(Tab)), 4, EditorStyles.toolbarButton);

        if (currentTab != prevTab && setter.AutoUpdate)
        {
            ResetupPlanetoid(setter);
            ResetupSphere(setter);
        }

        prevTab = currentTab;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        switch (currentTab)
        {
            case Tab.Main:
                DrawGUIForMain(setter);
                break;
            case Tab.Noise:
                DrawGUIForNoise(setter);
                break;
            case Tab.Texturing:
                DrawGUIForTexturing(setter);
                break;
            case Tab.Clouds:
                DrawGUIForClouds(setter);
                break;
            case Tab.Nature:
                DrawGUIForNature(setter);
                break;
            case Tab.Montes:
                DrawGUIForMontes(setter);
                break;
            case Tab.Dunes:
                DrawGUIForDunes(setter);
                break;
            case Tab.Hills:
                DrawGUIForHills(setter);
                break;
            case Tab.Canyons:
                DrawGUIForCanyons(setter);
                break;
            case Tab.Rivers:
                DrawGUIForRivers(setter);
                break;
            case Tab.Cracks:
                DrawGUIForCracks(setter);
                break;
            case Tab.Craters:
                DrawGUIForCraters(setter);
                break;
            case Tab.Volcanoes:
                DrawGUIForVolcanoes(setter);
                break;
            case Tab.Mare:
                DrawGUIForMare(setter);
                break;
            case Tab.Venus:
                DrawGUIForVenus(setter);
                break;
        }
    }
}
