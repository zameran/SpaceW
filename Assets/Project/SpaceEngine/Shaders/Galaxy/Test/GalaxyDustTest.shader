Shader "SpaceEngine/Galaxy/DustTest"
{
	Properties
	{
		
	}
	SubShader
	{
		Tags { "PreviewType" = "Plane" }

		CGINCLUDE
		#include "../Galaxy.cginc"
		
		uniform StructuredBuffer<GalaxyStar> stars;
		
		uniform float3 _Galaxy_Position;
				
		uniform float4x4 _Globals_WorldToCamera;
		uniform float4x4 _Globals_CameraToScreen;

		uniform sampler2D _Particle;
		
		struct appdata
		{
			float4 vertex : POSITION;
			
			uint instanceId : SV_InstanceID;
		};
		
		struct v2f
		{
			float4 vertex : SV_POSITION;
			float4 color : TEXCOORD0;
			float2 data : TECOORD1;
			float3 direction : TEXCOORD2;
			float3 rayEnd : TEXCOORD3;
			float3 rayStart : TEXCOORD4;
			float3 position : TEXCOORD5;
		};
		
		void PackStar(in appdata v, inout StructuredBuffer<GalaxyStar> buffer, in float size, out v2f o)
		{
			uint id = v.instanceId;
		
			float3 starPosition = buffer[id].position;
			float4 starColor = buffer[id].color;
			float starSize = buffer[id].size * size;
			float starTemperature = buffer[id].temperature;
			
			float3 localPosition = v.vertex.xyz * starSize;
			float3 worldPosition = starPosition + localPosition;
			float3 relativePosition = _Galaxy_Position + worldPosition;
			
			o.vertex = mul(mul(_Globals_CameraToScreen, _Globals_WorldToCamera), float4(worldPosition, 1.0f));
			o.vertex.z = o.vertex.z * o.vertex.w * 0.0000000000001;
			o.color = lerp(starColor, GetMaterial(starTemperature), 0.5f);
			o.data = float2(starSize, starSize / 2.0f);
			o.direction = relativePosition;
			o.rayEnd = localPosition;
			o.rayStart = relativePosition;
			o.position = starPosition;
		}

		void UnpackStar(in v2f i, in float stength, out float4 outputColor)
		{
			float4 starColor = i.color;
			float starSize = i.data.y;
			float3 direction = normalize(i.direction);
			
			float tx = (direction.x > 0.0f ? starSize + i.rayEnd.x : starSize - i.rayEnd.x) / abs(direction.x);
			float ty = (direction.y > 0.0f ? starSize + i.rayEnd.y : starSize - i.rayEnd.y) / abs(direction.y);
			float tz = (direction.z > 0.0f ? starSize + i.rayEnd.z : starSize - i.rayEnd.z) / abs(direction.z);
			float t = min(min(tx, ty), tz);
			float rayDistance = length(i.rayStart);
			float dt = (rayDistance < t ? rayDistance : t);
			
			float hts = 0.0f;
			float hte = 0.0f;
			
			bool hit = RaySphereIntersect(i.rayEnd - (direction * dt), direction, starSize, hts, hte);
			
			UNITY_BRANCH
			if (!hit) discard;
			
			float rlen = hte - hts;
			float rlenn = 0.5f * rlen / starSize;

			float alpha = saturate(rlenn * rlenn);
			
			starColor *= alpha;
			starColor *= stength;
			starColor = clamp(starColor, -1.0f, 1.0f);

			outputColor = float4(starColor.xyz, alpha);
		}

		void vert_dust(in appdata v, out v2f o)
		{
			PackStar(v, stars, dustSize, o);
		}

		void vert_gas(in appdata v, out v2f o)
		{
			PackStar(v, stars, gasSize, o);
		}
		
		void frag_dust(in v2f i, out float4 color : SV_Target)
		{
			float4 starColor;

			UnpackStar(i, dustStrength, starColor);
			
			color = float4(starColor.rgb, starColor.a);
		}

		void frag_gas(in v2f i, out float4 color : SV_Target)
		{
			float4 starColor;

			UnpackStar(i, gasStrength, starColor);
			
			starColor.a *= smoothstep(-1.0f, gasCenterFalloff, length(i.position) - gasCenterFalloff);

			color = float4(-starColor.rgb, starColor.a);
		}
		ENDCG

		Pass
		{
			Name "Dust"
			Tags 
			{
				"Queue"					= "Transparent"
				"RenderType"			= "Transparent"
				"ForceNoShadowCasting"	= "True"
				"IgnoreProjector"		= "True"

				"LightMode"				= "Always"
			}

			Blend OneMinusDstColor One
			Cull Front
			ZWrite On
			ZTest Always

			CGPROGRAM
			#pragma target 5.0
			#pragma fragmentoption arb_precision_hint_fastest
			#pragma vertex vert_dust
			#pragma fragment frag_dust
			ENDCG
		}

		Pass
		{
			Name "Dust (Gas)"
			Tags 
			{
				"Queue"					= "Transparent"
				"RenderType"			= "Transparent"
				"ForceNoShadowCasting"	= "True"
				"IgnoreProjector"		= "True"

				"LightMode"				= "Always"
			}

			Blend SrcAlpha One
			Cull Front
			ZWrite On
			ZTest Always

			CGPROGRAM
			#pragma target 5.0
			#pragma fragmentoption arb_precision_hint_fastest
			#pragma vertex vert_gas
			#pragma fragment frag_gas
			ENDCG
		}
	}
}
