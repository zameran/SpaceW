using SpaceEngine.Core.Tile.Storage;

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
                if (Texture != null) Texture.Release();
            }

            public GPUSlot(TileStorage owner, RenderTexture texture) : base(owner)
            {
                Texture = texture;
            }
        }

        [SerializeField]
        public RenderTextureFormat Format = RenderTextureFormat.ARGB32;

        [SerializeField]
        public TextureWrapMode WrapMode = TextureWrapMode.Clamp;

        [SerializeField]
        public FilterMode FilterMode = FilterMode.Point;

        [SerializeField]
        public RenderTextureReadWrite ReadWrite;

        [SerializeField]
        public bool Mipmaps;

        [SerializeField]
        public bool EnableRandomWrite;

        [SerializeField]
        public int AnisoLevel;

        protected override void Awake()
        {
            base.Awake();

            for (var i = 0; i < Capacity; i++)
            {
                var texture = new RenderTexture(TileSize, TileSize, 0, Format, ReadWrite);
                texture.filterMode = FilterMode;
                texture.wrapMode = WrapMode;
                texture.useMipMap = Mipmaps;
                texture.anisoLevel = AnisoLevel;
                texture.enableRandomWrite = EnableRandomWrite;

                var slot = new GPUSlot(this, texture);

                AddSlot(i, slot);
            }
        }
    }
}