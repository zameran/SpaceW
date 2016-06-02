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
// Creation Date: 2016.05.15
// Creation Time: 21:25
// Creator: zameran
#endregion

using UnityEngine;

using ZFramework.Unity.Common.Types;

public static class CPUSpaceUtils
{
	public static void HeightMain(QuadGenerationConstants constants, int nVerticesPerSideWithBorder, out OutputStruct[] patchPreOutput)
	{
		patchPreOutput = new OutputStruct[nVerticesPerSideWithBorder * nVerticesPerSideWithBorder];

		Vector3i id = Vector3i.Zero;

		for (int x = id.X; x < nVerticesPerSideWithBorder; x++)
		{
			for (int y = id.Y; y < nVerticesPerSideWithBorder; y++)
			{
				int outBuffOffset = id.X + id.Y * nVerticesPerSideWithBorder;

				Vector3 cubeCoord = CubeCoord(constants, nVerticesPerSideWithBorder, id, (int)constants.borderMod.x, constants.spacing);

				Vector3 patchCenter = constants.patchCubeCenter.normalized * constants.planetRadius;
				Vector3 patchNormalizedCoord = cubeCoord.normalized;

				Vector3 patchCoord = constants.planetRadius * patchNormalizedCoord;
				Vector3 patchCoordCentered = patchCoord - patchCenter;
				Vector3 patchCubeCoordCentered = patchNormalizedCoord;
				Vector3 patchCubeCoordCenteredFlat = patchNormalizedCoord;

				float noise = 1;

				float height = noise * constants.terrainMaxHeight;

				patchCoordCentered += patchNormalizedCoord * height;
				patchCubeCoordCentered += patchNormalizedCoord * height;
				patchCubeCoordCenteredFlat += patchNormalizedCoord;

				Vector4 output = new Vector4(patchCoordCentered.x, patchCoordCentered.y, patchCoordCentered.z, 0.0f);
				Vector4 cubeOutput = new Vector4(patchCubeCoordCentered.x, patchCubeCoordCentered.y, patchCubeCoordCentered.z, 0.0f);

				patchPreOutput[outBuffOffset].noise = noise;
				patchPreOutput[outBuffOffset].patchCenter = patchCenter;
				patchPreOutput[outBuffOffset].position = output;
				patchPreOutput[outBuffOffset].cubePosition = cubeOutput;
			}
		}
	}

	public static void Transfer(int VerticesPerSide, int VerticesPerSideWithBorder, OutputStruct[] patchPreOutput, out OutputStruct[] patchOutput)
	{
		patchOutput = new OutputStruct[VerticesPerSide * VerticesPerSide];

		Vector3i id = Vector3i.Zero;

		for (int x = id.X; x < VerticesPerSideWithBorder; x++)
		{
			for (int y = id.Y; y < VerticesPerSideWithBorder; y++)
			{
				int inBuffOffset = (id.X + 1) + (id.Y + 1) * VerticesPerSideWithBorder;
				int outBuffOffset = id.X + id.Y * VerticesPerSide;

				patchOutput[outBuffOffset] = patchPreOutput[inBuffOffset];
			}
		}
	}

	public static Vector3 CubeCoord(QuadGenerationConstants constants, int VerticesPerSide, Vector3i id, int mod, float spacing)
	{
		double eastValue = (id.X - ((VerticesPerSide - mod) * 0.5)) * spacing;
		double northValue = (id.Y - ((VerticesPerSide - mod) * 0.5)) * spacing;

		Vector3 cubeCoordEast = constants.cubeFaceEastDirection * (float)eastValue;
		Vector3 cubeCoordNorth = constants.cubeFaceNorthDirection * (float) northValue;

		Vector3 cubeCoord = cubeCoordEast + cubeCoordNorth + constants.patchCubeCenter;

		return cubeCoord;
	}
}