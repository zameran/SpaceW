using UnityEngine;

//TODO : Make it space engine shaders like.

public class SphereConicShadow : SphereShadow
{
    public override Texture GetTexture()
    {
        return base.GetTexture();
    }

    public override bool CalculateShadow()
    {
        return false;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected()
    {
        
    }
#endif
}