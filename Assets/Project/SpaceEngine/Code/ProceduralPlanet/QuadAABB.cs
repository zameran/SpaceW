using System;

using UnityEngine;

public class QuadAABB
{
    public Vector3[] AABB { get; set; }

    public QuadAABB(Vector3[] AABB, bool forCulling)
    {
        this.AABB = new Vector3[AABB.Length];

        Array.Copy(AABB, this.AABB, forCulling ? 8 : 14);
    }
}