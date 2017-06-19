using System;

using UnityEngine;

using Functions = SpaceEngine.Core.Numerics.Functions;

namespace SpaceEngine.Core.Terrain.Deformation
{
    /// <summary>
    /// A Deformation of space transforming planes to spheres. This deformation
    /// transforms the plane z=0 into a sphere of radius R centered at (0,0,-R).
    /// The plane z=h is transformed into the sphere of radius R+h. The
    /// deformation of p=(x,y,z) in local space is q=(R+z) P /\norm P\norm,
    /// where P=(x,y,R).
    /// See <see cref="DeformationBase"/> class for more info.
    /// </summary>
    public class DeformationSpherical : DeformationBase
    {
        /// <summary>
        /// The radius of the sphere into which the plane z = 0 must be deformed.
        /// </summary>
        public double R { get; protected set; }

        public DeformationSpherical(double R)
        {
            this.R = R;
        }

        public override Vector3d LocalToDeformed(Vector3d localPoint)
        {
            return (new Vector3d(localPoint.x, localPoint.y, R)).Normalized(localPoint.z + R);
        }

        public override Matrix4x4d LocalToDeformedDifferential(Vector3d localPoint, bool clamp = false)
        {
            if (!Functions.IsFinite(localPoint.x) || !Functions.IsFinite(localPoint.y) || !Functions.IsFinite(localPoint.z))
            {
                return Matrix4x4d.identity;
            }

            var point = new Vector3d(localPoint);

            if (clamp)
            {
                point.x = point.x - Math.Floor((point.x + R) / (2.0 * R)) * 2.0 * R;
                point.y = point.y - Math.Floor((point.y + R) / (2.0 * R)) * 2.0 * R;
            }

            var l = point.x * point.x + point.y * point.y + R * R;
            var c0 = 1.0 / Math.Sqrt(l);
            var c1 = c0 * R / l;

            return new Matrix4x4d((point.y * point.y + R * R) * c1, -point.x * point.y * c1, point.x * c0, R * point.x * c0,
                                   -point.x * point.y * c1, (point.x * point.x + R * R) * c1, point.y * c0, R * point.y * c0,
                                   -point.x * R * c1, -point.y * R * c1, R * c0, (R * R) * c0, 0.0, 0.0, 0.0, 1.0);
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
                return new Box2d();
            }

            var k = (1.0 - deformedRadius * deformedRadius / (2.0 * R * R)) * (new Vector3d(point.x, point.y, R)).Magnitude();
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

        public override Frustum.VISIBILITY GetVisibility(TerrainNode node, Box3d localBox)
        {
            var deformedBox = new Vector3d[4];
            deformedBox[0] = LocalToDeformed(new Vector3d(localBox.xmin, localBox.ymin, localBox.zmin));
            deformedBox[1] = LocalToDeformed(new Vector3d(localBox.xmax, localBox.ymin, localBox.zmin));
            deformedBox[2] = LocalToDeformed(new Vector3d(localBox.xmax, localBox.ymax, localBox.zmin));
            deformedBox[3] = LocalToDeformed(new Vector3d(localBox.xmin, localBox.ymax, localBox.zmin));

            var a = (localBox.zmax + R) / (localBox.zmin + R);
            var dx = (localBox.xmax - localBox.xmin) / 2 * a;
            var dy = (localBox.ymax - localBox.ymin) / 2 * a;
            var dz = localBox.zmax + R;
            var f = Math.Sqrt(dx * dx + dy * dy + dz * dz) / (localBox.zmin + R);

            var v0 = GetClipVisibility(node.DeformedFrustumPlanes[0], deformedBox, f);
            if (v0 == Frustum.VISIBILITY.INVISIBLE) { return Frustum.VISIBILITY.INVISIBLE; }

            var v1 = GetClipVisibility(node.DeformedFrustumPlanes[1], deformedBox, f);
            if (v1 == Frustum.VISIBILITY.INVISIBLE) { return Frustum.VISIBILITY.INVISIBLE; }

            var v2 = GetClipVisibility(node.DeformedFrustumPlanes[2], deformedBox, f);
            if (v2 == Frustum.VISIBILITY.INVISIBLE) { return Frustum.VISIBILITY.INVISIBLE; }

            var v3 = GetClipVisibility(node.DeformedFrustumPlanes[3], deformedBox, f);
            if (v3 == Frustum.VISIBILITY.INVISIBLE) { return Frustum.VISIBILITY.INVISIBLE; }

            var v4 = GetClipVisibility(node.DeformedFrustumPlanes[4], deformedBox, f);
            if (v4 == Frustum.VISIBILITY.INVISIBLE) { return Frustum.VISIBILITY.INVISIBLE; }

            var lSq = node.DeformedCameraPosition.SqrMagnitude();
            var rm = R + Math.Min(0.0, localBox.zmin);
            var rM = R + localBox.zmax;
            var rmSq = rm * rm;
            var rMSq = rM * rM;

            var farPlane = new Vector4d(node.DeformedCameraPosition.x, 
                                        node.DeformedCameraPosition.y, 
                                        node.DeformedCameraPosition.z, Math.Sqrt((lSq - rmSq) * (rMSq - rmSq)) - rmSq);

            var v5 = GetClipVisibility(farPlane, deformedBox, f);
            if (v5 == Frustum.VISIBILITY.INVISIBLE) { return Frustum.VISIBILITY.INVISIBLE; }

            if (v0 == Frustum.VISIBILITY.FULLY && v1 == Frustum.VISIBILITY.FULLY &&
                v2 == Frustum.VISIBILITY.FULLY && v3 == Frustum.VISIBILITY.FULLY &&
                v4 == Frustum.VISIBILITY.FULLY && v5 == Frustum.VISIBILITY.FULLY)
            {
                return Frustum.VISIBILITY.FULLY;
            }

            return Frustum.VISIBILITY.PARTIALLY;
        }

        public static Frustum.VISIBILITY GetClipVisibility(Vector4d clip, Vector3d[] b, double f)
        {
            var o = b[0].x * clip.x + b[0].y * clip.y + b[0].z * clip.z;
            var p = o + clip.w > 0.0;

            if ((o * f + clip.w > 0.0) == p)
            {
                o = b[1].x * clip.x + b[1].y * clip.y + b[1].z * clip.z;

                if ((o + clip.w > 0.0) == p && (o * f + clip.w > 0.0) == p)
                {
                    o = b[2].x * clip.x + b[2].y * clip.y + b[2].z * clip.z;

                    if ((o + clip.w > 0.0) == p && (o * f + clip.w > 0.0) == p)
                    {
                        o = b[3].x * clip.x + b[3].y * clip.y + b[3].z * clip.z;

                        return (o + clip.w > 0.0) == p && (o * f + clip.w > 0.0) == p ? (p ? Frustum.VISIBILITY.FULLY : Frustum.VISIBILITY.INVISIBLE) : Frustum.VISIBILITY.PARTIALLY;
                    }
                }
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
            matPropertyBlock.SetMatrix(uniforms.screenQuadCorners, (localToScreen * quad.DeformedCorners).ToMatrix4x4());
            matPropertyBlock.SetMatrix(uniforms.screenQuadVerticals, (localToScreen * quad.DeformedVerticals).ToMatrix4x4());
            matPropertyBlock.SetVector(uniforms.screenQuadCornerNorms, quad.Lengths.ToVector4());
            matPropertyBlock.SetMatrix(uniforms.tangentFrameToWorld, quad.TangentFrameToWorld.ToMatrix4x4());
        }
    }
}