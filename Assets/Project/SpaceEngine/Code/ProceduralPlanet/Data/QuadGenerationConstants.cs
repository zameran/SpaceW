﻿#region License
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
// Creation Date: 2016.05.15
// Creation Time: 21:12
// Creator: zameran
#endregion

using System;
using UnityEngine;

[Serializable]
public struct QuadGenerationConstants
{
    public float planetRadius; //4
    public float spacing; //4
    public float spacingreal;
    public float spacingsub;
    public float terrainMaxHeight; //4
    public float LODLevel; //4
    public float LODOctaveModifier;
    public float orientation;

    //x - nVerticesPerSide
    //y - nVerticesPerSideWithBorder
    //z - nVerticesPerSideSub
    //w - nVerticesPerSideWithBorderSub
    public Vector4 meshSettings;

    public Vector3 cubeFaceEastDirection; //12
    public Vector3 cubeFaceNorthDirection; //12
    public Vector3 patchCubeCenter; //12

    //12 * 3 + 4 * 3 = 36 + 12 = 48

    public static QuadGenerationConstants Init()
    {
        QuadGenerationConstants temp = new QuadGenerationConstants();

        temp.meshSettings = new Vector4(QuadSettings.nVertsPerEdge, QuadSettings.nVertsPerEdgeReal, QuadSettings.nVertsPerEdgeSub, QuadSettings.nVertsPerEdgeSubReal);

        temp.spacing = QuadSettings.nSpacing;
        temp.spacingreal = QuadSettings.nSpacingReal;
        temp.spacingsub = QuadSettings.nSpacingSub;
        temp.terrainMaxHeight = 64.0f;

        return temp;
    }

    public static QuadGenerationConstants Init(float terrainMaxHeight)
    {
        QuadGenerationConstants temp = new QuadGenerationConstants();

        temp.meshSettings = new Vector4(QuadSettings.nVertsPerEdge, QuadSettings.nVertsPerEdgeReal, QuadSettings.nVertsPerEdgeSub, QuadSettings.nVertsPerEdgeSubReal);

        temp.spacing = QuadSettings.nSpacing;
        temp.spacingreal = QuadSettings.nSpacingReal;
        temp.spacingsub = QuadSettings.nSpacingSub;
        temp.terrainMaxHeight = terrainMaxHeight;

        return temp;
    }
}