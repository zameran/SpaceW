using SpaceEngine.Core.Tile.Storage;
using SpaceEngine.Core.Utilities;

using UnityEngine;

namespace SpaceEngine.Core.Storage
{
    /// <summary>
    /// A TileStorage that stores tiles in 2D textures.
    /// </summary>
    public class GPUTileStorage : TileStorage
    {
        /// <summary>
        /// A slot managed by a GPUTileStorage containing the texture.
        /// </summary>
        public class GPUSlot : Slot
        {
            public RenderTexture Texture { get; private set; }

            public override void Release()
            {
                Texture.ReleaseAndDestroy();
            }

            public override void Clear()
            {
                RTUtility.ClearColor(Texture);
            }

            public GPUSlot(TileStorage owner, RenderTexture texture) : base(owner)
            {
                Texture = texture;
            }
        }

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
    }
}