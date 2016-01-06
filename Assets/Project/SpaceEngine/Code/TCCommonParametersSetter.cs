using UnityEngine;

public class TCCommonParametersSetter : MonoBehaviour
{
    public Planetoid Planet;

    public Vector3 Randomize;       // Randomize

    public Vector4f faceParams;     // (x0,             y0,             size,                   face)
    public Vector4f scaleParams;    // (offsU,          offsV,          scale,                  tidalLock)
    public Vector4f mainParams;     // (mainFreq,       terraceProb,    surfType,               snowLevel)
    public Vector4f colorParams;    // (colorDistMagn,  colorDistFreq,  latIceCaps,             latTropic)
    public Vector4f climateParams;  // (climatePole,    climateTropic,  climateEquator,         tropicWidth)
    public Vector4f mareParams;     // (seaLevel,       mareFreq,       sqrt(mareDensity),      icecapHeight)
    public Vector4f montesParams;   // (montesMagn,     montesFreq,     montesFraction,         montesSpiky)
    public Vector4f dunesParams;    // (dunesMagn,      dunesFreq,      dunesDensity,           drivenDarkening)
    public Vector4f hillsParams;    // (hillsMagn,      hillsFreq,      hillsDensity,           hills2Density)
    public Vector4f canyonsParams;  // (canyonsMagn,    canyonsFreq,    canyonsFraction,        erosion)
    public Vector4f riversParams;   // (riversMagn,     riversFreq,     riversSin,              riversOctaves)
    public Vector4f cracksParams;   // (cracksMagn,     cracksFreq,     cracksOctaves,          craterRayedFactor)
    public Vector4f craterParams;   // (craterMagn,     craterFreq,     sqrt(craterDensity),    craterOctaves)
    public Vector4f volcanoParams1; // (volcanoMagn,    volcanoFreq,    sqrt(volcanoDensity),   volcanoOctaves)
    public Vector4f volcanoParams2; // (volcanoActivity,volcanoFlows,   volcanoRadius,          volcanoTemp)
    public Vector4f lavaParams;	    // (lavaCoverage,   lavaTemperature,surfTemperature,        heightTempGrad)
    public Vector4f textureParams;  // (texScale,       texColorConv,   venusMagn,              venusFreq)
    public Vector4f cloudsParams1;  // (cloudsFreq,     cloudsOctaves,  twistZones,             twistMagn)
    public Vector4f cloudsParams2;  // (cloudsLayer,    cloudsNLayers,  cloudsStyle,            cloudsCoverage)
    public Vector4f cycloneParams;  // (cycloneMagn,    cycloneFreq,    sqrt(cycloneDensity),   cycloneOctaves)
    
    [ContextMenu("UpdateUniforms")]
    public void UpdateUniforms()
    {
        if (Planet.Quads.Count == 0) return;
        if (Planet.Quads == null) return;

        for (int i = 0; i < Planet.Quads.Count; i++)
        {
            UpdateUniforms(Planet.Quads[i].HeightShader);
        }

        Planet.Dispatch();
    }

    public void UpdateUniforms(ComputeShader shader)
    {
        if (shader == null) return;

        shader.SetVector("Randomize", Randomize);
        shader.SetVector("faceParams", faceParams);
        shader.SetVector("scaleParams", scaleParams);
        shader.SetVector("mainParams", mainParams);
        shader.SetVector("colorParams", colorParams);
        shader.SetVector("climateParams", climateParams);
        shader.SetVector("mareParams", mareParams);
        shader.SetVector("montesParams", montesParams);
        shader.SetVector("dunesParams", dunesParams);
        shader.SetVector("hillsParams", hillsParams);
        shader.SetVector("canyonsParams", canyonsParams);
        shader.SetVector("riversParams", riversParams);
        shader.SetVector("cracksParams", cracksParams);
        shader.SetVector("craterParams", craterParams);
        shader.SetVector("volcanoParams1", volcanoParams1);
        shader.SetVector("volcanoParams2", volcanoParams2);
        shader.SetVector("lavaParams", lavaParams);
        shader.SetVector("textureParams", textureParams);
        shader.SetVector("cloudsParams1", cloudsParams1);
        shader.SetVector("cloudsParams2", cloudsParams2);
        shader.SetVector("cycloneParams", cycloneParams);
    }
}