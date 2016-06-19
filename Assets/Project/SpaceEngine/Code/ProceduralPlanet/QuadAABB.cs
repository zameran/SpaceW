using System;

using UnityEngine;

public class QuadAABB
{
    public Vector3[] AABB { get; set; }

    public Bounds Bounds { get; set; }

    public Vector3 Min { get; set; }
    public Vector3 Max { get; set; }

    public QuadAABB(Vector3[] AABB, bool forCulling)
    {
        this.AABB = new Vector3[AABB.Length];

        this.Bounds = new Bounds(Vector3.zero, new Vector3(9e37f, 9e37f, 9e37f));

        Array.Copy(AABB, this.AABB, forCulling ? 8 : 14);
    }
}