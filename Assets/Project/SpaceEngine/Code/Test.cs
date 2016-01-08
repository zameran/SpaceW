using UnityEngine;

public class Test : MonoBehaviour
{
    public int nVertsPerEdge = 64;

    [ContextMenu("Dispatch")]
    public void Dispatch()
    {

    }

    [ContextMenu("CreateMesh")]
    public void CreateMesh()
    {
        this.GetComponent<MeshFilter>().sharedMesh = MeshFactory.SetupQuadMesh(nVertsPerEdge);
    }

    [ContextMenu("CreateMeshExtra")]
    public void CreateMeshExtra()
    {
        this.GetComponent<MeshFilter>().sharedMesh = MeshFactory.SetupQuadMeshExtra(nVertsPerEdge);
    }
}