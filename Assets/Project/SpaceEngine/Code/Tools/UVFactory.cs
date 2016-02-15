using UnityEngine;

public static class UVFactory
{
    public static Vector2 GetSgtSphericalUv(Vector3 vertex)
    {
        return VectorHelper.CartesianToPolarUV(vertex);
    }

    public static Vector2 GetSurfaceUV(int detail, int col, int row)
    {
        return new Vector2((float)row / detail, (float)col / detail);
    }

    public static Vector2 GetContinuousUV(int detail, int col, int row, float uvResolution, float uvStartX, float uvStartY)
    {
        return new Vector2(uvStartX + ((float)row / (detail - 1)) * uvResolution,
                          (uvStartY + ((float)col / (detail - 1)) * uvResolution));
    }

    public static Vector2 GetSphericalUv(int detail, int col, int row, Vector3 vertex, bool staticX, bool staticY)
    {
        Vector2 uv = new Vector2();

        uv.x = -(Mathf.Atan2(vertex.x, vertex.z) / (2f * Mathf.PI) + 0.5f);
        uv.y = (Mathf.Asin(vertex.y) / Mathf.PI + .5f);

        if (staticX)
        {
            if (vertex.x < 0)
            {
                if ((row == detail - 1) && vertex.z > -0.01f && vertex.z < 0.01f) uv.x = 0;
                if ((row == 0) && vertex.z > -0.01f && vertex.z < 0.01f) uv.x = 1;
            }
        }
        else if (staticY)
        {
            if (vertex.y > 0)
            {
                if ((col == detail - 1) && vertex.x < 0 && vertex.z > -0.01f && vertex.z < 0.01f) uv.x = 0;
                if ((col == 0) && vertex.x < 0 && vertex.z > -0.01f && vertex.z < 0.01f) uv.x = 1;
            }
            else
            {
                if ((col == detail - 1) && vertex.x < 0 && vertex.z > -0.01f && vertex.z < 0.01f) uv.x = 1;
                if ((col == 0) && vertex.x < 0 && vertex.z < 0.01f && vertex.z > -0.01f) uv.x = 0;
            }
        }
        return uv;
    }
}