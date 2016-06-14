using UnityEngine;

public class Cloudsphere : MonoBehaviour
{
    public Planetoid planetoid;

    public Mesh CloudsphereMesh;

    public Shader CloudShader;
    public Material CloudMaterial;

    public EngineRenderQueue RenderQueue = EngineRenderQueue.Transparent;
    public int RenderQueueOffset = 0;

    public float Radius;
    public float Height;

    [Range(0.0f, 1.0f)]
    public float TransmittanceOffset = 0.625f;

    public Cubemap CloudTexture;

    private void Start()
    {
        InitMaterials();
    }

    private void Update()
    {

    }

    public void Render(int drawLayer = 8)
    {
        SetUniforms(CloudMaterial);

        CloudMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

        if (CloudsphereMesh == null) return;

        Matrix4x4 CloudsTRS = Matrix4x4.TRS(planetoid.transform.position, transform.rotation, Vector3.one * (Radius + Height));

        Graphics.DrawMesh(CloudsphereMesh, CloudsTRS, CloudMaterial, drawLayer, CameraHelper.Main(), 0, planetoid.QuadAtmosphereMPB);
    }

    public void InitMaterials()
    {
        if (CloudMaterial == null)
        {
            CloudMaterial = new Material(CloudShader);
            CloudMaterial.name = "Clouds" + "(Instance)" + Random.Range(float.MinValue, float.MaxValue);
        }
    }

    public void InitUniforms()
    {
        InitUniforms(CloudMaterial);
    }

    public void InitUniforms(Material mat)
    {
        if (mat == null) return;

        if (planetoid != null)
        {
            if (planetoid.Atmosphere != null)
            {
                planetoid.Atmosphere.InitUniforms(planetoid.QuadAtmosphereMPB, mat, true);
            }
        }

        if (CloudTexture != null) mat.SetTexture("_Cloud", CloudTexture);

        mat.SetFloat("_TransmittanceOffset", TransmittanceOffset);
    }

    public void SetUniforms()
    {
        SetUniforms(CloudMaterial);
    }

    public void SetUniforms(Material mat)
    {
        if (mat == null) return;

        if (planetoid != null)
        {
            if (planetoid.Atmosphere != null)
            {
                planetoid.Atmosphere.SetUniforms(planetoid.QuadAtmosphereMPB, mat, true);
            }
        }

        if (CloudTexture != null) mat.SetTexture("_Cloud", CloudTexture);

        mat.SetFloat("_TransmittanceOffset", TransmittanceOffset);
    }
}