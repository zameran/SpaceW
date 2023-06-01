﻿#region License

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
using SpaceEngine.Core.Numerics.Matrices;
using SpaceEngine.Core.Numerics.Shapes;
using SpaceEngine.Core.Numerics.Vectors;
using UnityEngine;

namespace SpaceEngine.Core.Terrain.Deformation
{
    /// <summary>
    ///     A deformation of space. Such a deformation maps a 3D source point to a 3D destination point.
    ///     The source space is called the local space, while the destination space is called the deformed space.
    ///     Source and destination points are defined with their x,y,z coordinates in an orthonormal reference frame.
    ///     A Deformation is also responsible to set the shader uniforms.
    ///     The default implementation of this class implements the identity deformation, i.e. the deformed point is equal to the local one.
    /// </summary>
    public class DeformationBase
    {
        protected Uniforms uniforms;

        public DeformationBase()
        {
            uniforms = new Uniforms();
        }

        /// <summary>
        ///     The corresponding point in the deformed (destination) space.
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
        ///     This differential gives a linear approximation of the deformation around a given point, represented with a matrix.
        ///     More precisely, if 'p' is near localPoint, then the deformed point corresponding to 'p' can be approximated with:
        ///     <example>
        ///         <code>
        /// LocalToDeformedDifferential(localPoint) * (p - localPoint);
        /// </code>
        ///     </example>
        /// </summary>
        /// <param name="localPoint"></param>
        /// <param name="clamp">Clamp values to space?</param>
        /// <returns>Returns the differential of the deformation function at the given local point.</returns>
        public virtual Matrix4x4d LocalToDeformedDifferential(Vector3d localPoint, bool clamp = false)
        {
            return Matrix4x4d.Translate(new Vector3d(localPoint.x, localPoint.y, 0.0));
        }

        /// <summary>
        ///     The local point corresponding to the given source point.
        /// </summary>
        /// <param name="deformedPoint">A point in the deformed (destination) space.</param>
        /// <returns>Returns the corresponding point in the local (source) space.</returns>
        public virtual Vector3d DeformedToLocal(Vector3d deformedPoint)
        {
            return deformedPoint;
        }

        /// <summary>
        ///     The local bounding box corresponding to the given source disk.
        /// </summary>
        /// <param name="deformedCenter">The source disk center in deformed space.</param>
        /// <param name="deformedRadius">The source disk radius in deformed space.</param>
        /// <returns>Returns the local bounding box corresponding to the given source disk.</returns>
        public virtual Box2d DeformedToLocalBounds(Vector3d deformedCenter, double deformedRadius)
        {
            return new Box2d(deformedCenter.x - deformedRadius, deformedCenter.x + deformedRadius, deformedCenter.y - deformedRadius, deformedCenter.y + deformedRadius);
        }

        /// <summary>
        ///     This reference frame is such that its xy plane is the tangent plane, at deformedPoint to the deformed surface corresponding to the local plane z = 0.
        ///     Note that this orthonormal reference frame doesn't give the differential of the inverse deformation funtion, which in general is not an orthonormal transformation.
        ///     If 'p' is a deformed point, then <code>DeformedToLocalFrame(deformedPoint) * p</code> gives the coordinates of 'p' in the orthonormal reference frame defined above.
        /// </summary>
        /// <param name="deformedPoint">A point in the deformed (destination) space.</param>
        /// <returns>Returns an orthonormal reference frame of the tangent space at the given deformed point.</returns>
        public virtual Matrix4x4d DeformedToTangentFrame(Vector3d deformedPoint)
        {
            return Matrix4x4d.Translate(new Vector3d(-deformedPoint.x, -deformedPoint.y, 0.0));
        }

        /// <summary>
        ///     The distance in local (source) space between a point and a bounding box.
        /// </summary>
        /// <param name="localPoint">A point in local space.</param>
        /// <param name="localBox">A bounding box in local space.</param>
        /// <returns>Returns the distance in local (source) space between 'a' point and a bounding box.</returns>
        public virtual double GetLocalDistance(Vector3d localPoint, Box3d localBox)
        {
            return Math.Max(Math.Abs(localPoint.z - localBox.Max.z),
                            Math.Max(Math.Min(Math.Abs(localPoint.x - localBox.Min.x),
                                              Math.Abs(localPoint.x - localBox.Max.x)),
                                     Math.Min(Math.Abs(localPoint.y - localBox.Min.y),
                                              Math.Abs(localPoint.y - localBox.Max.y))));
        }

        /// <summary>
        ///     The visibility of a bounding box in local space, in a view frustum defined in deformed space.
        /// </summary>
        /// <param name="node">
        ///     A TerrainNode. This is node is used to get the camera position in local and deformed space with
        ///     <code>TerrainNode.GetLocalCamera</code> and <code>TerrainNode.GetDeformedCamera</code>,
        ///     as well as the view frustum planes in deformed space with <code>TerrainNode.GetDeformedFrustumPlanes</code>.
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
            if (target == null || node == null)
            {
                return;
            }

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
                               (float)((node.LocalCameraPosition.z - node.ParentBody.HeightZ) / (quad.Length * node.DistanceFactor)),
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
            if (target == null || node == null || quad == null)
            {
                return;
            }

            target.SetVector(uniforms.offset, CalculateDeformedOffset(quad));
            target.SetVector(uniforms.camera, CalculateDeformedCameraPosition(node, quad));

            target.SetMatrix(uniforms.tileToTangent, CalculateDeformedLocalToTangent(node, quad));

            SetScreenUniforms(node, quad, target);
        }

        public virtual void SetUniforms(TerrainNode node, TerrainQuad quad, Material target)
        {
            if (target == null || node == null || quad == null)
            {
                return;
            }

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
    }
}