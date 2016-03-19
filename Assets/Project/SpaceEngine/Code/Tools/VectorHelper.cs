using UnityEngine;

public static class VectorHelper
{
    public static Vector3 CombineVectors(params Vector3[] vectors)
    {
        Vector3 summ = Vector3.zero;

        for (int i = 0; i < vectors.Length; i++)
        {
            summ += vectors[i];
        }

        return summ;
    }

    public static Vector3 Abs(this Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    public static Vector3 NormalizeToRadius(this Vector3 v, float radius)
    {
        return v.normalized * radius;
    }

    public static Vector3 NormalizeToRadius1(this Vector3 v, float radius)
    {
        return v * radius;
    }

    public static Vector2 CartesianToPolar(Vector3 xyz)
    {
        var longitude = Mathf.Atan2(xyz.x, xyz.z);
        var latitude = Mathf.Asin(xyz.y / xyz.magnitude);

        return new Vector2(longitude, latitude);
    }

    public static Vector2 CartesianToPolarUV(Vector3 xyz)
    {
        var uv = CartesianToPolar(xyz);

        uv.x = Mathf.Repeat(0.5f - uv.x / (Mathf.PI * 2.0f), 1.0f);
        uv.y = 0.5f + uv.y / Mathf.PI;

        return uv;
    }

    public static Vector3 SpherifyPoint(Vector3 point)
    {
        float dX2 = point.x * point.x;
        float dY2 = point.y * point.y;
        float dZ2 = point.z * point.z;

        float dX2Half = dX2 * 0.5f;
        float dY2Half = dY2 * 0.5f;
        float dZ2Half = dZ2 * 0.5f;

        point.x = point.x * Mathf.Sqrt(1f - dY2Half - dZ2Half + (dY2 * dZ2) * (1f / 3f));
        point.y = point.y * Mathf.Sqrt(1f - dZ2Half - dX2Half + (dZ2 * dX2) * (1f / 3f));
        point.z = point.z * Mathf.Sqrt(1f - dX2Half - dY2Half + (dX2 * dY2) * (1f / 3f));

        return point;
    }


    public static Vector3 Plane3Intersect(Plane p1, Plane p2, Plane p3)
    {
        return ((-p1.distance * Vector3.Cross(p2.normal, p3.normal)) +
                (-p2.distance * Vector3.Cross(p3.normal, p1.normal)) +
                (-p3.distance * Vector3.Cross(p1.normal, p2.normal))) / (Vector3.Dot(p1.normal, Vector3.Cross(p2.normal, p3.normal)));
    }
}