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

        public DeformationBase()
        {
            uniforms = new Uniforms();
        }

        /// <summary>
        /// The corresponding point in the deformed (destination) space.
        /// </summary>
        /// <param name="x">A X coordinate of point in the local (source) space.</param>
        /// <param name="y">A Y coordinate of point in the local (source) space.</param>
        /// <param name="z">A Z coordinate of point in the local (source) space.</param>
        /// <returns>Returns the deformed point corresponding to the given source point.</returns>
        public virtual Vector3d LocalToDeformed(double x, double y, double z)
        {
            return new Vector3d(x, y, z);
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
        /// The distance in local (source) space between a point and a bounding box.
        /// </summary>
        /// <param name="localPoint">A point in local space.</param>
        /// <param name="localBox">A bounding box in local space.</param>
        /// <returns>Returns the distance in local (source) space between 'a' point and a bounding box.</returns>
        public virtual double GetLocalDistance(Vector3d localPoint, Box3d localBox)
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
        /// <param name="localBox">A bounding box in local space.</param>
        /// <param name="deformedBox">A bounding box in deformation space. Should be precalculated.</param>
        /// <returns>Returns the visibility of a bounding box in local space, in a view frustum defined in deformed space.</returns>
        public virtual Frustum3d.VISIBILITY GetVisibility(TerrainNode node, Box3d localBox, Vector3d[] deformedBox)
        {
            // localBox = deformedBox, so we can compare the deformed frustum with it
            return Frustum3d.GetVisibility(node.DeformedFrustumPlanes, localBox);
        }

        public virtual void SetUniforms(TerrainNode node, Material target)
        {
            if (target == null || node == null) return;

            target.SetVector(uniforms.blending, node.DistanceBlending);
            target.SetMatrix(uniforms.localToScreen, node.LocalToScreen.ToMatrix4x4());
            target.SetMatrix(uniforms.localToWorld, node.LocalToWorld.ToMatrix4x4());
        }

        public virtual Vector4 CalculateDeformedOffset(TerrainQuad quad)
        {
            return quad.DeformedOffset;
        }

        public virtual Vector4 CalculateDeformedCameraPosition(TerrainNode node, TerrainQuad quad)
        {
            return new Vector4((float)((node.LocalCameraPosition.x - quad.Ox) / quad.Length),
                               (float)((node.LocalCameraPosition.y - quad.Oy) / quad.Length),
                               (float)((node.LocalCameraPosition.z - node.ParentBody.HeightZ) / (quad.Length * (double)node.DistanceFactor)),
                               (float)node.LocalCameraPosition.z);
        }

        public virtual Matrix4x4 CalculateDeformedLocalToTangent(TerrainNode node, TerrainQuad quad)
        {
            return (node.DeformedLocalToTangent * new Matrix4x4d(quad.Length, 0.0, quad.Ox - node.LocalCameraPosition.x, 0.0,
                                                                 0.0, quad.Length, quad.Oy - node.LocalCameraPosition.y, 0.0,
                                                                 0.0, 0.0, 1.0, 0.0,
                                                                 0.0, 0.0, 0.0, 1.0)).ToMatrix4x4();
        }

        public virtual Matrix4x4 CalculateDeformedScreenQuadCorners(TerrainNode node, TerrainQuad quad)
        {
            return (node.LocalToScreen * quad.FlatCorners).ToMatrix4x4();
        }

        public virtual Matrix4x4 CalculateDeformedScreenQuadVerticals(TerrainNode node, TerrainQuad quad)
        {
            return (node.LocalToScreen * quad.FlatVerticals).ToMatrix4x4();
        }

        public virtual void SetUniforms(TerrainNode node, TerrainQuad quad, MaterialPropertyBlock target)
        {
            if (target == null || node == null || quad == null) return;

            target.SetVector(uniforms.offset, CalculateDeformedOffset(quad));
            target.SetVector(uniforms.camera, CalculateDeformedCameraPosition(node, quad));

            target.SetMatrix(uniforms.tileToTangent, CalculateDeformedLocalToTangent(node, quad));

            SetScreenUniforms(node, quad, target);
        }

        public virtual void SetUniforms(TerrainNode node, TerrainQuad quad, Material target)
        {
            if (target == null || node == null || quad == null) return;

            target.SetVector(uniforms.offset, CalculateDeformedOffset(quad));
            target.SetVector(uniforms.camera, CalculateDeformedCameraPosition(node, quad));

            target.SetMatrix(uniforms.tileToTangent, CalculateDeformedLocalToTangent(node, quad));

            SetScreenUniforms(node, quad, target);
        }

        protected virtual void SetScreenUniforms(TerrainNode node, TerrainQuad quad, MaterialPropertyBlock target)
        {
            target.SetMatrix(uniforms.screenQuadCorners, CalculateDeformedScreenQuadCorners(node, quad));
            target.SetMatrix(uniforms.screenQuadVerticals, CalculateDeformedScreenQuadVerticals(node, quad));
        }

        protected virtual void SetScreenUniforms(TerrainNode node, TerrainQuad quad, Material target)
        {
            target.SetMatrix(uniforms.screenQuadCorners, CalculateDeformedScreenQuadCorners(node, quad));
            target.SetMatrix(uniforms.screenQuadVerticals, CalculateDeformedScreenQuadVerticals(node, quad));
        }
    }
}