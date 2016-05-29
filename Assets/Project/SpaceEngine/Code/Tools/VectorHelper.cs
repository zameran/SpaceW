#region License
/* Procedural planet generator.
 *
 * Copyright (C) 2015-2016 Denis Ovchinnikov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using UnityEngine;

public static class VectorHelper
{
    public static Vector3 xzy(this Vector3 v)
    {
        return new Vector3(v.x, v.z, v.y);
    }

    public static Vector3 CombineVectors(params Vector3[] vectors)
    {
        Vector3 summ = Vector3.zero;

        for (int i = 0; i < vectors.Length; i++)
        {
            summ += vectors[i];
        }

        return summ;
    }

    public static Vector3 Middle(this Vector3 v1, Vector3 v2)
    {
        return v1 - v2;
    }

    public static Vector3 Abs(this Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    public static Vector3 NormalizeToRadius(this Vector3 v, float radius)
    {
        Vector3 normalized = new Vector3();

        normalized = v.normalized * radius;

        return normalized;
    }

    public static Vector3 NormalizeToRadiusUnNormalized(this Vector3 v, float radius)
    {
        Vector3 vector = new Vector3();

        vector = v * radius;

        return vector;
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
                (-p3.distance * Vector3.Cross(p1.normal, p2.normal))) /
                               (Vector3.Dot(p1.normal, Vector3.Cross(p2.normal, p3.normal)));
    }

    public static Vector3 RoundToInt(this Vector3 v)
    {
        return new Vector3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
    }

    public static float AngularRadius(Vector3 from, Vector3 to, float actualRadius)
    {
        return 2.0f * Mathf.Asin(actualRadius / (2 * Vector3.Distance(from, to)));
    }
}