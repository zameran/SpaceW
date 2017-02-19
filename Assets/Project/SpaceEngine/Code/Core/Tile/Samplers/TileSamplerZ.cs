using SpaceEngine.Core.Storage;
using SpaceEngine.Core.Terrain;
using SpaceEngine.Types.Containers;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Core.Tile.Samplers
{
    /// <summary>
    /// A TileSampler to be used with a ElevationProducer.
    /// This class reads back the elevation data of newly created elevation tiles in order to update the TerrainQuad.ZMin and TerrainQuad.ZMax fields. 
    /// It also reads back the elevation value below the current viewer position.
    /// </summary>
    public class TileSamplerZ : TileSampler
    {
        /// <summary>
        /// An internal quadtree to store the texture tile associated with each
        /// terrain TerrainQuad, and to keep track of tiles that need to be read back.
        /// </summary>
        class QuadTreeZ : QuadTree
        {
            public TerrainQuad TerrainQuad;

            public bool ReadBack;

            public QuadTreeZ(QuadTree parent, TerrainQuad terrainQuad) : base(parent)
            {
                this.TerrainQuad = terrainQuad;
                ReadBack = false;
            }
        }

        /// <summary>
        /// Helper class to store the retrived height data and the [min, max] values
        /// </summary>
        private class ElevationInfo
        {
            public float[] Elevations = null;
            public float Min = float.PositiveInfinity;
            public float Max = float.NegativeInfinity;
        }

        [SerializeField]
        bool EnableReadBack = false;

        [SerializeField]
        int MaxReadBacksPerFrame = 5;

        [SerializeField]
        int MaxStoredElevations = 10000;

        /// <summary>
        /// The terrain <see cref="TerrainQuad"/> directly below the current viewer position.
        /// </summary>
        QuadTreeZ CameraQuad;

        /// <summary>
        /// The relative viewer position in the <see cref="CameraQuad"/> <see cref="TerrainQuad"/>.
        /// </summary>
        Vector2 CameraQuadCoordinates;

        /// <summary>
        /// Last camera position used to perform a readback of the camera elevation above the ground. 
        /// This is used to avoid reading back this value at each frame when the camera does not move.
        /// </summary>
        Vector3d OldLocalCamera;

        /// <summary>
        /// A container for the <see cref="TerrainQuad"/> trees that need to have there elevations read back.
        /// </summary>
        Dictionary<Tile.Id, QuadTreeZ> NeedsReadBackDictionary;

        /// <summary>
        /// A container of all the tiles that have had there elevations read back.
        /// </summary>
        DictionaryQueue<Tile.Id, ElevationInfo> ElevationsDicionary;

        ComputeBuffer ElevationsBuffer;
        ComputeBuffer GroundBuffer;

        protected override void Start()
        {
            base.Start();

            OldLocalCamera = Vector3d.zero;

            NeedsReadBackDictionary = new Dictionary<Tile.Id, QuadTreeZ>(new Tile.EqualityComparerID());
            ElevationsDicionary = new DictionaryQueue<Tile.Id, ElevationInfo>(new Tile.EqualityComparerID());

            var size = GetProducer().GetTileSize(0);

            ElevationsBuffer = new ComputeBuffer(size * size, sizeof(float));
            GroundBuffer = new ComputeBuffer(1, sizeof(float));

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            ElevationsBuffer.Release();
            GroundBuffer.Release();
        }

        /// <summary>
        /// Override the default <see cref="TileSampler"/>'s NeedTile to retrive the tile that is below the camera as well as its default behaviour.
        /// </summary>
        /// <param name="quad">Quad.</param>
        /// <returns>Return 'True' if needs tile.</returns>
        protected override bool NeedTile(TerrainQuad quad)
        {
            var localCameraPosition = quad.GetOwner().LocalCameraPosition;
            var l = quad.GetLevel();
            var ox = quad.GetOX();
            var oy = quad.GetOY();

            if (localCameraPosition.x >= ox && localCameraPosition.x < ox + l && localCameraPosition.y >= oy && localCameraPosition.y < oy + l)
            {
                return true;
            }

            return base.NeedTile(quad);
        }

        public override void UpdateSampler()
        {
            base.UpdateSampler();

            UpdateMinMax();
            UpdateGroundHeight();
        }

        /// <summary>
        /// Updates the ground height below camera.
        /// </summary>
        private void UpdateGroundHeight()
        {
            var localCameraPosition = TerrainNode.LocalCameraPosition;

            // If camera has moved update ground height
            if ((localCameraPosition - OldLocalCamera).Magnitude() > 1.0 && CameraQuad != null && CameraQuad.Tile != null)
            {
                var slot = CameraQuad.Tile.Slot[0] as GPUTileStorage.GPUSlot;

                if (slot != null)
                {
                    //var border = GetProducer().GetBorder();
                    //var tileSize = GetProducer().GetTileSizeMinBorder(0);

                    //float dx = CameraQuadCoordinates.x * tileSize;
                    //float dy = CameraQuadCoordinates.y * tileSize;

                    // TODO : CORE
                    // x,y are the non-normalized position in the elevations texture where the ground height below the camera is.
                    //float x = dx + (float)border;
                    //float y = dy + (float)border;

                    // TODO : CORE
                    // Read the single value from the render texture
                    //CBUtility.ReadSingleFromRenderTexture(slot.Texture, x, y, 0, GroundBuffer, Manager.GetReadData(), true);

                    // Get single height value from buffer
                    float[] height = new float[1];

                    GroundBuffer.GetData(height);

                    // TODO : CORE
                    // Update the ground height. Stored as a static value in the TerrainNode script
                    //GetView().SetGroundHeight(Math.Max(0.0, height[0]));

                    OldLocalCamera.x = localCameraPosition.x;
                    OldLocalCamera.y = localCameraPosition.y;
                    OldLocalCamera.z = localCameraPosition.z;

                }
            }

            CameraQuad = null;
        }

        /// <summary>
        /// Updates the terrainQuads min and max values. Used to create a better fitting bounding box.
        /// Is not essental and can be disabled if retriving the heights data from the GPU is causing  performance issues.
        /// </summary>
        private void UpdateMinMax()
        {
            // If no quads need read back or if disabled return
            if (NeedsReadBackDictionary.Count == 0 || !EnableReadBack) return;

            // Make a copy of all the keys of the tiles that need to be read back
            var ids = new Tile.Id[NeedsReadBackDictionary.Count];
            NeedsReadBackDictionary.Keys.CopyTo(ids, 0);

            // Sort the keys by there level, lowest -> highest
            Array.Sort(ids, new Tile.ComparerID());

            var count = 0;

            // Foreach key read back the tiles data until the maxReadBacksPerFrame limit is reached
            foreach (var id in ids)
            {
                QuadTreeZ treeZ = NeedsReadBackDictionary[id];

                // If elevations container already contains key then data has been read back before so just reapply the [min, max] values to TerranQuad
                if (ElevationsDicionary.ContainsKey(id))
                {
                    ElevationInfo info = ElevationsDicionary.Get(id);

                    treeZ.TerrainQuad.SetZMin(info.Min);
                    treeZ.TerrainQuad.SetZMax(info.Max);

                    NeedsReadBackDictionary.Remove(id);
                }
                else
                {
                    // If for some reason the tile is null remove from container and continue
                    if (treeZ.Tile == null)
                    {
                        NeedsReadBackDictionary.Remove(id);

                        continue;
                    }

                    var slot = treeZ.Tile.Slot[0] as GPUTileStorage.GPUSlot;

                    // If for some reason this is not a GPUSlot remove and continue
                    if (slot == null)
                    {
                        NeedsReadBackDictionary.Remove(id);

                        continue;
                    }

                    var texture = slot.Texture;
                    var size = texture.width * texture.height;

                    var elevationInfo = new ElevationInfo();
                    elevationInfo.Elevations = new float[size];

                    // TODO : CORE
                    // Read back heights data from texture
                    //CBUtility.ReadFromRenderTexture(texture, 1, ElevationsBuffer, Manager.GetReadData());

                    // Copy into elevations info
                    ElevationsBuffer.GetData(elevationInfo.Elevations);

                    // Find the min/max values
                    for (int i = 0; i < size; i++)
                    {
                        if (elevationInfo.Elevations[i] < elevationInfo.Min) elevationInfo.Min = elevationInfo.Elevations[i];
                        if (elevationInfo.Elevations[i] > elevationInfo.Max) elevationInfo.Max = elevationInfo.Elevations[i];
                    }

                    // Update TerrainQuad
                    treeZ.TerrainQuad.SetZMin(elevationInfo.Min);
                    treeZ.TerrainQuad.SetZMax(elevationInfo.Max);

                    // Store elevations to prevent having to read back again soon
                    // Add to end of container
                    ElevationsDicionary.AddLast(id, elevationInfo);
                    NeedsReadBackDictionary.Remove(id);

                    count++;

                    // If the number of rad back to do per frame has hit the limit stop loop.
                    if (count >= MaxReadBacksPerFrame) break;
                }
            }

            // If the number of elevation info to store has exceded limit remove from start of container
            while (ElevationsDicionary.Count() > MaxStoredElevations)
            {
                ElevationsDicionary.RemoveFirst();
            }

        }

        protected override void GetTiles(QuadTree parent, ref QuadTree tree, TerrainQuad quad)
        {
            if (tree == null)
            {
                tree = new QuadTreeZ(parent, quad);
                tree.IsNeedTile = NeedTile(quad);
            }

            //var treeZ = tree as QuadTreeZ;

            // TODO : CORE
            /*
            // If tile needs elevation data read back add to container
            if (treeZ.Tile != null && treeZ.Tile.Task.IsDone && !treeZ.ReadBack && MaxReadBacksPerFrame > 0)
            {
                if (!NeedsReadBackDictionary.ContainsKey(treeZ.Tile.GetId()))
                {
                    treeZ.ReadBack = true;
                    NeedsReadBackDictionary.Add(treeZ.Tile.GetId(), treeZ);
                }
            }
            */

            base.GetTiles(parent, ref tree, quad);

            // TODO : CORE
            /*
            // Check if this TerrainQuad is below the camera. If so store a reference to it.
            if (CameraQuad == null && treeZ.Tile != null && treeZ.Tile.Task.IsDone)
            {
                var cameraPosition = quad.GetOwner().LocalCameraPosition;

                var l = quad.GetLength();
                var ox = quad.GetOX();
                var oy = quad.GetOY();

                if (cameraPosition.x >= ox && cameraPosition.x < ox + l && cameraPosition.y >= oy && cameraPosition.y < oy + l)
                {
                    CameraQuadCoordinates = new Vector2((float)((cameraPosition.x - ox) / l), (float)((cameraPosition.y - oy) / l));
                    CameraQuad = treeZ;
                }
            }
            */
        }
    }
}