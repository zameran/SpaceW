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
// Creation Date: 2017.03.28
// Creation Time: 2:18 PM
// Creator: zameran

#endregion

using System;
using SpaceEngine.Core.Numerics;
using SpaceEngine.Core.Numerics.Matrices;
using SpaceEngine.Core.Numerics.Shapes;
using SpaceEngine.Core.Numerics.Vectors;
using UnityEngine;

namespace SpaceEngine.Core.Terrain.Deformation
{
    /// <summary>
    ///     A Deformation of space transforming planes to spheres. This deformation
    ///     transforms the plane z=0 into a sphere of radius R centered at (0,0,-R).
    ///     The plane z=h is transformed into the sphere of radius R+h. The
    ///     deformation of p=(x,y,z) in local space is q=(R+z) P /\norm P\norm,
    ///     where P=(x,y,R).
    ///     See <see cref="DeformationBase" /> class for more info.
    /// </summary>
    public class DeformationSpherical : DeformationBase
    {
        public DeformationSpherical(double R)
        {
            this.R = R;
        }

        /// <summary>
        ///     The radius of the sphere into which the plane z = 0 must be deformed.
        /// </summary>
        public double R { get; protected set; }

        public override Vector3d LocalToDeformed(double x, double y, double z)
        {
            return new Vector3d(x, y, R).Normalized(z + R);
        }

        public Vector3d LocalToDeformed(Vector3d localPoint)
        {
            return LocalToDeformed(localPoint.x, localPoint.y, localPoint.z);
        }

        public override Matrix4x4d LocalToDeformedDifferential(Vector3d localPoint, bool clamp = false)
        {
            if (!Functions.IsFinite(localPoint.x) || !Functions.IsFinite(localPoint.y) || !Functions.IsFinite(localPoint.z))
            {
                return Matrix4x4d.identity;
            }

            var point = new Vector2d(localPoint);

            if (clamp)
            {
                point.x = point.x - Math.Floor((point.x + R) / (2.0 * R)) * 2.0 * R;
                point.y = point.y - Math.Floor((point.y + R) / (2.0 * R)) * 2.0 * R;
            }

            var r2 = R * R;
            var l = point.x * point.x + point.y * point.y + r2;
            var c0 = 1.0 / Math.Sqrt(l);
            var c1 = c0 * R / l;

            return new Matrix4x4d((point.y * point.y + r2) * c1, -point.x * point.y * c1, point.x * c0, R * point.x * c0,
                                  -point.x * point.y * c1, (point.x * point.x + r2) * c1, point.y * c0, R * point.y * c0,
                                  -point.x * R * c1, -point.y * R * c1, R * c0, r2 * c0, 0.0, 0.0, 0.0, 1.0);
        }

