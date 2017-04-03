#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//     notice, this list of conditions and the following disclaimer.
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
// Creation Date: 2017.03.25
// Creation Time: 2:17 PM
// Creator: zameran
#endregion

using System;

using UnityEngine;

namespace SpaceEngine.Core.Terrain.Deformation
{
    /// <summary>
    /// A Deformation of space transforming planes to cylinders. 
    /// This deformation transforms the plane z=0 into a cylinder of radius R.
    /// The deformation of p = (x, y, z) in local space is q = (x, r.sin(a), r.cos(a)), where r = R - z and a = y / R.
    /// </summary>
    public class DeformationCylindrical : DeformationBase
    {
        /// <summary>
        /// The radius of the cylinder into which the plane z = 0 must be deformed.
        /// </summary>
        public double R { get; protected set; }

        public DeformationCylindrical(double R)
        {
            this.R = R;
        }

        public override Vector3d LocalToDeformed(Vector3d localPoint)
        {
            var alpha = localPoint.y / R;
            var r = R - localPoint.z;

            return new Vector3d(localPoint.x, r * Math.Sin(alpha), -r * Math.Cos(alpha));
        }

        public override Matrix4x4d LocalToDeformedDifferential(Vector3d localPoint, bool clamp = false)
        {
            var alpha = localPoint.y / R;

            return new Matrix4x4d(1.0, 0.0, 0.0, localPoint.x,
                                  0.0, Math.Cos(alpha), -Math.Sin(alpha), R * Math.Sin(alpha),
                                  0.0, Math.Sin(alpha), Math.Cos(alpha), -R * Math.Cos(alpha),
                                  0.0, 0.0, 0.0, 1.0);
        }

        public override Vector3d DeformedToLocal(Vector3d deformedPoint)
        {
            var y = R * Math.Atan2(deformedPoint.y, -deformedPoint.z);
            var z = R - Math.Sqrt(deformedPoint.y * deformedPoint.y + deformedPoint.z * deformedPoint.z);

            return new Vector3d(deformedPoint.x, y, z);
        }

        public override Box2d DeformedToLocalBounds(Vector3d deformedCenter, double deformedRadius)
        {
            // TODO : Make it!
            return base.DeformedToLocalBounds(deformedCenter, deformedRadius);
        }

        public override Matrix4x4d DeformedToTangentFrame(Vector3d deformedPoint)
        {
            var Uz = new Vector3d(0.0, -deformedPoint.y, -deformedPoint.z).normalized;
            var Ux = Vector3d.right; // TODO : UNIT_X
            var Uy = Uz.Cross(Ux);
            var O = new Vector3d(deformedPoint.x, -Uz.y * R, -Uz.z * R);

            return new Matrix4x4d(Ux.x, Ux.y, Ux.z, -O.Dot(Ux),
                                  Uy.x, Uy.y, Uy.z, -O.Dot(Uy),
                                  Uz.x, Uz.y, Uz.z, -O.Dot(Uz),
                                  0.0, 0.0, 0.0, 1.0);
        }

        public override Frustum.VISIBILITY GetVisibility(TerrainNode node, Box3d localBox)
        {
            var deformedBox = new Vector3d[4];
            deformedBox[0] = LocalToDeformed(new Vector3d(localBox.xmin, localBox.ymin, localBox.zmax));
            deformedBox[1] = LocalToDeformed(new Vector3d(localBox.xmax, localBox.ymin, localBox.zmax));
            deformedBox[2] = LocalToDeformed(new Vector3d(localBox.xmax, localBox.ymax, localBox.zmax));
            deformedBox[3] = LocalToDeformed(new Vector3d(localBox.xmin, localBox.ymax, localBox.zmax));

            var dy = localBox.ymax - localBox.ymin;
            var f = (float)((R - localBox.zmin) / ((R - localBox.zmax) * Math.Cos(dy / (2.0 * R))));

            Vector4d[] deformedFrustumPlanes = node.DeformedFrustumPlanes;

            Frustum.VISIBILITY v0 = GetVisibility(deformedFrustumPlanes[0], deformedBox, f);
            if (v0 == Frustum.VISIBILITY.INVISIBLE)
            {
                return Frustum.VISIBILITY.INVISIBLE;
            }
            Frustum.VISIBILITY v1 = GetVisibility(deformedFrustumPlanes[1], deformedBox, f);
            if (v1 == Frustum.VISIBILITY.INVISIBLE)
            {
                return Frustum.VISIBILITY.INVISIBLE;
            }
            Frustum.VISIBILITY v2 = GetVisibility(deformedFrustumPlanes[2], deformedBox, f);
            if (v2 == Frustum.VISIBILITY.INVISIBLE)
            {
                return Frustum.VISIBILITY.INVISIBLE;
            }
            Frustum.VISIBILITY v3 = GetVisibility(deformedFrustumPlanes[3], deformedBox, f);
            if (v3 == Frustum.VISIBILITY.INVISIBLE)
            {
                return Frustum.VISIBILITY.INVISIBLE;
            }
            Frustum.VISIBILITY v4 = GetVisibility(deformedFrustumPlanes[4], deformedBox, f);
            if (v4 == Frustum.VISIBILITY.INVISIBLE)
            {
                return Frustum.VISIBILITY.INVISIBLE;
            }

            if (v0 == Frustum.VISIBILITY.FULLY && v1 == Frustum.VISIBILITY.FULLY &&
                v2 == Frustum.VISIBILITY.FULLY && v3 == Frustum.VISIBILITY.FULLY &&
                v4 == Frustum.VISIBILITY.FULLY)
            {
                return Frustum.VISIBILITY.FULLY;
            }

            return Frustum.VISIBILITY.PARTIALLY;
        }

        public static Frustum.VISIBILITY GetVisibility(Vector4d clip, Vector3d[] b, double f)
        {
            double c1 = b[0].x * clip.x + clip.w;
            double c2 = b[1].x * clip.x + clip.w;
            double c3 = b[2].x * clip.x + clip.w;
            double c4 = b[3].x * clip.x + clip.w;
            double o1 = b[0].y * clip.y + b[0].z * clip.z;
            double o2 = b[1].y * clip.y + b[1].z * clip.z;
            double o3 = b[2].y * clip.y + b[2].z * clip.z;
            double o4 = b[3].y * clip.y + b[3].z * clip.z;
            double p1 = o1 + c1;
            double p2 = o2 + c2;
            double p3 = o3 + c3;
            double p4 = o4 + c4;
            double p5 = o1 * f + c1;
            double p6 = o2 * f + c2;
            double p7 = o3 * f + c3;
            double p8 = o4 * f + c4;

            if (p1 <= 0 && p2 <= 0 && p3 <= 0 && p4 <= 0 && p5 <= 0 && p6 <= 0 && p7 <= 0 && p8 <= 0)
            {
                return Frustum.VISIBILITY.INVISIBLE;
            }

            if (p1 > 0 && p2 > 0 && p3 > 0 && p4 > 0 && p5 > 0 && p6 > 0 && p7 > 0 && p8 > 0)
            {
                return Frustum.VISIBILITY.FULLY;
            }

            return Frustum.VISIBILITY.PARTIALLY;
        }

        public override void SetUniforms(TerrainNode node, Material mat)
        {
            if (mat == null || node == null) return;

            base.SetUniforms(node, mat);

            mat.SetFloat(uniforms.radius, (float)R);
        }

        protected override void SetScreenUniforms(TerrainNode node, TerrainQuad quad, MaterialPropertyBlock matPropertyBlock)
        {
            base.SetScreenUniforms(node, quad, matPropertyBlock);
        }
    }
}