#region License

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

using SpaceEngine.Core.Tile.Samplers;

namespace SpaceEngine.Core.Terrain
{
    /// <summary>
    ///     An internal quadtree to store the texture tile associated with each terrain quad.
    /// </summary>
    public class QuadTree
    {
        /// <summary>
        ///     The subquads of this quad.
        /// </summary>
        public QuadTree[] Children = new QuadTree[4];

        /// <summary>
        ///     Is a tile is needed for this quad?
        /// </summary>
        public bool IsNeedTile;

        /// <summary>
        ///     The ParentTree quad of this quad.
        /// </summary>
        public QuadTree ParentTree;

        /// <summary>
        ///     The texture tile associated with this quad.
        /// </summary>
        public Tile.Tile Tile;

        public QuadTree(QuadTree parentTree)
        {
            ParentTree = parentTree;
        }

        public bool IsLeaf => Children[0] == null; // NOTE : Use LINQ.All/Any(child => child == null) to 100% ensure in childs unexistance, but too slow.

        /// <summary>
        ///     Deletes All trees subelements.
        ///     Releases all the corresponding texture tiles.
        /// </summary>
        /// <param name="owner">Owner of quad.</param>
        public void RecursiveDeleteChildren(TileSampler owner)
        {
            if (!IsLeaf)
            {
                for (var i = 0; i < 4; i++)
                {
                    Children[i].RecursiveDelete(ref owner);
                    Children[i] = null;
                }
            }
        }

        /// <summary>
        ///     Deletes this tree and all its subelements.
        ///     Releases all the corresponding texture tiles.
        /// </summary>
        /// <param name="owner"></param>
        public void RecursiveDelete(ref TileSampler owner)
        {
            if (Tile != null && owner != null)
            {
                owner.Producer.PutTile(Tile);

                Tile = null;
            }

            if (!IsLeaf)
            {
                for (var i = 0; i < 4; i++)
                {
                    Children[i].RecursiveDelete(ref owner);
                    Children[i] = null;
                }
            }
        }
    }
}