        public override Vector3d DeformedToLocal(Vector3d deformedPoint)
        {
            var l = deformedPoint.Magnitude();

            if (deformedPoint.z >= Math.Abs(deformedPoint.x) && deformedPoint.z >= Math.Abs(deformedPoint.y))
            {
                return new Vector3d(deformedPoint.x / deformedPoint.z * R, deformedPoint.y / deformedPoint.z * R, l - R);
            }

            if (deformedPoint.z <= -Math.Abs(deformedPoint.x) && deformedPoint.z <= -Math.Abs(deformedPoint.y))
            {
                return new Vector3d(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
            }

            if (deformedPoint.y >= Math.Abs(deformedPoint.x) && deformedPoint.y >= Math.Abs(deformedPoint.z))
            {
                return new Vector3d(deformedPoint.x / deformedPoint.y * R, (2.0 - deformedPoint.z / deformedPoint.y) * R, l - R);
            }

            if (deformedPoint.y <= -Math.Abs(deformedPoint.x) && deformedPoint.y <= -Math.Abs(deformedPoint.z))
            {
                return new Vector3d(-deformedPoint.x / deformedPoint.y * R, (-2.0 - deformedPoint.z / deformedPoint.y) * R, l - R);
            }

            if (deformedPoint.x >= Math.Abs(deformedPoint.y) && deformedPoint.x >= Math.Abs(deformedPoint.z))
            {
                return new Vector3d((2.0 - deformedPoint.z / deformedPoint.x) * R, deformedPoint.y / deformedPoint.x * R, l - R);
            }

            if (deformedPoint.x <= -Math.Abs(deformedPoint.y) && deformedPoint.x <= -Math.Abs(deformedPoint.z))
            {
                return new Vector3d((-2.0 - deformedPoint.z / deformedPoint.x) * R, -deformedPoint.y / deformedPoint.x * R, l - R);
            }

            Debug.Log("DeformationSpherical: DeformToLocal fail!");

            return Vector3d.zero;
        }

        public override Box2d DeformedToLocalBounds(Vector3d deformedCenter, double deformedRadius)
        {
            var point = DeformedToLocal(deformedCenter);

            if (double.IsInfinity(point.x) || double.IsInfinity(point.y))
            {
                return Box2d.infinity;
            }

            var k = (1.0 - deformedRadius * deformedRadius / (2.0 * R * R)) * new Vector3d(point.x, point.y, R).Magnitude();
            var A = k * k - point.x * point.x;
            var B = k * k - point.y * point.y;
            var C = -2.0 * point.x * point.y;
            var D = -2.0 * R * R * point.x;
            var E = -2.0 * R * R * point.y;
            var F = R * R * (k * k - R * R);

            var a = C * C - 4.0 * A * B;
            var b = 2.0 * C * E - 4.0 * B * D;
            var c = E * E - 4.0 * B * F;
            var d = Math.Sqrt(b * b - 4.0 * a * c);
            var x1 = (-b - d) / (2.0 * a);
            var x2 = (-b + d) / (2.0 * a);

            b = 2.0 * C * D - 4.0 * A * E;
            c = D * D - 4.0 * A * F;
            d = Math.Sqrt(b * b - 4.0 * a * c);

            var y1 = (-b - d) / (2.0 * a);
            var y2 = (-b + d) / (2.0 * a);

            return new Box2d(new Vector2d(x1, y1), new Vector2d(x2, y2));
        }

        public override Matrix4x4d DeformedToTangentFrame(Vector3d deformedPoint)
        {
            var Uz = deformedPoint.Normalized();
            var Ux = new Vector3d(0.0, 1.0, 0.0).Cross(Uz).Normalized();
            var Uy = Uz.Cross(Ux);

            return new Matrix4x4d(Ux.x, Ux.y, Ux.z, 0.0,
                                  Uy.x, Uy.y, Uy.z, 0.0,
                                  Uz.x, Uz.y, Uz.z, -R,
                                  0.0, 0.0, 0.0, 1.0);
        }

        public override Frustum3d.VISIBILITY GetVisibility(TerrainNode node, Box3d localBox, Vector3d[] deformedBox)
        {
            var a = (localBox.Max.z + R) / (localBox.Min.z + R);
            var dx = (localBox.Max.x - localBox.Min.x) / 2 * a;
            var dy = (localBox.Max.y - localBox.Min.y) / 2 * a;
            var dz = localBox.Max.z + R;
            var f = Math.Sqrt(dx * dx + dy * dy + dz * dz) / (localBox.Min.z + R);

            var v0 = GetClipVisibility(node.DeformedFrustumPlanes[0], deformedBox, f);

            if (v0 == Frustum3d.VISIBILITY.INVISIBLE)
            {
                return Frustum3d.VISIBILITY.INVISIBLE;
            }

            var v1 = GetClipVisibility(node.DeformedFrustumPlanes[1], deformedBox, f);

            if (v1 == Frustum3d.VISIBILITY.INVISIBLE)
            {
                return Frustum3d.VISIBILITY.INVISIBLE;
            }

            var v2 = GetClipVisibility(node.DeformedFrustumPlanes[2], deformedBox, f);

            if (v2 == Frustum3d.VISIBILITY.INVISIBLE)
            {
                return Frustum3d.VISIBILITY.INVISIBLE;
            }

            var v3 = GetClipVisibility(node.DeformedFrustumPlanes[3], deformedBox, f);

            if (v3 == Frustum3d.VISIBILITY.INVISIBLE)
            {
                return Frustum3d.VISIBILITY.INVISIBLE;
            }

            var v4 = GetClipVisibility(node.DeformedFrustumPlanes[4], deformedBox, f);

            if (v4 == Frustum3d.VISIBILITY.INVISIBLE)
            {
                return Frustum3d.VISIBILITY.INVISIBLE;
            }

            var lSq = node.DeformedCameraPosition.SqrMagnitude();
            var rm = R + Math.Min(0.0, localBox.Min.z);
            var rM = R + localBox.Max.z;
            var rmSq = rm * rm;
            var rMSq = rM * rM;

            var farPlane = new Vector4d(node.DeformedCameraPosition.x,
                                        node.DeformedCameraPosition.y,
                                        node.DeformedCameraPosition.z, Math.Sqrt((lSq - rmSq) * (rMSq - rmSq)) - rmSq);

            var v5 = GetClipVisibility(farPlane, deformedBox, f);

            if (v5 == Frustum3d.VISIBILITY.INVISIBLE)
            {
                return Frustum3d.VISIBILITY.INVISIBLE;
            }

            if (v0 == Frustum3d.VISIBILITY.FULLY && v1 == Frustum3d.VISIBILITY.FULLY &&
                v2 == Frustum3d.VISIBILITY.FULLY && v3 == Frustum3d.VISIBILITY.FULLY &&
                v4 == Frustum3d.VISIBILITY.FULLY && v5 == Frustum3d.VISIBILITY.FULLY)
            {
                return Frustum3d.VISIBILITY.FULLY;
            }

            return Frustum3d.VISIBILITY.PARTIALLY;
        }

        public static Frustum3d.VISIBILITY GetClipVisibility(Vector4d clip, Vector3d[] b, double f)
        {
            var o = b[0].x * clip.x + b[0].y * clip.y + b[0].z * clip.z;
            var p = o + clip.w > 0.0;

            if (o * f + clip.w > 0.0 == p)
            {
                o = b[1].x * clip.x + b[1].y * clip.y + b[1].z * clip.z;

                if (o + clip.w > 0.0 == p && o * f + clip.w > 0.0 == p)
                {
                    o = b[2].x * clip.x + b[2].y * clip.y + b[2].z * clip.z;

                    if (o + clip.w > 0.0 == p && o * f + clip.w > 0.0 == p)
                    {
                        o = b[3].x * clip.x + b[3].y * clip.y + b[3].z * clip.z;

                        return o + clip.w > 0.0 == p && o * f + clip.w > 0.0 == p ? p ? Frustum3d.VISIBILITY.FULLY : Frustum3d.VISIBILITY.INVISIBLE : Frustum3d.VISIBILITY.PARTIALLY;
                    }
                }
            }

            return Frustum3d.VISIBILITY.PARTIALLY;
        }

        /// <inheritdoc />
        public override Matrix4x4 CalculateDeformedScreenQuadCorners(TerrainNode node, TerrainQuad quad)
        {
            return (node.LocalToScreen * quad.DeformedCorners).ToMatrix4x4();
        }

        /// <inheritdoc />
        public override Matrix4x4 CalculateDeformedScreenQuadVerticals(TerrainNode node, TerrainQuad quad)
        {
            return (node.LocalToScreen * quad.DeformedVerticals).ToMatrix4x4();
        }

        public override void SetUniforms(TerrainNode node, Material target)
        {
            if (target == null || node == null)
            {
                return;
            }

            base.SetUniforms(node, target);

            target.SetFloat(uniforms.radius, (float)R);
        }

        protected override void SetScreenUniforms(TerrainNode node, TerrainQuad quad, MaterialPropertyBlock target)
        {
            base.SetScreenUniforms(node, quad, target);

            target.SetVector(uniforms.screenQuadCornerNorms, quad.Lengths.ToVector4());
            target.SetMatrix(uniforms.tangentFrameToWorld, quad.TangentFrameToWorld.ToMatrix4x4());
        }

        protected override void SetScreenUniforms(TerrainNode node, TerrainQuad quad, Material target)
        {
            base.SetScreenUniforms(node, quad, target);

            target.SetVector(uniforms.screenQuadCornerNorms, quad.Lengths.ToVector4());
            target.SetMatrix(uniforms.tangentFrameToWorld, quad.TangentFrameToWorld.ToMatrix4x4());
        }
    }
}