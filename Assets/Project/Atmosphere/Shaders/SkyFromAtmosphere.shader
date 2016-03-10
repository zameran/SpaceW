
Shader "Atmosphere/SkyFromAtmosphere" 
{
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
    	Pass 
    	{		
			Cull Front
				
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "Atmosphere.cginc"

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
		
			struct v2f 
			{
    			float4 pos : SV_POSITION;
    			float2 uv : TEXCOORD0;
    			float3 t0 : TEXCOORD1;
    			float3 c0 : COLOR0;
    			float3 c1 : COLOR1;
			};
					
			v2f vert(appdata_base v)
			{	
				float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate; 	// The camera's current position
				float fCameraHeight = length(v3CameraPos);					// The camera's current height
				//float fCameraHeight2 = fCameraHeight*fCameraHeight;		// fCameraHeight^2
			
				// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
				float3 v3Pos = mul(_Object2World, v.vertex).xyz - v3Translate;
				float3 v3Ray = v3Pos - v3CameraPos;
				float fFar = length(v3Ray);
				v3Ray /= fFar;
				
				// Calculate the ray's starting position, then calculate its scattering offset
				float3 v3Start = v3CameraPos;
				float fHeight = length(v3Start);
				float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fCameraHeight));
				float fStartAngle = dot(v3Ray, v3Start) / fHeight;
				float fStartOffset = fDepth * Scale(fStartAngle, 0.25);
				
				const float fSamples = GetSamplesCount();
			
				// Initialize the scattering loop variables
				float fSampleLength = fFar / fSamples;
				float fScaledLength = fSampleLength * fScale;
				float3 v3SampleRay = v3Ray * fSampleLength;
				float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;
				
				// Now loop through the sample rays
				float3 v3FrontColor = float3(0.0, 0.0, 0.0);
				for(int i=0; i<int(fSamples); i++)
				{
					float fHeight = length(v3SamplePoint);
					float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
					float fLightAngle = dot(v3LightPos, v3SamplePoint) / fHeight;
					float fCameraAngle = dot(v3Ray, v3SamplePoint) / fHeight;
					float fScatter = (fStartOffset + fDepth*(Scale(fLightAngle, 0.25) - Scale(fCameraAngle, 0.25)));
					float3 v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
					v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
					v3SamplePoint += v3SampleRay;
				}
			
    			v2f OUT;
    			OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
    			OUT.uv = v.texcoord.xy;
    			
    			// Finally, scale the Mie and Rayleigh colors and set up the varying variables for the pixel shader
				OUT.c0 = v3FrontColor * (v3InvWavelength * fKrESun);
				OUT.c1 = v3FrontColor * fKmESun;
				OUT.t0 = v3CameraPos - v3Pos;
							
    			return OUT;
			}
			
			half4 frag(v2f IN) : COLOR
			{

				float fCos = dot(v3LightPos, IN.t0) / length(IN.t0);
				float fCos2 = fCos*fCos;
				float3 col = GetRayleighPhase(fCos2) * IN.c0 + GetMiePhase(fCos, fCos2, g, g2) * IN.c1;
				//Adjust color from HDR
				col = 1.0 - exp(col * -fHdrExposure);
				
				return float4(col,1.0);
			}
			
			ENDCG
    	}
	}
}