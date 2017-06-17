Shader "SpaceEngine/Ocean/Ocean" 
{
	SubShader 
	{
		CGINCLUDE

		#include "../HDR.cginc"
		#include "../Atmosphere.cginc"
		#include "OceanBRDF.cginc"
		#include "OceanDisplacement.cginc"

		uniform float3 _Ocean_Color;

		struct a2v
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f 
		{
			float4 pos : SV_POSITION;
			float2 oceanU : TEXCOORD0;
			float3 oceanP : TEXCOORD1;
		};

		void vert(in a2v v, out v2f o)
		{
			float t = 0;
			float3 cameraDir = 0;
			float3 oceanDir = 0;
			
			float4 vertex = float4(v.vertex.xy * 1.25, v.vertex.zw);

			float2 u = OceanPos(vertex, _Globals_ScreenToCamera, t, cameraDir, oceanDir);
			float2 dux = OceanPos(vertex + float4(_Ocean_ScreenGridSize.x, 0.0, 0.0, 0.0), _Globals_ScreenToCamera) - u;
			float2 duy = OceanPos(vertex + float4(0.0, _Ocean_ScreenGridSize.y, 0.0, 0.0), _Globals_ScreenToCamera) - u;
			float3 dP = float3(0.0, 0.0, _Ocean_HeightOffset);
				
			if(duy.x != 0.0 || duy.y != 0.0) 
			{
				float4 GRID_SIZES = _Ocean_GridSizes;
				float4 CHOPPYNESS = _Ocean_Choppyness;
					
				dP.z += Tex2DGrad(_Ocean_Map0, u / GRID_SIZES.x, dux / GRID_SIZES.x, duy / GRID_SIZES.x, _Ocean_MapSize).x;
				dP.z += Tex2DGrad(_Ocean_Map0, u / GRID_SIZES.y, dux / GRID_SIZES.y, duy / GRID_SIZES.y, _Ocean_MapSize).y;
				dP.z += Tex2DGrad(_Ocean_Map0, u / GRID_SIZES.z, dux / GRID_SIZES.z, duy / GRID_SIZES.z, _Ocean_MapSize).z;
				dP.z += Tex2DGrad(_Ocean_Map0, u / GRID_SIZES.w, dux / GRID_SIZES.w, duy / GRID_SIZES.w, _Ocean_MapSize).w;
					
				dP.xy += CHOPPYNESS.x * Tex2DGrad(_Ocean_Map3, u / GRID_SIZES.x, dux / GRID_SIZES.x, duy / GRID_SIZES.x, _Ocean_MapSize).xy;
				dP.xy += CHOPPYNESS.y * Tex2DGrad(_Ocean_Map3, u / GRID_SIZES.y, dux / GRID_SIZES.y, duy / GRID_SIZES.y, _Ocean_MapSize).zw;
				dP.xy += CHOPPYNESS.z * Tex2DGrad(_Ocean_Map4, u / GRID_SIZES.z, dux / GRID_SIZES.z, duy / GRID_SIZES.z, _Ocean_MapSize).xy;
				dP.xy += CHOPPYNESS.w * Tex2DGrad(_Ocean_Map4, u / GRID_SIZES.w, dux / GRID_SIZES.w, duy / GRID_SIZES.w, _Ocean_MapSize).zw;
			}
				
			float3x3 otoc = _Ocean_OceanToCamera;

			#ifdef OCEAN_ONLY_SPHERICAL
				float tClamped = clamp(t * 0.25, 0.0, 1.0);
				dP = lerp(float3(0.0, 0.0, -0.1), dP, tClamped);
			#endif

			float4 screenP = float4(t * cameraDir + mul(otoc, dP), 1.0);
			float3 oceanP = t * oceanDir + dP + float3(0.0, 0.0, _Ocean_CameraPos.z);
				
			o.pos = mul(_Globals_CameraToScreen, screenP);
			o.oceanU = u;
			o.oceanP = oceanP;
		}

		void frag(in v2f i, out float4 color : SV_Target)
		{
			float3 L = _Ocean_SunDir;
			float radius = _Ocean_Radius;
			float2 u = i.oceanU;
			float3 oceanP = i.oceanP;
			
			float3 earthCamera = float3(0.0, 0.0, _Ocean_CameraPos.z + radius);

			// NOTE : Vertices some times pass or lay under atmosphere 'ground terminator' and InScattering returns solid radiance color...
			#ifdef OCEAN_INSCATTER_FIX
				float3 earthP = radius > 0.0 ? normalize(oceanP + float3(0.0, 0.0, radius)) * (radius + 11.0) : oceanP;
			#else
				float3 earthP = radius > 0.0 ? normalize(oceanP + float3(0.0, 0.0, radius)) * (radius + 10.0) : oceanP;
			#endif
				
			float3 oceanCamera = float3(0.0, 0.0, _Ocean_CameraPos.z);
			float3 V = normalize(oceanCamera - oceanP);
			
			float2 slopes = float2(0.0, 0.0);
			slopes += tex2D(_Ocean_Map1, u / _Ocean_GridSizes.x).xy;
			slopes += tex2D(_Ocean_Map1, u / _Ocean_GridSizes.y).zw;
			slopes += tex2D(_Ocean_Map2, u / _Ocean_GridSizes.z).xy;
			slopes += tex2D(_Ocean_Map2, u / _Ocean_GridSizes.w).zw;
							
			float3 N = normalize(float3(-slopes.x, -slopes.y, 1.0));
			
			if (radius > 0.0) { slopes -= oceanP.xy / (radius + oceanP.z); }

			// Reflects backfacing normals
			if (dot(V, N) < 0.0) { N = reflect(N, V); }
				
			float Jxx = ddx(u.x);
			float Jxy = ddy(u.x);
			float Jyx = ddx(u.y);
			float Jyy = ddy(u.y);
			float A = Jxx * Jxx + Jyx * Jyx;
			float B = Jxx * Jxy + Jyx * Jyy;
			float C = Jxy * Jxy + Jyy * Jyy;
			
			const float SCALE = 10.0;

			float ua = pow(A / SCALE, 0.25);
			float ub = 0.5 + 0.5 * B / sqrt(A * C);
			float uc = pow(C / SCALE, 0.25);
			float sigmaSq = tex3D(_Ocean_Variance, float3(ua, ub, uc)).x;

			sigmaSq = max(sigmaSq, 2e-5);
			
			float3 sunL;
			float3 skyE;
			float3 extinction;

			SunRadianceAndSkyIrradiance(earthP, N, L, sunL, skyE);

			#ifdef OCEAN_SKY_REFLECTIONS_ON
				float fresnel = MeanFresnel(V, N, sigmaSq);
				float3 Lsky = fresnel * ReflectedSky(V, N, L, earthP);
				float3 Lsun = ReflectedSunRadiance(L, V, N, sigmaSq) * sunL;
				float3 Lsea = 0.98 * (1.0 - fresnel) * _Ocean_Color * (skyE / M_PI);
			#else
				float3 Lsky = MeanFresnel(V, N, sigmaSq) * skyE / M_PI;
				float3 Lsun = ReflectedSunRadiance(L, V, N, sigmaSq) * sunL;
				float3 Lsea = RefractedSeaRadiance(V, N, sigmaSq) * _Ocean_Color * skyE / M_PI;
			#endif

			float3 surfaceColor = 0;
			float surfaceAlpha = 1;

			// TODO : Sample scene depth for ocean transparency...

			#ifdef OCEAN_FFT
				surfaceColor = Lsun + Lsky + Lsea;
				surfaceAlpha = min(max(hdr(Lsun), fresnel + surfaceAlpha), 1.0);
			#endif
					
			#ifdef OCEAN_WHITECAPS
				// Extract mean and variance of the jacobian matrix determinant
				float2 jm1 = tex2D(_Ocean_Foam0, u / _Ocean_GridSizes.x).xy;
				float2 jm2 = tex2D(_Ocean_Foam0, u / _Ocean_GridSizes.y).zw;
				float2 jm3 = tex2D(_Ocean_Foam1, u / _Ocean_GridSizes.z).xy;
				float2 jm4 = tex2D(_Ocean_Foam1, u / _Ocean_GridSizes.w).zw;
				float2 jm  = jm1 + jm2 + jm3 + jm4;

				float jSigma2 = max(jm.y - (jm1.x * jm1.x + jm2.x * jm2.x + jm3.x * jm3.x + jm4.x * jm4.x), 0.0);

				// Get coverage
				// Modulated...
				//float noiseValue = Noise(float3(u * 0.01, 0)) * _Ocean_WhiteCapStr;
				//float whiteCapStr = clamp((noiseValue + 1.0) * 0.5, 0.0, 1.0);
				//float W = WhitecapCoverage(whiteCapStr, jm.x, jSigma2);

				// Simple...
				float W = WhitecapCoverage(_Ocean_WhiteCapStr, jm.x, jSigma2);
				
				// Compute and add whitecap radiance
				float3 l = (sunL * (max(dot(N, L), 0.0)) + skyE) / M_PI;
				//float3 l = (sunL * (max(dot(N, L), 0.0)) + skyE + UNITY_LIGHTMODEL_AMBIENT.rgb * 30) / M_PI;

				float3 R_ftot = float3(W * l * 0.4);
				
				surfaceColor = Lsun + Lsky + Lsea + R_ftot;
				surfaceAlpha = min(max(hdr(Lsun + R_ftot), fresnel + surfaceAlpha), 1.0);
			#endif

			// Aerial perspective
			float3 inscatter = InScattering(earthCamera, earthP, L, extinction, 0.0);
			float3 finalColor = surfaceColor * extinction + inscatter;
			
			color = float4(hdr(finalColor), surfaceAlpha);
		}
		ENDCG

		Pass
		{	
			Name "Ocean"
			Tags 
			{
				"Queue"					= "Geometry+100"
				"RenderType"			= "Geometry"
				"ForceNoShadowCasting"	= "True"
				"IgnoreProjector"		= "True"

				"LightMode"				= "Always"
			}

			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			Lighting Off
			ZWrite Off
			ZTest LEqual
			Fog { Mode Off }

			CGPROGRAM
			#pragma target 5.0
			#pragma only_renderers d3d11 glcore
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile OCEAN_SKY_REFLECTIONS_ON OCEAN_SKY_REFLECTIONS_OFF
			#pragma multi_compile OCEAN_NONE OCEAN_FFT OCEAN_WHITECAPS
			#pragma multi_compile OCEAN_INSCATTER_FIX
			ENDCG
		}
	}
}