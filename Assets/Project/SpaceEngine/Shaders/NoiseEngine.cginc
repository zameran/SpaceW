uniform sampler2D PermSampler;
uniform sampler2D PermGradSampler;

uniform float noiseLacunarity;
uniform float noiseH;
uniform float noiseOffset;
uniform float noiseRidgeSmooth;

float Noise2D(float2 p, float seed = 0)
{
	const float one = 1.0 / 256;

	float2 P = fmod(floor(p), 256) * one;
	p -= floor(p);

	// Get weights from the coordinate fraction
	float2 w = p * p * p * (p * (p * 6 - 15) + 10);
	float4 w4 = float4(1, w.x, w.y, w.x * w.y);

	// Get the four randomly permutated indices from the noise lattice nearest to
	// p and offset these numbers with the seed number.
	float4 perm = tex2D(PermSampler, P) + seed;

	// Permutate the four offseted indices again and get the 2D gradient for each
	// of the four permutated coordinates-seed pairs.
	float4 g1 = tex2D(PermGradSampler, perm.xy) * 2 - 1;
	float4 g2 = tex2D(PermGradSampler, perm.zw) * 2 - 1;

	// Evaluate the four lattice gradients at p
	float a = dot(g1.xy, p);
	float b = dot(g2.xy, p + float2(-1,  0));
	float c = dot(g1.zw, p + float2( 0, -1));
	float d = dot(g2.zw, p + float2(-1, -1));

	// Bi-linearly blend between the gradients, using w4 as blend factors.
	float4 grads = float4(a, b - a, c - a, a - b - c + d);
	float n = dot(grads, w4);

	// Return the noise value, roughly normalized in the range [-1, 1]
	return n * 1.5;
}

float3 Noise2DPseudoDeriv(float2 p, float seed = 0)
{
	const float one = 1.0 / 256;

	float2 P = fmod(floor(p), 256) * one;
	p -= floor(p);

	float2 f = p * p * p * (p * (p * 6 - 15) + 10);
	float2 df = p * p * (p * (30 * p - 60) + 30);

	float4 AA = tex2D(PermSampler, P) + seed / 256;
	float4 G1 = tex2D(PermGradSampler, AA.xy) * 2 - 1;
	float4 G2 = tex2D(PermGradSampler, AA.zw) * 2 - 1;

	float a = dot(G1.xy, p);
	float b = dot(G2.xy, p + float2(-1,  0));
	float c = dot(G1.zw, p + float2( 0, -1));
	float d = dot(G2.zw, p + float2(-1, -1));

	float k0 = a;
	float k1 = b - a;
	float k2 = c - a;
	float k3 = a - b - c + d;

	float n = k0 + k1 * f.x + k2 * f.y + k3 * f.x * f.y;

	float dx = df.x * (k1 + k3 * f.y);
	float dy = df.y * (k2 + k3 * f.x);

	return float3(n, dx, dy) * 1.5;
}

float3 Noise2DDeriv(float2 p, float seed = 0)
{
	const float one = 1.0 / 256;

	float2 P = fmod(floor(p), 256) * one;
	p -= floor(p);

	float2 w = p * p * p * (p * (p * 6 - 15) + 10);
	float2 dw = p * p * (p * (p * 30 - 60) + 30);
	float2 dwp = p * p * p * (p * (p * 36 - 75) + 40);

	float4 AA = tex2D(PermSampler, P) + seed / 256;
	float4 G1 = tex2D(PermGradSampler, AA.xy) * 2 - 1;
	float4 G2 = tex2D(PermGradSampler, AA.zw) * 2 - 1;

	float k0 = G1.x * p.x + G1.y * p.y;
	float k1 = (G2.x - G1.x) * p.x + (G2.y - G1.y) * p.y - G2.x;
	float k2 = (G1.z - G1.x) * p.x + (G1.w - G1.y) * p.y - G1.w;
	float k3 = (G1.x - G2.x - G1.z + G2.z) * p.x + (G1.y - G2.y - G1.w + G2.w) * p.y + G2.x + G1.w - G2.z - G2.w; // a - b - c + d

	float n = k0 + k1 * w.x + k2 * w.y + k3 * w.x * w.y;

	float dx = (G1.x + (G1.z - G1.x) * w.y) + ((G2.y - G1.y) * p.y - G2.x + ((G1.y - G2.y - G1.w + G2.w) * p.y + G2.x + G1.w - G2.z - G2.w) * w.y) * dw.x + ((G2.x - G1.x) + (G1.x - G2.x - G1.z + G2.z) * w.y) * dwp.x;
	float dy = (G1.y + (G2.y - G1.y) * w.x) + ((G1.z - G1.x) * p.x - G1.w + ((G1.x - G2.x - G1.z + G2.z) * p.x + G2.x + G1.w - G2.z - G2.w) * w.x) * dw.y + ((G1.w - G1.y) + (G1.y - G2.y - G1.w + G2.w) * w.x) * dwp.y;

	return float3(n, dx, dy) * 1.5;
}

