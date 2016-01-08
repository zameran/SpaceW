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
}