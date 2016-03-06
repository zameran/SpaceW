using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TCCommonParametersSetter))]
public class TCCommonParametersSetterEditor : Editor
{
    public enum Tab
    {
        Main, Noise, Texturing, Clouds, Nature, Montes, Dunes, Hills, Canyons, Rivers, Cracks, Craters, Radials, Volcanoes, Mare, Venus
    }
    private Tab currentTab = Tab.Main;
    private string[] tabTexts = new string[16] { "Main", "Noise", "Texturing", "Clouds", "Nature", "Montes", "Dunes", "Hills",
                                                 "Canyons", "Rivers", "Cracks", "Craters", "Radials", "Volcanoes",
                                                 "Mare", "Venus" };

    private void DrawGUIForMain(TCCommonParametersSetter setter)
    {
        EditorGUILayout.LabelField("Auto update: ", EditorStyles.boldLabel);
        setter.AutoUpdate = EditorGUILayout.Toggle(setter.AutoUpdate);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Use Custom TexCoord: ", EditorStyles.boldLabel);
        setter.UseCustomTexCoord = EditorGUILayout.Toggle(setter.UseCustomTexCoord);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Planetoid to setup: ", EditorStyles.boldLabel);
        setter.Planet = EditorGUILayout.ObjectField(setter.Planet, typeof(Planetoid), true) as Planetoid;

        EditorGUILayout.Space();
    }

    private void DrawGUIForNoise(TCCommonParametersSetter setter)
    {
        setter.Jitter = EditorGUILayout.Slider("jitter ", setter.Jitter, -1000.0f, 1000.0f);

        setter.scaleParams.w = EditorGUILayout.Slider("tidalLock ", setter.scaleParams.w, -1.0f, 1.0f);

        setter.Randomize.x = EditorGUILayout.Slider("Randomize.x ", setter.Randomize.x, -2.0f, 2.0f);
        setter.Randomize.y = EditorGUILayout.Slider("Randomize.y ", setter.Randomize.y, -2.0f, 2.0f);
        setter.Randomize.z = EditorGUILayout.Slider("Randomize.z ", setter.Randomize.z, -2.0f, 2.0f);

        setter.mainParams.x = EditorGUILayout.Slider("mainFreq ", setter.mainParams.x, 0.0f, 5.0f);

        setter.canyonsParams.w = EditorGUILayout.Slider("erosion", setter.canyonsParams.w, 0.0f, 1.0f);

        setter.mainParams.y = EditorGUILayout.Slider("terraceProb ", setter.mainParams.y, 0.0f, 1.0f);

        setter.crHeightParams.x = EditorGUILayout.Slider("heightFloor", setter.crHeightParams.x, -1.0f, 1.0f);
        setter.crHeightParams.y = EditorGUILayout.Slider("heightPeak", setter.crHeightParams.y, 0.0f, 1.0f);
        setter.crHeightParams.z = EditorGUILayout.Slider("heightRim", setter.crHeightParams.z, 0.0f, 1.0f);
        setter.crHeightParams.w = EditorGUILayout.Slider("heightCrew", setter.crHeightParams.w, 0.0f, 1.0f);
    }

