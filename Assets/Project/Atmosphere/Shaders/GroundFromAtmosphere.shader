
Shader "Atmosphere/GronudFromAtmosphere" 
{
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
    	Pass 
    	{  		
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
    			float3 c0 : COLOR0;
    			float3 c1 : COLOR1;
			};
					
			v2f vert(appdata_base v)
			{
			
				float3 v3CameraPos = _WorldSpaceCameraPos - v3Translate;	// The camera's current position
				float fCameraHeight = length(v3CameraPos);					// The camera's current height
				//float fCameraHeight2 = fCameraHeight*fCameraHeight;		// fCameraHeight^2
			
				// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
				float3 v3Pos = mul(_Object2World, v.vertex).xyz - v3Translate;
				float3 v3Ray = v3Pos - v3CameraPos;
				v3Pos = normalize(v3Pos);
				float fFar = length(v3Ray);
				v3Ray /= fFar;
				
				// Calculate the ray's starting position, then calculate its scattering offset
				float3 v3Start = v3CameraPos;
				float fDepth = exp((fInnerRadius - fCameraHeight) * (1.0/fScaleDepth));
				float fCameraAngle = dot(-v3Ray, v3Pos);
				float fLightAngle = dot(v3LightPos, v3Pos);
				float fCameraScale = Scale(fCameraAngle, 0.25);
				float fLightScale = Scale(fLightAngle, 0.25);
				float fCameraOffset = fDepth*fCameraScale;
				float fTemp = (fLightScale + fCameraScale);
				
				const float fSamples = GetSamplesCount();
			
				// Initialize the scattering loop variables
				float fSampleLength = fFar / fSamples;
				float fScaledLength = fSampleLength * fScale;
				float3 v3SampleRay = v3Ray * fSampleLength;
				float3 v3SamplePoint = v3Start + v3SampleRay * 0.5;
				
				// Now loop through the sample rays
				float3 v3FrontColor = float3(0.0, 0.0, 0.0);
				float3 v3Attenuate;
				for(int i=0; i<int(fSamples); i++)
				{
					float fHeight = length(v3SamplePoint);
					float fDepth = exp(fScaleOverScaleDepth * (fInnerRadius - fHeight));
					float fScatter = fDepth*fTemp - fCameraOffset;
					v3Attenuate = exp(-fScatter * (v3InvWavelength * fKr4PI + fKm4PI));
					v3FrontColor += v3Attenuate * (fDepth * fScaledLength);
					v3SamplePoint += v3SampleRay;
				}
			
    			v2f OUT;
    			OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
    			OUT.uv = v.texcoord.xy;
    			
				OUT.c0.rgb = v3FrontColor * (v3InvWavelength * fKrESun + fKmESun);
				OUT.c1.rgb = v3Attenuate;
							
    			return OUT;
			}
			
			half4 frag(v2f IN) : COLOR
			{

				float3 v4DiffuseColor = float3(0.1,0.4,0.1); //green color
					
				float3 col = IN.c0 + v4DiffuseColor * IN.c1;
					
				return float4(col, 1.0);
			}
			
			ENDCG
    	}
	}
}