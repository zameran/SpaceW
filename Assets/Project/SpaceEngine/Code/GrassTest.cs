using System;
using System.Collections.Generic;

using UnityEngine;

public class GrassTest : MonoBehaviour
{
    public Texture2D grassTexture;
    public Texture2D noiseTexture;

    public Shader grassShader;
    public Material grassMaterial;

    public List<Grass> grass = new List<Grass>();

    public int size = 10;

    public int layer = 0;

    private bool canRender = false;

    private Mesh grassMesh = null;

    void Start()
    {
        grassMaterial = new Material(grassShader);
        grassMaterial.name = "GrassMaterial(Instance)_" + UnityEngine.Random.Range(float.MinValue, float.MaxValue);
        grassMaterial.SetTexture("_MainTex", grassTexture);
        grassMaterial.SetTexture("_NoiseTexture", noiseTexture);

        grassMesh = MeshFactory.MakePlane(2, 2, MeshFactory.PLANE.XZ, true, false, false);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (grassMesh != null)
                    grass.Add(new Grass(grassMesh, new Vector3(i, 0, j)));
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

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (grass[i * j].grassMesh == null) return;

                grassMaterial.SetPass(0);
                Graphics.DrawMeshNow(grass[i * j].grassMesh, grass[i * j].position, Quaternion.identity);
                //grassMaterial.SetPass(1);
                //Graphics.DrawMeshNow(grass[i * j].grassMesh, grass[i * j].position, Quaternion.identity);
            }
        }
    }

    void Update()
    {

    }
}

[Serializable]
public class Grass
{
    public Mesh grassMesh;

    public Vector3 position;

    public Grass(Mesh grassMesh, Vector3 position)
    {
        this.grassMesh = grassMesh;

        this.position = position;
    }
}