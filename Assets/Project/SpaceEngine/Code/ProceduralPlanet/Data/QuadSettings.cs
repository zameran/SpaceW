#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2016 Denis Ovchinnikov [zameran] 
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

[Serializable]
public static class QuadSettings
{
    private static int TextureScaleModifier { get { return 2; } }

    public static int BorderModMesh { get { return 3; } }
    public static int BorderModTexture { get { return 7; } }

    public static int VerticesPerSideWithBorder { get { return 64; } }
    public static int VerticesPerSide { get { return 60; } }
    public static int VerticesPerSideWithBorderFull { get { return VerticesPerSideWithBorder * TextureScaleModifier; } }
    public static int VerticesPerSideFull { get { return VerticesPerSide * TextureScaleModifier; } }

    public static int VerticesWithBorder { get { return VerticesPerSideWithBorder * VerticesPerSideWithBorder; } }
    public static int Vertices { get { return VerticesPerSide * VerticesPerSide; } }
    public static int VerticesWithBorderFull { get { return VerticesPerSideWithBorderFull * VerticesPerSideWithBorderFull; } }
    public static int VerticesFull { get { return VerticesPerSideFull * VerticesPerSideFull; } }

    public static float Spacing { get { return 2.0f / (VerticesPerSide - 1.0f); } }
    public static float SpacingFull { get { return 2.0f / (VerticesPerSideFull - 1.0f); } }

    private static int THREADS_PER_GROUP { get { return 30; } }
    private static int THREADS_PER_GROUP_FULL { get { return 32; } }

    public static int THREADGROUP_SIZE { get { return VerticesPerSide / THREADS_PER_GROUP; } }

    public static int THREADGROUP_SIZE_BORDER { get { return VerticesPerSideWithBorder / THREADS_PER_GROUP_FULL; } }

    public static int THREADGROUP_SIZE_FULL { get { return VerticesPerSideFull / THREADS_PER_GROUP; } }

    public static int THREADGROUP_SIZE_BORDER_FULL { get { return VerticesPerSideWithBorderFull / THREADS_PER_GROUP_FULL; } }
}