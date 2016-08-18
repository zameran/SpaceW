#region License
//
// Procedural planet renderer.
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
// Modified by Denis Ovchinnikov 2015-2016
#endregion

using UnityEngine;

using System.IO;

static public class CBUtility
{
    static public ComputeBuffer CreateArgBuffer(int vertexCountPerInstance, int instanceCount, int startVertex, int startInstance)
    {
        ComputeBuffer buffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        int[] args = new int[] { vertexCountPerInstance, instanceCount, startVertex, startInstance };
        buffer.SetData(args);

        return buffer;
    }

    static public int GetVertexCountPerInstance(ComputeBuffer buffer)
    {
        int[] args = new int[] { 0, 0, 0, 0 };
        buffer.GetData(args);
        return args[0];
    }

    static public void ReadFromRenderTexture(RenderTexture tex, int channels, ComputeBuffer buffer, ComputeShader readData)
    {
        if (tex == null)
        {
            Debug.Log("CBUtility::ReadFromRenderTexture - RenderTexture is null");
            return;
        }

        if (buffer == null)
        {
            Debug.Log("CBUtility::ReadFromRenderTexture - buffer is null");
            return;
        }

        if (readData == null)
        {
            Debug.Log("CBUtility::ReadFromRenderTexture - Computer shader is null");
            return;
        }

        if (channels < 1 || channels > 4)
        {
            Debug.Log("CBUtility::ReadFromRenderTexture - Channels must be 1, 2, 3, or 4");
            return;
        }

        if (!tex.IsCreated())
        {
            Debug.Log("CBUtility::ReadFromRenderTexture - tex has not been created (Call Create() on tex)");
            return;
        }

        int kernel = -1;
        int depth = 1;
        string D = "2D";
        string C = "C" + channels.ToString();

        if (tex.isVolume)
        {
            depth = tex.volumeDepth;
            D = "3D";
        }

        kernel = readData.FindKernel("read" + D + C);

        if (kernel == -1)
        {
            Debug.Log("CBUtility::ReadFromRenderTexture - could not find kernel " + "read" + D + C);
            return;
        }

        int width = tex.width;
        int height = tex.height;

        //set the compute shader uniforms
        readData.SetTexture(kernel, "_Tex" + D, tex);
        readData.SetInt("_Width", width);
        readData.SetInt("_Height", height);
        readData.SetInt("_Depth", depth);
        readData.SetBuffer(kernel, "_Buffer" + D + C, buffer);
        //run the  compute shader. Runs in threads of 8 so non divisable by 8 numbers will need
        //some extra threadBlocks. This will result in some unneeded threads running 
        int padX = (width % 8 == 0) ? 0 : 1;
        int padY = (height % 8 == 0) ? 0 : 1;
        int padZ = (depth % 8 == 0) ? 0 : 1;

        readData.Dispatch(kernel, Mathf.Max(1, width / 8 + padX), Mathf.Max(1, height / 8 + padY), Mathf.Max(1, depth / 8 + padZ));

    }

    static public void ReadSingleFromRenderTexture(RenderTexture tex, float x, float y, float z, ComputeBuffer buffer, ComputeShader readData, bool useBilinear)
    {
        if (tex == null)
        {
            Debug.Log("CBUtility::ReadSingleFromRenderTexture - RenderTexture is null");
            return;
        }

        if (buffer == null)
        {
            Debug.Log("CBUtility::ReadSingleFromRenderTexture - buffer is null");
            return;
        }

        if (readData == null)
        {
            Debug.Log("CBUtility::ReadSingleFromRenderTexture - Computer shader is null");
            return;
        }

        if (!tex.IsCreated())
        {
            Debug.Log("CBUtility::ReadSingleFromRenderTexture - tex has not been created (Call Create() on tex)");
            return;
        }

        int kernel = -1;
        int depth = 1;
        string D = "2D";
        string B = (useBilinear) ? "Bilinear" : "";

        if (tex.isVolume)
        {
            depth = tex.volumeDepth;
            D = "3D";
        }

        kernel = readData.FindKernel("readSingle" + B + D);

        if (kernel == -1)
        {
            Debug.Log("CBUtility::ReadSingleFromRenderTexture - could not find kernel " + "readSingle" + B + D);
            return;
        }

        int width = tex.width;
        int height = tex.height;

        //set the compute shader uniforms
        readData.SetTexture(kernel, "_Tex" + D, tex);
        readData.SetBuffer(kernel, "_BufferSingle" + D, buffer);
        //used for point sampling
        readData.SetInt("_IdxX", (int)x);
        readData.SetInt("_IdxY", (int)y);
        readData.SetInt("_IdxZ", (int)z);
        //used for bilinear sampling
        readData.SetVector("_UV", new Vector4(x / (float)(width - 1), y / (float)(height - 1), z / (float)(depth - 1), 0.0f));

        readData.Dispatch(kernel, 1, 1, 1);

    }

