using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TCCommonParametersSetter))]
public class TCCommonParametersSetterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TCCommonParametersSetter setter = (TCCommonParametersSetter)target;

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Auto update: ", EditorStyles.boldLabel);
        setter.AutoUpdate = EditorGUILayout.Toggle(setter.AutoUpdate);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Planetoid to setup: ", EditorStyles.boldLabel);
        setter.Planet = EditorGUILayout.ObjectField(setter.Planet, typeof(Planetoid), true) as Planetoid;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Landscape Properties: ", EditorStyles.boldLabel);

		setter.scaleParams.w = EditorGUILayout.Slider("tidalLock ", setter.scaleParams.w, -1.0f, 1.0f);
		
        setter.Randomize.x = EditorGUILayout.Slider("Randomize.x ", setter.Randomize.x, -2.0f, 2.0f);
        setter.Randomize.y = EditorGUILayout.Slider("Randomize.y ", setter.Randomize.y, -2.0f, 2.0f);
        setter.Randomize.z = EditorGUILayout.Slider("Randomize.z ", setter.Randomize.z, -2.0f, 2.0f);

        setter.textureParams.x = EditorGUILayout.Slider("texScale ", setter.textureParams.x, 1.0f, (1.0486f * 10e6f));
        setter.textureParams.y = EditorGUILayout.Slider("texColorConv ", setter.textureParams.y, 0.0f, 1.0f);

        setter.colorParams.x = EditorGUILayout.Slider("colorDistMagn ", setter.colorParams.x, 0.0f, 1.0f);
        setter.colorParams.y = EditorGUILayout.Slider("colorDistFreq ", setter.colorParams.y, 1.0f, 10000.0f);

        setter.mainParams.x = EditorGUILayout.Slider("mainFreq ", setter.mainParams.x, 0.0f, 5.0f);

        setter.textureParams.z = EditorGUILayout.Slider("venusMagn ", setter.textureParams.z, 0.0f, 1.0f);
        setter.textureParams.w = EditorGUILayout.Slider("venusFreq ", setter.textureParams.w, 0.0f, 2.0f);

        setter.mareParams.x = EditorGUILayout.Slider("seaLevel ", setter.mareParams.x, -1.0f, 1.0f);

        setter.mainParams.w = EditorGUILayout.Slider("snowLevel ", setter.mainParams.w, 0.0f, 1.0f);

        setter.dunesParams.w = EditorGUILayout.Slider("drivenDarkening ", setter.dunesParams.w, -1.0f, 1.0f);

        setter.climateParams.x = EditorGUILayout.Slider("climatePole ", setter.climateParams.x, 0.0f, 1.0f);
        setter.climateParams.y = EditorGUILayout.Slider("climateTropic ", setter.climateParams.y, 0.0f, 1.0f);
        setter.climateParams.z = EditorGUILayout.Slider("climateEquator ", setter.climateParams.z, 0.0f, 1.0f);

        setter.lavaParams.w = EditorGUILayout.Slider("heightTempGrad ", setter.lavaParams.w, 0.0f, 1.0f);

        setter.climateParams.w = EditorGUILayout.Slider("tropicWidth ", setter.climateParams.w, 0.0f, 1.0f);

        setter.colorParams.w = EditorGUILayout.Slider("latTropic", setter.colorParams.w, 0.0f, 1.0f);
        setter.colorParams.z = EditorGUILayout.Slider("latIceCaps", setter.colorParams.z, 0.0f, 1.0f);

        setter.mareParams.w = EditorGUILayout.Slider("icecapHeight ", setter.mareParams.w, 0.0f, 1.0f);
        setter.mareParams.y = EditorGUILayout.Slider("mareFreq ", setter.mareParams.y, 0.001f, 1000.0f);
        setter.mareParams.z = EditorGUILayout.Slider("mareDensity ", setter.mareParams.z, 0.0f, 1.0f);

        setter.mainParams.y = EditorGUILayout.Slider("terraceProb ", setter.mainParams.y, 0.0f, 1.0f);

        setter.canyonsParams.w = EditorGUILayout.Slider("erosion", setter.canyonsParams.w, 0.0f, 1.0f);

        setter.montesParams.x = EditorGUILayout.Slider("montesMagn", setter.montesParams.x, 0.0f, 1.0f);
        setter.montesParams.y = EditorGUILayout.Slider("montesFreq", setter.montesParams.y, 0.0f, 500.0f);
        setter.montesParams.z = EditorGUILayout.Slider("montesFraction", setter.montesParams.z, 0.0f, 1.0f);
        setter.montesParams.w = EditorGUILayout.Slider("montesSpiky", setter.montesParams.w, 0.0f, 1.0f);

        setter.dunesParams.x = EditorGUILayout.Slider("dunesMagn", setter.dunesParams.x, 0.0f, 1.0f);
        setter.dunesParams.y = EditorGUILayout.Slider("dunesFreq", setter.dunesParams.y, 0.0f, 100.0f);
        setter.dunesParams.z = EditorGUILayout.Slider("dunesDensity", setter.dunesParams.z, 0.0f, 1.0f);

        setter.hillsParams.x = EditorGUILayout.Slider("hillsMagn", setter.hillsParams.x, 0.0f, 1.0f);
        setter.hillsParams.y = EditorGUILayout.Slider("hillsFreq", setter.hillsParams.y, 0.0f, 1000.0f);
        setter.hillsParams.z = EditorGUILayout.Slider("hillsDensity", setter.hillsParams.z, 0.0f, 1.0f);
        setter.hillsParams.w = EditorGUILayout.Slider("hills2Density", setter.hillsParams.w, 0.0f, 1.0f);

        setter.canyonsParams.x = EditorGUILayout.Slider("canyonsMagn", setter.canyonsParams.x, 0.0f, 1.0f);
        setter.canyonsParams.y = EditorGUILayout.Slider("canyonsFreq", setter.canyonsParams.y, 0.0f, 100.0f);
        setter.canyonsParams.z = EditorGUILayout.Slider("canyonsFraction", setter.canyonsParams.z, 0.0f, 1.0f);

        setter.riversParams.x = EditorGUILayout.Slider("riversMagn", setter.riversParams.x, 0.0f, 100.0f);
        setter.riversParams.y = EditorGUILayout.Slider("riversFreq", setter.riversParams.y, 0.0f, 10.0f);
        setter.riversParams.z = EditorGUILayout.Slider("riversSin", setter.riversParams.z, 0.0f, 10.0f);
        setter.riversParams.w = (float)Mathf.RoundToInt(EditorGUILayout.Slider("riversOctaves", setter.riversParams.w, 0.0f, 5.0f));

        setter.cracksParams.x = EditorGUILayout.Slider("cracksMagn", setter.cracksParams.x, 0.0f, 1.0f);
        setter.cracksParams.y = EditorGUILayout.Slider("cracksFreq", setter.cracksParams.y, 0.0f, 5.0f);
        setter.cracksParams.z = (float)Mathf.RoundToInt(EditorGUILayout.Slider("cracksOctaves", setter.cracksParams.z, 0.0f, 15.0f));

        setter.craterParams.x = EditorGUILayout.Slider("craterMagn", setter.craterParams.x, 0.0f, 2.0f);
        setter.craterParams.y = EditorGUILayout.Slider("craterFreq", setter.craterParams.y, 0.0f, 60.0f);
        setter.craterParams.z = EditorGUILayout.Slider("craterDensity", setter.craterParams.z, 0.0f, 2.0f);
        setter.craterParams.w = (float)Mathf.RoundToInt(EditorGUILayout.Slider("craterOctaves", setter.craterParams.w, 0.0f, 20.0f));

        setter.cracksParams.w = EditorGUILayout.Slider("craterRayedFactor", setter.cracksParams.w, 0.0f, 1.0f);

        setter.volcanoParams1.x = EditorGUILayout.Slider("volcanoMagn", setter.volcanoParams1.x, 0.0f, 2.0f);
        setter.volcanoParams1.y = EditorGUILayout.Slider("volcanoFreq", setter.volcanoParams1.y, 0.0f, 5.0f);
        setter.volcanoParams1.z = EditorGUILayout.Slider("volcanoDensity", setter.volcanoParams1.z, 0.0f, 1.0f);
        setter.volcanoParams1.w = (float)Mathf.RoundToInt(EditorGUILayout.Slider("volcanoOctaves", setter.volcanoParams1.w, 0.0f, 5.0f));

        setter.volcanoParams2.z = EditorGUILayout.Slider("volcanoRadius", setter.volcanoParams2.z, 0.0f, 5.0f);
        setter.volcanoParams2.y = EditorGUILayout.Slider("volcanoFlows", setter.volcanoParams2.y, 0.0f, 1.0f);
        setter.volcanoParams2.w = EditorGUILayout.Slider("volcanoTemp", setter.volcanoParams2.w, 0.0f, 3000.0f);
        setter.volcanoParams2.x = EditorGUILayout.Slider("volcanoActivity", setter.volcanoParams2.x, 0.0f, 2.0f);

        EditorGUILayout.EndVertical();

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

        if(setter.AutoUpdate)
        {
            if (GUI.changed)
                setter.UpdateUniforms();
        }
    }
}
