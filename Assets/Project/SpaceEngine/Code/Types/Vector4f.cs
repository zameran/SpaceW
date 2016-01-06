using System;
using UnityEngine;

[Serializable]
public struct Vector4f
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public string Xs;
    public string Ys;
    public string Zs;
    public string Ws;

    public Vector4f(float X, float Y, float Z, float W)
    {
        this.X = X;
        this.Y = Y;
        this.Z = Z;
        this.W = W;

        this.Xs = "";
        this.Ys = "";
        this.Zs = "";
        this.Ws = "";
    }

    public Vector4f(string Xs, string Ys, string Zs, string Ws)
    {
        this.X = 0;
        this.Y = 0;
        this.Z = 0;
        this.W = 0;

        this.Xs = Xs;
        this.Ys = Ys;
        this.Zs = Zs;
        this.Ws = Ws;
    }

    public Vector4f(float X, float Y, float Z, float W, string Xs, string Ys, string Zs, string Ws)
    {
        this.X = X;
        this.Y = Y;
        this.Z = Z;
        this.W = W;

        this.Xs = Xs;
        this.Ys = Ys;
        this.Zs = Zs;
        this.Ws = Ws;
    }

    public static implicit operator Vector4(Vector4f Vector)
    {
        return new Vector4(Vector.X, Vector.Y, Vector.Z, Vector.W);
    }

    public static implicit operator Vector4f(Vector4 Vector)
    {
        return new Vector4f(Vector.x, Vector.y, Vector.z, Vector.w, "", "", "", "");
    }

    public override string ToString()
    {
        return string.Format("Vector4f({0}, {1}, {2}, {3})", X, Y, Z, W);
    }
}