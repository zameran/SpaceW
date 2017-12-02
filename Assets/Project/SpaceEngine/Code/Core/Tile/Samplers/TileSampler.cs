using SpaceEngine.Core.Patterns.Strategy.Uniformed;
using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Terrain;
using SpaceEngine.Core.Tile.Filter;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Utilities;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core.Tile.Samplers
{
    /// <summary>
    /// This class can set the uniforms necessary to access a given texture tile on GPU, stored in a GPUTileStorage. 
    /// This class also manages the creation of new texture tiles when a terrain quadtree is updated, via a TileProducer.
    /// </summary>
    public class TileSampler : NodeSlave<TileSampler>, IUniformed<MaterialPropertyBlock, TerrainQuad>, IUniformed<Material, TerrainQuad>
    {
        /// <summary>
        /// Class used to sort a <see cref="TileSampler"/> based on it's priority.
        /// </summary>
        public class Sort : IComparer<TileSampler>
        {
            int IComparer<TileSampler>.Compare(TileSampler a, TileSampler b)
            {
                if (a == null || b == null) return 0;

                if (a.Priority > b.Priority)
                    return 1;
                if (a.Priority < b.Priority)
                    return -1;
                else
                    return 0;
            }
        }

        public class Uniforms
        {
            public int tile, tileSize, tileCoords;

            public Uniforms(string name)
            {
                tile = Shader.PropertyToID("_" + name + "_Tile");
                tileSize = Shader.PropertyToID("_" + name + "_TileSize");
                tileCoords = Shader.PropertyToID("_" + name + "_TileCoords");
            }
        }

        public TerrainNode TerrainNode { get; set; }

        /// <summary>
        /// Store texture tiles for leaf quads?
        /// </summary>
        public bool StoreLeaf = true;

        /// <summary>
        /// Store texture tiles for non leaf quads?
        /// </summary>
        public bool StoreParent = true;

        /// <summary>
        /// store texture tiles for invisible quads?
        /// </summary>
        public bool StoreInvisible = false;

        /// <summary>
        /// The order in which to update samplers.
        /// </summary>
        public int Priority = -1;

        /// <summary>
        /// An internal quadtree to store the texture tiles associated with each quad.
        /// </summary>
        public QuadTree QuadTreeRoot = null;

        private Uniforms uniforms;

        /// <summary>
        /// The producer to be used to create texture tiles for newly created quads.
        /// </summary>
        public TileProducer Producer { get; private set; }

        /// <summary>
        /// The <see cref="TileFilter"/>'s array to be used.
        /// </summary>
        public TileFilter[] Filters { get; private set; }

        private RenderTexture SamplerTextureBuffer;
        private Vector3 SamplerCoordsBuffer;
        private Vector3 SamplerSizeBuffer;

        #region NodeSlave<TileSampler>

        public override void InitNode()
        {
            Producer = GetComponent<TileProducer>();
            TerrainNode = GetComponentInParent<TerrainNode>();
            uniforms = new Uniforms(Producer.Name);
            Filters = GetComponents<TileFilter>();

            Producer.InitNode();
        }

        public override void UpdateNode()
        {
            Producer.UpdateNode();

            UpdateSampler();
        }

        #endregion

        public virtual void UpdateSampler()
        {
            if (StoreInvisible) TerrainNode.SplitInvisibleQuads = true;

            PutTiles(QuadTreeRoot, TerrainNode.TerrainQuadRoot);
            GetTiles(null, ref QuadTreeRoot, TerrainNode.TerrainQuadRoot);
        }

        /// <summary>
        /// A tile is needed for the given terrain quad?
        /// </summary>
        /// <param name="quad">Quad.</param>
        /// <returns>Returns 'True' if a tile is needed for the given terrain quad.</returns>
        protected virtual bool NeedTile(TerrainQuad quad)
        {
            var needTile = StoreLeaf;

            // If the quad is not a leaf and producer has children and if have been asked not to store parent then dont need tile
            if (!StoreParent && !quad.IsLeaf && Producer.HasChildren(quad.Level, quad.Tx, quad.Ty))
            {
                needTile = false;
            }

            // Check if any of the filters have determined that this tile is not needed
            foreach (var filter in Filters)
            {
                if (filter.DiscardTile(quad))
                {
                    needTile = false;

                    break;
                }
            }

            // If this quad is not visilbe and have not been asked to store invisilbe quads dont need tile
            if (!StoreInvisible && !quad.IsVisible)
            {
                needTile = false;
            }

            return needTile;
        }

        /// <summary>
        /// Updates the internal quadtree to make it identical to the given terrain quadtree.
        /// This method releases the texture tiles corresponding to deleted quads.
        /// </summary>
        /// <param name="tree">Internal quadtree.</param>
        /// <param name="quad">Quad.</param>
        protected virtual void PutTiles(QuadTree tree, TerrainQuad quad)
        {
            if (tree == null) return;

            // Check if this tile is needed, if not put tile.
            tree.IsNeedTile = NeedTile(quad);

            if (!tree.IsNeedTile && tree.Tile != null)
            {
                Producer.PutTile(tree.Tile);

                tree.Tile = null;
            }

            // If this qiad is a leaf then all children of the tree are not needed
            if (quad.IsLeaf)
            {
                if (!tree.IsLeaf)
                {
                    tree.RecursiveDeleteChildren(this);
                }
            }
            else if (Producer.HasChildren(quad.Level, quad.Tx, quad.Ty))
            {
                for (byte i = 0; i < 4; ++i)
                {
                    PutTiles(tree.Children[i], quad.GetChild(i));
                }
            }
        }

        /// <summary>
        /// Updates the internal <see cref="QuadTree"/> to make it identical to the given terrain <see cref="QuadTree"/>. 
        /// Collects the tasks necessary to create the missing texture tiles, corresponding to newly created quads.
        /// </summary>
        /// <param name="parent">Parent quadtree.</param>
        /// <param name="tree">Internal quadtree.</param>
        /// <param name="quad">Quad.</param>
        protected virtual void GetTiles(QuadTree parent, ref QuadTree tree, TerrainQuad quad)
        {
            // If tree not created, create a new tree and check if its tile is needed
            if (tree == null)
            {
                tree = new QuadTree(parent);
                tree.IsNeedTile = NeedTile(quad);
            }

            // If this trees tile is needed get a tile and add its task to the schedular if the task is not already done
            if (tree.IsNeedTile && tree.Tile == null)
            {
                tree.Tile = Producer.GetTile(quad.Level, quad.Tx, quad.Ty);

                if (!tree.Tile.Task.IsDone)
                {
                    // If task not done schedule task
                    Schedular.Instance.Add(tree.Tile.Task);
                }
            }

            if (!quad.IsLeaf && Producer.HasChildren(quad.Level, quad.Tx, quad.Ty))
            {
                for (byte i = 0; i < 4; ++i)
                {
                    GetTiles(tree, ref tree.Children[i], quad.GetChild(i));
                }
            }
        }

        #region IUniformed<Material> 

        /// <summary> 
        /// Init special <see cref="TileProducer"/> uniforms for target <see cref="TerrainQuad"/>. 
        /// </summary> 
        /// <param name="target">Target <see cref="Material"/>.</param> 
        /// <param name="quad">Target quad.</param> 
        public void InitUniforms(Material target, TerrainQuad quad)
        {
            if (target == null) return;
            if (quad == null) return;
            if (!Producer.IsGPUProducer) return;
        }

        /// <summary> 
        /// Set special <see cref="TileProducer"/> uniforms for target <see cref="TerrainQuad"/>. 
        /// </summary> 
        /// <param name="target">Target <see cref="Material"/>.</param> 
        /// <param name="quad">Target quad.</param> 
        public void SetUniforms(Material target, TerrainQuad quad)
        {
            if (target == null) return;
            if (quad == null) return;
            if (!Producer.IsGPUProducer) return;

            CalculateTileGPUCoordinates(ref SamplerTextureBuffer, ref SamplerCoordsBuffer, ref SamplerSizeBuffer, quad.Level, quad.Tx, quad.Ty);

            target.SetTexture(uniforms.tile, SamplerTextureBuffer);
            target.SetVector(uniforms.tileCoords, SamplerCoordsBuffer);
            target.SetVector(uniforms.tileSize, SamplerSizeBuffer);
        }

        #endregion

        #region IUniformed<MaterialPropertyBlock>

        /// <summary>
        /// Init special <see cref="TileProducer"/> uniforms for target <see cref="TerrainQuad"/>.
        /// </summary>
        /// <param name="target">Target <see cref="MaterialPropertyBlock"/>.</param>
        /// <param name="quad">Target quad.</param>
        public void InitUniforms(MaterialPropertyBlock target, TerrainQuad quad)
        {
            if (target == null) return;
            if (quad == null) return;
            if (!Producer.IsGPUProducer) return;
        }

        /// <summary>
        /// Set special <see cref="TileProducer"/> uniforms for target <see cref="TerrainQuad"/>.
        /// </summary>
        /// <param name="target">Target <see cref="MaterialPropertyBlock"/>.</param>
        /// <param name="quad">Target quad.</param>
        public void SetUniforms(MaterialPropertyBlock target, TerrainQuad quad)
        {
            if (target == null) return;
            if (quad == null) return;
            if (!Producer.IsGPUProducer) return;

            CalculateTileGPUCoordinates(ref SamplerTextureBuffer, ref SamplerCoordsBuffer, ref SamplerSizeBuffer, quad.Level, quad.Tx, quad.Ty);

            target.SetTexture(uniforms.tile, SamplerTextureBuffer);
            target.SetVector(uniforms.tileCoords, SamplerCoordsBuffer);
            target.SetVector(uniforms.tileSize, SamplerSizeBuffer);
        }

        #endregion

        #region IUniformed

        public void InitSetUniforms()
        {

        }

        #endregion

        /// <summary>
        /// Calculates the uniforms necessary to access the texture tile for the given quad. 
        /// The samplers producer must be using a <see cref="GPUTileStorage"/> at the first slot for this function to work.
        /// </summary>
        /// <param name="tileTexture"></param>
        /// <param name="coordinates"></param>
        /// <param name="size"></param>
        /// <param name="level"></param>
        /// <param name="tx"></param>
        /// <param name="ty"></param>
        private void CalculateTileGPUCoordinates(ref RenderTexture tileTexture, ref Vector3 coordinates, ref Vector3 size, int level, int tx, int ty)
        {
            if (!Producer.IsGPUProducer) return;

            Tile t = null;

            var border = Producer.GetBorder();
            var tileSize = Producer.Cache.GetStorage(0).TileSize;

            var dx = 0.0f;
            var dy = 0.0f;
            var tileSizeHalf = tileSize / 2;
            var tileSizeCentered = tileSizeHalf * 2.0f - 2.0f * border;
            var ds = tileSizeCentered;

            if (!Producer.HasTile(level, tx, ty))
            {
                Debug.Log("TileSampler.SetTile: Invalid level!");
                return;
            }

            QuadTree tt = QuadTreeRoot;
            QuadTree tc;

            var tl = 0;

            while (tl != level && (tc = tt.Children[((tx >> (level - tl - 1)) & 1) | ((ty >> (level - tl - 1)) & 1) << 1]) != null)
            {
                tl += 1;
                tt = tc;
            }

            t = tt.Tile;

            dx = dx * (tileSizeHalf * 2 - 2 * border) / 1.0f;
            dy = dy * (tileSizeHalf * 2 - 2 * border) / 1.0f;

            if (t == null)
            {
                Debug.Log("TileSampler.SetTile: Null tile!");
                return;
            }

            var gpuSlot = t.GetSlot(0) as GPUTileStorage.GPUSlot;

            if (gpuSlot == null)
            {
                Debug.Log("TileSampler.SetTile: Null gpuSlot!");
                return;
            }

            float w = gpuSlot.Texture.width;
            float h = gpuSlot.Texture.height;

            Vector4 coords;

            if (tileSize % 2 == 0)
            {
                coords = new Vector4((dx + border) / w, (dy + border) / h, 0.0f, ds / w);
            }
            else
            {
                coords = new Vector4((dx + border + 0.5f) / w, (dy + border + 0.5f) / h, 0.0f, ds / w);
            }

            tileTexture = gpuSlot.Texture;
            coordinates = new Vector3(coords.x, coords.y, coords.z);
            size = new Vector3(coords.w, coords.w, tileSizeCentered);
        }

        /// <summary>
        /// Calculates the uniforms necessary to access the texture tile for the given quad. 
        /// The samplers producer must be using a <see cref="GPUTileStorage"/> at the first slot for this function to work.
        /// </summary>
        /// <param name="tileTexture"></param>
        /// <param name="coordinates"></param>
        /// <param name="size"></param>
        /// <param name="level"></param>
        /// <param name="tx"></param>
        /// <param name="ty"></param>
        [Obsolete("Use CalculateTileGPUCoordinates method instead, it's much faster.")]
        private void TileGPUCoordinates(ref RenderTexture tileTexture, ref Vector3 coordinates, ref Vector3 size, int level, int tx, int ty)
        {
            // NOTE : BOTTLENECK!

            if (!Producer.IsGPUProducer) return;

            Tile t = null;

            var border = Producer.GetBorder();
            var tileSize = Producer.Cache.GetStorage(0).TileSize;
            var tileSizeHalf = tileSize / 2;
            var tileSizeCentered = tileSizeHalf * 2.0f - 2.0f * border;

            var dx = 0.0f;
            var dy = 0.0f;
            var dd = 1.0f;
            var ds0 = tileSizeCentered;
            var ds = ds0;

            while (!Producer.HasTile(level, tx, ty))
            {
                dx += (tx % 2) * dd;
                dy += (ty % 2) * dd;
                dd *= 2;
                ds /= 2;
                level -= 1;
                tx /= 2;
                ty /= 2;

                if (level < 0)
                {
                    Debug.Log("TileSampler.SetTile: Invalid level!");
                    return;
                }
            }

            QuadTree tt = QuadTreeRoot;
            QuadTree tc;

            var tl = 0;

            while (tl != level && (tc = tt.Children[((tx >> (level - tl - 1)) & 1) | ((ty >> (level - tl - 1)) & 1) << 1]) != null)
            {
                tl += 1;
                tt = tc;
            }

            while (level > tl)
            {
                dx += (tx % 2) * dd;
                dy += (ty % 2) * dd;
                dd *= 2;
                ds /= 2;
                level -= 1;
                tx /= 2;
                ty /= 2;
            }

            t = tt.Tile;

            while (t == null)
            {
                dx += (tx % 2) * dd;
                dy += (ty % 2) * dd;
                dd *= 2;
                ds /= 2;
                level -= 1;
                tx /= 2;
                ty /= 2;
                tt = tt.ParentTree;

                if (tt == null)
                {
                    Debug.Log("TileSampler.SetTile: Null tile!");
                    return;
                }

                t = tt.Tile;
            }

            dx = dx * (tileSizeHalf * 2 - 2 * border) / dd;
            dy = dy * (tileSizeHalf * 2 - 2 * border) / dd;

            var gpuSlot = t.GetSlot(0) as GPUTileStorage.GPUSlot;

            if (gpuSlot == null)
            {
                Debug.Log("TileSampler.SetTile: Null gpuSlot!");
                return;
            }

            if (gpuSlot.Texture == null)
            {
                Debug.Log("TileSampler.SetTile: Null gpuSlot.Texture!");
                return;
            }

            float w = gpuSlot.Texture.width;
            float h = gpuSlot.Texture.height;

            Vector4 coords;

            if (tileSize % 2 == 0)
            {
                coords = new Vector4((dx + border) / w, (dy + border) / h, 0.0f, ds / w);
            }
            else
            {
                coords = new Vector4((dx + border + 0.5f) / w, (dy + border + 0.5f) / h, 0.0f, ds / w);
            }

            tileTexture = gpuSlot.Texture;
            coordinates = new Vector3(coords.x, coords.y, coords.z);
            size = new Vector3(coords.w, coords.w, tileSizeCentered);
        }
    }
}