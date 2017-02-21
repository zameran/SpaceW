using System;

using UnityEngine;

namespace SpaceEngine.Core.Terrain.Deformation
{
    /// <summary>
    /// A deformation of space. Such a deformation maps a 3D source point to a 3D destination point. 
    /// The source space is called the local space, while the destination space is called the deformed space. 
    /// Source and destination points are defined with their x,y,z coordinates in an orthonormal reference frame. 
    /// A Deformation is also responsible to set the shader uniforms. 
    /// The default implementation of this class implements the identity deformation, i.e. the deformed point is equal to the local one.
    /// </summary>
    public class DeformationBase
    {
        public class Uniforms
        {
            public int blending, localToWorld, localToScreen;
            public int offset, camera, screenQuadCorners;
            public int screenQuadVerticals, radius, screenQuadCornerNorms;
            public int tangentFrameToWorld, tileToTangent;

            public Uniforms()
            {
                blending = Shader.PropertyToID("_Deform_Blending");
                localToWorld = Shader.PropertyToID("_Deform_LocalToWorld");
                localToScreen = Shader.PropertyToID("_Deform_LocalToScreen");
                offset = Shader.PropertyToID("_Deform_Offset");
                camera = Shader.PropertyToID("_Deform_Camera");
                screenQuadCorners = Shader.PropertyToID("_Deform_ScreenQuadCorners");
                screenQuadVerticals = Shader.PropertyToID("_Deform_ScreenQuadVerticals");
                radius = Shader.PropertyToID("_Deform_Radius");
                screenQuadCornerNorms = Shader.PropertyToID("_Deform_ScreenQuadCornerNorms");
                tangentFrameToWorld = Shader.PropertyToID("_Deform_TangentFrameToWorld");
                tileToTangent = Shader.PropertyToID("_Deform_TileToTangent");
            }
        }

        protected Uniforms uniforms;
        protected Matrix4x4d localToCamera;
        protected Matrix4x4d localToScreen;
        protected Matrix3x3d localToTangent;

        public DeformationBase()
        {
            uniforms = new Uniforms();
            localToCamera = new Matrix4x4d();
            localToScreen = new Matrix4x4d();
            localToTangent = new Matrix3x3d();
        }

        /// <summary>
        /// The corresponding point in the deformed (destination) space.
        /// </summary>
        /// <param name="localPoint">A point in the local (source) space.</param>
        /// <returns>Returns the deformed point corresponding to the given source point.</returns>
        public virtual Vector3d LocalToDeformed(Vector3d localPoint)
        {
            return localPoint;
        }

        /// <summary>
        /// This differential gives a linear approximation of the deformation around a given point, represented with a matrix. 
        /// More precisely, if 'p' is near localPoint, then the deformed point corresponding to 'p' can be approximated with:
        /// <example>
        /// <code>
        /// LocalToDeformedDifferential(localPoint) * (p - localPoint);
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="localPoint"></param>
        /// <param name="clamp">Clamp values to space?</param>
        /// <returns>Returns the differential of the deformation function at the given local point.</returns>
        public virtual Matrix4x4d LocalToDeformedDifferential(Vector3d localPoint, bool clamp = false)
        {
            return Matrix4x4d.Translate(new Vector3d(localPoint.x, localPoint.y, 0.0));
        }

        /// <summary>
        /// The local point corresponding to the given source point.
        /// </summary>
        /// <param name="deformedPoint">A point in the deformed (destination) space.</param>
        /// <returns>Returns the corresponding point in the local (source) space.</returns>
        public virtual Vector3d DeformedToLocal(Vector3d deformedPoint)
        {
            return deformedPoint;
        }

        /// <summary>
        /// The local bounding box corresponding to the given source disk.
        /// </summary>
        /// <param name="deformedCenter">The source disk center in deformed space.</param>
        /// <param name="deformedRadius">The source disk radius in deformed space.</param>
        /// <returns>Returns the local bounding box corresponding to the given source disk.</returns>
        public virtual Box2d DeformedToLocalBounds(Vector3d deformedCenter, double deformedRadius)
        {
            return new Box2d(deformedCenter.x - deformedRadius, deformedCenter.x + deformedRadius, deformedCenter.y - deformedRadius, deformedCenter.y + deformedRadius);
        }

        /// <summary>
        /// This reference frame is such that its xy plane is the tangent plane, at deformedPoint to the deformed surface corresponding to the local plane z = 0. 
        /// Note that this orthonormal reference frame doesn't give the differential of the inverse deformation funtion, which in general is not an orthonormal transformation. 
        /// If 'p' is a deformed point, then <code>DeformedToLocalFrame(deformedPoint) * p</code> gives the coordinates of 'p' in the orthonormal reference frame defined above.
        /// </summary>
        /// <param name="deformedPoint">A point in the deformed (destination) space.</param>
        /// <returns>Returns an orthonormal reference frame of the tangent space at the given deformed point.</returns>
        public virtual Matrix4x4d DeformedToTangentFrame(Vector3d deformedPoint)
        {
            return Matrix4x4d.Translate(new Vector3d(-deformedPoint.x, -deformedPoint.y, 0.0));
        }

