#region License

// Procedural planet renderer.
//
// Copyright (c) 2008-2011 INRIA
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// Proland is distributed under a dual-license scheme.
// You can obtain a specific license from Inria: proland-licensing@inria.fr.
//
// Authors: Justin Hawkins 2014.
// Modified by Denis Ovchinnikov 2015-2017

#endregion

using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpaceEngine.Utilities
{
    public static class CBUtility
    {
        [Serializable]
        public enum Channels : byte
        {
            R = 1,
            RG = 2,
            RGB = 3,
            RGBA = 4
        }

        [Serializable]
        public enum Manipulation : byte
        {
            Read = 1,
            Write = 2,
            WriteFromFile = 3
        }

        public static ComputeBuffer CreateArgBuffer(int vertexCountPerInstance, int instanceCount, int startVertex, int startInstance)
        {
            var buffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
            var args = new[] { vertexCountPerInstance, instanceCount, startVertex, startInstance };

            buffer.SetData(args);

            return buffer;
        }

        public static int GetVertexCountPerInstance(ComputeBuffer buffer)
        {
            var args = new[] { 0, 0, 0, 0 };

            buffer.GetData(args);

            return args[0];
        }

        public static void Execute(RenderTexture tex, Channels channels, ComputeBuffer buffer, Manipulation manipulation, ComputeShader manipulationShader, params object[] args)
        {
            if (tex == null)
            {
                Debug.Log("CBUtility.Execute: RenderTexture is null!");

                return;
            }

            if (buffer == null)
            {
                Debug.Log("CBUtility.Execute: Buffer is null!");

                return;
            }

            if (manipulationShader == null)
            {
                Debug.Log("CBUtility.Execute: Computer shader is null!");

                return;
            }

            if (!tex.IsCreated())
            {
                Debug.Log("CBUtility.Execute: Parameter tex has not been created (Call Create() on tex)!");

                return;
            }

            var filePath = "";

            if (manipulation == Manipulation.WriteFromFile)
            {
                if (!tex.enableRandomWrite)
                {
                    Debug.Log("CBUtility.Execute: You must enable random write on target render texture!");

                    return;
                }

                if (args == null || args.Length == 0)
                {
                    Debug.Log("CBUtility.Execute: Can't proceed without args!");

                    return;
                }

                var filePathArg = args[0] as string;
                if (filePathArg != null)
                {
                    filePath = filePathArg;
                }
            }

            var kernel = -1;
            var depth = -1;
            var width = tex.width;
            var height = tex.height;

            var channelsCount = (byte)channels;

            var D = "";
            var C = $"C{channelsCount}";
            var M = manipulation.ToString().ToLower();

            if (tex.dimension == TextureDimension.Tex3D)
            {
                depth = tex.volumeDepth;
                D = "3D";
            }
            else
            {
                depth = 1;
                D = "2D";
            }

            var dimensionAndChannel = $"{D}{C}";

            kernel = manipulationShader.FindKernel($"{M}{dimensionAndChannel}");

            if (kernel == -1)
            {
                Debug.Log(string.Format("CBUtility.Execute: Could not find kernel {1}{0}", dimensionAndChannel, M));

                return;
            }

            switch (manipulation)
            {
                case Manipulation.Read:
                {
                    manipulationShader.SetTexture(kernel, $"_Tex{D}", tex);
                    manipulationShader.SetInt("_Width", width);
                    manipulationShader.SetInt("_Height", height);
                    manipulationShader.SetInt("_Depth", depth);
                    manipulationShader.SetBuffer(kernel, $"_Buffer{dimensionAndChannel}", buffer);
                }

                    break;
                case Manipulation.Write:
                {
                    manipulationShader.SetTexture(kernel, $"_Des{dimensionAndChannel}", tex);
                    manipulationShader.SetInt("_Width", width);
                    manipulationShader.SetInt("_Height", height);
                    manipulationShader.SetInt("_Depth", depth);
                    manipulationShader.SetBuffer(kernel, $"_Buffer{dimensionAndChannel}", buffer);
                }

                    break;
                case Manipulation.WriteFromFile:
                {
                    var size = width * height * depth * channelsCount;

                    var map = new float[size];

                    if (!LoadRawFile(filePath, map, size))
                    {
                        return;
                    }

                    buffer.SetData(map);

                    //set the compute shader uniforms
                    manipulationShader.SetTexture(kernel, $"_Des{D}{C}", tex);
                    manipulationShader.SetInt("_Width", width);
                    manipulationShader.SetInt("_Height", height);
                    manipulationShader.SetInt("_Depth", depth);
                    manipulationShader.SetBuffer(kernel, $"_Buffer{D}{C}", buffer);
                }

                    break;
                default:
                    throw new ArgumentOutOfRangeException("manipulation", manipulation, null);
            }

            // NOTE : Runs in threads of 8 so non divisable by 8 numbers will need some extra threadBlocks. This will result in some unneeded threads running...
            var padX = width % 8 == 0 ? 0 : 1;
            var padY = height % 8 == 0 ? 0 : 1;
            var padZ = depth % 8 == 0 ? 0 : 1;

            manipulationShader.Dispatch(kernel, Mathf.Max(1, width / 8 + padX), Mathf.Max(1, height / 8 + padY), Mathf.Max(1, depth / 8 + padZ));
        }

        public static void ReadFromRenderTexture(RenderTexture tex, Channels channels, ComputeBuffer buffer, ComputeShader readData)
        {
            Execute(tex, channels, buffer, Manipulation.Read, readData);
        }

        public static void ReadSingleFromRenderTexture(RenderTexture tex, float x, float y, float z, ComputeBuffer buffer, ComputeShader readData, bool useBilinear)
        {
            if (tex == null)
            {
                Debug.Log("CBUtility.ReadSingleFromRenderTexture: RenderTexture is null!");

                return;
            }

            if (buffer == null)
            {
                Debug.Log("CBUtility.ReadSingleFromRenderTexture: Buffer is null!");

                return;
            }

            if (readData == null)
            {
                Debug.Log("CBUtility.ReadSingleFromRenderTexture: Computer shader is null!");

                return;
            }

            if (!tex.IsCreated())
            {
                Debug.Log("CBUtility.ReadSingleFromRenderTexture: Parameter tex has not been created (Call Create() on tex)!");

                return;
            }

            var kernel = -1;
            var depth = -1;
            var D = "";
            var B = useBilinear ? "Bilinear" : "";

            if (tex.dimension == TextureDimension.Tex3D)
            {
                depth = tex.volumeDepth;
                D = "3D";
            }
            else
            {
                depth = 1;
                D = "2D";
            }

            kernel = readData.FindKernel($"readSingle{B}{D}");

            if (kernel == -1)
            {
                Debug.Log($"CBUtility.ReadSingleFromRenderTexture: Could not find kernel readSingle{B + D}");

                return;
            }

            var width = tex.width;
            var height = tex.height;

            //set the compute shader uniforms
            readData.SetTexture(kernel, $"_Tex{D}", tex);
            readData.SetBuffer(kernel, $"_BufferSingle{D}", buffer);

            //used for point sampling
            readData.SetInt("_IdxX", (int)x);
            readData.SetInt("_IdxY", (int)y);
            readData.SetInt("_IdxZ", (int)z);

            //used for bilinear sampling
            readData.SetVector("_UV", new Vector4(x / (width - 1), y / (height - 1), z / (depth - 1), 0.0f));

            readData.Dispatch(kernel, 1, 1, 1);
        }

        public static void WriteIntoRenderTexture(RenderTexture tex, Channels channels, ComputeBuffer buffer, ComputeShader writeData)
        {
            Execute(tex, channels, buffer, Manipulation.Write, writeData);
        }

        public static void Three2Three(RenderTexture from, RenderTexture to, ComputeShader transfer)
        {
            var kernel = transfer.FindKernel("Three2Three");
            var depth = from.volumeDepth;

            if (kernel == -1)
            {
                Debug.Log("CBUtility.Three2Three: Could not find kernel " + "Three2Three");

                return;
            }

            var width = from.width;
            var height = from.height;

            transfer.SetTexture(kernel, "_From", from);
            transfer.SetTexture(kernel, "_To", to);
            transfer.SetInt("_Width", width);
            transfer.SetInt("_Height", height);
            transfer.SetInt("_Depth", depth);

            var padX = width % 8 == 0 ? 0 : 1;
            var padY = height % 8 == 0 ? 0 : 1;
            var padZ = depth % 8 == 0 ? 0 : 1;

            transfer.Dispatch(kernel, Mathf.Max(1, width / 8 + padX), Mathf.Max(1, height / 8 + padY), Mathf.Max(1, depth / 8 + padZ));
        }

        public static void WriteIntoRenderTexture(RenderTexture tex, Channels channels, string path, ComputeBuffer buffer, ComputeShader writeData)
        {
            Execute(tex, channels, buffer, Manipulation.WriteFromFile, writeData, path);
        }

        private static bool LoadRawFile(string path, float[] map, int size)
        {
            var fi = new FileInfo(path);
            var fs = fi.OpenRead();

            var data = new byte[fi.Length];
            fs.Read(data, 0, (int)fi.Length);
            fs.Close();

            //divide by 4 as there are 4 bytes in a 32 bit float
            if (size > fi.Length / 4)
            {
                Debug.Log($"CBUtility.LoadRawFile: Raw file is not the required size! {path}");

                return false;
            }

            for (int x = 0, i = 0; x < size; x++, i += 4)
                //Convert 4 bytes to 1 32 bit float
            {
                map[x] = BitConverter.ToSingle(data, i);
            }

            return true;
        }
    }
}