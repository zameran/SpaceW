using UnityEngine;
using System.Collections;

public class Cloudsphere : MonoBehaviour
{
    public Planetoid Planetoid;

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

    public void Render(bool now, int drawLayer = 8)
    {
        SetUniforms(CloudMaterial);
        CloudMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;
        CloudMaterial.SetPass(0);

        if (CloudsphereMesh == null) return;

        Matrix4x4 CloudsTRS = Matrix4x4.TRS(Planetoid.transform.position, transform.rotation, Vector3.one * (Radius + Height));

        if (!now)
            Graphics.DrawMesh(CloudsphereMesh, CloudsTRS, CloudMaterial, drawLayer);
        else
            Graphics.DrawMeshNow(CloudsphereMesh, CloudsTRS);
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

        if (Planetoid != null)
        {
            if (Planetoid.Atmosphere != null)
            {
                Planetoid.Atmosphere.InitUniforms(mat);
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

        if (Planetoid != null)
        {
            if (Planetoid.Atmosphere != null)
            {
                Planetoid.Atmosphere.SetUniforms(mat);
            }
        }

        if (CloudTexture != null) mat.SetTexture("_Cloud", CloudTexture);

        mat.SetFloat("_TransmittanceOffset", TransmittanceOffset);
    }
}