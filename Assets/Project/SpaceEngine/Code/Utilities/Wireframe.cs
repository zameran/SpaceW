using UnityEngine;

public class Wireframe : MonoBehaviour
{
    public bool Enabled = false;

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
            Enabled = !Enabled;
    }

    private void OnPreRender()
    {
        if (Enabled)
            GL.wireframe = true;
    }

    private void OnPostRender()
    {
        if (Enabled)
            GL.wireframe = false;
    }
}