        /// <summary>
        /// The distance in local (source) space between 'a' point and a bounding box.
        /// </summary>
        /// <param name="localPoint">A point in local space.</param>
        /// <param name="localBox">A bounding box in local space.</param>
        /// <returns>Returns the distance in local (source) space between 'a' point and a bounding box.</returns>
        public virtual double GetLocalDist(Vector3d localPoint, Box3d localBox)
        {
            return Math.Max(Math.Abs(localPoint.z - localBox.zmax),
                   Math.Max(Math.Min(Math.Abs(localPoint.x - localBox.xmin),
                   Math.Abs(localPoint.x - localBox.xmax)),
                   Math.Min(Math.Abs(localPoint.y - localBox.ymin),
                   Math.Abs(localPoint.y - localBox.ymax))));
        }

        /// <summary>
        /// The visibility of a bounding box in local space, in a view frustum defined in deformed space.
        /// </summary>
        /// <param name="node">
        /// A TerrainNode. This is node is used to get the camera position in local and deformed space with 
        /// <code>TerrainNode.GetLocalCamera</code> and <code>TerrainNode.GetDeformedCamera</code>,
        /// as well as the view frustum planes in deformed space with <code>TerrainNode.GetDeformedFrustumPlanes</code>.
        /// </param>
        /// <param name="localBox">a bounding box in local space.</param>
        /// <returns>Returns the visibility of a bounding box in local space, in a view frustum defined in deformed space.</returns>
        public virtual Frustum.VISIBILITY GetVisibility(TerrainNode node, Box3d localBox)
        {
            // localBox = deformedBox, so we can compare the deformed frustum with it
            return Frustum.GetVisibility(node.DeformedFrustumPlanes, localBox);
        }

        public virtual void SetUniforms(TerrainNode node, Material mat)
        {
            if (mat == null || node == null) return;

            var d1 = node.SplitDistance + 1.0f;
            var d2 = 2.0f * node.SplitDistance;

            localToCamera = (Matrix4x4d)GodManager.Instance.WorldToCamera * node.LocalToWorld;
            localToScreen = (Matrix4x4d)GodManager.Instance.CameraToScreen * localToCamera;

            Vector3d localCameraPos = node.LocalCameraPosition;
            Vector3d worldCamera = (Vector3d)GodManager.Instance.WorldCameraPos;

            Matrix4x4d A = LocalToDeformedDifferential(localCameraPos);
            Matrix4x4d B = DeformedToTangentFrame(worldCamera);

            Matrix4x4d ltot = B * node.LocalToWorld * A;

            localToTangent = new Matrix3x3d(ltot.m[0, 0], ltot.m[0, 1], ltot.m[0, 3], ltot.m[1, 0], ltot.m[1, 1], ltot.m[1, 3], ltot.m[3, 0], ltot.m[3, 1], ltot.m[3, 3]);

            mat.SetVector(uniforms.blending, new Vector2(d1, d2 - d1));
            mat.SetMatrix(uniforms.localToScreen, localToScreen.ToMatrix4x4());
            mat.SetMatrix(uniforms.localToWorld, node.LocalToWorld.ToMatrix4x4());

        }

        public virtual void SetUniforms(TerrainNode node, TerrainQuad quad, MaterialPropertyBlock matPropertyBlock)
        {
            if (matPropertyBlock == null || node == null || quad == null) return;

            var ox = quad.Ox;
            var oy = quad.Oy;
            var l = quad.Length;
            var distFactor = (double)node.DistanceFactor;
            var level = quad.Level;

            Vector3d camera = node.LocalCameraPosition;
            Vector3d c = node.LocalCameraPosition;

            Matrix3x3d m = localToTangent * (new Matrix3x3d(l, 0.0, ox - c.x, 0.0, l, oy - c.y, 0.0, 0.0, 1.0));

            matPropertyBlock.SetVector(uniforms.offset, new Vector4((float)ox, (float)oy, (float)l, (float)level));
            matPropertyBlock.SetVector(uniforms.camera, new Vector4((float)((camera.x - ox) / l), (float)((camera.y - oy) / l), (float)((camera.z - node.Body.HeightZ) / (l * distFactor)), (float)camera.z));
            matPropertyBlock.SetMatrix(uniforms.tileToTangent, m.ToMatrix4x4());

            SetScreenUniforms(node, quad, matPropertyBlock);
        }

        protected virtual void SetScreenUniforms(TerrainNode node, TerrainQuad quad, MaterialPropertyBlock matPropertyBlock)
        {
            var ox = quad.Ox;
            var oy = quad.Oy;
            var l = quad.Length;

            var p0 = new Vector3d(ox, oy, 0.0);
            var p1 = new Vector3d(ox + l, oy, 0.0);
            var p2 = new Vector3d(ox, oy + l, 0.0);
            var p3 = new Vector3d(ox + l, oy + l, 0.0);

            var corners = new Matrix4x4d(p0.x, p1.x, p2.x, p3.x, p0.y, p1.y, p2.y, p3.y, p0.z, p1.z, p2.z, p3.z, 1.0, 1.0, 1.0, 1.0);
            var verticals = new Matrix4x4d(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 1.0, 1.0, 1.0, 0.0, 0.0, 0.0, 0.0);

            matPropertyBlock.SetMatrix(uniforms.screenQuadCorners, (localToScreen * corners).ToMatrix4x4());
            matPropertyBlock.SetMatrix(uniforms.screenQuadVerticals, (localToScreen * verticals).ToMatrix4x4());
        }
    }
}