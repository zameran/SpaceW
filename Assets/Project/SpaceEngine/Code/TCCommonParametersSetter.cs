using UnityEngine;

public sealed class TCCommonParametersSetter : MonoBehaviour
{
    public Planetoid Planet;

    public float Jitter;
    public float texturingHeightOffset;
    public float texturingSlopeOffset;

    public Vector2 TexCoord;

    public Vector3 Randomize;

    public Vector4 faceParams;
    public Vector4 scaleParams;
    public Vector4 mainParams;
    public Vector4 colorParams;
    public Vector4 climateParams;
    public Vector4 mareParams;
    public Vector4 montesParams;
    public Vector4 dunesParams;
    public Vector4 hillsParams;
    public Vector4 canyonsParams;
    public Vector4 riversParams;
    public Vector4 cracksParams;
    public Vector4 craterParams;
    public Vector4 volcanoParams1;
    public Vector4 volcanoParams2;
    public Vector4 lavaParams;
    public Vector4 textureParams;
    public Vector4 cloudsParams1;
    public Vector4 cloudsParams2;
    public Vector4 cycloneParams;

    public Vector4 radParams;
    public Vector4 crHeightParams;
    public Vector4 craterParams1;
    public Vector4 craterParams2;

    public Vector2 texturingUVAtlasOffset;
    public Vector2 InvSize;

    public Color PlanetGlobalColor = Color.white;

    public bool AutoUpdate = false;
    public bool UseCustomTexCoord = false;

    private void Start()
    {
        if ((Planet != null))
            UpdateUniforms();
    }

    [ContextMenu("UpdateUniforms")]
    public void UpdateUniforms()
    {
        if (Planet.Quads.Count == 0) return;
        if (Planet.Quads == null) return;

        for (int i = 0; i < Planet.Quads.Count; i++)
        {
            UpdateUniforms(Planet.Quads[i].CoreShader);

            if (Application.isPlaying)
                Planet.Quads[i].Dispatch();
        }
    }

    public void UpdateUniforms(ComputeShader shader)
    {
        if (shader == null) return;

        shader.SetFloat("jitter", Jitter);
        shader.SetFloat("texturingHeightOffset", texturingHeightOffset);
        shader.SetFloat("texturingSlopeOffset", texturingSlopeOffset);

        if (UseCustomTexCoord)
            shader.SetVector("TexCoord", TexCoord);

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

        shader.SetVector("radParams", radParams);
        shader.SetVector("crHeightParams", crHeightParams);
        shader.SetVector("craterParams1", craterParams1);
        shader.SetVector("craterParams2", craterParams2);

        shader.SetVector("texturingUVAtlasOffset", texturingUVAtlasOffset);
        shader.SetVector("InvSize", InvSize);

        shader.SetVector("planetGlobalColor", new Vector4(PlanetGlobalColor.r, PlanetGlobalColor.g, PlanetGlobalColor.b, PlanetGlobalColor.a));
    }
}