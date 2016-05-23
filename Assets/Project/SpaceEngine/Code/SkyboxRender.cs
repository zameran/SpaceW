using UnityEngine;
using System.Collections;

public class SkyboxRender : MonoBehaviour
{
    public Material SkyboxMaterial;

    public int RenderingQueue = 0;

    public Mesh SkyboxMesh = null;

    private void Awake()
    {
        SkyboxMesh = MeshFactory.MakePlane(2, 2, MeshFactory.PLANE.XY, false, false, false);
        SkyboxMesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));
    }

    private void Update()
    {
        if (SkyboxMesh != null && SkyboxMaterial != null)
        {
            Graphics.DrawMesh(SkyboxMesh, Vector3.zero, Quaternion.identity, SkyboxMaterial, 8);
        }
    }
}