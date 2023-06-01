//-----------------------------------------------------------------------------
#define USESAVEPOW
//-----------------------------------------------------------------------------

uniform sampler2D PermSampler; // 3d
uniform sampler2D PermGradSampler; // 3d

//-----------------------------------------------------------------------------
inline float SavePow(float f, float p)
{
    #ifdef USESAVEPOW
    return pow(abs(f), p);
    #else
        return pow(f, p);
    #endif
}

//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
inline float2 Interpolation_C2(float2 x) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }
inline float3 Interpolation_C2(float3 x) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }
inline float2 Interpolation_C2_Deriv(float2 x) { return x * x * (x * (x * 30.0 - 60.0) + 30.0); }
inline float3 Interpolation_C2_Deriv(float3 x) { return x * x * (x * (x * 30.0 - 60.0) + 30.0); }
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
const float3 vyd = float3(3.33, 5.71, 1.96);
const float3 vzd = float3(7.77, 2.65, 4.37);
const float3 vwd = float3(1.13, 2.73, 6.37);
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// 3D Perlin noise
float Noise(float3 p)
{
    const float one = 1.0 / 256.0;

    // Find unit cube that contains point
    // Find relative x,y,z of point in cube
    float3 P = fmod(floor(p), 256.0) * one;
    p -= floor(p);

    // Compute fade curves for each of x,y,z
    float3 ff = Interpolation_C2(p);

    // Hash coordinates of the 8 cube corners
    // NOTE : I FUCKING DID IT! I FIX DAT FORMULA FOR 3D!
    // Solution is "+ P.z"
    float4 AA = tex2Dlod(PermSampler, float4(P.xy, 0.0, 0.0)) + P.z;

    float a = dot(tex2Dlod(PermGradSampler, AA.x).rgb, p);
    float b = dot(tex2Dlod(PermGradSampler, AA.z).rgb, p + float3(-1.0, 0.0, 0.0));
    float c = dot(tex2Dlod(PermGradSampler, AA.y).rgb, p + float3(0.0, -1.0, 0.0));
    float d = dot(tex2Dlod(PermGradSampler, AA.w).rgb, p + float3(-1.0, -1.0, 0.0));
    float e = dot(tex2Dlod(PermGradSampler, AA.x + one).rgb, p + float3(0.0, 0.0, -1.0));
    float f = dot(tex2Dlod(PermGradSampler, AA.z + one).rgb, p + float3(-1.0, 0.0, -1.0));
    float g = dot(tex2Dlod(PermGradSampler, AA.y + one).rgb, p + float3(0.0, -1.0, -1.0));
    float h = dot(tex2Dlod(PermGradSampler, AA.w + one).rgb, p + float3(-1.0, -1.0, -1.0));

    float k0 = a;
    float k1 = b - a;
    float k2 = c - a;
    float k3 = e - a;
    float k4 = a - b - c + d;
    float k5 = a - c - e + g;
    float k6 = a - b - e + f;
    float k7 = -a + b + c - d + e - f - g + h;

    return k0 + k1 * ff.x + k2 * ff.y + k3 * ff.z + k4 * ff.x * ff.y + k5 * ff.y * ff.z + k6 * ff.z * ff.x + k7 * ff.x * ff.y * ff.z;
}

//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// 3D Perlin noise with derivatives, returns vec4(xderiv, yderiv, zderiv, noise)
float4 NoiseDeriv(float3 p)
{
    const float one = 1.0 / 256.0;

    // Find unit cube that contains point
    // Find relative x,y,z of point in cube
    float3 P = fmod(floor(p), 256.0) * one;
    p -= floor(p);

    // Compute fade curves for each of x,y,z
    float3 df = 30.0 * p * p * (p * (p - 2.0) + 1.0);
    float3 ff = Interpolation_C2(p);

    // Hash coordinates of the 8 cube corners
    // NOTE : I FUCKING DID IT! I FIX DAT FORMULA FOR 3D!
    // Solution is "+ P.z"
    float4 AA = tex2Dlod(PermSampler, float4(P.xy, 0.0, 0.0)) + P.z; // <- Here!

    float a = dot(tex2Dlod(PermGradSampler, AA.x).rgb, p);
    float b = dot(tex2Dlod(PermGradSampler, AA.z).rgb, p + float3(-1.0, 0.0, 0.0));
    float c = dot(tex2Dlod(PermGradSampler, AA.y).rgb, p + float3(0.0, -1.0, 0.0));
    float d = dot(tex2Dlod(PermGradSampler, AA.w).rgb, p + float3(-1.0, -1.0, 0.0));
    float e = dot(tex2Dlod(PermGradSampler, AA.x + one).rgb, p + float3(0.0, 0.0, -1.0));
    float f = dot(tex2Dlod(PermGradSampler, AA.z + one).rgb, p + float3(-1.0, 0.0, -1.0));
    float g = dot(tex2Dlod(PermGradSampler, AA.y + one).rgb, p + float3(0.0, -1.0, -1.0));
    float h = dot(tex2Dlod(PermGradSampler, AA.w + one).rgb, p + float3(-1.0, -1.0, -1.0));

    float k0 = a;
    float k1 = b - a;
    float k2 = c - a;
    float k3 = e - a;
    float k4 = a - b - c + d;
    float k5 = a - c - e + g;
    float k6 = a - b - e + f;
    float k7 = -a + b + c - d + e - f - g + h;

    return float4(df.x * (k1 + k4 * ff.y + k6 * ff.z + k7 * ff.y * ff.z),
                  df.y * (k2 + k5 * ff.z + k4 * ff.x + k7 * ff.z * ff.x),
                  df.z * (k3 + k6 * ff.x + k5 * ff.y + k7 * ff.x * ff.y),
                  k0 + k1 * ff.x + k2 * ff.y + k3 * ff.z + k4 * ff.x * ff.y + k5 * ff.y * ff.z + k6 * ff.z * ff.x + k7 * ff.x * ff.y * ff.z);
}

