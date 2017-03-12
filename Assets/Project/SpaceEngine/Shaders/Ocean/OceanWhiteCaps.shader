Shader "SpaceEngine/Ocean/OceanWhiteCaps" 
{
	SubShader 
	{
		Tags { "Queue" = "Geometry+100" "RenderType"="" }
	
		Pass 
		{	
			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma target 5.0
			#pragma only_renderers d3d11 glcore
			#pragma vertex vert
			#pragma fragment frag

			#define OCEAN_INSCATTER_FIX
			
			#include "../HDR.cginc"
			#include "../Atmosphere.cginc"
			#include "OceanBRDF.cginc"
			#include "OceanDisplacement.cginc"
			
			uniform float4x4 _Globals_WorldToScreen;

			uniform float2 _Ocean_MapSize;
			uniform float4 _Ocean_Choppyness;
			uniform float3 _Ocean_SunDir;
			uniform float3 _Ocean_Color;
			uniform float4 _Ocean_GridSizes;
			uniform float2 _Ocean_ScreenGridSize;
			uniform float _Ocean_WhiteCapStr;
			uniform sampler3D _Ocean_Variance;
			uniform sampler2D _Ocean_Map0;
			uniform sampler2D _Ocean_Map1;
			uniform sampler2D _Ocean_Map2;
			uniform sampler2D _Ocean_Map3;
			uniform sampler2D _Ocean_Map4;
			uniform sampler2D _Ocean_Foam0;
			uniform sampler2D _Ocean_Foam1;
			
			uniform sampler2D _Sky_Map;
			
			struct v2f 
			{
				float4 pos : SV_POSITION;
				float2 oceanU : TEXCOORD0;
				float3 oceanP : TEXCOORD1;
			};
		
			v2f vert(appdata_base v)
			{
				float t;
				float3 cameraDir, oceanDir;
				
				float4 vert = v.vertex;
				vert.xy *= 1.25;
			
				float2 u = OceanPos(vert, _Globals_ScreenToCamera, t, cameraDir, oceanDir);
				float2 dux = OceanPos(vert + float4(_Ocean_ScreenGridSize.x, 0.0, 0.0, 0.0), _Globals_ScreenToCamera) - u;
				float2 duy = OceanPos(vert + float4(0.0, _Ocean_ScreenGridSize.y, 0.0, 0.0), _Globals_ScreenToCamera) - u;
				float3 dP = float3(0, 0, _Ocean_HeightOffset);
				
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

				v2f OUT;
				
				float3x3 otoc = _Ocean_OceanToCamera;
				float4 screenP = float4(t * cameraDir + mul(otoc, dP), 1.0);
				float3 oceanP = t * oceanDir + dP + float3(0.0, 0.0, _Ocean_CameraPos.z);
				
				OUT.pos = mul(_Globals_CameraToScreen, screenP);
				OUT.oceanU = u;
				OUT.oceanP = oceanP;
				 
				return OUT;
			}
			
			float4 frag(v2f IN) : COLOR
			{	
				float3 L = _Ocean_SunDir;
				float radius = _Ocean_Radius;
				float2 u = IN.oceanU;
				float3 oceanP = IN.oceanP;
				
				float3 earthCamera = float3(0.0, 0.0, _Ocean_CameraPos.z + radius);

				// NOTE : Vertices some times pass or lay under atmosphere 'ground terminator' and InScattering returns solid radiance color...
				#ifdef OCEAN_INSCATTER_FIX
					float3 earthP = radius > 0.0 ? normalize(oceanP + float3(0.0, 0.0, radius)) * (radius + 11.0) : oceanP;
				#else
					float3 earthP = radius > 0.0 ? normalize(oceanP + float3(0.0, 0.0, radius)) * (radius + 10.0) : oceanP;
				#endif
				
				float3 oceanCamera = float3(0.0, 0.0, _Ocean_CameraPos.z);
				float3 V = normalize(oceanCamera - oceanP);
			
				float2 slopes = float2(0,0);
				slopes += tex2D(_Ocean_Map1, u / _Ocean_GridSizes.x).xy;
				slopes += tex2D(_Ocean_Map1, u / _Ocean_GridSizes.y).zw;
				slopes += tex2D(_Ocean_Map2, u / _Ocean_GridSizes.z).xy;
				slopes += tex2D(_Ocean_Map2, u / _Ocean_GridSizes.w).zw;
				
				if (radius > 0.0) 
				{
					slopes -= oceanP.xy / (radius + oceanP.z);
				}
				
				float3 N = normalize(float3(-slopes.x, -slopes.y, 1.0));
				
				if (dot(V, N) < 0.0) 
				{
					N = reflect(N, V); // reflects backfacing normals
				}
				
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
				
				float3 Lsky;

				if(radius == 0.0)
				{
					Lsky = ReflectedSkyRadiance(_Sky_Map, V, N, sigmaSq, L);
				}
				else
				{
					Lsky = MeanFresnel(V, N, sigmaSq) * skyE / M_PI;
				}
				
				float3 Lsun = ReflectedSunRadiance(L, V, N, sigmaSq) * sunL;
				float3 Lsea = RefractedSeaRadiance(V, N, sigmaSq) * _Ocean_Color * skyE / M_PI;
				
				// extract mean and variance of the jacobian matrix determinant
				float2 jm1 = tex2D(_Ocean_Foam0, u / _Ocean_GridSizes.x).xy;
				float2 jm2 = tex2D(_Ocean_Foam0, u / _Ocean_GridSizes.y).zw;
				float2 jm3 = tex2D(_Ocean_Foam1, u / _Ocean_GridSizes.z).xy;
				float2 jm4 = tex2D(_Ocean_Foam1, u / _Ocean_GridSizes.w).zw;
				float2 jm  = jm1 + jm2 + jm3 + jm4;

				float jSigma2 = max(jm.y - (jm1.x * jm1.x + jm2.x * jm2.x + jm3.x * jm3.x + jm4.x * jm4.x), 0.0);

				// get coverage
				float W = WhitecapCoverage(_Ocean_WhiteCapStr, jm.x, jSigma2);
				
				// compute and add whitecap radiance
				float3 l = (sunL * (max(dot(N, L), 0.0)) + skyE) / M_PI;
				float3 R_ftot = float3(W * l * 0.4);
				
				float3 surfaceColor = Lsun + Lsky + Lsea + R_ftot;
				
				// aerial perspective
				float3 inscatter = InScattering(earthCamera, earthP, L, extinction, 0.0);
				float3 finalColor = surfaceColor * extinction + inscatter;
				
				return float4(hdr(finalColor), 1);
			}
			
			ENDCG
		}
	}
}