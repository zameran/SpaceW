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
		uniform StructuredBuffer<GalaxyStar> bulge;
		
		uniform float3 _Galaxy_Position;
		uniform float4 _Galaxy_Orientation;
		uniform float4 _Galaxy_OrientationInverse;
		
		uniform float4x4 _Globals_WorldToCamera;
		uniform float4x4 _Globals_CameraToScreen;
		
		struct appdata
		{
			float4 vertex : POSITION;
			
			uint instanceId : SV_InstanceID;
		};
		
		struct v2f
		{
			float4 vertex : SV_POSITION;
			float4 color : TEXCOORD0;
			float4 data : TECOORD1;
			float3 direction : TEXCOORD2;
			float3 rayEnd : TEXCOORD3;
			float3 rayStart : TEXCOORD4;
			float3 position : TEXCOORD5;

			uint instanceId : SV_InstanceID;
		};
		
		void PackStar(in appdata v, inout StructuredBuffer<GalaxyStar> buffer, out v2f o)
		{
			uint id = v.instanceId;
		
			float3 starPosition = buffer[id].position;
			float4 starColor = buffer[id].color;
			float starSize = buffer[id].size;
			float starTemperature = buffer[id].temperature;
			float starFade = 1.0;
			float starShine = 2.0;
		
			starSize *= dustSize;
			
			float3 localPosition = v.vertex.xyz * starSize;
			float3 worldPosition = starPosition + localPosition;
			float3 relativePosition = _Galaxy_Position + mulvq(worldPosition, _Galaxy_Orientation);
			float3 direction = mulvq(relativePosition, _Galaxy_OrientationInverse);
			
			o.vertex = mul(mul(_Globals_CameraToScreen, _Globals_WorldToCamera), float4(worldPosition, 1.0));
			o.vertex.z = o.vertex.z * o.vertex.w * 0.0000000000001;
			o.color = lerp(starColor, GetMaterial(starTemperature), 0.5);
			o.data = float4(starSize / 2.0, 0.0, starFade, starShine);
			o.direction = direction;
			o.rayEnd = localPosition;
			o.rayStart = relativePosition;
			o.position = starPosition;
			o.instanceId = id;
		}

		void UnpackStar(in v2f i, out float4 outputColor)
		{
			float4 starColor = i.color;
			float starSize = i.data.x;
			float starFade = i.data.z;
			float starShine = i.data.w;
			float3 direction = normalize(i.direction);
			float3 rayEnd = i.rayEnd;
			float3 rayStart = i.rayStart;
			
			float tx = (direction.x > 0.0 ? starSize + rayEnd.x : starSize - rayEnd.x) / abs(direction.x);
			float ty = (direction.y > 0.0 ? starSize + rayEnd.y : starSize - rayEnd.y) / abs(direction.y);
			float tz = (direction.z > 0.0 ? starSize + rayEnd.z : starSize - rayEnd.z) / abs(direction.z);
			float t = min(min(tx, ty), tz);
			float rayDistance = length(rayStart);
			float dt = (rayDistance < t ? rayDistance : t);
			
			float3 rs = rayEnd - (direction * dt);
			
			float hts = 0;
			float hte = 0;
			
			bool hit = RaySphereIntersect(rs, direction, starSize, hts, hte);
			
			if (!hit) discard;
			
			float rlen = hte - hts;
			float rlenn = 0.5 * rlen / starSize;

			float alpha = saturate(rlenn * rlenn);
			
			starColor *= alpha;
			starColor *= dustStrength;
			starColor = clamp(starColor, -1.0, 1.0);
			//starColor = float4(ToneMapFilmicALU(starColor.xyz * 2.2), starColor.w);

			outputColor = float4(starColor.xyz, alpha);
		}

		void vert(in appdata v, out v2f o)
		{
			PackStar(v, stars, o);
		}

		void vert_bulge(in appdata v, out v2f o)
		{
			PackStar(v, bulge, o);
		}
		
		void frag_dust(in v2f i, out float4 color : SV_Target)
		{
			float4 starColor;

			UnpackStar(i, starColor);
			
			color = float4(starColor.rgb, starColor.a);
		}

		void frag_gas(in v2f i, out float4 color : SV_Target)
		{
			float4 starColor;

			UnpackStar(i, starColor);
			
			starColor.a *= smoothstep(-1.0, gasCenterFalloff, length(i.position) - gasCenterFalloff);

			color = float4(-starColor.rgb * M_PI, starColor.a);
		}

		void frag_bulge(in v2f i, out float4 color : SV_Target)
		{
			float4 starColor;

			UnpackStar(i, starColor);
			
			color = float4(starColor.rgb, starColor.a);
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
			#pragma vertex vert
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
			#pragma vertex vert
			#pragma fragment frag_gas
			ENDCG
		}

		Pass
		{
			Name "Bulge"
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
			#pragma vertex vert_bulge
			#pragma fragment frag_bulge
			ENDCG
		}
	}
}