    private void DrawGUIForTexturing(TCCommonParametersSetter setter)
    {
        setter.faceParams.x = EditorGUILayout.Slider("x0 ", setter.faceParams.x, 0.0f, 1.0f);
        setter.faceParams.y = EditorGUILayout.Slider("y0 ", setter.faceParams.y, 0.0f, 1.0f);
        setter.faceParams.z = EditorGUILayout.Slider("size", setter.faceParams.z, 0.0f, 480.0f);
        setter.faceParams.w = EditorGUILayout.Slider("face ", setter.faceParams.w, 0.0f, 5.0f);

        setter.textureParams.x = EditorGUILayout.Slider("texScale ", setter.textureParams.x, 0.0f, 10.0f);
        setter.textureParams.y = EditorGUILayout.Slider("texColorConv ", setter.textureParams.y, -1.0f, 1.0f);

        setter.TexCoord.x = EditorGUILayout.Slider("TexCoord.x ", setter.TexCoord.x, 0.0f, 1.0f);
        setter.TexCoord.y = EditorGUILayout.Slider("TexCoord.y ", setter.TexCoord.y, 0.0f, 1.0f);

        setter.texturingHeightOffset = EditorGUILayout.Slider("texturingHeightOffset ", setter.texturingHeightOffset, -1.0f, 1.0f);
        setter.texturingSlopeOffset = EditorGUILayout.Slider("texturingSlopeOffset ", setter.texturingSlopeOffset, -1.0f, 1.0f);

        setter.colorParams.x = EditorGUILayout.Slider("colorDistMagn ", setter.colorParams.x, 0.0f, 1.0f);
        setter.colorParams.y = EditorGUILayout.Slider("colorDistFreq ", setter.colorParams.y, 0.0f, 10000.0f);

        setter.lavaParams.x = EditorGUILayout.Slider("lavaCoverage ", setter.lavaParams.x, -10.0f, 1.0f);
        setter.lavaParams.y = EditorGUILayout.Slider("lavaTemperature ", setter.lavaParams.y, 0.0f, 373.0f);
        setter.lavaParams.z = EditorGUILayout.Slider("surfTemperature ", setter.lavaParams.z, 0.0f, 373.0f);
        setter.lavaParams.w = EditorGUILayout.Slider("heightTempGrad ", setter.lavaParams.w, 0.0f, 1.0f);

        setter.dunesParams.w = EditorGUILayout.Slider("drivenDarkening ", setter.dunesParams.w, -1.0f, 1.0f);
    }

    private void DrawGUIForClouds(TCCommonParametersSetter setter)
    {
        setter.cloudsParams2.z = EditorGUILayout.Slider("cloudsStyle ", setter.cloudsParams2.z, -1.0f, 1.0f);

        //TODO Clouds settings.

        //twistZones 0-10
        //twistMagn 0-5
        //mainFreq 0-3
        //mainOctaves 0-15
        //Coverage 0-1
        //cycloneMagn 0-20
        //cycloneFreq 0-1
        //cycloneDensity 0-1
        //cycloneOctaves 0-5
    }

    private void DrawGUIForNature(TCCommonParametersSetter setter)
    {
        setter.mareParams.x = EditorGUILayout.Slider("seaLevel ", setter.mareParams.x, -1.0f, 1.0f);
        setter.mainParams.w = EditorGUILayout.Slider("snowLevel ", setter.mainParams.w, 0.0f, 1.0f);

        setter.climateParams.x = EditorGUILayout.Slider("climatePole ", setter.climateParams.x, 0.0f, 1.0f);
        setter.climateParams.y = EditorGUILayout.Slider("climateTropic ", setter.climateParams.y, 0.0f, 1.0f);
        setter.climateParams.z = EditorGUILayout.Slider("climateEquator ", setter.climateParams.z, 0.0f, 1.0f);

        setter.climateParams.w = EditorGUILayout.Slider("tropicWidth ", setter.climateParams.w, 0.0f, 1.0f);

        setter.colorParams.w = EditorGUILayout.Slider("latTropic", setter.colorParams.w, 0.0f, 1.0f);
        setter.colorParams.z = EditorGUILayout.Slider("latIceCaps", setter.colorParams.z, 0.0f, 1.0f);

        setter.mareParams.w = EditorGUILayout.Slider("icecapHeight ", setter.mareParams.w, 0.0f, 1.0f);
    }

    private void DrawGUIForMontes(TCCommonParametersSetter setter)
    {
        setter.montesParams.x = EditorGUILayout.Slider("montesMagn", setter.montesParams.x, 0.0f, 1.0f);
        setter.montesParams.y = EditorGUILayout.Slider("montesFreq", setter.montesParams.y, 0.0f, 500.0f);
        setter.montesParams.z = EditorGUILayout.Slider("montesFraction", setter.montesParams.z, 0.0f, 1.0f);
        setter.montesParams.w = EditorGUILayout.Slider("montesSpiky", setter.montesParams.w, 0.0f, 1.0f);
    }

