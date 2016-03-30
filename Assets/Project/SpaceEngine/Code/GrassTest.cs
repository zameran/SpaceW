using System.Collections.Generic;

using UnityEngine;

public class GrassTest : MonoBehaviour
{
    public Texture2D grassTexture;
    public Texture2D noiseTexture;

    public Shader grassShader;
    public Material grassMaterial;

    public List<Mesh> grassMeshes = new List<Mesh>();

    public int layer = 0;

    private bool canRender = false;

    void Start()
    {
        grassMaterial = new Material(grassShader);
        grassMaterial.name = "GrassMaterial(Instance)_" + Random.Range(float.MinValue, float.MaxValue);
        grassMaterial.SetTexture("_MainTex", grassTexture);
        grassMaterial.SetTexture("_NoiseTexture", noiseTexture);

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Mesh grassMesh = MeshFactory.MakePlane(2, 2, MeshFactory.PLANE.XY, true, true, false);
                grassMeshes.Add(grassMesh);
            }
        }

        canRender = true;
    }

    void OnDestroy()
    {

    }

    void OnPostRender()
    {
        if (!canRender) return;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Mesh grassMesh = grassMeshes[i * j];

                if (grassMesh != null)
                {
                    grassMaterial.SetPass(0);
                    Graphics.DrawMeshNow(grassMesh, new Vector3(i, 0, j), Quaternion.identity);
                    grassMaterial.SetPass(1);
                    Graphics.DrawMeshNow(grassMesh, new Vector3(i, 0, j), Quaternion.identity);
                }
            }
        }
    }

    void Update()
    {

    }
}