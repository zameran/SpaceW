using System;
using System.Collections.Generic;

[Serializable]
public class PatchManager
{
    private List<int[]> patches = new List<int[]>();

    public List<int[]> Patches { get { return patches; } }

    public PatchManager(PatchConfig config, PatchSphere planet)
    {
        for (byte i = 0; i < 16; i++)
        {
            GenerateGrid(i, config, planet);
        }
    }

    private void GenerateGrid(byte edges, PatchConfig config, PatchSphere sphere)
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
        ushort edgeFullTrianglesCount = (ushort)((config.PatchSize - 1) * 2 - 2);

        //number of indices in one full-res edge
        ushort edgeFullIndexCount = (ushort)(edgeFullTrianglesCount * 3);

        //number of triangles in one half-res edge
        //discounting the two extremities triangles that aren't included on this edge
        ushort edgeHalfTriangleCount = (ushort)((edgeFullTrianglesCount / 2) + (edgeFullTrianglesCount / 4));

        //number of indices in one half-res edge
        ushort edgeHalfIndexCount = (ushort)(edgeHalfTriangleCount * 3);

        //number of indices within the main part of the patch,
        //discounting the four edges that will sum up to this
        ushort mainIndexCount = (ushort)(((config.PatchSize - 3) * (config.PatchSize - 3)) * 6);

        //our total of indices
        //starts with the mainIndexCount,
        //and gets update with edges index counts
        ushort totalIndexCount = mainIndexCount;

        //add index count for each edge
        int i = 1;
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
        int[] idxList = new int[totalIndexCount];

