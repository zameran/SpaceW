using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainRenderer : MonoBehaviour
{
    public Atmosphere atmosphere;

    void Start()
    {

    }

    void OnRenderObject()
    {
        if(atmosphere != null)
        {
            atmosphere.Sun.UpdateNode();
            atmosphere.UpdateNode();
            atmosphere.Render(true);
        }
    }
}