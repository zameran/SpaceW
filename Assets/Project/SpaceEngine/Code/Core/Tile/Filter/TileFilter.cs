using SpaceEngine.Core.Terrain;

using UnityEngine;

namespace SpaceEngine.Core.Tile.Filter
{
    /// <summary>
    /// A filter to decide whether a texture tile must be produced or not for a given quad.
    /// </summary>
    public abstract class TileFilter : MonoBehaviour
    {
        /// <summary>
        /// A texture tile must be produced for the given quad?
        /// </summary>
        /// <param name="quad"></param>
        /// <returns>Returns 'True' if a texture tile must be produced for the given quad.</returns>
        public abstract bool DiscardTile(TerrainQuad quad);
    }
}