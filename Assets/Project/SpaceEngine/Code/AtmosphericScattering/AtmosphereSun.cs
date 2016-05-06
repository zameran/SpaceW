using UnityEngine;

public sealed class AtmosphereSun : MonoBehaviour
{
    [Range(1, 2)]
    public int sunID = 1;

    static readonly Vector3 Z_AXIS = new Vector3(0, 0, 1);

    Vector3 m_startSunDirection = Z_AXIS;

    private bool HasMoved = false;

    public Matrix4x4 WorldToLocalRotation
    {
        get
        {
            Quaternion q = Quaternion.FromToRotation(GetDirection(), Z_AXIS);

            return Matrix4x4.TRS(Vector3.zero + Origin, q, Vector3.one);
        }
        private set { WorldToLocalRotation = value; }
    }

    public Matrix4x4 LocalToWorldRotation
    {
        get
        {
            return WorldToLocalRotation.inverse;
        }
        private set { LocalToWorldRotation = value; }
    }

    public float SunIntensity = 100.0f;

    public Vector3 Origin;

    public Vector3 GetDirection()
    {
        return -transform.forward;
    }

    public bool GetHasMoved()
    {
        return HasMoved;
    }

    private void Start()
    {
        if (m_startSunDirection.magnitude < Mathf.Epsilon)
            m_startSunDirection = Z_AXIS;

        transform.forward = m_startSunDirection.normalized;
    }

    public void UpdateNode()
    {
        if ((sunID == 1 && Input.GetKey(KeyCode.RightControl)) || (sunID == 2 && Input.GetKey(KeyCode.RightShift)))
        {
            HasMoved = false;

            float h = Input.GetAxis("HorizontalArrows") * 0.75f;
            float v = Input.GetAxis("VerticalArrows") * 0.75f;

            transform.Rotate(new Vector3(v, h, 0), Space.World);

            HasMoved = true;
        }
    }

    public void SetUniforms(Material mat)
    {
        if (mat == null) return;

        mat.SetFloat("_Sun_Intensity", SunIntensity);

        switch (sunID)
        {
            case 1:
                mat.SetVector("_Sun_WorldSunDir_1", GetDirection());
                mat.SetMatrix("_Sun_WorldToLocal_1", WorldToLocalRotation);
                break;
            case 2:
                mat.SetVector("_Sun_WorldSunDir_2", GetDirection());
                mat.SetMatrix("_Sun_WorldToLocal_2", WorldToLocalRotation);
                break;
            default:
                mat.SetVector("_Sun_WorldSunDir", GetDirection());
                mat.SetMatrix("_Sun_WorldToLocal", WorldToLocalRotation);
                break;
        }

        mat.SetVector("_Sun_Position", transform.position);
    }
}