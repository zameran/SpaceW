#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
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
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using System;
using System.Collections.Generic;

[Serializable]
public class PatchManager
{
    public List<int[]> Patches { get; private set; }

    public PatchManager()
    {
        Patches = new List<int[]>();

        for (byte i = 0; i < 16; i++)
        {
            GenerateGrid(i);
        }
    }

    private void GenerateGrid(byte edges)
    {
        /*
        +++ +O+ +++ +O+
        +++ +++ O+O O+O
        +++ +O+ +++ +O+

        +O+ +++ +++ +++
        +++ ++O +++ O++
        +++ +++ +O+ +++
               
        +O+ +++ +++ +O+
        ++O ++O O++ O++
        +++ +O+ +O+ +++

        +O+ +O+ +++ +O+
        O+O ++O O+O O++
        +++ +O+ +O+ +O+
        */

        //number of triangles in one full-res line
        //discounting the two extremities triangles that aren't included on this edge
        var edgeFullTrianglesCount = (ushort)((PatchSettings.VerticesPerSide - 1) * 2 - 2);

        //number of indices in one full-res edge
        var edgeFullIndexCount = (ushort)(edgeFullTrianglesCount * 3);

        //number of triangles in one half-res edge
        //discounting the two extremities triangles that aren't included on this edge
        var edgeHalfTriangleCount = (ushort)((edgeFullTrianglesCount / 2) + (edgeFullTrianglesCount / 4));

        //number of indices in one half-res edge
        var edgeHalfIndexCount = (ushort)(edgeHalfTriangleCount * 3);

        //number of indices within the main part of the patch,
        //discounting the four edges that will sum up to this
        var mainIndexCount = (ushort)(((PatchSettings.VerticesPerSide - 3) * (PatchSettings.VerticesPerSide - 3)) * 6);

        //our total of indices
        //starts with the mainIndexCount,
        //and gets update with edges index counts
        var totalIndexCount = mainIndexCount;

        //add index count for each edge
        var i = 1;
        while (i < 16)
        {
            if ((edges & i) > 0)
            {
                //half-res edge
                totalIndexCount += edgeHalfIndexCount;
            }
            else
            {
                //full-res edge
                totalIndexCount += edgeFullIndexCount;
            }

            i <<= 1;
        }

        //allocate indices (triangles) for the patch type
        var idxList = new int[totalIndexCount];
        var flag = false;

        ushort idx = 0;
        ushort position = 0;


        for (ushort row = 1; row < PatchSettings.VerticesPerSide - 2; row++)
        {
            var pos = (ushort)(row * PatchSettings.VerticesPerSide);

            for (ushort column = 1; column < PatchSettings.VerticesPerSide - 2; column++)
            {
                position = (ushort)(pos + column);

                idxList[idx++] = position + PatchSettings.VerticesPerSide;
                idxList[idx++] = position + 1;
                idxList[idx++] = position;

                idxList[idx++] = position + 1 + PatchSettings.VerticesPerSide;
                idxList[idx++] = position + 1;
                idxList[idx++] = position + PatchSettings.VerticesPerSide;
            }
        }

        //0000 == all edges at full-res
        //0001 (1) == top edge at half-res
        //0010 (2) == right edge at half-res
        //0100 (4) == bottom edge at half-res
        //1000 (8) == left edge at half-res

        //top edge
        if ((edges & (1 << (int)NeighborDirection.Top)) > 0)
        {
            //top edge at half-resolution

            ushort x = 0;

            position = 0;

            while (x < PatchSettings.VerticesPerSide - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide + 1;
                    idxList[idx++] = position + 2;

                    x += 2;
                    position += 2;
                }
                else
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide - 1;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide;

                    idxList[idx++] = position;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide + 1;
                }

                flag = !flag;
            }

