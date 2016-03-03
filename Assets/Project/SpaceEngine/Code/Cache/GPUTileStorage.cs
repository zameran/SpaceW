using UnityEngine;
using System.Collections.Generic;

namespace Proland
{
    /// <summary>
    /// A TileStorage that stores tiles in 2D textures.
    /// </summary>
    public class GPUTileStorage : TileStorage
    {
        public class GPUSlot : Slot
        {
            RenderTexture m_height_texture;
            RenderTexture m_normal_texture;

            public RenderTexture GetHeightTexture()
            {
                return m_height_texture;
            }

            public RenderTexture GetNormalTexture()
            {
                return m_normal_texture;
            }

            public override void Release()
            {
                if (m_height_texture != null) m_height_texture.Release();
                if (m_normal_texture != null) m_normal_texture.Release();
            }

            public GPUSlot(TileStorage owner, RenderTexture height_texture, RenderTexture normal_texture)
                : base(owner)
            {
                m_height_texture = height_texture;
                m_normal_texture = normal_texture;
            }
        };

        [SerializeField]
        RenderTextureFormat m_internalFormat = RenderTextureFormat.ARGB32;

        [SerializeField]
        TextureWrapMode m_wrapMode = TextureWrapMode.Clamp;

        [SerializeField]
        FilterMode m_filterMode = FilterMode.Point;

        [SerializeField]
        RenderTextureReadWrite m_readWrite;

        [SerializeField]
        bool m_mipmaps;

        [SerializeField]
        bool m_enableRandomWrite;

        [SerializeField]
        int m_ansio;

        public RenderTextureFormat GetInternalFormat()
        {
            return m_internalFormat;
        }

        public TextureWrapMode GetWrapMode()
        {
            return m_wrapMode;
        }

        public FilterMode GetFilterMode()
        {
            return m_filterMode;
        }

        public RenderTextureReadWrite GetReadWrite()
        {
            return m_readWrite;
        }

        public bool HasMipMaps()
        {
            return m_mipmaps;
        }

        public bool RandomWriteEnabled()
        {
            return m_enableRandomWrite;
        }

        public int GetAnsioLevel()
        {
            return m_ansio;
        }

        protected override void Awake()
        {
            base.Awake();

            int tileSize = GetTileSize();
            int capacity = GetCapacity();

            for (int i = 0; i < capacity; i++)
            {
                RenderTexture height_texture = new RenderTexture(tileSize, tileSize, 0, m_internalFormat, m_readWrite);
                height_texture.filterMode = m_filterMode;
                height_texture.wrapMode = m_wrapMode;
                height_texture.useMipMap = m_mipmaps;
                height_texture.anisoLevel = m_ansio;
                height_texture.enableRandomWrite = m_enableRandomWrite;

                RenderTexture normal_texture = new RenderTexture(tileSize, tileSize, 0, m_internalFormat, m_readWrite);
                normal_texture.filterMode = m_filterMode;
                normal_texture.wrapMode = m_wrapMode;
                normal_texture.useMipMap = m_mipmaps;
                normal_texture.anisoLevel = m_ansio;
                normal_texture.enableRandomWrite = m_enableRandomWrite;

                GPUSlot slot = new GPUSlot(this, height_texture, normal_texture);

                AddSlot(i, slot);
            }
        }
    }
}