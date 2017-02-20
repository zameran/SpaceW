using SpaceEngine.Core.Tile.Storage;

using UnityEngine;

namespace SpaceEngine.Core.Storage
{
    /// <summary>
    /// A tile storage that can contain compute buffers.
    /// </summary>
    public class CBTileStorage : TileStorage
    {
        /// <summary>
        /// A slot managed by a CBTileStorage containing the buffer.
        /// </summary>
        public class CBSlot : Slot
        {
            public ComputeBuffer Buffer { get; private set; }

            public ComputeBuffer GetBuffer()
            {
                return Buffer;
            }

            public override void Release()
            {
                BufferHelper.ReleaseAndDisposeBuffers(Buffer);
            }

            public CBSlot(TileStorage owner, ComputeBuffer buffer) : base(owner)
            {
                Buffer = buffer;
            }
        }

        public enum DATA_TYPE : byte
        {
            FLOAT,
            INT,
            BYTE
        }

        [SerializeField]
        public DATA_TYPE DataType = DATA_TYPE.FLOAT;

        [SerializeField]
        public int Channels = 1;

        [SerializeField]
        ComputeBufferType computeBufferType = ComputeBufferType.Default;
        public ComputeBufferType ComputeBufferType { get { return computeBufferType; } set { computeBufferType = value; } }

        protected override void Awake()
        {
            base.Awake();

            for (var i = 0; i < Capacity; i++)
            {
                ComputeBuffer buffer;

                switch (DataType)
                {
                    case DATA_TYPE.FLOAT:
                        buffer = new ComputeBuffer(TileSize, sizeof(float) * Channels, computeBufferType);
                        break;

                    case DATA_TYPE.INT:
                        buffer = new ComputeBuffer(TileSize, sizeof(int) * Channels, computeBufferType);
                        break;

                    case DATA_TYPE.BYTE:
                        buffer = new ComputeBuffer(TileSize, sizeof(byte) * Channels, computeBufferType);
                        break;
                    default:
                        buffer = new ComputeBuffer(TileSize, sizeof(float) * Channels, computeBufferType);
                        break;
                }

                var slot = new CBSlot(this, buffer);

                AddSlot(i, slot);
            }
        }
    }
}