using SpaceEngine.Core.Tile.Storage;
using SpaceEngine.Helpers;

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

        public DATA_TYPE DataType = DATA_TYPE.FLOAT;

        public int Channels = 1;

        public ComputeBufferType ComputeBufferType = ComputeBufferType.Default;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void InitSlots()
        {
            base.InitSlots();

            for (ushort i = 0; i < Capacity; i++)
            {
                ComputeBuffer buffer;

                switch (DataType)
                {
                    case DATA_TYPE.FLOAT:
                        buffer = new ComputeBuffer(TileSize, sizeof(float) * Channels, ComputeBufferType);
                        break;
                    case DATA_TYPE.INT:
                        buffer = new ComputeBuffer(TileSize, sizeof(int) * Channels, ComputeBufferType);
                        break;
                    case DATA_TYPE.BYTE:
                        buffer = new ComputeBuffer(TileSize, sizeof(byte) * Channels, ComputeBufferType);
                        break;
                    case DATA_TYPE.SHORT:
                    default:
                    {
                        buffer = new ComputeBuffer(TileSize, sizeof(float) * Channels, ComputeBufferType);

                        Debug.LogWarning($"TileStorage: {DataType.ToString()} data type isn't supported by {GetType().Name}! Float type will be used!");

                        break;
                    }
                }

                var slot = new CBSlot(this, buffer);

                AddSlot(i, slot);
            }
        }
    }
}