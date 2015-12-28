using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class NoiseParametersSetter : MonoBehaviour
{
    public Texture2D Noise = null;
    public Texture2D GradientTable = null;
    public Texture2D PermutationTable = null;

    public Material MaterialToUpdate = null;

    public float Octaves = 4f;
    public float Lacunarity = 2.218281828459f;
    public float H = 0.5f;
    public float Offset = 0.8f;
    public float RidgeSmooth = 0.0001f;

    public ImprovedPerlinNoise Perlin = null;

    public void Update()
    {
        UpdateUniforms();
    }

    [ContextMenu("Update Uniforms")]
    public void UpdateUniforms()
    {
        if (Perlin == null)
        {
            Perlin = new ImprovedPerlinNoise(0);
        }

        Perlin.LoadResourcesFor3DNoise();
        Perlin.LoadPrecomputedRandomVolume();

        PermutationTable = Perlin.GetPermutationTable2D();
        GradientTable = Perlin.GetGradient3D();
        Noise = Perlin.GetPermutationTable2D();

        SetUniforms(MaterialToUpdate);
    }

    public void SetUniforms(Material mat)
    {
        if (mat == null)
            return;

        mat.SetFloat("noiseOctaves", Octaves);
        mat.SetFloat("noiseLacunarity", Lacunarity);
        mat.SetFloat("noiseH", H);
        mat.SetFloat("noiseOffset", Offset);
        mat.SetFloat("noiseRidgeSmooth", RidgeSmooth);

        mat.SetTexture("NoiseSampler", Noise);
        mat.SetTexture("PermGradSampler", GradientTable);
        mat.SetTexture("PermSampler", PermutationTable);
    }
}