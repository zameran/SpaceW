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

using System;
using System.IO;
using SpaceEngine.Core.Debugging;
using UnityEngine;
using Logger = SpaceEngine.Core.Debugging.Logger;

namespace SpaceEngine.Core.Preprocess.Terrain.InputMap
{
    /// <summary>
    /// Used to load data from a raw file. The file maybe 8 or 16 bit.
    /// If the file is 16 bit it may have a mac or windows byte order.
    /// If the file is large the option of caching maybe enabed so only the tiles that are need are loaded from the file. 
    /// This is slower but will allow files that are larger than the maximum memory of the system to be processed.
    /// </summary>
    [UseLogger(LoggerCategory.Core)]
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
        private string FileName = "/Resources/Preprocess/Terrain/HeightMap.raw";

        [SerializeField]
        private BYTE_ORDER ByteOrder = BYTE_ORDER.MAC;

        [SerializeField]
        private BYTES Bytes = BYTES.BIT16;

        [SerializeField]
        private bool UseCaching = false;

        float[] Data;

        public override int Width => width;

        public override int Height => height;

        public override int Channels => channels;

        private string ApplicationDataPath = "";

        protected override void Awake()
        {
            base.Awake();

            ApplicationDataPath = Application.dataPath;

            if (!UseCaching)
            {
                // If caching not used load all data into memory.
                if (Bytes == BYTES.BIT8)
                {
                    LoadRawFile8(ApplicationDataPath + FileName);
                }
                else if (Bytes == BYTES.BIT16)
                {
                    LoadRawFile16(ApplicationDataPath + FileName, ByteOrder == BYTE_ORDER.MAC);
                }

                Logger.Log($"RawInputMap.Awake: {FileName} loaded!");
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

                var values = new float[(int)tileSize.x * (int)tileSize.y * Channels];
                var strip = new float[(int)tileSize.x * Channels];

                for (var j = 0; j < (int)tileSize.y; ++j)
                {
                    // The index into the file that the current strip can be found at
                    var idx = (long)((long)tx + (long)(ty + j) * (long)width) * (long)Channels;

                    // The data for a 2D map can be accessed in the file in contiguous strips
                    if (Bytes == BYTES.BIT8)
                    {
                        LoadStrip8(ApplicationDataPath + FileName, idx, strip);
                    }
                    else if (Bytes == BYTES.BIT16)
                    {
                        LoadStrip16(ApplicationDataPath + FileName, (tx + (ty + j) * width) * Channels * 2, strip, ByteOrder == BYTE_ORDER.MAC);
                    }

                    for (var i = 0; i < (int)tileSize.x; ++i)
                    {
                        var offset = (i + j * (int)tileSize.x) * Channels;

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

            for (var x = 0; x < strip.Length; x++)
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