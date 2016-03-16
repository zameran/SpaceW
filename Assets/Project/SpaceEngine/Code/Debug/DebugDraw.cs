using UnityEngine;

public abstract class DebugDraw : MonoBehaviour
{
    public Planetoid Planet = null;
    public Material lineMaterial = null;

    protected virtual void Start()
    {
        if (lineMaterial == null)
            CreateLineMaterial();
    }

    protected virtual void OnPostRender()
    {
        if (Planet == null) return;
        if (lineMaterial == null) CreateLineMaterial();

        Draw();
    }

    protected virtual void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            lineMaterial = new Material(Shader.Find("Lines/Colored Blended"));

            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    protected abstract void Draw();
}