            flag = false;
        }
        else
        {
            //top edge at full-resolution
            ushort x = 0;

            position = 0;

            while (x < PatchSettings.VerticesPerSide - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide + 1;
                    idxList[idx++] = position + 1;

                    idxList[idx++] = position + 1;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide + 1;
                    idxList[idx++] = position + 2;

                    x += 2;
                    position += 2;
                }
                else
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide - 1;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide;

                    idxList[idx++] = position;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide + 1;
                }

                flag = !flag;
            }

            flag = false;
        }

        //right edge
        if ((edges & (1 << (int)NeighborDirection.Right)) > 0)
        {
            //right edge at half-resolution
            ushort y = 0;

            position = (ushort)(PatchSettings.VerticesPerSide - 1);

            while (y < PatchSettings.VerticesPerSide - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide - 1;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide * 2;

                    y += 2;
                    position += (ushort)(PatchSettings.VerticesPerSide * 2);
                }
                else
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide - 1;
                    idxList[idx++] = position - 1;

                    idxList[idx++] = position;
                    idxList[idx++] = position - 1;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide - 1;
                }

                flag = !flag;
            }

            flag = false;
        }
        else
        {
            //right edge at full-resolution
            ushort y = 0;

            position = (ushort)(PatchSettings.VerticesPerSide - 1);

            while (y < PatchSettings.VerticesPerSide - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide - 1;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide;

                    idxList[idx++] = position + PatchSettings.VerticesPerSide;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide - 1;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide * 2;

                    y += 2;
                    position += (ushort)(PatchSettings.VerticesPerSide * 2);
                }
                else
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide - 1;
                    idxList[idx++] = position - 1;

                    idxList[idx++] = position;
                    idxList[idx++] = position - 1;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide - 1;
                }

                flag = !flag;
            }

            flag = false;
        }

        //bottom edge
        if ((edges & (1 << (int)NeighborDirection.Bottom)) > 0)
        {
            //bottom edge at half-resolution
            ushort x = 0;

            position = (ushort)((PatchSettings.VerticesPerSide - 1) * PatchSettings.VerticesPerSide);

            while (x < PatchSettings.VerticesPerSide - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position + 2;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide + 1;

                    x += 2;
                    position += 2;
                }
                else
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide - 1;

                    idxList[idx++] = position;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide + 1;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide;
                }

                flag = !flag;
            }

            flag = false;
        }
        else
        {
            //bottom edge at full-resolution
            ushort x = 0;

            position = (ushort)((PatchSettings.VerticesPerSide - 1) * PatchSettings.VerticesPerSide);

            while (x < PatchSettings.VerticesPerSide - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position + 1;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide + 1;

                    idxList[idx++] = position + 1;
                    idxList[idx++] = position + 2;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide + 1;

                    x += 2;
                    position += 2;
                }
                else
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide - 1;

                    idxList[idx++] = position;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide + 1;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide;
                }

                flag = !flag;
            }

            flag = false;
        }

        //left edge
        if ((edges & (1 << (int)NeighborDirection.Left)) > 0)
        {
            //left edge at half-resolution
            ushort y = 0;

            position = 0;

            while (y < PatchSettings.VerticesPerSide - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide * 2;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide + 1;

                    y += 2;
                    position += (ushort)(PatchSettings.VerticesPerSide * 2);
                }
                else
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position + 1;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide + 1;

                    idxList[idx++] = position;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide + 1;
                    idxList[idx++] = position + 1;
                }

                flag = !flag;
            }

            flag = false;
        }
        else
        {
            //left edge at full-resolution
            ushort y = 0;

            position = 0;

            while (y < PatchSettings.VerticesPerSide - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide + 1;

                    idxList[idx++] = position + PatchSettings.VerticesPerSide;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide * 2;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide + 1;

                    y += 2;
                    position += (ushort)(PatchSettings.VerticesPerSide * 2);
                }
                else
                {
                    idxList[idx++] = position;
                    idxList[idx++] = position + PatchSettings.VerticesPerSide + 1;
                    idxList[idx++] = position + 1;

                    idxList[idx++] = position;
                    idxList[idx++] = position + 1;
                    idxList[idx++] = position - PatchSettings.VerticesPerSide + 1;
                }

                flag = !flag;
            }

            flag = false;
        }

        Patches.Add(idxList);
    }
}