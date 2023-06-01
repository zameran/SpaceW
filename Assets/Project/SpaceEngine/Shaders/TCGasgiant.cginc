/* Procedural planet generator.
 *
 * Copyright (C) 2012-2015 Vladimir Romanyuk
 * All rights reserved.
 *
 * Modified and ported to Unity by Denis Ovchinnikov 2015-2017
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */

/*
 * Author: Vladimir Romanyuk
 * Modified and ported to Unity by Denis Ovchinnikov 2015-2023
 */

#if !defined (TCCOMMON)
#include "TCCommon.cginc"
#endif

//-----------------------------------------------------------------------------
// TODO : Fix gas giants! Looks like some shit with points...
float HeightMapCloudsGasGiant(float3 ppoint)
{
	if (cloudsLayer == 0.0)
	{
		return HeightMapCloudsGasGiantCore(ppoint);
	}
	else
	{
		return 0.0;
	}
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float HeightMapFogGasGiant(float3 ppoint)
{
	return 0.75 + 0.3 * Noise(ppoint * float3(0.2, 6.0, 0.2));
}

float4 ColorMapCloudsGasGiant(float3 ppoint, float height, float slope)
{
	if (cloudsLayer == 0.0)
	{
		float3 color = height * GetGasGiantCloudsColor(height).rgb;

		return float4(color, 5.0 * dot(color.rgb, float3(0.299, 0.587, 0.114)));
	}
	else
	{
		return float4(HeightMapFogGasGiant(ppoint) * GetGasGiantCloudsColor(1.0).rgb, 1.0);
	}
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float4 GlowMapCloudsGasGiant(float3 ppoint, float height, float slope)
{
	return float4(UnitToColor24(log((1.0 - 0.2 * height) * surfTemperature) * 0.188 + 0.1316), 1.0);
}
//-----------------------------------------------------------------------------