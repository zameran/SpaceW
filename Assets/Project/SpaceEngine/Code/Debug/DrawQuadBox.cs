using UnityEngine;

public class DrawQuadBox : MonoBehaviour
{
    public Planetoid Planet = null;
    public Material lineMaterial = null;

    private void Start()
    {
        if (lineMaterial == null)
            CreateLineMaterial();
    }

    public void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            lineMaterial = new Material(Shader.Find("Lines/Colored Blended"));

            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    public void OnPostRender()
    {
        if (Planet == null) return;
        if (lineMaterial == null) CreateLineMaterial();

        for (int i = 0; i < Planet.Quads.Count; i++)
        {
            Quad q = Planet.Quads[i];

            if (q.Generated && q.ShouldDraw)
            {
                q.RenderOutline(Planet.LODTarget.GetComponent<Camera>(), lineMaterial);
            }
        }
    }
}