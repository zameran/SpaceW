Shader "SpaceEngine/Galaxy/Dust"
{
	Properties
	{
		
	}
	SubShader
	{
		Tags { "PreviewType" = "Plane" }

		CGINCLUDE
		#include "Galaxy.cginc"
		
		uniform StructuredBuffer<GalaxyParticle> dust;
		uniform StructuredBuffer<GalaxyParticle> gas;
		
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
		
		void PackParticle(in appdata v, inout StructuredBuffer<GalaxyParticle> buffer, in float size, out v2f o)
		{
			uint id = v.instanceId;
		
			float3 particlePosition = buffer[id].position;
			float4 particleColor = buffer[id].color;
			float particleSize = buffer[id].size * size;
			float particleTemperature = buffer[id].temperature;
			
			float3 localPosition = v.vertex.xyz * particleSize;
			float3 worldPosition = particlePosition + localPosition;
			float3 relativePosition = _Galaxy_Position + worldPosition;
			
			o.vertex = mul(mul(_Globals_CameraToScreen, _Globals_WorldToCamera), float4(worldPosition, 1.0f));
			o.vertex.z = o.vertex.z * o.vertex.w * 0.0000000000001;
			o.color = lerp(particleColor, GetMaterial(particleTemperature), 0.5f);
			o.data = float2(particleSize, particleSize / 2.0f);
			o.direction = relativePosition;
			o.rayEnd = localPosition;
			o.rayStart = relativePosition;
			o.position = particlePosition;
		}

		void UnpackParticle(in v2f i, in float stength, out float4 outputColor)
		{
			float4 particleColor = i.color;
			float particleSize = i.data.y;
			float3 direction = normalize(i.direction);

			// NOTE : The old one was: 
			// (direction.x > 0.0f ? particleSize + i.rayEnd.x : particleSize - i.rayEnd.x) / abs(direction.x);
			// ...
			// etc
			float3 directionSign = sign(direction);
			float3 txyz = particleSize + (i.rayEnd * directionSign) / abs(direction);

			// NOTE : length(txyz) can be used maybe...
			float t = max(max(txyz.x, txyz.y), txyz.z);
			float rayDistance = length(i.rayStart);
			float dt = (rayDistance < t ? rayDistance : t);
			
			float hts = 0.0f;
			float hte = 0.0f;
			
			bool hit = RaySphereIntersect(i.rayEnd - (direction * dt), direction, particleSize, hts, hte);
			
			UNITY_BRANCH
			if (!hit) discard;

			float rlen = hte - hts;
			float rlenn = 0.5f * rlen / particleSize;

			float alpha = saturate(rlenn * rlenn);
			
			particleColor *= alpha;
			particleColor *= stength;
			particleColor = clamp(particleColor, -1.0f, 1.0f);

			outputColor = float4(particleColor.xyz, alpha);
		}

		void vert_dust(in appdata v, out v2f o)
		{
			PackParticle(v, dust, dustSize, o);
		}

		void vert_gas(in appdata v, out v2f o)
		{
			PackParticle(v, gas, gasSize, o);
		}
		
		void frag_dust(in v2f i, out float4 color : SV_Target)
		{
			float4 particleColor;

			UnpackParticle(i, dustStrength, particleColor);
			
			color = float4(particleColor.rgb, particleColor.a);
		}

		void frag_gas(in v2f i, out float4 color : SV_Target)
		{
			float4 particleColor;

			UnpackParticle(i, gasStrength, particleColor);

			color = float4(-particleColor.rgb * float3(0.52312f, 0.99695f, 0.94122f), particleColor.a);
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
			Cull Back
			ZWrite Off
			ZTest Off

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
			Cull Back
			ZWrite Off
			ZTest Off

			CGPROGRAM
			#pragma target 5.0
			#pragma fragmentoption arb_precision_hint_fastest
			#pragma vertex vert_gas
			#pragma fragment frag_gas
			ENDCG
		}
	}
}
