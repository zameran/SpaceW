using SpaceEngine.Core.Tile.Samplers;

namespace SpaceEngine.Core.Terrain
{
    /// <summary>
    /// An internal quadtree to store the texture tile associated with each terrain quad.
    /// </summary>
    public class QuadTree
    {
        // TODO - make members private?

        /// <summary>
        /// Is a tile is needed for this quad?
        /// </summary>
        public bool IsNeedTile;

        /// <summary>
        /// The ParentTree quad of this quad.
        /// </summary>
        public QuadTree ParentTree;

        /// <summary>
        /// The texture tile associated with this quad.
        /// </summary>
        public Tile.Tile Tile;

        /// <summary>
        /// The subquads of this quad.
        /// </summary>
        public QuadTree[] Children = new QuadTree[4];

        public bool IsLeaf { get { return (Children[0] == null); } }

        public QuadTree(QuadTree parentTree)
        {
            this.ParentTree = parentTree;
        }

        /// <summary>
        /// Deletes All trees subelements.
        /// Releases all the corresponding texture tiles.
        /// </summary>
        /// <param name="owner">Owner of quad.</param>
        public void RecursiveDeleteChildren(TileSampler owner)
        {
            if (!IsLeaf)
            {
                for (byte i = 0; i < 4; i++)
                {
                    Children[i].RecursiveDelete(owner);
                    Children[i] = null;
                }
            }
        }

        /// <summary>
        /// Deletes this tree and all its subelements. 
        /// Releases all the corresponding texture tiles.
        /// </summary>
        /// <param name="owner"></param>
        public void RecursiveDelete(TileSampler owner)
        {
            if (Tile != null && owner != null)
            {
                owner.Producer.PutTile(Tile);

                Tile = null;
            }

            if (!IsLeaf)
            {
                for (byte i = 0; i < 4; i++)
                {
                    Children[i].RecursiveDelete(owner);
                    Children[i] = null;
                }
            }
        }
    }
}