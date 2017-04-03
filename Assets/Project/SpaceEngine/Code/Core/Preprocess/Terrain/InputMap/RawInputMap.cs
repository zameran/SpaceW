using System;
using System.IO;

using UnityEngine;

namespace SpaceEngine.Core.Preprocess.Terrain
{
    /// <summary>
    /// Used to load data from a raw file. The file maybe 8 or 16 bit.
    /// If the file is 16 bit it may have a mac or windows byte order.
    /// If the file is large the option of caching maybe enabed so only the tiles that are need are loaded from the file. 
    /// This is slower but will allow files that are larger than the maximum memory of the system to be processed.
    /// </summary>
    public class RawInputMap : InputMap
    {
        [Serializable]
        private enum BYTE_ORDER
        {
            WINDOWS,
            MAC
        };

        [Serializable]
        private enum BYTES
        {
            BIT16,
            BIT8
        };

        /// <summary>
        /// The width of this map.
        /// </summary>
        [SerializeField]
        int width;

        /// <summary>
        /// The height of this map.
        /// </summary>
        [SerializeField]
        int height;

        /// <summary>
        /// The number of components per pixel of this map.
        /// </summary>
        [SerializeField]
        private int channels;

        [SerializeField]
        private string FileName = "/Proland/Textures/Terrain/Source/HeightMap.raw";

        [SerializeField]
        private BYTE_ORDER ByteOrder = BYTE_ORDER.MAC;

        [SerializeField]
        private BYTES Bytes = BYTES.BIT16;

        [SerializeField]
        private bool UseCaching = false;

        float[] Data;

        public override int Width { get { return width; } }

        public override int Height { get { return height; } }

        public override int Channels { get { return channels; } }

        protected override void Start()
        {
            base.Start();

            if (!UseCaching)
            {
                // If caching not used load all data into memory.
                if (Bytes == BYTES.BIT8)
                {
                    LoadRawFile8(Application.dataPath + FileName);
                }
                else if (Bytes == BYTES.BIT16)
                {
                    LoadRawFile16(Application.dataPath + FileName, ByteOrder == BYTE_ORDER.MAC);
                }

                Debug.Log(string.Format("RawInputMap.Start: {0} loaded!", FileName));
            }
        }

        public override Vector4 GetValue(int x, int y)
        {
            var value = Vector4.zero;

            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);

            value.x = Data[(x + y * width) * Channels + 0];

            if (Channels > 1) value.y = Data[(x + y * width) * Channels + 1];
            if (Channels > 2) value.z = Data[(x + y * width) * Channels + 2];
            if (Channels > 3) value.w = Data[(x + y * width) * Channels + 3];

            return value;
        }

        public override float[] GetValues(int tx, int ty)
        {
            if (!UseCaching)
            {
                // If caching not used load all data into memory.
                return base.GetValues(tx, ty);
            }
            else
            {
                // If caching is used load only the tile needed into memory.
                var tileSize = GetTileSize();

                var values = new float[tileSize * tileSize * Channels];
                var strip = new float[tileSize * Channels];

                for (int j = 0; j < tileSize; ++j)
                {
                    // The index into the file that the current strip can be found at
                    var idx = (long)((long)tx + (long)(ty + j) * (long)width) * (long)Channels;

                    // The data for a 2D map can be accessed in the file in contiguous strips
                    if (Bytes == BYTES.BIT8)
                    {
                        LoadStrip8(Application.dataPath + FileName, idx, strip);
                    }
                    else if (Bytes == BYTES.BIT16)
                    {
                        LoadStrip16(Application.dataPath + FileName, (tx + (ty + j) * width) * Channels * 2, strip, ByteOrder == BYTE_ORDER.MAC);
                    }

                    for (int i = 0; i < tileSize; ++i)
                    {
                        var offset = (i + j * tileSize) * Channels;

                        values[offset] = strip[i * Channels + 0];

                        if (Channels > 1)
                        {
                            values[offset + 1] = strip[i * Channels + 1];
                        }

                        if (Channels > 2)
                        {
                            values[offset + 2] = strip[i * Channels + 2];
                        }

                        if (Channels > 3)
                        {
                            values[offset + 3] = strip[i * Channels + 3];
                        }
                    }
                }

                return values;
            }
        }

        private void LoadRawFile8(string path)
        {
            long size = Width * Height * Channels;

            var data = new byte[size];

            using (Stream stream = new FileStream(path, FileMode.Open))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, data.Length);
            }

            Data = new float[size];

            for (long x = 0; x < size; x++)
            {
                Data[x] = (float)data[x] / 255.0f;
            }
        }

        private void LoadRawFile16(string path, bool bigendian)
        {
            long size = Width * Height * Channels;

            var data = new byte[size * 2];

            using (Stream stream = new FileStream(path, FileMode.Open))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, data.Length);
            }

            Data = new float[size];

            for (long x = 0, i = 0; x < size; x++)
            {
                // Extract 16 bit data and normalize.
                Data[x] = (bigendian) ? (data[i++] * 256.0f + data[i++]) : (data[i++] + data[i++] * 256.0f);
                Data[x] /= 65535.0f;
            }
        }

        private void LoadStrip8(string path, long offset, float[] strip)
        {
            var data = new byte[strip.Length];

            using (Stream stream = new FileStream(path, FileMode.Open))
            {
                stream.Seek(offset, SeekOrigin.Begin);
                stream.Read(data, 0, data.Length);
            }

            for (int x = 0; x < strip.Length; x++)
            {
                strip[x] = (float)data[x] / 255.0f;
            }
        }

        private void LoadStrip16(string path, long offset, float[] strip, bool bigendian)
        {
            var data = new byte[strip.Length * 2];

            using (Stream stream = new FileStream(path, FileMode.Open))
            {
                stream.Seek(offset, SeekOrigin.Begin);
                stream.Read(data, 0, data.Length);
            }

            for (int x = 0, i = 0; x < strip.Length; x++)
            {
                // Extract 16 bit data and normalize.
                strip[x] = (bigendian) ? (data[i++] * 256.0f + data[i++]) : (data[i++] + data[i++] * 256.0f);
                strip[x] /= 65535.0f;
            }
        }
    }
}