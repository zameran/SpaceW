using UnityEngine;

public class Test : MonoBehaviour
{
    public int nVertsPerEdge = 64;

    [ContextMenu("CreateMesh")]
    public void CreateMesh()
    {
        //this.GetComponent<MeshFilter>().sharedMesh = MeshFactory.SetupQuadMesh(nVertsPerEdge);
    }

    [ContextMenu("CreateMeshExtra")]
    public void CreateMeshExtra()
    {
        //this.GetComponent<MeshFilter>().sharedMesh = MeshFactory.SetupQuadMeshExtra(nVertsPerEdge);
    }
}