        ushort idx = 0;
        for (ushort lin = 1; lin < config.PatchSize - 2; lin++)
        {
            ushort pos = (ushort)(lin * config.PatchSize);

            for (ushort col = 1; col < config.PatchSize - 2; col++)
            {
                ushort position = (ushort)(pos + col);

                idxList[idx++] = position + config.PatchSize;
                idxList[idx++] = position + 1;
                idxList[idx++] = position;

                idxList[idx++] = position + 1 + config.PatchSize;
                idxList[idx++] = position + 1;
                idxList[idx++] = position + config.PatchSize;
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
            bool flag = false;
            ushort x = 0;
            ushort pp = 0;
            while (x < config.PatchSize - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp + config.PatchSize + 1;
                    idxList[idx++] = pp + 2;

                    x += 2;
                    pp += 2;
                }
                else
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp + config.PatchSize - 1;
                    idxList[idx++] = pp + config.PatchSize;

                    idxList[idx++] = pp;
                    idxList[idx++] = pp + config.PatchSize;
                    idxList[idx++] = pp + config.PatchSize + 1;
                }

                flag = !flag;
            }
        }
        else
        {
            //top edge at full-resolution
            bool flag = false;
            ushort x = 0;
            ushort pp = 0;
            while (x < config.PatchSize - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp + config.PatchSize + 1;
                    idxList[idx++] = pp + 1;

                    idxList[idx++] = pp + 1;
                    idxList[idx++] = pp + config.PatchSize + 1;
                    idxList[idx++] = pp + 2;

                    x += 2;
                    pp += 2;
                }
                else
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp + config.PatchSize - 1;
                    idxList[idx++] = pp + config.PatchSize;

                    idxList[idx++] = pp;
                    idxList[idx++] = pp + config.PatchSize;
                    idxList[idx++] = pp + config.PatchSize + 1;
                }

                flag = !flag;
            }
        }

        //right edge
        if ((edges & (1 << (int)NeighborDirection.Right)) > 0)
        {
            //right edge at half-resolution
            bool flag = false;
            ushort y = 0;
            ushort pp = (ushort)(config.PatchSize - 1);
            while (y < config.PatchSize - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp + config.PatchSize - 1;
                    idxList[idx++] = pp + config.PatchSize * 2;

                    y += 2;
                    pp += (ushort)(config.PatchSize * 2);
                }
                else
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp - config.PatchSize - 1;
                    idxList[idx++] = pp - 1;

                    idxList[idx++] = pp;
                    idxList[idx++] = pp - 1;
                    idxList[idx++] = pp + config.PatchSize - 1;
                }

                flag = !flag;
            }
        }
        else
        {
            //right edge at full-resolution
            bool flag = false;
            ushort y = 0;
            ushort pp = (ushort)(config.PatchSize - 1);
            while (y < config.PatchSize - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp + config.PatchSize - 1;
                    idxList[idx++] = pp + config.PatchSize;

                    idxList[idx++] = pp + config.PatchSize;
                    idxList[idx++] = pp + config.PatchSize - 1;
                    idxList[idx++] = pp + config.PatchSize * 2;

                    y += 2;
                    pp += (ushort)(config.PatchSize * 2);
                }
                else
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp - config.PatchSize - 1;
                    idxList[idx++] = pp - 1;

                    idxList[idx++] = pp;
                    idxList[idx++] = pp - 1;
                    idxList[idx++] = pp + config.PatchSize - 1;
                }

                flag = !flag;
            }
        }

        //bottom edge
        if ((edges & (1 << (int)NeighborDirection.Bottom)) > 0)
        {
            //bottom edge at half-resolution
            bool flag = false;
            ushort x = 0;
            ushort pp = (ushort)((config.PatchSize - 1) * config.PatchSize);
            while (x < config.PatchSize - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp + 2;
                    idxList[idx++] = pp - config.PatchSize + 1;

                    x += 2;
                    pp += 2;
                }
                else
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp - config.PatchSize;
                    idxList[idx++] = pp - config.PatchSize - 1;

                    idxList[idx++] = pp;
                    idxList[idx++] = pp - config.PatchSize + 1;
                    idxList[idx++] = pp - config.PatchSize;
                }

                flag = !flag;
            }
        }
        else
        {
            //bottom edge at full-resolution
            bool flag = false;
            ushort x = 0;
            ushort pp = (ushort)((config.PatchSize - 1) * config.PatchSize);
            while (x < config.PatchSize - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp + 1;
                    idxList[idx++] = pp - config.PatchSize + 1;

                    idxList[idx++] = pp + 1;
                    idxList[idx++] = pp + 2;
                    idxList[idx++] = pp - config.PatchSize + 1;

                    x += 2;
                    pp += 2;
                }
                else
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp - config.PatchSize;
                    idxList[idx++] = pp - config.PatchSize - 1;

                    idxList[idx++] = pp;
                    idxList[idx++] = pp - config.PatchSize + 1;
                    idxList[idx++] = pp - config.PatchSize;
                }

                flag = !flag;
            }
        }

        //left edge
        if ((edges & (1 << (int)NeighborDirection.Left)) > 0)
        {
            //left edge at half-resolution
            bool flag = false;
            ushort y = 0;
            ushort pp = 0;
            while (y < config.PatchSize - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp + config.PatchSize * 2;
                    idxList[idx++] = pp + config.PatchSize + 1;

                    y += 2;
                    pp += (ushort)(config.PatchSize * 2);
                }
                else
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp + 1;
                    idxList[idx++] = pp - config.PatchSize + 1;

                    idxList[idx++] = pp;
                    idxList[idx++] = pp + config.PatchSize + 1;
                    idxList[idx++] = pp + 1;
                }

                flag = !flag;
            }
        }
        else
        {
            //left edge at full-resolution
            bool flag = false;
            ushort y = 0;
            ushort pp = 0;
            while (y < config.PatchSize - 2)
            {
                if (!flag)
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp + config.PatchSize;
                    idxList[idx++] = pp + config.PatchSize + 1;

                    idxList[idx++] = pp + config.PatchSize;
                    idxList[idx++] = pp + config.PatchSize * 2;
                    idxList[idx++] = pp + config.PatchSize + 1;

                    y += 2;
                    pp += (ushort)(config.PatchSize * 2);
                }
                else
                {
                    idxList[idx++] = pp;
                    idxList[idx++] = pp + config.PatchSize + 1;
                    idxList[idx++] = pp + 1;

                    idxList[idx++] = pp;
                    idxList[idx++] = pp + 1;
                    idxList[idx++] = pp - config.PatchSize + 1;
                }

                flag = !flag;
            }
        }

        patches.Add(idxList);
    }
}