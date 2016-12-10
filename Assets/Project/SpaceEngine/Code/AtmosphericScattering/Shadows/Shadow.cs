using UnityEngine;

public abstract class Shadow : MonoBehaviour
{
    public Light Light;

    public Matrix4x4 Matrix;

    public float Ratio;

    public abstract Texture GetTexture();

    public virtual void Start()
    {

    }

    public virtual bool CalculateShadow()
    {
        if (Helper.Enabled(Light) == true && Light.intensity > 0.0f)
        {
            return true;
        }

        return false;
    }
}