    static public void WriteIntoRenderTexture(RenderTexture tex, int channels, ComputeBuffer buffer, ComputeShader writeData)
    {
        if (tex == null)
        {
            Debug.Log("CBUtility::WriteIntoRenderTexture - RenderTexture is null");
            return;
        }

        if (buffer == null)
        {
            Debug.Log("CBUtility::WriteIntoRenderTexture - buffer is null");
            return;
        }

        if (writeData == null)
        {
            Debug.Log("CBUtility::WriteIntoRenderTexture - Computer shader is null");
            return;
        }

        if (channels < 1 || channels > 4)
        {
            Debug.Log("CBUtility::WriteIntoRenderTexture - Channels must be 1, 2, 3, or 4");
            return;
        }

        if (!tex.enableRandomWrite)
        {
            Debug.Log("CBUtility::WriteIntoRenderTexture - you must enable random write on render texture");
            return;
        }

        if (!tex.IsCreated())
        {
            Debug.Log("CBUtility::WriteIntoRenderTexture - tex has not been created (Call Create() on tex)");
            return;
        }

        int kernel = -1;
        int depth = 1;
        string D = "2D";
        string C = "C" + channels.ToString();

        if (tex.isVolume)
        {
            depth = tex.volumeDepth;
            D = "3D";
        }

        kernel = writeData.FindKernel("write" + D + C);

        if (kernel == -1)
        {
            Debug.Log("CBUtility::WriteIntoRenderTexture - could not find kernel " + "write" + D + C);
            return;
        }

        int width = tex.width;
        int height = tex.height;

        //set the compute shader uniforms
        writeData.SetTexture(kernel, "_Des" + D + C, tex);
        writeData.SetInt("_Width", width);
        writeData.SetInt("_Height", height);
        writeData.SetInt("_Depth", depth);
        writeData.SetBuffer(kernel, "_Buffer" + D + C, buffer);
        //run the  compute shader. Runs in threads of 8 so non divisable by 8 numbers will need
        //some extra threadBlocks. This will result in some unneeded threads running 
        int padX = (width % 8 == 0) ? 0 : 1;
        int padY = (height % 8 == 0) ? 0 : 1;
        int padZ = (depth % 8 == 0) ? 0 : 1;

        writeData.Dispatch(kernel, Mathf.Max(1, width / 8 + padX), Mathf.Max(1, height / 8 + padY), Mathf.Max(1, depth / 8 + padZ));
    }

    static public void Three2Three(RenderTexture from, RenderTexture to, ComputeShader transfer)
    {
        int kernel = transfer.FindKernel("Three2Three");
        int depth = from.volumeDepth;

        if (transfer == null)
        {
            Debug.Log("CBUtility::Three2Three - Computer shader is null");
            return;
        }

        if (kernel == -1)
        {
            Debug.Log("CBUtility::Three2Three - could not find kernel " + "Three2Three");
            return;
        }

        int width = from.width;
        int height = from.height;

        transfer.SetTexture(kernel, "_From", from);
        transfer.SetTexture(kernel, "_To", to);
        transfer.SetInt("_Width", width);
        transfer.SetInt("_Height", height);
        transfer.SetInt("_Depth", depth);

        int padX = (width % 8 == 0) ? 0 : 1;
        int padY = (height % 8 == 0) ? 0 : 1;
        int padZ = (depth % 8 == 0) ? 0 : 1;

        transfer.Dispatch(kernel, Mathf.Max(1, width / 8 + padX), Mathf.Max(1, height / 8 + padY), Mathf.Max(1, depth / 8 + padZ));
    }

