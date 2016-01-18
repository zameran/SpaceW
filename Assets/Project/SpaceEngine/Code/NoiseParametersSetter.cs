using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class NoiseParametersSetter : MonoBehaviour
{
    public Texture2D PlanetAtlas = null;

    public ComputeShader ComputeShaderToUpdate = null;
    public Material MaterialToUpdate = null;

    public float Octaves = 4f;
    public float Lacunarity = 2.218281828459f;
    public float H = 0.5f;
    public float Offset = 0.8f;
    public float RidgeSmooth = 0.0001f;

    public bool SetDefaultParameters = false;

    public ImprovedPerlinNoise Perlin = null;

    void Start()
    {
        LoadAndInit();
    }

    void Update()
    {
        //UpdateUniforms();
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

        PlanetAtlas = LoadTextureFromResources("PlanetAtlas");
    }

    public Texture2D LoadTextureFromResources(string name)
    {
        Texture2D temp = Resources.Load("Textures/" + name, typeof(Texture2D)) as Texture2D;

        return temp;
    }

    public void SetUniforms(Material mat)
    {
        if (mat == null)
            return;

        if (SetDefaultParameters)
        {
            mat.SetFloat("noiseOctaves", Octaves);
        }

        mat.SetFloat("noiseLacunarity", Lacunarity);
        mat.SetFloat("noiseH", H);
        mat.SetFloat("noiseOffset", Offset);
        mat.SetFloat("noiseRidgeSmooth", RidgeSmooth);

        mat.SetTexture("AtlasDiffSampler", PlanetAtlas);
    }

    public void SetUniforms(ComputeShader shader)
    {
        if (shader == null)
            return;

        if (SetDefaultParameters)
        {
            shader.SetFloat("noiseOctaves", Octaves);
        }

        shader.SetFloat("noiseLacunarity", Lacunarity);
        shader.SetFloat("noiseH", H);
        shader.SetFloat("noiseOffset", Offset);
        shader.SetFloat("noiseRidgeSmooth", RidgeSmooth);

        shader.SetTexture(0, "AtlasDiffSampler", PlanetAtlas);
    }

    public void SetUniforms(ComputeShader shader, int kernel)
    {
        if (shader == null)
            return;

        if (SetDefaultParameters)
        {
            shader.SetFloat("noiseOctaves", Octaves);
        }

        shader.SetFloat("noiseLacunarity", Lacunarity);
        shader.SetFloat("noiseH", H);
        shader.SetFloat("noiseOffset", Offset);
        shader.SetFloat("noiseRidgeSmooth", RidgeSmooth);

        shader.SetTexture(kernel, "AtlasDiffSampler", PlanetAtlas);
    }
}