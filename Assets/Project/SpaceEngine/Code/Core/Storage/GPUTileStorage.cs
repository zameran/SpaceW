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

using SpaceEngine.Core.Tile.Storage;
using SpaceEngine.Tools;
using SpaceEngine.Utilities;
using UnityEngine;

namespace SpaceEngine.Core.Storage
{
    /// <summary>
    ///     A TileStorage that stores tiles in 2D textures.
    /// </summary>
    public class GPUTileStorage : TileStorage
    {
        public RenderTextureFormat Format = RenderTextureFormat.ARGB32;

        public TextureWrapMode WrapMode = TextureWrapMode.Clamp;

        public FilterMode FilterMode = FilterMode.Point;

        public RenderTextureReadWrite ReadWrite;

        public bool Mipmaps;

        public bool EnableRandomWrite;

        public int AnisoLevel;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void InitSlots()
        {
            base.InitSlots();

            for (ushort i = 0; i < Capacity; i++)
            {
                var texture = RTExtensions.CreateRTexture(new Vector2(TileSize, TileSize), 0, Format, FilterMode, WrapMode, Mipmaps, AnisoLevel, EnableRandomWrite);
                var slot = new GPUSlot(this, texture);

                AddSlot(i, slot);
            }
        }

        /// <summary>
        ///     A slot managed by a GPUTileStorage containing the texture.
        /// </summary>
        public class GPUSlot : Slot
        {
            public GPUSlot(TileStorage owner, RenderTexture texture) : base(owner)
            {
                Texture = texture;
            }

            public RenderTexture Texture { get; }

            public override void Release()
            {
                Texture.ReleaseAndDestroy();
            }

            public override void Clear()
            {
                RTUtility.ClearColor(Texture);
            }
        }
    }
}