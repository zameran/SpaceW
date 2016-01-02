using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class NoiseParametersSetter : MonoBehaviour
{
    public Texture2D Noise = null;
    public Texture2D GradientTable = null;
    public Texture2D PermutationTable = null;

    public ComputeShader ComputeShaderToUpdate = null;
    public Material MaterialToUpdate = null;

    public float Octaves = 4f;
    public float Lacunarity = 2.218281828459f;
    public float H = 0.5f;
    public float Offset = 0.8f;
    public float RidgeSmooth = 0.0001f;

    public ImprovedPerlinNoise Perlin = null;

    void Start()
    {
        LoadAndInit();
    }

    void Update()
    {
        UpdateUniforms();
    }

    void OnDestroy()
    {
        if (Noise != null)
            DestroyImmediate(Noise);

        if (GradientTable != null)
            DestroyImmediate(GradientTable);

        if (PermutationTable != null)
            DestroyImmediate(PermutationTable);
    }

    [ContextMenu("Update Uniforms")]
    public void UpdateUniforms()
    {
        LoadAndInit();

        SetUniforms(MaterialToUpdate);
        SetUniforms(ComputeShaderToUpdate);
    }

    public void Init()
    {
        if (Perlin == null)
        {
            Perlin = new ImprovedPerlinNoise(0);
        }
    }

    public void LoadAndInit()
    {
        Init();

        Perlin.LoadResourcesFor3DNoise();
        //Perlin.LoadPrecomputedRandomVolume();

        PermutationTable = Perlin.GetPermutationTable2D();
        GradientTable = Perlin.GetGradient3D();
        Noise = Perlin.GetPermutationTable2D();
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

    public void SetUniforms(ComputeShader shader)
    {
        if (shader == null)
            return;

        shader.SetFloat("noiseOctaves", Octaves);
        shader.SetFloat("noiseLacunarity", Lacunarity);
        shader.SetFloat("noiseH", H);
        shader.SetFloat("noiseOffset", Offset);
        shader.SetFloat("noiseRidgeSmooth", RidgeSmooth);

        shader.SetTexture(0, "NoiseSampler", Noise);
        shader.SetTexture(0, "PermGradSampler", GradientTable);
        shader.SetTexture(0, "PermSampler", PermutationTable);
    }
}