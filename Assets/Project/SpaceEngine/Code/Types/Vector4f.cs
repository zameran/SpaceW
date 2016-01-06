using System;
using UnityEngine;

[Serializable]
public struct Vector4f
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Vector4f(float X, float Y, float Z, float W)
    {
        this.X = X;
        this.Y = Y;
        this.Z = Z;
        this.W = W;
    }

    public static implicit operator Vector4(Vector4f Vector)
    {
        return new Vector4(Vector.X, Vector.Y, Vector.Z, Vector.W);
    }

    public static implicit operator Vector4f(Vector4 Vector)
    {
        return new Vector4f(Vector.x, Vector.y, Vector.z, Vector.w);
    }

    public override string ToString()
    {
        return string.Format("Vector4f({0}, {1}, {2}, {3})", X, Y, Z, W);
    }
}