    static public void WriteIntoRenderTexture(RenderTexture tex, int channels, string path, ComputeBuffer buffer, ComputeShader writeData)
    {
        if (tex == null)
        {
            Debug.Log("CBUtility::WriteIntoRenderTexture - RenderTexture is null");
            return;
        }

        if (buffer == null)
        {
            Debug.Log("CBUtility::WriteIntoRenderTexture - buffer is null");
            return;
        }

        if (writeData == null)
        {
            Debug.Log("CBUtility::WriteIntoRenderTexture - Computer shader is null");
            return;
        }

        if (channels < 1 || channels > 4)
        {
            Debug.Log("CBUtility::WriteIntoRenderTexture - Channels must be 1, 2, 3, or 4");
            return;
        }

        if (!tex.enableRandomWrite)
        {
            Debug.Log("CBUtility::WriteIntoRenderTexture - you must enable random write on render texture");
            return;
        }

        if (!tex.IsCreated())
        {
            Debug.Log("CBUtility::WriteIntoRenderTexture - tex has not been created (Call Create() on tex)");
            return;
        }

        int kernel = -1;
        int depth = 1;
        string D = "2D";
        string C = "C" + channels.ToString();

        if (tex.isVolume)
        {
            depth = tex.volumeDepth;
            D = "3D";
        }

        kernel = writeData.FindKernel("write" + D + C);

        if (kernel == -1)
        {
            Debug.Log("CBUtility::WriteIntoRenderTexture - could not find kernel " + "write" + D + C);
            return;
        }

        int width = tex.width;
        int height = tex.height;
        int size = width * height * depth * channels;

        float[] map = new float[size];

        if (!LoadRawFile(path, map, size)) return;

        buffer.SetData(map);

        //set the compute shader uniforms
        writeData.SetTexture(kernel, "_Des" + D + C, tex);
        writeData.SetInt("_Width", width);
        writeData.SetInt("_Height", height);
        writeData.SetInt("_Depth", depth);
        writeData.SetBuffer(kernel, "_Buffer" + D + C, buffer);
        //run the  compute shader. Runs in threads of 8 so non divisable by 8 numbers will need
        //some extra threadBlocks. This will result in some unneeded threads running 
        int padX = (width % 8 == 0) ? 0 : 1;
        int padY = (height % 8 == 0) ? 0 : 1;
        int padZ = (depth % 8 == 0) ? 0 : 1;

        writeData.Dispatch(kernel, Mathf.Max(1, width / 8 + padX), Mathf.Max(1, height / 8 + padY), Mathf.Max(1, depth / 8 + padZ));
    }

    static bool LoadRawFile(string path, float[] map, int size)
    {
        FileInfo fi = new FileInfo(path);

        if (fi == null)
        {
            Debug.Log("CBUtility::LoadRawFile - Raw file not found (" + path + ")");
            return false;
        }

        FileStream fs = fi.OpenRead();

        byte[] data = new byte[fi.Length];
        fs.Read(data, 0, (int)fi.Length);
        fs.Close();

        //divide by 4 as there are 4 bytes in a 32 bit float
        if (size > fi.Length / 4)
        {
            Debug.Log("CBUtility::LoadRawFile - Raw file is not the required size (" + path + ")");
            return false;
        }

        for (int x = 0, i = 0; x < size; x++, i += 4)
        {
            //Convert 4 bytes to 1 32 bit float
            map[x] = System.BitConverter.ToSingle(data, i);
        };

        return true;
    }
}