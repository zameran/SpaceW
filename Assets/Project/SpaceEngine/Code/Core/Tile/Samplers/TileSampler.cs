using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Terrain;
using SpaceEngine.Core.Tile.Filter;
using SpaceEngine.Core.Tile.Producer;
using SpaceEngine.Core.Utilities;

using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core.Tile.Samplers
{
    /// <summary>
    /// This class can set the uniforms necessary to access a given texture tile on GPU, stored in a GPUTileStorage. 
    /// This class also manages the creation of new texture tiles when a terrain quadtree is updated, via a TileProducer.
    /// </summary>
    public class TileSampler : Node<TileSampler>
    {
        /// <summary>
        /// Class used to sort a <see cref="TileSampler"/> based on its priority.
        /// </summary>
        public class Sort : IComparer<TileSampler>
        {
            int IComparer<TileSampler>.Compare(TileSampler a, TileSampler b)
            {
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
        QuadTree QuadTreeRoot = null;

        Uniforms uniforms;

        /// <summary>
        /// The producer to be used to create texture tiles for newly created quads.
        /// </summary>
        public TileProducer Producer { get; private set; }

        /// <summary>
        /// The <see cref="TileFilter"/>'s array to be used.
        /// </summary>
        public TileFilter[] Filters { get; private set; }

        #region Node

        protected override void InitNode()
        {
            Producer = GetComponent<TileProducer>();
            TerrainNode = GetComponentInParent<TerrainNode>();
            uniforms = new Uniforms(Producer.GetName());
            Filters = GetComponents<TileFilter>();
        }

        protected override void UpdateNode()
        {

        }


        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
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
        /// Updates the internal quadtree to make it identical to the given terrain quadtree. 
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

        public void SetTile(MaterialPropertyBlock matPropertyBlock, int level, int tx, int ty)
        {
            if (!Producer.IsGPUProducer) return;

            RenderTexture tex = null;
            Vector3 coords = Vector3.zero, size = Vector3.zero;

            SetTile(ref tex, ref coords, ref size, level, tx, ty);

            matPropertyBlock.SetTexture(uniforms.tile, tex);
            matPropertyBlock.SetVector(uniforms.tileCoords, coords);
            matPropertyBlock.SetVector(uniforms.tileSize, size);
        }

        public void SetTile(Material mat, int level, int tx, int ty)
        {
            if (!Producer.IsGPUProducer) return;

            RenderTexture tex = null;
            Vector3 coords = Vector3.zero, size = Vector3.zero;

            SetTile(ref tex, ref coords, ref size, level, tx, ty);

            mat.SetTexture(uniforms.tile, tex);
            mat.SetVector(uniforms.tileCoords, coords);
            mat.SetVector(uniforms.tileSize, size);
        }

        /// <summary>
        /// Sets the uniforms necessary to access the texture tile for the given quad. 
        /// The samplers producer must be using a GPUTileStorage at the first slot for this function to work.
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="coord"></param>
        /// <param name="size"></param>
        /// <param name="level"></param>
        /// <param name="tx"></param>
        /// <param name="ty"></param>
        void SetTile(ref RenderTexture tex, ref Vector3 coord, ref Vector3 size, int level, int tx, int ty)
        {
            if (!Producer.IsGPUProducer) return;

            Tile t = null;

            var b = Producer.GetBorder();
            var s = Producer.Cache.GetStorage(0).TileSize;
            var sDivTwo = s / 2;

            var dx = 0.0f;
            var dy = 0.0f;
            var dd = 1.0f;
            var ds0 = sDivTwo * 2.0f - 2.0f * b;
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
                    Debug.Log("Invalid level!");
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
                    Debug.Log("Null tile!");
                    return;
                }

                t = tt.Tile;
            }

            dx = dx * (sDivTwo * 2 - 2 * b) / dd;
            dy = dy * (sDivTwo * 2 - 2 * b) / dd;

            var gpuSlot = t.GetSlot(0) as GPUTileStorage.GPUSlot;

            if (gpuSlot == null)
            {
                Debug.Log("Null gpuSlot!");
                return;
            }

            float w = gpuSlot.Texture.width;
            float h = gpuSlot.Texture.height;

            var coords = Vector4.zero;

            if (s % 2 == 0)
            {
                coords = new Vector4((dx + b) / w, (dy + b) / h, 0.0f, ds / w);
            }
            else
            {
                coords = new Vector4((dx + b + 0.5f) / w, (dy + b + 0.5f) / h, 0.0f, ds / w);
            }

            tex = gpuSlot.Texture;
            coord = new Vector3(coords.x, coords.y, coords.z);
            size = new Vector3(coords.w, coords.w, sDivTwo * 2.0f - 2.0f * b);
        }
    }
}