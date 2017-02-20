using SpaceEngine.Core.Exceptions;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Terrain.Deformation;
using SpaceEngine.Core.Tile;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Tile.Storage;
using System.Collections.Generic;
using UnityEngine;

namespace Proland
{
    public class NormalProducer : TileProducer
    {
        public class Uniforms
        {
            public int tileSD, elevationSampler, elevationOSL;
            public int patchCorners, patchVerticals, patchCornerNorms;
            public int deform, worldToTangentFrame;

            public Uniforms()
            {
                tileSD = Shader.PropertyToID("_TileSD");
                elevationSampler = Shader.PropertyToID("_ElevationSampler");
                elevationOSL = Shader.PropertyToID("_ElevationOSL");
                patchCorners = Shader.PropertyToID("_PatchCorners");
                patchVerticals = Shader.PropertyToID("_PatchVerticals");
                patchCornerNorms = Shader.PropertyToID("_PatchCornerNorms");
                deform = Shader.PropertyToID("_Deform");
                worldToTangentFrame = Shader.PropertyToID("_WorldToTangentFrame");
            }
        }

        [SerializeField]
        GameObject ElevationProducerGameObject;

        TileProducer ElevationProducer;

        [SerializeField]
        Material m_normalsMat;

        Uniforms m_uniforms;

        // Use this for initialization
        protected override void Start()
        {
            base.Start();

            m_uniforms = new Uniforms();

            ElevationProducer = ElevationProducerGameObject.GetComponent<TileProducer>();

            if (ElevationProducer.Cache == null) ElevationProducer.InitCache(); // NOTE : Brutal fix - force initialization.

            int tileSize = Cache.GetStorage(0).TileSize;
            int elevationTileSize = ElevationProducer.Cache.GetStorage(0).TileSize;

            if (tileSize != elevationTileSize)
            {
                throw new InvalidParameterException("Tile size must equal elevation tile size" + string.Format(": {0}-{1}", tileSize, elevationTileSize));
            }

            if (GetBorder() != ElevationProducer.GetBorder())
            {
                throw new InvalidParameterException("Border size must be equal to elevation border size");
            }

            GPUTileStorage storage = Cache.GetStorage(0) as GPUTileStorage;

            if (storage == null)
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
            GPUTileStorage.GPUSlot gpuSlot = slot[0] as GPUTileStorage.GPUSlot;

            Tile elevationTile = ElevationProducer.FindTile(level, tx, ty, false, true);
            GPUTileStorage.GPUSlot elevationGpuSlot = null;

            if (elevationTile != null)
            {
                elevationGpuSlot = elevationTile.GetSlot(0) as GPUTileStorage.GPUSlot;
            }
            else
            {
                throw new MissingTileException("Find elevation tile failed");
            }

            int tileWidth = gpuSlot.Owner.TileSize;

            m_normalsMat.SetVector(m_uniforms.tileSD, new Vector2((float)tileWidth, (float)(tileWidth - 1) / (float)(TerrainNode.Body.GridResolution - 1)));

            RenderTexture elevationTex = elevationGpuSlot.Texture;

            m_normalsMat.SetTexture(m_uniforms.elevationSampler, elevationTex);

            Vector4 elevationOSL = new Vector4(0.25f / (float)elevationTex.width, 0.25f / (float)elevationTex.height, 1.0f / (float)elevationTex.width, 0.0f);

            m_normalsMat.SetVector(m_uniforms.elevationOSL, elevationOSL);

            if (TerrainNode.Deformation.GetType() == typeof(DeformationSpherical))
            {
                double D = TerrainNode.TerrainQuadRoot.GetLength();
                double R = D / 2.0;

                double x0 = (double)(tx) / (double)(1 << level) * D - R;
                double x1 = (double)(tx + 1) / (double)(1 << level) * D - R;
                double y0 = (double)(ty) / (double)(1 << level) * D - R;
                double y1 = (double)(ty + 1) / (double)(1 << level) * D - R;

                Vector3d p0 = new Vector3d(x0, y0, R);
                Vector3d p1 = new Vector3d(x1, y0, R);
                Vector3d p2 = new Vector3d(x0, y1, R);
                Vector3d p3 = new Vector3d(x1, y1, R);
                Vector3d pc = new Vector3d((x0 + x1) * 0.5, (y0 + y1) * 0.5, R);

                double l0 = 0, l1 = 0, l2 = 0, l3 = 0;

                Vector3d v0 = p0.Normalized(ref l0);
                Vector3d v1 = p1.Normalized(ref l1);
                Vector3d v2 = p2.Normalized(ref l2);
                Vector3d v3 = p3.Normalized(ref l3);
                Vector3d vc = (v0 + v1 + v2 + v3) * 0.25;

                Matrix4x4d deformedCorners = new Matrix4x4d(v0.x * R - vc.x * R, v1.x * R - vc.x * R, v2.x * R - vc.x * R, v3.x * R - vc.x * R, v0.y * R - vc.y * R, v1.y * R - vc.y * R,
                    v2.y * R - vc.y * R, v3.y * R - vc.y * R, v0.z * R - vc.z * R, v1.z * R - vc.z * R, v2.z * R - vc.z * R, v3.z * R - vc.z * R, 1.0, 1.0, 1.0, 1.0);

                Matrix4x4d deformedVerticals = new Matrix4x4d(v0.x, v1.x, v2.x, v3.x, v0.y, v1.y, v2.y, v3.y, v0.z, v1.z, v2.z, v3.z, 0.0, 0.0, 0.0, 0.0);

                Vector3d uz = pc.Normalized();
                Vector3d ux = (new Vector3d(0, 1, 0)).Cross(uz).Normalized();
                Vector3d uy = uz.Cross(ux);

                Matrix4x4d worldToTangentFrame = new Matrix4x4d(ux.x, ux.y, ux.z, 0.0, uy.x, uy.y, uy.z, 0.0, uz.x, uz.y, uz.z, 0.0, 0.0, 0.0, 0.0, 0.0);

                m_normalsMat.SetMatrix(m_uniforms.patchCorners, deformedCorners.ToMatrix4x4());
                m_normalsMat.SetMatrix(m_uniforms.patchVerticals, deformedVerticals.ToMatrix4x4());
                m_normalsMat.SetVector(m_uniforms.patchCornerNorms, new Vector4((float)l0, (float)l1, (float)l2, (float)l3));
                m_normalsMat.SetVector(m_uniforms.deform, new Vector4((float)x0, (float)y0, (float)D / (float)(1 << level), (float)R));
                m_normalsMat.SetMatrix(m_uniforms.worldToTangentFrame, worldToTangentFrame.ToMatrix4x4());
            }
            else
            {
                double D = TerrainNode.TerrainQuadRoot.GetLength();
                double R = D / 2.0;
                double x0 = (double)tx / (double)(1 << level) * D - R;
                double y0 = (double)ty / (double)(1 << level) * D - R;

                m_normalsMat.SetMatrix(m_uniforms.worldToTangentFrame, Matrix4x4.identity);
                m_normalsMat.SetVector(m_uniforms.deform, new Vector4((float)x0, (float)y0, (float)D / (float)(1 << level), 0.0f));

            }

            Graphics.Blit(null, gpuSlot.Texture, m_normalsMat);

            base.DoCreateTile(level, tx, ty, slot);
        }
    }
}