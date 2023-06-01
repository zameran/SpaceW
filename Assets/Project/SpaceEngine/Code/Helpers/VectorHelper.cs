#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using System;
using System.Linq;
using SpaceEngine.Core.Numerics.Vectors;
using UnityEngine;

namespace SpaceEngine.Helpers
{
    /// <summary>
    /// Class - extensions holder for a <see cref="Vector2"/>, <see cref="Vector3"/> and <see cref="Vector4"/>.
    /// </summary>
    public static class VectorHelper
    {
        public static Vector3 xzy(this Vector3 v)
        {
            return new Vector3(v.x, v.z, v.y);
        }

        public static Vector3 xy0(this Vector3 v)
        {
            return new Vector3(v.x, v.y, 0.0f);
        }

        public static Vector2 xy(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        /// <summary>
        /// Inverse of Y component of normalized target vector.
        /// </summary>
        /// <param name="v">Target.</param>
        /// <returns>Vector with inversed Y component.</returns>
        public static Vector2 InverseY(this Vector2 v)
        {
            return new Vector2(v.x, 1.0f - v.y);
        }

        /// <summary>
        /// Inverse of Y component of normalized target vector.
        /// </summary>
        /// <param name="v">Target.</param>
        /// <returns>Vector with inversed Y component.</returns>
        public static Vector3 InverseY(this Vector3 v)
        {
            return new Vector3(v.x, 1.0f - v.y, v.z);
        }

        public static Vector3 Summ(params Vector3[] vectors)
        {
            return vectors.Aggregate(Vector3.zero, (current, t) => current + t);
        }

        public static Vector3 Middle(this Vector3 v1, Vector3 v2)
        {
            return v1 - v2;
        }

        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        public static Vector3d Abs(this Vector3d v)
        {
            return new Vector3d(Math.Abs(v.x), Math.Abs(v.y), Math.Abs(v.z));
        }

        public static Vector4d Abs(this Vector4d v)
        {
            return new Vector4d(Math.Abs(v.x), Math.Abs(v.y), Math.Abs(v.z), Math.Abs(v.w));
        }

        public static Vector3 RotatePointAroundPivot(this Vector3 point, Vector3 pivot, Vector3 angles)
        {
            return RotatePointAroundPivot(point, pivot, Quaternion.Euler(angles));
        }

        public static Vector3 RotatePointAroundPivot(this Vector3 point, Vector3 pivot, Quaternion rotation)
        {
            return rotation * (point - pivot) + pivot;
        }

        public static Vector3 NormalizeToRadius(this Vector3 v, float radius)
        {
            return v.normalized * radius;
        }

        public static Vector3 NormalizeToRadiusUnNormalized(this Vector3 v, float radius)
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
            var dX2 = point.x * point.x;
            var dY2 = point.y * point.y;
            var dZ2 = point.z * point.z;

            var dX2Half = dX2 * 0.5f;
            var dY2Half = dY2 * 0.5f;
            var dZ2Half = dZ2 * 0.5f;

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

        public static Vector3 RoundToInt(this Vector3 v)
        {
            return new Vector3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        }

        public static float AngularRadius(Vector3 from, Vector3 to, float actualRadius)
        {
            return 2.0f * Mathf.Asin(actualRadius / (2 * Vector3.Distance(from, to)));
        }

        public static float QuickDistance(Vector3 v1, Vector3 v2)
        {
            var diff = v2 - v1;

            return diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;
        }

        public static Vector4 MakeFrom(Vector3 xyz, float w)
        {
            return new Vector4(xyz.x, xyz.y, xyz.z, w);
        }

        public static Vector3 MakeFrom(Vector3 xyz)
        {
            return new Vector3(xyz.x, xyz.y, xyz.z);
        }

        public static Vector3 Max(params Vector3[] vectors)
        {
            if (vectors == null || vectors.Length == 0) { Debug.Log("VectorHelper.Max: Problem!"); return Vector3.zero; }

            var max = new Vector3(-9e37f, -9e37f, -9e37f);

            return vectors.Aggregate(max, (current, t) => Vector3.Max(current, t));
        }

        public static Vector3 Min(params Vector3[] vectors)
        {
            if (vectors == null || vectors.Length == 0) { Debug.Log("VectorHelper.Min: Problem!"); return Vector3.zero; }

            var min = new Vector3(9e37f, 9e37f, 9e37f);

            return vectors.Aggregate(min, (current, t) => Vector3.Min(current, t));
        }

        public static Vector4 FromColor(this Color color)
        {
            return new Vector4(color.r, color.g, color.b, color.a);
        }

        public static Vector4 FromColor(this Color color, float customAlpha)
        {
            return new Vector4(color.r, color.g, color.b, customAlpha);
        }

        public static Color ToColor(this Vector4 vector)
        {
            return new Color(vector.x, vector.y, vector.z, vector.w);
        }

        public static Color ToColor(this Vector3 vector, float a)
        {
            return new Color(vector.x, vector.y, vector.z, a);
        }

        public static bool OneOfIsBiggerThan(this Vector3 v, float value)
        {
            return (v.x >= value || v.y >= value || v.z >= value);
        }
    }
}