float3 Noise2DDeriv2(float2 p, float seed = 0)
{
	const float one = 1.0 / 256;

	float2 P = fmod(floor(p), 256) * one;
	p -= floor(p);

	float2 f = p * p * p * (p * (p * 6 - 15) + 10);
	float2 ddf = p * (p * (p * 120 - 180) + 60);
	float2 ddfp = p * p * (p * (p * 180 - 300) + 120);

	float4 AA = tex2D(PermSampler, P) + seed / 256;
	float4 G1 = tex2D(PermGradSampler, AA.xy) * 2 - 1;
	float4 G2 = tex2D(PermGradSampler, AA.zw) * 2 - 1;

	float k0 = G1.x * p.x + G1.y * p.y;
	float k1 = (G2.x - G1.x) * p.x + (G2.y - G1.y) * p.y - G2.x;
	float k2 = (G1.z - G1.x) * p.x + (G1.w - G1.y) * p.y - G1.w;
	float k3 = (G1.x - G2.x - G1.z + G2.z) * p.x + (G1.y - G2.y - G1.w + G2.w) * p.y + G2.x + G1.w - G2.z - G2.w; // a - b - c + d

	float n = k0 + k1 * f.x + k2 * f.y + k3 * f.x * f.y;

	float ddx = ((G2.y - G1.y) * p.y - G2.x + ((G1.y - G2.y - G1.w + G2.w) * p.y + G2.x + G1.w - G2.z - G2.w) * f.y) * ddf.x + ((G2.x-G1.x) + (G1.x - G2.x - G1.z + G2.z) * f.y) * ddfp.x;
	float ddy = ((G1.z - G1.x) * p.x - G1.w + ((G1.x - G2.x - G1.z + G2.z) * p.x + G2.x + G1.w - G2.z - G2.w) * f.x) * ddf.y + ((G1.w-G1.y) + (G1.y - G2.y - G1.w + G2.w) * f.x) * ddfp.y;

	return float3(n, ddx, ddy) * 1.5;
}

float Noise3D(float3 p)
{
	const float one = 1.0 / 256;

	// Find unit cube that contains point
	// Find relative x,y,z of point in cube
	float3 P = fmod(floor(p), 256) * one;
	p -= floor(p);

	// Compute fade curves for each of x,y,z
	float3 ff = p * p * p * (p * (p * 6.0 - 15.0) + 10.0);

	// Hash coordinates of the 8 cube corners
	float4 AA = tex2Dlod(PermSampler, float4(P.xyz, 0));

	float a = dot(tex2Dlod(PermGradSampler, AA.x).rgb, p);
	float b = dot(tex2Dlod(PermGradSampler, AA.z).rgb, p + float3(-1, 0, 0));
	float c = dot(tex2Dlod(PermGradSampler, AA.y).rgb, p + float3(0, -1, 0));
	float d = dot(tex2Dlod(PermGradSampler, AA.w).rgb, p + float3(-1, -1, 0));
	float e = dot(tex2Dlod(PermGradSampler, AA.x + one).rgb, p + float3(0, 0, -1));
	float f = dot(tex2Dlod(PermGradSampler, AA.z + one).rgb, p + float3(-1, 0, -1));
	float g = dot(tex2Dlod(PermGradSampler, AA.y + one).rgb, p + float3(0, -1, -1));
	float h = dot(tex2Dlod(PermGradSampler, AA.w + one).rgb, p + float3(-1, -1, -1));

	float k0 = a;
	float k1 = b - a;
	float k2 = c - a;
	float k3 = e - a;
	float k4 = a - b - c + d;
	float k5 = a - c - e + g;
	float k6 = a - b - e + f;
	float k7 = -a + b + c - d + e - f - g + h;

	float result = k0 + k1 * ff.x + k2 * ff.y + k3 * ff.z + k4 * ff.x * ff.y + k5 * ff.y * ff.z + k6 * ff.z * ff.x + k7 * ff.x * ff.y * ff.z;

	return result * 1.5;
}

float4 Noise3DDeriv(float3 p)
{
	const float one = 1.0 / 256;

	// Find unit cube that contains point
	// Find relative x,y,z of point in cube
	float3 P = fmod(floor(p), 256) * one;
	p -= floor(p);

	// Compute fade curves for each of x,y,z
	float3 df = 30.0 * p * p * (p * (p - 2.0) + 1.0);
	float3 ff = p * p * p * (p * (p * 6.0 - 15.0) + 10.0);

	// Hash coordinates of the 8 cube corners
	float4 AA = tex2Dlod(PermSampler, float4(P.xyz, 0));

	float a = dot(tex2Dlod(PermGradSampler, AA.x).rgb, p);
	float b = dot(tex2Dlod(PermGradSampler, AA.z).rgb, p + float3(-1, 0, 0));
	float c = dot(tex2Dlod(PermGradSampler, AA.y).rgb, p + float3(0, -1, 0));
	float d = dot(tex2Dlod(PermGradSampler, AA.w).rgb, p + float3(-1, -1, 0));
	float e = dot(tex2Dlod(PermGradSampler, AA.x + one).rgb, p + float3(0, 0, -1));
	float f = dot(tex2Dlod(PermGradSampler, AA.z + one).rgb, p + float3(-1, 0, -1));
	float g = dot(tex2Dlod(PermGradSampler, AA.y + one).rgb, p + float3(0, -1, -1));
	float h = dot(tex2Dlod(PermGradSampler, AA.w + one).rgb, p + float3(-1, -1, -1));

	float k0 = a;
	float k1 = b - a;
	float k2 = c - a;
	float k3 = e - a;
	float k4 = a - b - c + d;
	float k5 = a - c - e + g;
	float k6 = a - b - e + f;
	float k7 = -a + b + c - d + e - f - g + h;

	float4 result = float4(df.x * (k1 + k4 * ff.y + k6 * ff.z + k7 * ff.y * ff.z),
						   df.y * (k2 + k5 * ff.z + k4 * ff.x + k7 * ff.z * ff.x),
						   df.z * (k3 + k6 * ff.x + k5 * ff.y + k7 * ff.x * ff.y),
						   k0 + k1 * ff.x + k2 * ff.y + k3 * ff.z + k4 * ff.x * ff.y + k5 * ff.y * ff.z + k6 * ff.z * ff.x + k7 * ff.x * ff.y * ff.z);

	return result * 1.5;
}