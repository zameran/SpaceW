using UnityEngine;

public abstract class DebugDraw : MonoBehaviour, IDebug
{
    public Planetoid Planet = null;
    public Shader lineShader = null;
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
        if (lineShader == null) throw new System.NullReferenceException("Line Shader is null!");

        if (!lineMaterial)
        {
            lineMaterial = new Material(lineShader);

            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    protected abstract void Draw();
}