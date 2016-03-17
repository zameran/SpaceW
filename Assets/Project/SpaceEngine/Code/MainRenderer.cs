using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainRenderer : MonoBehaviour
{
    public Planetoid Planet;

    void Start()
    {

    }

    void OnRenderObject()
    {
        Planet.Atmosphere.Render(true);

        foreach (Quad q in Planet.Quads)
        {
            q.Render();
        }
    }
}