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

#define MATH

#if !defined (M_PI)
#define M_PI 3.14159265358
#endif

#if !defined (M_PI2)
#define M_PI2 6.28318530716
#endif

float2 Complex(float2 z) 
{
	return float2(-z.y, z.x); // returns i times z (complex number)
}

float2 GetSpectrum(float t, float w, float2 s0, float2 s0c) 
{
	float c = cos(w * t);
	float s = sin(w * t);

	return float2((s0.x + s0c.x) * c - (s0.y + s0c.y) * s, (s0.x - s0c.x) * s + (s0.y - s0c.y) * c);
}

float IntersectInnerSphere(float3 p1, float3 d, float3 p3, float r)
{
	float a = dot(d, d);
	float b = 2.0 * dot(d, p1 - p3);
	float c = dot(p3, p3) + dot(p1, p1) - 2.0 * dot(p3, p1) - r * r;
	float test = b * b - 4.0 * a * c;

	if (test < 0) return -1.0;

	float u = (-b - sqrt(test)) / (2.0 * a);	
								
	return u;
}

float IntersectOuterSphere(float3 p1, float3 d, float3 p3, float r)
{
	// p1 starting point
	// d look direction
	// p3 is the sphere center

	float a = dot(d, d);
	float b = 2.0 * dot(d, p1 - p3);
	float c = dot(p3, p3) + dot(p1, p1) - 2.0 * dot(p3, p1) - r * r;
	float test = b * b - 4.0 * a * c;

	if (test < 0) return -1.0;

	float u = (-b - sqrt(test)) / (2.0 * a);

	u = (u < 0) ? (-b + sqrt(test)) / (2.0 * a) : u;
			
	return u;
}

float IntersectOuterSphereInverted(float3 p1, float3 d, float3 p3, float r)
{
	// p1 starting point
	// d look direction
	// p3 is the sphere center

	float a = dot(d, d);
	float b = 2.0 * dot(d, p1 - p3);
	float c = dot(p3, p3) + dot(p1, p1) - 2.0 * dot(p3, p1) - r * r;
	float test = b * b - 4.0 * a * c;

	if (test < 0) return -1.0;
			
	return (-b + sqrt(test)) / (2.0 * a);
}