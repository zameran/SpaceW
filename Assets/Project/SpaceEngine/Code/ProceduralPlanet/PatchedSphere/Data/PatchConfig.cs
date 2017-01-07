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

public enum PatchQuality { Minimum, Low, Standard, High, Maximum };

public enum PatchResolution { Minimum, Low, Standard, High, Maximum };

public enum NeighborDirection { Top = 0, Right = 1, Bottom = 2, Left = 3 };

public class PatchConfig
{
    ushort[] levelHeightMapRes;

    public ushort PatchSize { get; set; }
    public ushort GridSize { get; set; }
    public ushort LevelHeightMapRes(ushort level) { return levelHeightMapRes[(level >= MaxSplitLevel ? MaxSplitLevel : level)]; }
    public ushort MaxSplitLevel { get { return (ushort)(levelHeightMapRes.Length - 1); } }

    public PatchConfig(PatchQuality patchQuality, PatchResolution normalQuality)
    {
        switch (patchQuality)
        {
            case PatchQuality.Maximum:
                {
                    PatchSize = 33;
                    break;
                }

            case PatchQuality.High:
                {
                    PatchSize = 21;
                    break;
                }

            case PatchQuality.Standard:
                {
                    PatchSize = 17;
                    break;
                }

            case PatchQuality.Low:
                {
                    PatchSize = 11;
                    break;
                }

            case PatchQuality.Minimum:
                {
                    PatchSize = 7;
                    break;
                }
        }

        //heightmap resolution at each level
        //starting from level 0 (higher resolution) to highest level (lower resolution).
        //the last entry in the array will be used again in deeper split levels.
        //for example, if you define only 512,256,128 then level 0=512, level 1=256, level 2=128, level 3=128, level 4=128...
        switch (normalQuality)
        {
            case PatchResolution.Maximum:
                {
                    levelHeightMapRes = new ushort[]
                    {
                    512, 256
                    };
                    break;
                }

            case PatchResolution.High:
                {
                    levelHeightMapRes = new ushort[]
                    {
                    256
                    };
                    break;
                }

            case PatchResolution.Standard:
                {
                    levelHeightMapRes = new ushort[]
                    {
                    256, 128
                    };
                    break;
                }

            case PatchResolution.Low:
                {
                    levelHeightMapRes = new ushort[]
                    {
                    128
                    };
                    break;
                }

            case PatchResolution.Minimum:
                {
                    levelHeightMapRes = new ushort[]
                    {
                    128, 64
                    };
                    break;
                }
        }

        GridSize = (ushort)(PatchSize * PatchSize);
    }
}