//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float2 NoiseVec2(float3 p) { return float2(Noise(p), Noise(p + vyd)); }
float3 NoiseVec3(float3 p) { return float3(Noise(p), Noise(p + vyd), Noise(p + vzd)); }
float4 NoiseVec4(float3 p) { return float4(Noise(p), Noise(p + vyd), Noise(p + vzd), Noise(p + vwd)); }
float NoiseU(float3 p) { return Noise(p) * 0.5 + 0.5; }
float3 NoiseUVec3(float3 p) { return NoiseVec3(p) * 0.5 + float3(0.5, 0.5, 0.5); }
float4 NoiseUVec4(float3 p) { return NoiseVec4(p) * 0.5 + float4(0.5, 0.5, 0.5, 0.5); }
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float DistNoise(float3 p, float d) { return Noise(p + NoiseVec3(p + 0.5) * d); }
float3 DistNoise3D(float3 p, float d) { return NoiseVec3(p + NoiseVec3(p + 0.5) * d); }
float4 DistNoise4D(float3 p, float d) { return NoiseVec4(p + NoiseVec3(p + 0.5) * d); }
float FiltNoise(float3 p, float w) { return Noise(p) * (1.0 - smoothstep(0.2, 0.6, w)); }
float3 FiltNoise3D(float3 p, float w) { return NoiseVec3(p) * (1.0 - smoothstep(0.2, 0.6, w)); }
float4 FiltNoise4D(float3 p, float w) { return NoiseVec4(p) * (1.0 - smoothstep(0.2, 0.6, w)); }
float FiltDistNoise(float3 p, float w, float d) { return DistNoise(p, d) * (1.0 - smoothstep(0.2, 0.6, w)); }
float3 FiltDistNoise3D(float3 p, float w, float d) { return DistNoise3D(p, d) * (1.0 - smoothstep(0.2, 0.6, w)); }
float4 FiltDistNoise4D(float3 p, float w, float d) { return DistNoise4D(p, d) * (1.0 - smoothstep(0.2, 0.6, w)); }
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float Fbm(float3 ppoint, float noiseLacunarity, float noiseH, int noiseOctaves)
{
    float summ = 0.0;
    float ampl = 1.0;
    float gain = SavePow(noiseLacunarity, -noiseH);

    for (int i = 0; i < noiseOctaves; ++i)
    {
        summ += Noise(ppoint) * ampl;
        ampl *= gain;
        ppoint *= noiseLacunarity;
    }

    return summ;
}

float SwissFbm(float3 ppoint, float gain, float warp, float noiseLacunarity, int noiseOctaves)
{
    float sum = 0.0f;
    float freq = 1.0f;
    float amp = 1.0f;
    float3 deriv = float3(0.0f, 0.0f, 0.0f);

    for (int i = 0; i < noiseOctaves; ++i)
    {
        float4 n = NoiseDeriv((ppoint + warp * deriv) * freq);
        sum += amp * (0.5f - abs(n.w));
        deriv += amp * n.xyz * -n.w;
        freq *= noiseLacunarity;
        amp *= gain * saturate(sum);
    }

    return sum;
}

float RidgedMultifractal(float3 ppoint, float gain, float noiseOffset, float noiseRidgeSmooth, float noiseLacunarity, float noiseH, int noiseOctaves)
{
    float signal = 1.0;
    float summ = 0.0;
    float frequency = 1.0;
    float weight;

    for (int i = 0; i < noiseOctaves; ++i)
    {
        weight = saturate(signal * gain);
        signal = Noise(ppoint * frequency);
        signal = noiseOffset - sqrt(noiseRidgeSmooth + signal * signal);
        signal *= signal * weight;
        summ += signal * SavePow(frequency, -noiseH);
        frequency *= noiseLacunarity;
    }

    return summ;
}

float RidgedMultifractalEroded(float3 ppoint, float gain, float warp, float noiseOffset, float noiseRidgeSmooth, float noiseLacunarity, float noiseH, int noiseOctaves)
{
    float frequency = 1.0;
    float amplitude = 1.0;
    float summ = 0.0;
    float signal = 1.0;
    float weight;
    float3 dsum = float3(0.0, 0.0, 0.0);
    float4 noiseDeriv;

    for (int i = 0; i < noiseOctaves; ++i)
    {
        noiseDeriv = NoiseDeriv((ppoint + warp * dsum) * frequency);
        weight = saturate(signal * gain);
        signal = noiseOffset - sqrt(noiseRidgeSmooth + noiseDeriv.w * noiseDeriv.w);
        signal *= signal * weight;
        amplitude = SavePow(abs(frequency), -noiseH);
        summ += signal * amplitude;
        frequency *= noiseLacunarity;
        dsum -= amplitude * noiseDeriv.xyz * noiseDeriv.w;
    }

    return summ;
}

float RidgedNoise(float3 ppoint)
{
    return 1.0f - abs(Noise(ppoint));
}


//-----------------------------------------------------------------------------
