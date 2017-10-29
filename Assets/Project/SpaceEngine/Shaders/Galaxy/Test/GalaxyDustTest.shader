Shader "SpaceEngine/Galaxy/DustTest"
{
	Properties
	{
		
	}
	SubShader
	{
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

			Blend One OneMinusSrcColor
			Cull Front
			ZWrite On
			ZTest Always

			CGPROGRAM
			#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			#include "../Galaxy.cginc"

			uniform StructuredBuffer<GalaxyStar> stars;

			uniform float3 _Galaxy_Position;
			uniform float4 _Galaxy_Orientation;
			uniform float4 _Galaxy_OrientationInverse;

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
			};

			void vert(in appdata v, out v2f o)
			{
				uint id = v.instanceId;
				
				float3 starPosition = stars[id].position;
				float4 starColor = stars[id].color;
				float starSize = stars[id].size;
				float starTemperature = stars[id].temperature;
				float starFade = 1.0;
				float starShine = 2.0;

				float3 localPosition = v.vertex.xyz * starSize;
				float3 worldPosition = starPosition + localPosition;
				float3 relativePosition = _Galaxy_Position + mulvq(worldPosition, _Galaxy_Orientation);
				float3 direction = mulvq(relativePosition, _Galaxy_OrientationInverse);

				o.vertex = UnityObjectToClipPos(float4(worldPosition, 1.0));
				o.vertex.z = o.vertex.z * o.vertex.w * 0.0000000000001;
				o.color = starColor;
				o.data = float4(starSize / 2.0, 0.0, starFade, starShine);
				o.direction = direction;
				o.rayEnd = localPosition;
				o.rayStart = relativePosition;
			}
			
			void frag(in v2f i, out float4 color : SV_Target)
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

				starColor *= saturate(rlenn * rlenn);
				starColor *= 0.025;
				starColor = clamp(starColor, 0.0, 1.0);

				color = starColor;
			}
			ENDCG
		}
	}
}