    private void DrawGUIForDunes(TCCommonParametersSetter setter)
    {
        setter.dunesParams.x = EditorGUILayout.Slider("dunesMagn", setter.dunesParams.x, 0.0f, 1.0f);
        setter.dunesParams.y = EditorGUILayout.Slider("dunesFreq", setter.dunesParams.y, 0.0f, 100.0f);
        setter.dunesParams.z = EditorGUILayout.Slider("dunesDensity", setter.dunesParams.z, 0.0f, 1.0f);
    }

    private void DrawGUIForHills(TCCommonParametersSetter setter)
    {
        setter.hillsParams.x = EditorGUILayout.Slider("hillsMagn", setter.hillsParams.x, 0.0f, 10.0f);
        setter.hillsParams.y = EditorGUILayout.Slider("hillsFreq", setter.hillsParams.y, 0.0f, 1000.0f);
        setter.hillsParams.z = EditorGUILayout.Slider("hillsDensity", setter.hillsParams.z, 0.0f, 1.0f);
        setter.hillsParams.w = EditorGUILayout.Slider("hills2Density", setter.hillsParams.w, 0.0f, 1.0f);
    }

    private void DrawGUIForCanyons(TCCommonParametersSetter setter)
    {
        setter.canyonsParams.x = EditorGUILayout.Slider("canyonsMagn", setter.canyonsParams.x, 0.0f, 1.0f);
        setter.canyonsParams.y = EditorGUILayout.Slider("canyonsFreq", setter.canyonsParams.y, 0.0f, 100.0f);
        setter.canyonsParams.z = EditorGUILayout.Slider("canyonsFraction", setter.canyonsParams.z, 0.0f, 1.0f);
    }

    private void DrawGUIForRivers(TCCommonParametersSetter setter)
    {
        setter.riversParams.x = EditorGUILayout.Slider("riversMagn", setter.riversParams.x, 0.0f, 100.0f);
        setter.riversParams.y = EditorGUILayout.Slider("riversFreq", setter.riversParams.y, 0.0f, 10.0f);
        setter.riversParams.z = EditorGUILayout.Slider("riversSin", setter.riversParams.z, 0.0f, 10.0f);
        setter.riversParams.w = (float)Mathf.RoundToInt(EditorGUILayout.Slider("riversOctaves", setter.riversParams.w, 0.0f, 5.0f));
    }

    private void DrawGUIForCracks(TCCommonParametersSetter setter)
    {
        setter.cracksParams.x = EditorGUILayout.Slider("cracksMagn", setter.cracksParams.x, 0.0f, 1.0f);
        setter.cracksParams.y = EditorGUILayout.Slider("cracksFreq", setter.cracksParams.y, 0.0f, 5.0f);
        setter.cracksParams.z = (float)Mathf.RoundToInt(EditorGUILayout.Slider("cracksOctaves", setter.cracksParams.z, 0.0f, 15.0f));
    }

    private void DrawGUIForCraters(TCCommonParametersSetter setter)
    {
        setter.craterParams.x = EditorGUILayout.Slider("craterMagn", setter.craterParams.x, 0.0f, 2.0f);
        setter.craterParams.y = EditorGUILayout.Slider("craterFreq", setter.craterParams.y, 0.0f, 60.0f);
        setter.craterParams.z = EditorGUILayout.Slider("craterDensity", setter.craterParams.z, 0.0f, 2.0f);
        setter.craterParams.w = (float)Mathf.RoundToInt(EditorGUILayout.Slider("craterOctaves", setter.craterParams.w, 0.0f, 20.0f));
        setter.cracksParams.w = EditorGUILayout.Slider("craterRayedFactor", setter.cracksParams.w, 0.0f, 1.0f);

        setter.craterParams1.x = EditorGUILayout.Slider("craterSphereRadius", setter.craterParams1.x, 0.0f, 1.0f);
        setter.craterParams1.y = EditorGUILayout.Slider("craterRoundDist", setter.craterParams1.y, 0.0f, 1.0f);
        setter.craterParams1.z = EditorGUILayout.Slider("craterDistortion", setter.craterParams1.z, 0.0f, 1.0f);
        setter.craterParams1.w = EditorGUILayout.Slider("craterRaysColor", setter.craterParams1.w, 0.0f, 1.0f);

        setter.craterParams2.x = EditorGUILayout.Slider("craterAmplitudePerOctave", setter.craterParams2.x, 0.0f, 1.0f);
        setter.craterParams2.y = EditorGUILayout.Slider("craterHeightPeakPerOctave", setter.craterParams2.y, 0.0f, 1.0f);
        setter.craterParams2.z = EditorGUILayout.Slider("craterHeightFloorPerOctave", setter.craterParams2.z, 0.0f, 2.0f);
        setter.craterParams2.w = EditorGUILayout.Slider("craterRadInnerPerOctave", setter.craterParams2.w, 0.0f, 1.0f);

        //amplitude *= 0.55;
        //heightPeak *= 0.25;
        //heightFloor *= 1.2;
        //radInner *= 0.60;
    }

