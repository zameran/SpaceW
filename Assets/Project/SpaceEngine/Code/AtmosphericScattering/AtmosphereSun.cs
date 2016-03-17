using UnityEngine;

public class AtmosphereSun : MonoBehaviour
{
    static readonly Vector3 Z_AXIS = new Vector3(0, 0, 1);

    Vector3 m_startSunDirection = Z_AXIS;

    Matrix4x4 WorldToLocalRotation;

    public float SunIntensity = 100.0f;

    bool HasMoved = false;

    public Vector3 Origin;

    public Vector3 GetDirection()
    {
        return transform.forward;
    }

    public bool GetHasMoved()
    {
        return HasMoved;
    }

    public Matrix4x4 GetWorldToLocalRotation()
    {
        return WorldToLocalRotation;
    }

    public Matrix4x4 GetLocalToWorldRotation()
    {
        return WorldToLocalRotation.inverse;
    }

    private void Start()
    {
        if (m_startSunDirection.magnitude < Mathf.Epsilon)
            m_startSunDirection = Z_AXIS;

        transform.forward = m_startSunDirection.normalized;
    }

    public void UpdateNode()
    {
        HasMoved = false;

        if (Input.GetMouseButton(2))
        {
            float y = Input.GetAxis("Mouse Y");
            float x = -Input.GetAxis("Mouse X");
            transform.Rotate(new Vector3(x, y, 0), Space.World);
            HasMoved = true;
        }

        Quaternion q = Quaternion.FromToRotation(GetDirection(), Z_AXIS);
        WorldToLocalRotation = Matrix4x4.TRS(Vector3.zero + Origin, q, Vector3.one);
    }

    public void SetUniforms(Material mat)
    {
        if (mat == null) return;

        mat.SetFloat("_Sun_Intensity", SunIntensity);
        mat.SetVector("_Sun_WorldSunDir", GetDirection());
    }
}