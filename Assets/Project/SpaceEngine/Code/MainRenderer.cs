using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainRenderer : MonoBehaviour
{
    public Planetoid Planet;
    public Proland.Manager Manager;

    void Start()
    {

    }

    void OnPostRender()
    {
        Manager.GetSkyNode().PostRender();

        foreach (Quad q in Planet.Quads)
        {
            q.Render();
        }
    }
}