    private void DrawGUIForRadials(TCCommonParametersSetter setter)
    {
        setter.radParams.x = EditorGUILayout.Slider("radPeak", setter.radParams.x, 0.0f, 0.5f);
        setter.radParams.y = EditorGUILayout.Slider("radInner", setter.radParams.y, 0.0f, 0.5f);
        setter.radParams.z = EditorGUILayout.Slider("radRim", setter.radParams.z, 0.0f, 0.5f);
        setter.radParams.w = EditorGUILayout.Slider("radOuter", setter.radParams.w, 0.0f, 0.5f);
    }

    private void DrawGUIForVolcanoes(TCCommonParametersSetter setter)
    {
        setter.volcanoParams1.x = EditorGUILayout.Slider("volcanoMagn", setter.volcanoParams1.x, 0.0f, 2.0f);
        setter.volcanoParams1.y = EditorGUILayout.Slider("volcanoFreq", setter.volcanoParams1.y, 0.0f, 5.0f);
        setter.volcanoParams1.z = EditorGUILayout.Slider("volcanoDensity", setter.volcanoParams1.z, 0.0f, 1.0f);
        setter.volcanoParams1.w = (float)Mathf.RoundToInt(EditorGUILayout.Slider("volcanoOctaves", setter.volcanoParams1.w, 0.0f, 5.0f));

        setter.volcanoParams2.z = EditorGUILayout.Slider("volcanoRadius", setter.volcanoParams2.z, 0.0f, 5.0f);
        setter.volcanoParams2.y = EditorGUILayout.Slider("volcanoFlows", setter.volcanoParams2.y, 0.0f, 1.0f);
        setter.volcanoParams2.w = EditorGUILayout.Slider("volcanoTemp", setter.volcanoParams2.w, 0.0f, 3000.0f);
        setter.volcanoParams2.x = EditorGUILayout.Slider("volcanoActivity", setter.volcanoParams2.x, 0.0f, 2.0f);
    }

    private void DrawGUIForMare(TCCommonParametersSetter setter)
    {
        setter.mareParams.y = EditorGUILayout.Slider("mareFreq ", setter.mareParams.y, 0.001f, 1000.0f);
        setter.mareParams.z = EditorGUILayout.Slider("mareDensity ", setter.mareParams.z, 0.0f, 1.0f);
    }

    private void DrawGUIForVenus(TCCommonParametersSetter setter)
    {
        setter.textureParams.z = EditorGUILayout.Slider("venusMagn ", setter.textureParams.z, 0.0f, 1.0f);
        setter.textureParams.w = EditorGUILayout.Slider("venusFreq ", setter.textureParams.w, 0.0f, 2.0f);
    }

    public override void OnInspectorGUI()
    {
        TCCommonParametersSetter setter = (TCCommonParametersSetter)target;

        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        currentTab = (Tab)GUILayout.SelectionGrid((int)currentTab, tabTexts, 4, EditorStyles.toolbarButton);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        switch(currentTab)
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
            case Tab.Radials:
                DrawGUIForRadials(setter);
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

        if(setter.AutoUpdate)
        {
            if (GUI.changed)
                setter.UpdateUniforms();
        }
    }
}
