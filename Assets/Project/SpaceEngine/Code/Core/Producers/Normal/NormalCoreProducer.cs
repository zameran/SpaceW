using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Terrain;
using SpaceEngine.Core.Terrain.Deformation;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core
{
    public class NormalCoreProducer : TileProducer
    {
        public GameObject ElevationProducerGameObject;

        private TileProducer ElevationProducer;

        public Material NormalsMaterial;

        protected override void Start()
        {
            base.Start();

            if (TerrainNode == null) { TerrainNode = transform.parent.GetComponent<TerrainNode>(); }
            if (TerrainNode.Body == null) { TerrainNode.Body = transform.parent.GetComponentInParent<CelestialBody>(); }
            if (ElevationProducer == null) { ElevationProducer = ElevationProducerGameObject.GetComponent<TileProducer>(); }
            if (ElevationProducer.Cache == null) { ElevationProducer.InitCache(); }

            var tileSize = Cache.GetStorage(0).TileSize;
            var elevationTileSize = ElevationProducer.Cache.GetStorage(0).TileSize;

            if (tileSize != elevationTileSize)
            {
                throw new InvalidParameterException("Tile size must equal elevation tile size" + string.Format(": {0}-{1}", tileSize, elevationTileSize));
            }

            if (GetBorder() != ElevationProducer.GetBorder())
            {
                throw new InvalidParameterException("Border size must be equal to elevation border size");
            }

            if (!(Cache.GetStorage(0) is GPUTileStorage))
            {
                throw new InvalidStorageException("Storage must be a GPUTileStorage");
            }

        }

        public override bool HasTile(int level, int tx, int ty)
        {
            return ElevationProducer.HasTile(level, tx, ty);
        }

        public override int GetBorder()
        {
            return 2;
        }

        public override void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot)
        {
            var gpuSlot = slot[0] as GPUTileStorage.GPUSlot;
            var elevationTile = ElevationProducer.FindTile(level, tx, ty, false, true);

            GPUTileStorage.GPUSlot elevationGpuSlot = null;

            if (elevationTile != null)
            {
                elevationGpuSlot = elevationTile.GetSlot(0) as GPUTileStorage.GPUSlot;
            }
            else
            {
                throw new MissingTileException("Find elevation tile failed");
            }

            if (gpuSlot == null)
            {
                throw new NullReferenceException("gpuSlot");
            }

            if (elevationGpuSlot == null)
            {
                throw new NullReferenceException("elevationGpuSlot");
            }

            var tileWidth = gpuSlot.Owner.TileSize;
            var elevationTex = elevationGpuSlot.Texture;
            var elevationOSL = new Vector4(0.25f / (float)elevationTex.width, 0.25f / (float)elevationTex.height, 1.0f / (float)elevationTex.width, 0.0f);

            if (TerrainNode.Deformation.GetType() == typeof(DeformationSpherical))
            {
                var D = TerrainNode.TerrainQuadRoot.Length;
                var R = D / 2.0;

                var x0 = (double)(tx) / (double)(1 << level) * D - R;
                var x1 = (double)(tx + 1) / (double)(1 << level) * D - R;
                var y0 = (double)(ty) / (double)(1 << level) * D - R;
                var y1 = (double)(ty + 1) / (double)(1 << level) * D - R;

                var p0 = new Vector3d(x0, y0, R);
                var p1 = new Vector3d(x1, y0, R);
                var p2 = new Vector3d(x0, y1, R);
                var p3 = new Vector3d(x1, y1, R);
                var pc = new Vector3d((x0 + x1) * 0.5, (y0 + y1) * 0.5, R);

                double l0 = 0, l1 = 0, l2 = 0, l3 = 0;

                var v0 = p0.Normalized(ref l0);
                var v1 = p1.Normalized(ref l1);
                var v2 = p2.Normalized(ref l2);
                var v3 = p3.Normalized(ref l3);
                var vc = (v0 + v1 + v2 + v3) * 0.25;

                var deformedCorners = new Matrix4x4d(v0.x * R - vc.x * R,
                                                     v1.x * R - vc.x * R,
                                                     v2.x * R - vc.x * R,
                                                     v3.x * R - vc.x * R,
                                                     v0.y * R - vc.y * R,
                                                     v1.y * R - vc.y * R,
                                                     v2.y * R - vc.y * R,
                                                     v3.y * R - vc.y * R,
                                                     v0.z * R - vc.z * R,
                                                     v1.z * R - vc.z * R,
                                                     v2.z * R - vc.z * R,
                                                     v3.z * R - vc.z * R, 1.0, 1.0, 1.0, 1.0);

                var deformedVerticals = new Matrix4x4d(v0.x, v1.x, v2.x, v3.x, v0.y, v1.y, v2.y, v3.y, v0.z, v1.z, v2.z, v3.z, 0.0, 0.0, 0.0, 0.0);

                var uz = pc.Normalized();
                var ux = new Vector3d(0.0, 1.0, 0.0).Cross(uz).Normalized();
                var uy = uz.Cross(ux);

                var worldToTangentFrame = new Matrix4x4d(ux.x, ux.y, ux.z, 0.0, uy.x, uy.y, uy.z, 0.0, uz.x, uz.y, uz.z, 0.0, 0.0, 0.0, 0.0, 0.0);

                NormalsMaterial.SetMatrix("_PatchCorners", deformedCorners.ToMatrix4x4());
                NormalsMaterial.SetMatrix("_PatchVerticals", deformedVerticals.ToMatrix4x4());
                NormalsMaterial.SetVector("_PatchCornerNorms", new Vector4((float)l0, (float)l1, (float)l2, (float)l3));
                NormalsMaterial.SetVector("_Deform", new Vector4((float)x0, (float)y0, (float)D / (float)(1 << level), (float)R));
                NormalsMaterial.SetMatrix("_WorldToTangentFrame", worldToTangentFrame.ToMatrix4x4());
            }
            else
            {
                var D = TerrainNode.TerrainQuadRoot.Length;
                var R = D / 2.0;
                var x0 = (double)tx / (double)(1 << level) * D - R;
                var y0 = (double)ty / (double)(1 << level) * D - R;

                NormalsMaterial.SetVector("_Deform", new Vector4((float)x0, (float)y0, (float)D / (float)(1 << level), 0.0f));
                NormalsMaterial.SetMatrix("_WorldToTangentFrame", Matrix4x4.identity);
            }

            NormalsMaterial.SetVector("_TileSD", new Vector2((float)tileWidth, (float)(tileWidth - 1) / (float)(TerrainNode.Body.GridResolution - 1)));
            NormalsMaterial.SetTexture("_ElevationSampler", elevationTex);
            NormalsMaterial.SetVector("_ElevationOSL", elevationOSL);

            Graphics.Blit(null, gpuSlot.Texture, NormalsMaterial);

            base.DoCreateTile(level, tx, ty, slot);
        }
    }
}