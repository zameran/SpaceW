using SpaceEngine.Core.Tile.Storage;

using UnityEngine;

namespace SpaceEngine.Core.Storage
{
    /// <summary>
    /// A TileStorage that store tiles on CPU as a 2D array of values where is the type of each tile pixel component(e.g. char, float, etc).
    /// </summary>
    public class CPUTileStorage : TileStorage
    {
        /// <summary>
        /// A slot managed by a GPUTileStorage and contains the array of values.
        /// </summary>
        /// <typeparam name="T">Type of container.</typeparam>
        public class CPUSlot<T> : Slot
        {
            public T[] Data { get; private set; }
            public int Size { get; private set; }

            public void ClearData()
            {
                Data = new T[Size];
            }

            public CPUSlot(TileStorage owner, int size) : base(owner)
            {
                Data = new T[size];
                Size = size;
            }
        }

        public enum DATA_TYPE
        {
            FLOAT,
            INT,
            SHORT,
            BYTE
        }

        [SerializeField]
        public DATA_TYPE DataType = DATA_TYPE.FLOAT;

        [SerializeField]
        public int Channels = 1;

        protected override void Awake()
        {
            base.Awake();

            // NOTE : Size is sqaured as the array is 2D (but stored as a 1D array)
            var size = TileSize * TileSize * Channels;

            for (var i = 0; i < Capacity; i++)
            {
                switch ((int)DataType)
                {
                    case (int)DATA_TYPE.FLOAT:
                        AddSlot(i, new CPUSlot<float>(this, size));
                        break;
                    case (int)DATA_TYPE.INT:
                        AddSlot(i, new CPUSlot<int>(this, size));
                        break;
                    case (int)DATA_TYPE.SHORT:
                        AddSlot(i, new CPUSlot<short>(this, size));
                        break;
                    case (int)DATA_TYPE.BYTE:
                        AddSlot(i, new CPUSlot<byte>(this, size));
                        break;
                    default:
                        AddSlot(i, new CPUSlot<float>(this, size));
                        break;
                }
            }
        }
    }
}