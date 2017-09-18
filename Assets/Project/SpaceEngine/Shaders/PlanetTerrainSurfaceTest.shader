Shader "SpaceEngine/Planet/Terrain (Surface)" 
{
	Properties 
	{
		[NoScaleOffset] _Elevation_Tile("Elevation", 2D) = "white" {}
		[NoScaleOffset] _Normals_Tile("Normals", 2D) = "white" {}
		[NoScaleOffset] _Color_Tile("Color", 2D) = "white" {}
		[NoScaleOffset] _Ortho_Tile("Ortho", 2D) = "white" {}

		[NoScaleOffset] _Ground_Diffuse("Ground Diffuse", 2D) = "white" {}
		[NoScaleOffset] _Ground_Normal("Ground Normal", 2D) = "white" {}
	}

	SubShader 
	{
		Tags 
		{
			"Queue"					= "Geometry"
			"RenderType"			= "Geometry"
			"ForceNoShadowCasting"	= "False"
			"IgnoreProjector"		= "True"
		}

		Cull Back
		ZWrite On
		ZTest On
		Fog { Mode Off }
		
		
	// ------------------------------------------------------------
	// Surface shader code generated out of a CGPROGRAM block:
	

	// ---- forward rendering base pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardBase" }

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma target 5.0
#pragma only_renderers d3d11
#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
#pragma multi_compile OCEAN_ON OCEAN_OFF
#pragma multi_compile_fwdbase novertexlight noshadowmask nodynlightmap nodirlightmap nolightmap
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: ATMOSPHERE_ON OCEAN_ON 
#if defined(ATMOSPHERE_ON) && defined(OCEAN_ON) && !defined(ATMOSPHERE_OFF) && !defined(OCEAN_OFF)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_FORWARDBASE
// Stripping Light Probe Proxy Volume code because nolppv pragma is used. Using normal probe blending as fallback.
#ifdef UNITY_LIGHT_PROBE_PROXY_VOLUME
#undef UNITY_LIGHT_PROBE_PROXY_VOLUME
#endif
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 27 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		//#pragma surface surf Standard vertex:vert exclude_path:prepass noinstancing novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
		//#pragma target 5.0
		//#pragma only_renderers d3d11

		#define TCCOMMON
		
		//#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
		////#pragma multi_compile SHINE_ON SHINE_OFF
		////#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
		//#pragma multi_compile OCEAN_ON OCEAN_OFF
		////#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

		#include "Core.cginc"

		#include "SpaceStuff.cginc"
		#include "SpaceEclipses.cginc"
		#include "SpaceAtmosphere.cginc"
		#include "Ocean/OceanBRDF.cginc"

		struct Input 
		{
			float4 pos;
			float3 localPos;
			float2 texcoord;
			float3 direction;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localPos, o.texcoord);

			float3 normal = texTileLod(_Normals_Tile, o.texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;		

			normal = DecodeNormal(normal);
			normal = mul(_Deform_TangentFrameToWorld, normal);

			v.vertex = float4(o.localPos, o.pos.w);
			v.normal = -normal;
			v.texcoord.xy = o.texcoord;

			o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float2 texcoord = IN.texcoord;

			float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
			float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
			float3 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;

			normal = DecodeNormal(normal);

			float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...

			normal = mul(_Deform_TangentFrameToWorld, float4(normal, 1.0)).xyz;

			o.Albedo = reflectance.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1.0;
			o.Normal = normal;
		}
		

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 custompack0 : TEXCOORD3; // pos
  float3 custompack1 : TEXCOORD4; // localPos
  float2 custompack2 : TEXCOORD5; // texcoord
  float3 custompack3 : TEXCOORD6; // direction
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_SHADOW_COORDS(8)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 custompack0 : TEXCOORD3; // pos
  float3 custompack1 : TEXCOORD4; // localPos
  float2 custompack2 : TEXCOORD5; // texcoord
  float3 custompack3 : TEXCOORD6; // direction
  float4 lmap : TEXCOORD7;
  UNITY_SHADOW_COORDS(8)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.pos;
  o.custompack1.xyz = customInputData.localPos;
  o.custompack2.xy = customInputData.texcoord;
  o.custompack3.xyz = customInputData.direction;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
	#if UNITY_SHOULD_SAMPLE_SH
	  o.sh = 0;
	  // Approximated illumination from non-important point lights
	  #ifdef VERTEXLIGHT_ON
		o.sh += Shade4PointLights (
		  unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		  unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		  unity_4LightAtten0, worldPos, worldNormal);
	  #endif
	  o.sh = ShadeSHPerVertex (worldNormal, o.sh);
	#endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_SHADOW(o,v.texcoord1.xy); // pass shadow coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.pos.x = 1.0;
  surfIN.localPos.x = 1.0;
  surfIN.texcoord.x = 1.0;
  surfIN.direction.x = 1.0;
  surfIN.pos = IN.custompack0.xyzw;
  surfIN.localPos = IN.custompack1.xyz;
  surfIN.texcoord = IN.custompack2.xy;
  surfIN.direction = IN.custompack3.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
	fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
	fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  fixed3 worldN;
  worldN.x = dot(IN.tSpace0.xyz, o.Normal);
  worldN.y = dot(IN.tSpace1.xyz, o.Normal);
  worldN.z = dot(IN.tSpace2.xyz, o.Normal);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
	giInput.lightmapUV = IN.lmap;
  #else
	giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
	giInput.ambient = IN.sh;
  #else
	giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
	giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
	giInput.boxMax[0] = unity_SpecCube0_BoxMax;
	giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
	giInput.boxMax[1] = unity_SpecCube1_BoxMax;
	giInput.boxMin[1] = unity_SpecCube1_BoxMin;
	giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif

// -------- variant for: ATMOSPHERE_ON OCEAN_OFF 
#if defined(ATMOSPHERE_ON) && defined(OCEAN_OFF) && !defined(ATMOSPHERE_OFF) && !defined(OCEAN_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_FORWARDBASE
// Stripping Light Probe Proxy Volume code because nolppv pragma is used. Using normal probe blending as fallback.
#ifdef UNITY_LIGHT_PROBE_PROXY_VOLUME
#undef UNITY_LIGHT_PROBE_PROXY_VOLUME
#endif
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 27 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		//#pragma surface surf Standard vertex:vert exclude_path:prepass noinstancing novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
		//#pragma target 5.0
		//#pragma only_renderers d3d11

		#define TCCOMMON
		
		//#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
		////#pragma multi_compile SHINE_ON SHINE_OFF
		////#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
		//#pragma multi_compile OCEAN_ON OCEAN_OFF
		////#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

		#include "Core.cginc"

		#include "SpaceStuff.cginc"
		#include "SpaceEclipses.cginc"
		#include "SpaceAtmosphere.cginc"
		#include "Ocean/OceanBRDF.cginc"

		struct Input 
		{
			float4 pos;
			float3 localPos;
			float2 texcoord;
			float3 direction;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localPos, o.texcoord);

			float3 normal = texTileLod(_Normals_Tile, o.texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;		

			normal = DecodeNormal(normal);
			normal = mul(_Deform_TangentFrameToWorld, normal);

			v.vertex = float4(o.localPos, o.pos.w);
			v.normal = -normal;
			v.texcoord.xy = o.texcoord;

			o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float2 texcoord = IN.texcoord;

			float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
			float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
			float3 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;

			normal = DecodeNormal(normal);

			float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...

			normal = mul(_Deform_TangentFrameToWorld, float4(normal, 1.0)).xyz;

			o.Albedo = reflectance.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1.0;
			o.Normal = normal;
		}
		

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 custompack0 : TEXCOORD3; // pos
  float3 custompack1 : TEXCOORD4; // localPos
  float2 custompack2 : TEXCOORD5; // texcoord
  float3 custompack3 : TEXCOORD6; // direction
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_SHADOW_COORDS(8)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 custompack0 : TEXCOORD3; // pos
  float3 custompack1 : TEXCOORD4; // localPos
  float2 custompack2 : TEXCOORD5; // texcoord
  float3 custompack3 : TEXCOORD6; // direction
  float4 lmap : TEXCOORD7;
  UNITY_SHADOW_COORDS(8)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.pos;
  o.custompack1.xyz = customInputData.localPos;
  o.custompack2.xy = customInputData.texcoord;
  o.custompack3.xyz = customInputData.direction;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
	#if UNITY_SHOULD_SAMPLE_SH
	  o.sh = 0;
	  // Approximated illumination from non-important point lights
	  #ifdef VERTEXLIGHT_ON
		o.sh += Shade4PointLights (
		  unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		  unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		  unity_4LightAtten0, worldPos, worldNormal);
	  #endif
	  o.sh = ShadeSHPerVertex (worldNormal, o.sh);
	#endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_SHADOW(o,v.texcoord1.xy); // pass shadow coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.pos.x = 1.0;
  surfIN.localPos.x = 1.0;
  surfIN.texcoord.x = 1.0;
  surfIN.direction.x = 1.0;
  surfIN.pos = IN.custompack0.xyzw;
  surfIN.localPos = IN.custompack1.xyz;
  surfIN.texcoord = IN.custompack2.xy;
  surfIN.direction = IN.custompack3.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
	fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
	fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  fixed3 worldN;
  worldN.x = dot(IN.tSpace0.xyz, o.Normal);
  worldN.y = dot(IN.tSpace1.xyz, o.Normal);
  worldN.z = dot(IN.tSpace2.xyz, o.Normal);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
	giInput.lightmapUV = IN.lmap;
  #else
	giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
	giInput.ambient = IN.sh;
  #else
	giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
	giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
	giInput.boxMax[0] = unity_SpecCube0_BoxMax;
	giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
	giInput.boxMax[1] = unity_SpecCube1_BoxMax;
	giInput.boxMin[1] = unity_SpecCube1_BoxMin;
	giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif

// -------- variant for: ATMOSPHERE_OFF OCEAN_ON 
#if defined(ATMOSPHERE_OFF) && defined(OCEAN_ON) && !defined(ATMOSPHERE_ON) && !defined(OCEAN_OFF)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_FORWARDBASE
// Stripping Light Probe Proxy Volume code because nolppv pragma is used. Using normal probe blending as fallback.
#ifdef UNITY_LIGHT_PROBE_PROXY_VOLUME
#undef UNITY_LIGHT_PROBE_PROXY_VOLUME
#endif
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 27 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		//#pragma surface surf Standard vertex:vert exclude_path:prepass noinstancing novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
		//#pragma target 5.0
		//#pragma only_renderers d3d11

		#define TCCOMMON
		
		//#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
		////#pragma multi_compile SHINE_ON SHINE_OFF
		////#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
		//#pragma multi_compile OCEAN_ON OCEAN_OFF
		////#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

		#include "Core.cginc"

		#include "SpaceStuff.cginc"
		#include "SpaceEclipses.cginc"
		#include "SpaceAtmosphere.cginc"
		#include "Ocean/OceanBRDF.cginc"

		struct Input 
		{
			float4 pos;
			float3 localPos;
			float2 texcoord;
			float3 direction;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localPos, o.texcoord);

			float3 normal = texTileLod(_Normals_Tile, o.texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;		

			normal = DecodeNormal(normal);
			normal = mul(_Deform_TangentFrameToWorld, normal);

			v.vertex = float4(o.localPos, o.pos.w);
			v.normal = -normal;
			v.texcoord.xy = o.texcoord;

			o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float2 texcoord = IN.texcoord;

			float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
			float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
			float3 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;

			normal = DecodeNormal(normal);

			float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...

			normal = mul(_Deform_TangentFrameToWorld, float4(normal, 1.0)).xyz;

			o.Albedo = reflectance.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1.0;
			o.Normal = normal;
		}
		

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 custompack0 : TEXCOORD3; // pos
  float3 custompack1 : TEXCOORD4; // localPos
  float2 custompack2 : TEXCOORD5; // texcoord
  float3 custompack3 : TEXCOORD6; // direction
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_SHADOW_COORDS(8)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 custompack0 : TEXCOORD3; // pos
  float3 custompack1 : TEXCOORD4; // localPos
  float2 custompack2 : TEXCOORD5; // texcoord
  float3 custompack3 : TEXCOORD6; // direction
  float4 lmap : TEXCOORD7;
  UNITY_SHADOW_COORDS(8)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.pos;
  o.custompack1.xyz = customInputData.localPos;
  o.custompack2.xy = customInputData.texcoord;
  o.custompack3.xyz = customInputData.direction;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
	#if UNITY_SHOULD_SAMPLE_SH
	  o.sh = 0;
	  // Approximated illumination from non-important point lights
	  #ifdef VERTEXLIGHT_ON
		o.sh += Shade4PointLights (
		  unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		  unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		  unity_4LightAtten0, worldPos, worldNormal);
	  #endif
	  o.sh = ShadeSHPerVertex (worldNormal, o.sh);
	#endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_SHADOW(o,v.texcoord1.xy); // pass shadow coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.pos.x = 1.0;
  surfIN.localPos.x = 1.0;
  surfIN.texcoord.x = 1.0;
  surfIN.direction.x = 1.0;
  surfIN.pos = IN.custompack0.xyzw;
  surfIN.localPos = IN.custompack1.xyz;
  surfIN.texcoord = IN.custompack2.xy;
  surfIN.direction = IN.custompack3.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
	fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
	fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  fixed3 worldN;
  worldN.x = dot(IN.tSpace0.xyz, o.Normal);
  worldN.y = dot(IN.tSpace1.xyz, o.Normal);
  worldN.z = dot(IN.tSpace2.xyz, o.Normal);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
	giInput.lightmapUV = IN.lmap;
  #else
	giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
	giInput.ambient = IN.sh;
  #else
	giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
	giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
	giInput.boxMax[0] = unity_SpecCube0_BoxMax;
	giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
	giInput.boxMax[1] = unity_SpecCube1_BoxMax;
	giInput.boxMin[1] = unity_SpecCube1_BoxMin;
	giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif

// -------- variant for: ATMOSPHERE_OFF OCEAN_OFF 
#if defined(ATMOSPHERE_OFF) && defined(OCEAN_OFF) && !defined(ATMOSPHERE_ON) && !defined(OCEAN_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_FORWARDBASE
// Stripping Light Probe Proxy Volume code because nolppv pragma is used. Using normal probe blending as fallback.
#ifdef UNITY_LIGHT_PROBE_PROXY_VOLUME
#undef UNITY_LIGHT_PROBE_PROXY_VOLUME
#endif
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 27 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		//#pragma surface surf Standard vertex:vert exclude_path:prepass noinstancing novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
		//#pragma target 5.0
		//#pragma only_renderers d3d11

		#define TCCOMMON
		
		//#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
		////#pragma multi_compile SHINE_ON SHINE_OFF
		////#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
		//#pragma multi_compile OCEAN_ON OCEAN_OFF
		////#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

		#include "Core.cginc"

		#include "SpaceStuff.cginc"
		#include "SpaceEclipses.cginc"
		#include "SpaceAtmosphere.cginc"
		#include "Ocean/OceanBRDF.cginc"

		struct Input 
		{
			float4 pos;
			float3 localPos;
			float2 texcoord;
			float3 direction;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localPos, o.texcoord);

			float3 normal = texTileLod(_Normals_Tile, o.texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;		

			normal = DecodeNormal(normal);
			normal = mul(_Deform_TangentFrameToWorld, normal);

			v.vertex = float4(o.localPos, o.pos.w);
			v.normal = -normal;
			v.texcoord.xy = o.texcoord;

			o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float2 texcoord = IN.texcoord;

			float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
			float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
			float3 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;

			normal = DecodeNormal(normal);

			float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...

			normal = mul(_Deform_TangentFrameToWorld, float4(normal, 1.0)).xyz;

			o.Albedo = reflectance.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1.0;
			o.Normal = normal;
		}
		

// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 custompack0 : TEXCOORD3; // pos
  float3 custompack1 : TEXCOORD4; // localPos
  float2 custompack2 : TEXCOORD5; // texcoord
  float3 custompack3 : TEXCOORD6; // direction
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD7; // SH
  #endif
  UNITY_SHADOW_COORDS(8)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 custompack0 : TEXCOORD3; // pos
  float3 custompack1 : TEXCOORD4; // localPos
  float2 custompack2 : TEXCOORD5; // texcoord
  float3 custompack3 : TEXCOORD6; // direction
  float4 lmap : TEXCOORD7;
  UNITY_SHADOW_COORDS(8)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};
#endif

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.pos;
  o.custompack1.xyz = customInputData.localPos;
  o.custompack2.xy = customInputData.texcoord;
  o.custompack3.xyz = customInputData.direction;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
	#if UNITY_SHOULD_SAMPLE_SH
	  o.sh = 0;
	  // Approximated illumination from non-important point lights
	  #ifdef VERTEXLIGHT_ON
		o.sh += Shade4PointLights (
		  unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		  unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
		  unity_4LightAtten0, worldPos, worldNormal);
	  #endif
	  o.sh = ShadeSHPerVertex (worldNormal, o.sh);
	#endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_SHADOW(o,v.texcoord1.xy); // pass shadow coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.pos.x = 1.0;
  surfIN.localPos.x = 1.0;
  surfIN.texcoord.x = 1.0;
  surfIN.direction.x = 1.0;
  surfIN.pos = IN.custompack0.xyzw;
  surfIN.localPos = IN.custompack1.xyz;
  surfIN.texcoord = IN.custompack2.xy;
  surfIN.direction = IN.custompack3.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
	fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
	fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);

  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  fixed3 worldN;
  worldN.x = dot(IN.tSpace0.xyz, o.Normal);
  worldN.y = dot(IN.tSpace1.xyz, o.Normal);
  worldN.z = dot(IN.tSpace2.xyz, o.Normal);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
	giInput.lightmapUV = IN.lmap;
  #else
	giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
	giInput.ambient = IN.sh;
  #else
	giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
	giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
	giInput.boxMax[0] = unity_SpecCube0_BoxMax;
	giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
	giInput.boxMax[1] = unity_SpecCube1_BoxMax;
	giInput.boxMin[1] = unity_SpecCube1_BoxMin;
	giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingStandard (o, worldViewDir, gi);
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif


ENDCG

}

	// ---- forward rendering additive lights pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardAdd" }
		ZWrite Off Blend One One

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma target 5.0
#pragma only_renderers d3d11
#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
#pragma multi_compile OCEAN_ON OCEAN_OFF
#pragma skip_variants INSTANCING_ON
#pragma multi_compile_fwdadd novertexlight noshadowmask nodynlightmap nodirlightmap nolightmap
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: ATMOSPHERE_ON OCEAN_ON 
#if defined(ATMOSPHERE_ON) && defined(OCEAN_ON) && !defined(ATMOSPHERE_OFF) && !defined(OCEAN_OFF)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_FORWARDADD
// Stripping Light Probe Proxy Volume code because nolppv pragma is used. Using normal probe blending as fallback.
#ifdef UNITY_LIGHT_PROBE_PROXY_VOLUME
#undef UNITY_LIGHT_PROBE_PROXY_VOLUME
#endif
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 27 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		//#pragma surface surf Standard vertex:vert exclude_path:prepass noinstancing novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
		//#pragma target 5.0
		//#pragma only_renderers d3d11

		#define TCCOMMON
		
		//#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
		////#pragma multi_compile SHINE_ON SHINE_OFF
		////#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
		//#pragma multi_compile OCEAN_ON OCEAN_OFF
		////#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

		#include "Core.cginc"

		#include "SpaceStuff.cginc"
		#include "SpaceEclipses.cginc"
		#include "SpaceAtmosphere.cginc"
		#include "Ocean/OceanBRDF.cginc"

		struct Input 
		{
			float4 pos;
			float3 localPos;
			float2 texcoord;
			float3 direction;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localPos, o.texcoord);

			float3 normal = texTileLod(_Normals_Tile, o.texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;		

			normal = DecodeNormal(normal);
			normal = mul(_Deform_TangentFrameToWorld, normal);

			v.vertex = float4(o.localPos, o.pos.w);
			v.normal = -normal;
			v.texcoord.xy = o.texcoord;

			o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float2 texcoord = IN.texcoord;

			float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
			float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
			float3 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;

			normal = DecodeNormal(normal);

			float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...

			normal = mul(_Deform_TangentFrameToWorld, float4(normal, 1.0)).xyz;

			o.Albedo = reflectance.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1.0;
			o.Normal = normal;
		}
		

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  fixed3 tSpace0 : TEXCOORD0;
  fixed3 tSpace1 : TEXCOORD1;
  fixed3 tSpace2 : TEXCOORD2;
  float3 worldPos : TEXCOORD3;
  float4 custompack0 : TEXCOORD4; // pos
  float3 custompack1 : TEXCOORD5; // localPos
  float2 custompack2 : TEXCOORD6; // texcoord
  float3 custompack3 : TEXCOORD7; // direction
  UNITY_SHADOW_COORDS(8)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.pos;
  o.custompack1.xyz = customInputData.localPos;
  o.custompack2.xy = customInputData.texcoord;
  o.custompack3.xyz = customInputData.direction;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = fixed3(worldTangent.x, worldBinormal.x, worldNormal.x);
  o.tSpace1 = fixed3(worldTangent.y, worldBinormal.y, worldNormal.y);
  o.tSpace2 = fixed3(worldTangent.z, worldBinormal.z, worldNormal.z);
  o.worldPos = worldPos;

  UNITY_TRANSFER_SHADOW(o,v.texcoord1.xy); // pass shadow coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.pos.x = 1.0;
  surfIN.localPos.x = 1.0;
  surfIN.texcoord.x = 1.0;
  surfIN.direction.x = 1.0;
  surfIN.pos = IN.custompack0.xyzw;
  surfIN.localPos = IN.custompack1.xyz;
  surfIN.texcoord = IN.custompack2.xy;
  surfIN.direction = IN.custompack3.xyz;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
	fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
	fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  fixed3 worldN;
  worldN.x = dot(IN.tSpace0.xyz, o.Normal);
  worldN.y = dot(IN.tSpace1.xyz, o.Normal);
  worldN.z = dot(IN.tSpace2.xyz, o.Normal);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  gi.light.color *= atten;
  c += LightingStandard (o, worldViewDir, gi);
  c.a = 0.0;
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif

// -------- variant for: ATMOSPHERE_ON OCEAN_OFF 
#if defined(ATMOSPHERE_ON) && defined(OCEAN_OFF) && !defined(ATMOSPHERE_OFF) && !defined(OCEAN_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_FORWARDADD
// Stripping Light Probe Proxy Volume code because nolppv pragma is used. Using normal probe blending as fallback.
#ifdef UNITY_LIGHT_PROBE_PROXY_VOLUME
#undef UNITY_LIGHT_PROBE_PROXY_VOLUME
#endif
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 27 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		//#pragma surface surf Standard vertex:vert exclude_path:prepass noinstancing novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
		//#pragma target 5.0
		//#pragma only_renderers d3d11

		#define TCCOMMON
		
		//#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
		////#pragma multi_compile SHINE_ON SHINE_OFF
		////#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
		//#pragma multi_compile OCEAN_ON OCEAN_OFF
		////#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

		#include "Core.cginc"

		#include "SpaceStuff.cginc"
		#include "SpaceEclipses.cginc"
		#include "SpaceAtmosphere.cginc"
		#include "Ocean/OceanBRDF.cginc"

		struct Input 
		{
			float4 pos;
			float3 localPos;
			float2 texcoord;
			float3 direction;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localPos, o.texcoord);

			float3 normal = texTileLod(_Normals_Tile, o.texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;		

			normal = DecodeNormal(normal);
			normal = mul(_Deform_TangentFrameToWorld, normal);

			v.vertex = float4(o.localPos, o.pos.w);
			v.normal = -normal;
			v.texcoord.xy = o.texcoord;

			o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float2 texcoord = IN.texcoord;

			float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
			float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
			float3 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;

			normal = DecodeNormal(normal);

			float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...

			normal = mul(_Deform_TangentFrameToWorld, float4(normal, 1.0)).xyz;

			o.Albedo = reflectance.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1.0;
			o.Normal = normal;
		}
		

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  fixed3 tSpace0 : TEXCOORD0;
  fixed3 tSpace1 : TEXCOORD1;
  fixed3 tSpace2 : TEXCOORD2;
  float3 worldPos : TEXCOORD3;
  float4 custompack0 : TEXCOORD4; // pos
  float3 custompack1 : TEXCOORD5; // localPos
  float2 custompack2 : TEXCOORD6; // texcoord
  float3 custompack3 : TEXCOORD7; // direction
  UNITY_SHADOW_COORDS(8)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.pos;
  o.custompack1.xyz = customInputData.localPos;
  o.custompack2.xy = customInputData.texcoord;
  o.custompack3.xyz = customInputData.direction;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = fixed3(worldTangent.x, worldBinormal.x, worldNormal.x);
  o.tSpace1 = fixed3(worldTangent.y, worldBinormal.y, worldNormal.y);
  o.tSpace2 = fixed3(worldTangent.z, worldBinormal.z, worldNormal.z);
  o.worldPos = worldPos;

  UNITY_TRANSFER_SHADOW(o,v.texcoord1.xy); // pass shadow coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.pos.x = 1.0;
  surfIN.localPos.x = 1.0;
  surfIN.texcoord.x = 1.0;
  surfIN.direction.x = 1.0;
  surfIN.pos = IN.custompack0.xyzw;
  surfIN.localPos = IN.custompack1.xyz;
  surfIN.texcoord = IN.custompack2.xy;
  surfIN.direction = IN.custompack3.xyz;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
	fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
	fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  fixed3 worldN;
  worldN.x = dot(IN.tSpace0.xyz, o.Normal);
  worldN.y = dot(IN.tSpace1.xyz, o.Normal);
  worldN.z = dot(IN.tSpace2.xyz, o.Normal);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  gi.light.color *= atten;
  c += LightingStandard (o, worldViewDir, gi);
  c.a = 0.0;
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif

// -------- variant for: ATMOSPHERE_OFF OCEAN_ON 
#if defined(ATMOSPHERE_OFF) && defined(OCEAN_ON) && !defined(ATMOSPHERE_ON) && !defined(OCEAN_OFF)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_FORWARDADD
// Stripping Light Probe Proxy Volume code because nolppv pragma is used. Using normal probe blending as fallback.
#ifdef UNITY_LIGHT_PROBE_PROXY_VOLUME
#undef UNITY_LIGHT_PROBE_PROXY_VOLUME
#endif
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 27 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		//#pragma surface surf Standard vertex:vert exclude_path:prepass noinstancing novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
		//#pragma target 5.0
		//#pragma only_renderers d3d11

		#define TCCOMMON
		
		//#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
		////#pragma multi_compile SHINE_ON SHINE_OFF
		////#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
		//#pragma multi_compile OCEAN_ON OCEAN_OFF
		////#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

		#include "Core.cginc"

		#include "SpaceStuff.cginc"
		#include "SpaceEclipses.cginc"
		#include "SpaceAtmosphere.cginc"
		#include "Ocean/OceanBRDF.cginc"

		struct Input 
		{
			float4 pos;
			float3 localPos;
			float2 texcoord;
			float3 direction;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localPos, o.texcoord);

			float3 normal = texTileLod(_Normals_Tile, o.texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;		

			normal = DecodeNormal(normal);
			normal = mul(_Deform_TangentFrameToWorld, normal);

			v.vertex = float4(o.localPos, o.pos.w);
			v.normal = -normal;
			v.texcoord.xy = o.texcoord;

			o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float2 texcoord = IN.texcoord;

			float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
			float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
			float3 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;

			normal = DecodeNormal(normal);

			float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...

			normal = mul(_Deform_TangentFrameToWorld, float4(normal, 1.0)).xyz;

			o.Albedo = reflectance.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1.0;
			o.Normal = normal;
		}
		

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  fixed3 tSpace0 : TEXCOORD0;
  fixed3 tSpace1 : TEXCOORD1;
  fixed3 tSpace2 : TEXCOORD2;
  float3 worldPos : TEXCOORD3;
  float4 custompack0 : TEXCOORD4; // pos
  float3 custompack1 : TEXCOORD5; // localPos
  float2 custompack2 : TEXCOORD6; // texcoord
  float3 custompack3 : TEXCOORD7; // direction
  UNITY_SHADOW_COORDS(8)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.pos;
  o.custompack1.xyz = customInputData.localPos;
  o.custompack2.xy = customInputData.texcoord;
  o.custompack3.xyz = customInputData.direction;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = fixed3(worldTangent.x, worldBinormal.x, worldNormal.x);
  o.tSpace1 = fixed3(worldTangent.y, worldBinormal.y, worldNormal.y);
  o.tSpace2 = fixed3(worldTangent.z, worldBinormal.z, worldNormal.z);
  o.worldPos = worldPos;

  UNITY_TRANSFER_SHADOW(o,v.texcoord1.xy); // pass shadow coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.pos.x = 1.0;
  surfIN.localPos.x = 1.0;
  surfIN.texcoord.x = 1.0;
  surfIN.direction.x = 1.0;
  surfIN.pos = IN.custompack0.xyzw;
  surfIN.localPos = IN.custompack1.xyz;
  surfIN.texcoord = IN.custompack2.xy;
  surfIN.direction = IN.custompack3.xyz;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
	fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
	fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  fixed3 worldN;
  worldN.x = dot(IN.tSpace0.xyz, o.Normal);
  worldN.y = dot(IN.tSpace1.xyz, o.Normal);
  worldN.z = dot(IN.tSpace2.xyz, o.Normal);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  gi.light.color *= atten;
  c += LightingStandard (o, worldViewDir, gi);
  c.a = 0.0;
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif

// -------- variant for: ATMOSPHERE_OFF OCEAN_OFF 
#if defined(ATMOSPHERE_OFF) && defined(OCEAN_OFF) && !defined(ATMOSPHERE_ON) && !defined(OCEAN_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_FORWARDADD
// Stripping Light Probe Proxy Volume code because nolppv pragma is used. Using normal probe blending as fallback.
#ifdef UNITY_LIGHT_PROBE_PROXY_VOLUME
#undef UNITY_LIGHT_PROBE_PROXY_VOLUME
#endif
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 27 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		//#pragma surface surf Standard vertex:vert exclude_path:prepass noinstancing novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
		//#pragma target 5.0
		//#pragma only_renderers d3d11

		#define TCCOMMON
		
		//#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
		////#pragma multi_compile SHINE_ON SHINE_OFF
		////#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
		//#pragma multi_compile OCEAN_ON OCEAN_OFF
		////#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

		#include "Core.cginc"

		#include "SpaceStuff.cginc"
		#include "SpaceEclipses.cginc"
		#include "SpaceAtmosphere.cginc"
		#include "Ocean/OceanBRDF.cginc"

		struct Input 
		{
			float4 pos;
			float3 localPos;
			float2 texcoord;
			float3 direction;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localPos, o.texcoord);

			float3 normal = texTileLod(_Normals_Tile, o.texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;		

			normal = DecodeNormal(normal);
			normal = mul(_Deform_TangentFrameToWorld, normal);

			v.vertex = float4(o.localPos, o.pos.w);
			v.normal = -normal;
			v.texcoord.xy = o.texcoord;

			o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float2 texcoord = IN.texcoord;

			float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
			float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
			float3 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;

			normal = DecodeNormal(normal);

			float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...

			normal = mul(_Deform_TangentFrameToWorld, float4(normal, 1.0)).xyz;

			o.Albedo = reflectance.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1.0;
			o.Normal = normal;
		}
		

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  fixed3 tSpace0 : TEXCOORD0;
  fixed3 tSpace1 : TEXCOORD1;
  fixed3 tSpace2 : TEXCOORD2;
  float3 worldPos : TEXCOORD3;
  float4 custompack0 : TEXCOORD4; // pos
  float3 custompack1 : TEXCOORD5; // localPos
  float2 custompack2 : TEXCOORD6; // texcoord
  float3 custompack3 : TEXCOORD7; // direction
  UNITY_SHADOW_COORDS(8)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.pos;
  o.custompack1.xyz = customInputData.localPos;
  o.custompack2.xy = customInputData.texcoord;
  o.custompack3.xyz = customInputData.direction;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = fixed3(worldTangent.x, worldBinormal.x, worldNormal.x);
  o.tSpace1 = fixed3(worldTangent.y, worldBinormal.y, worldNormal.y);
  o.tSpace2 = fixed3(worldTangent.z, worldBinormal.z, worldNormal.z);
  o.worldPos = worldPos;

  UNITY_TRANSFER_SHADOW(o,v.texcoord1.xy); // pass shadow coordinates to pixel shader
  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.pos.x = 1.0;
  surfIN.localPos.x = 1.0;
  surfIN.texcoord.x = 1.0;
  surfIN.direction.x = 1.0;
  surfIN.pos = IN.custompack0.xyzw;
  surfIN.localPos = IN.custompack1.xyz;
  surfIN.texcoord = IN.custompack2.xy;
  surfIN.direction = IN.custompack3.xyz;
  float3 worldPos = IN.worldPos;
  #ifndef USING_DIRECTIONAL_LIGHT
	fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
	fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  fixed3 worldN;
  worldN.x = dot(IN.tSpace0.xyz, o.Normal);
  worldN.y = dot(IN.tSpace1.xyz, o.Normal);
  worldN.z = dot(IN.tSpace2.xyz, o.Normal);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  gi.light.color *= atten;
  c += LightingStandard (o, worldViewDir, gi);
  c.a = 0.0;
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


#endif


ENDCG

}

	// ---- deferred shading pass:
	Pass {
		Name "DEFERRED"
		Tags { "LightMode" = "Deferred" }

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma target 5.0
#pragma only_renderers d3d11
#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
#pragma multi_compile OCEAN_ON OCEAN_OFF
#pragma exclude_renderers nomrt
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile_prepassfinal novertexlight noshadowmask nodynlightmap nodirlightmap nolightmap
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
// -------- variant for: ATMOSPHERE_ON OCEAN_ON 
#if defined(ATMOSPHERE_ON) && defined(OCEAN_ON) && !defined(ATMOSPHERE_OFF) && !defined(OCEAN_OFF)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_DEFERRED
// Stripping Light Probe Proxy Volume code because nolppv pragma is used. Using normal probe blending as fallback.
#ifdef UNITY_LIGHT_PROBE_PROXY_VOLUME
#undef UNITY_LIGHT_PROBE_PROXY_VOLUME
#endif
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 27 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		//#pragma surface surf Standard vertex:vert exclude_path:prepass noinstancing novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
		//#pragma target 5.0
		//#pragma only_renderers d3d11

		#define TCCOMMON
		
		//#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
		////#pragma multi_compile SHINE_ON SHINE_OFF
		////#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
		//#pragma multi_compile OCEAN_ON OCEAN_OFF
		////#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

		#include "Core.cginc"

		#include "SpaceStuff.cginc"
		#include "SpaceEclipses.cginc"
		#include "SpaceAtmosphere.cginc"
		#include "Ocean/OceanBRDF.cginc"

		struct Input 
		{
			float4 pos;
			float3 localPos;
			float2 texcoord;
			float3 direction;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localPos, o.texcoord);

			float3 normal = texTileLod(_Normals_Tile, o.texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;		

			normal = DecodeNormal(normal);
			normal = mul(_Deform_TangentFrameToWorld, normal);

			v.vertex = float4(o.localPos, o.pos.w);
			v.normal = -normal;
			v.texcoord.xy = o.texcoord;

			o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float2 texcoord = IN.texcoord;

			float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
			float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
			float3 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;

			normal = DecodeNormal(normal);

			float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...

			normal = mul(_Deform_TangentFrameToWorld, float4(normal, 1.0)).xyz;

			o.Albedo = reflectance.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1.0;
			o.Normal = normal;
		}
		

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 custompack0 : TEXCOORD3; // pos
  float3 custompack1 : TEXCOORD4; // localPos
  float2 custompack2 : TEXCOORD5; // texcoord
  float3 custompack3 : TEXCOORD6; // direction
#ifndef DIRLIGHTMAP_OFF
  half3 viewDir : TEXCOORD7;
#endif
  float4 lmap : TEXCOORD8;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH
	half3 sh : TEXCOORD9; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
	float4 lmapFadePos : TEXCOORD9;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.pos;
  o.custompack1.xyz = customInputData.localPos;
  o.custompack2.xy = customInputData.texcoord;
  o.custompack3.xyz = customInputData.direction;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  #ifndef DIRLIGHTMAP_OFF
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  #endif
  o.lmap.zw = 0;
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
	o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
	o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
	#if UNITY_SHOULD_SAMPLE_SH
	  o.sh = 0;
	  o.sh = ShadeSHPerVertex (worldNormal, o.sh);
	#endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
	out half4 outGBuffer0 : SV_Target0,
	out half4 outGBuffer1 : SV_Target1,
	out half4 outGBuffer2 : SV_Target2,
	out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
	, out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.pos.x = 1.0;
  surfIN.localPos.x = 1.0;
  surfIN.texcoord.x = 1.0;
  surfIN.direction.x = 1.0;
  surfIN.pos = IN.custompack0.xyzw;
  surfIN.localPos = IN.custompack1.xyz;
  surfIN.texcoord = IN.custompack2.xy;
  surfIN.direction = IN.custompack3.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
	fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
	fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  o.Normal = -o.Normal;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
	giInput.lightmapUV = IN.lmap;
  #else
	giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
	giInput.ambient = IN.sh;
  #else
	giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
	giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
	giInput.boxMax[0] = unity_SpecCube0_BoxMax;
	giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
	giInput.boxMax[1] = unity_SpecCube1_BoxMax;
	giInput.boxMin[1] = unity_SpecCube1_BoxMin;
	giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
	outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, float3(0, 0, 0));
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: ATMOSPHERE_ON OCEAN_OFF 
#if defined(ATMOSPHERE_ON) && defined(OCEAN_OFF) && !defined(ATMOSPHERE_OFF) && !defined(OCEAN_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_DEFERRED
// Stripping Light Probe Proxy Volume code because nolppv pragma is used. Using normal probe blending as fallback.
#ifdef UNITY_LIGHT_PROBE_PROXY_VOLUME
#undef UNITY_LIGHT_PROBE_PROXY_VOLUME
#endif
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 27 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		//#pragma surface surf Standard vertex:vert exclude_path:prepass noinstancing novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
		//#pragma target 5.0
		//#pragma only_renderers d3d11

		#define TCCOMMON
		
		//#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
		////#pragma multi_compile SHINE_ON SHINE_OFF
		////#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
		//#pragma multi_compile OCEAN_ON OCEAN_OFF
		////#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

		#include "Core.cginc"

		#include "SpaceStuff.cginc"
		#include "SpaceEclipses.cginc"
		#include "SpaceAtmosphere.cginc"
		#include "Ocean/OceanBRDF.cginc"

		struct Input 
		{
			float4 pos;
			float3 localPos;
			float2 texcoord;
			float3 direction;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localPos, o.texcoord);

			float3 normal = texTileLod(_Normals_Tile, o.texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;		

			normal = DecodeNormal(normal);
			normal = mul(_Deform_TangentFrameToWorld, normal);

			v.vertex = float4(o.localPos, o.pos.w);
			v.normal = -normal;
			v.texcoord.xy = o.texcoord;

			o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float2 texcoord = IN.texcoord;

			float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
			float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
			float3 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;

			normal = DecodeNormal(normal);

			float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...

			normal = mul(_Deform_TangentFrameToWorld, float4(normal, 1.0)).xyz;

			o.Albedo = reflectance.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1.0;
			o.Normal = normal;
		}
		

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 custompack0 : TEXCOORD3; // pos
  float3 custompack1 : TEXCOORD4; // localPos
  float2 custompack2 : TEXCOORD5; // texcoord
  float3 custompack3 : TEXCOORD6; // direction
#ifndef DIRLIGHTMAP_OFF
  half3 viewDir : TEXCOORD7;
#endif
  float4 lmap : TEXCOORD8;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH
	half3 sh : TEXCOORD9; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
	float4 lmapFadePos : TEXCOORD9;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.pos;
  o.custompack1.xyz = customInputData.localPos;
  o.custompack2.xy = customInputData.texcoord;
  o.custompack3.xyz = customInputData.direction;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  #ifndef DIRLIGHTMAP_OFF
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  #endif
  o.lmap.zw = 0;
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
	o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
	o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
	#if UNITY_SHOULD_SAMPLE_SH
	  o.sh = 0;
	  o.sh = ShadeSHPerVertex (worldNormal, o.sh);
	#endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
	out half4 outGBuffer0 : SV_Target0,
	out half4 outGBuffer1 : SV_Target1,
	out half4 outGBuffer2 : SV_Target2,
	out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
	, out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.pos.x = 1.0;
  surfIN.localPos.x = 1.0;
  surfIN.texcoord.x = 1.0;
  surfIN.direction.x = 1.0;
  surfIN.pos = IN.custompack0.xyzw;
  surfIN.localPos = IN.custompack1.xyz;
  surfIN.texcoord = IN.custompack2.xy;
  surfIN.direction = IN.custompack3.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
	fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
	fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  o.Normal = -o.Normal;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
	giInput.lightmapUV = IN.lmap;
  #else
	giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
	giInput.ambient = IN.sh;
  #else
	giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
	giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
	giInput.boxMax[0] = unity_SpecCube0_BoxMax;
	giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
	giInput.boxMax[1] = unity_SpecCube1_BoxMax;
	giInput.boxMin[1] = unity_SpecCube1_BoxMin;
	giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
	outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, float3(0, 0, 0));
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: ATMOSPHERE_OFF OCEAN_ON 
#if defined(ATMOSPHERE_OFF) && defined(OCEAN_ON) && !defined(ATMOSPHERE_ON) && !defined(OCEAN_OFF)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_DEFERRED
// Stripping Light Probe Proxy Volume code because nolppv pragma is used. Using normal probe blending as fallback.
#ifdef UNITY_LIGHT_PROBE_PROXY_VOLUME
#undef UNITY_LIGHT_PROBE_PROXY_VOLUME
#endif
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 27 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		//#pragma surface surf Standard vertex:vert exclude_path:prepass noinstancing novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
		//#pragma target 5.0
		//#pragma only_renderers d3d11

		#define TCCOMMON
		
		//#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
		////#pragma multi_compile SHINE_ON SHINE_OFF
		////#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
		//#pragma multi_compile OCEAN_ON OCEAN_OFF
		////#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

		#include "Core.cginc"

		#include "SpaceStuff.cginc"
		#include "SpaceEclipses.cginc"
		#include "SpaceAtmosphere.cginc"
		#include "Ocean/OceanBRDF.cginc"

		struct Input 
		{
			float4 pos;
			float3 localPos;
			float2 texcoord;
			float3 direction;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localPos, o.texcoord);

			float3 normal = texTileLod(_Normals_Tile, o.texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;		

			normal = DecodeNormal(normal);
			normal = mul(_Deform_TangentFrameToWorld, normal);

			v.vertex = float4(o.localPos, o.pos.w);
			v.normal = -normal;
			v.texcoord.xy = o.texcoord;

			o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float2 texcoord = IN.texcoord;

			float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
			float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
			float3 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;

			normal = DecodeNormal(normal);

			float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...

			normal = mul(_Deform_TangentFrameToWorld, float4(normal, 1.0)).xyz;

			o.Albedo = reflectance.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1.0;
			o.Normal = normal;
		}
		

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 custompack0 : TEXCOORD3; // pos
  float3 custompack1 : TEXCOORD4; // localPos
  float2 custompack2 : TEXCOORD5; // texcoord
  float3 custompack3 : TEXCOORD6; // direction
#ifndef DIRLIGHTMAP_OFF
  half3 viewDir : TEXCOORD7;
#endif
  float4 lmap : TEXCOORD8;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH
	half3 sh : TEXCOORD9; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
	float4 lmapFadePos : TEXCOORD9;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.pos;
  o.custompack1.xyz = customInputData.localPos;
  o.custompack2.xy = customInputData.texcoord;
  o.custompack3.xyz = customInputData.direction;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  #ifndef DIRLIGHTMAP_OFF
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  #endif
  o.lmap.zw = 0;
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
	o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
	o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
	#if UNITY_SHOULD_SAMPLE_SH
	  o.sh = 0;
	  o.sh = ShadeSHPerVertex (worldNormal, o.sh);
	#endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
	out half4 outGBuffer0 : SV_Target0,
	out half4 outGBuffer1 : SV_Target1,
	out half4 outGBuffer2 : SV_Target2,
	out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
	, out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.pos.x = 1.0;
  surfIN.localPos.x = 1.0;
  surfIN.texcoord.x = 1.0;
  surfIN.direction.x = 1.0;
  surfIN.pos = IN.custompack0.xyzw;
  surfIN.localPos = IN.custompack1.xyz;
  surfIN.texcoord = IN.custompack2.xy;
  surfIN.direction = IN.custompack3.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
	fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
	fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  o.Normal = -o.Normal;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
	giInput.lightmapUV = IN.lmap;
  #else
	giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
	giInput.ambient = IN.sh;
  #else
	giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
	giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
	giInput.boxMax[0] = unity_SpecCube0_BoxMax;
	giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
	giInput.boxMax[1] = unity_SpecCube1_BoxMax;
	giInput.boxMin[1] = unity_SpecCube1_BoxMin;
	giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
	outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, float3(0, 0, 0));
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif

// -------- variant for: ATMOSPHERE_OFF OCEAN_OFF 
#if defined(ATMOSPHERE_OFF) && defined(OCEAN_OFF) && !defined(ATMOSPHERE_ON) && !defined(OCEAN_ON)
// Surface shader code generated based on:
// vertex modifier: 'vert'
// writes to per-pixel normal: YES
// writes to emission: no
// writes to occlusion: no
// needs world space reflection vector: no
// needs world space normal vector: no
// needs screen space position: no
// needs world space position: no
// needs view direction: no
// needs world space view direction: no
// needs world space position for lighting: YES
// needs world space view direction for lighting: YES
// needs world space view direction for lightmaps: no
// needs vertex color: no
// needs VFACE: no
// passes tangent-to-world matrix to pixel shader: YES
// reads from normal: no
// 0 texcoords actually used
#define UNITY_PASS_DEFERRED
// Stripping Light Probe Proxy Volume code because nolppv pragma is used. Using normal probe blending as fallback.
#ifdef UNITY_LIGHT_PROBE_PROXY_VOLUME
#undef UNITY_LIGHT_PROBE_PROXY_VOLUME
#endif
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

// Original surface shader snippet:
#line 27 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
		//#pragma surface surf Standard vertex:vert exclude_path:prepass noinstancing novertexlights nolightmap nodynlightmap nodirlightmap nofog nometa nolppv noshadowmask
		//#pragma target 5.0
		//#pragma only_renderers d3d11

		#define TCCOMMON
		
		//#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
		////#pragma multi_compile SHINE_ON SHINE_OFF
		////#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
		//#pragma multi_compile OCEAN_ON OCEAN_OFF
		////#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

		#include "Core.cginc"

		#include "SpaceStuff.cginc"
		#include "SpaceEclipses.cginc"
		#include "SpaceAtmosphere.cginc"
		#include "Ocean/OceanBRDF.cginc"

		struct Input 
		{
			float4 pos;
			float3 localPos;
			float2 texcoord;
			float3 direction;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localPos, o.texcoord);

			float3 normal = texTileLod(_Normals_Tile, o.texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;		

			normal = DecodeNormal(normal);
			normal = mul(_Deform_TangentFrameToWorld, normal);

			v.vertex = float4(o.localPos, o.pos.w);
			v.normal = -normal;
			v.texcoord.xy = o.texcoord;

			o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			float2 texcoord = IN.texcoord;

			float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
			float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
			float3 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize).rgb;

			normal = DecodeNormal(normal);

			float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...

			normal = mul(_Deform_TangentFrameToWorld, float4(normal, 1.0)).xyz;

			o.Albedo = reflectance.rgb;
			o.Metallic = 0.0;
			o.Smoothness = 1.0;
			o.Alpha = 1.0;
			o.Normal = normal;
		}
		

// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float4 tSpace0 : TEXCOORD0;
  float4 tSpace1 : TEXCOORD1;
  float4 tSpace2 : TEXCOORD2;
  float4 custompack0 : TEXCOORD3; // pos
  float3 custompack1 : TEXCOORD4; // localPos
  float2 custompack2 : TEXCOORD5; // texcoord
  float3 custompack3 : TEXCOORD6; // direction
#ifndef DIRLIGHTMAP_OFF
  half3 viewDir : TEXCOORD7;
#endif
  float4 lmap : TEXCOORD8;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH
	half3 sh : TEXCOORD9; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
	float4 lmapFadePos : TEXCOORD9;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  Input customInputData;
  vert (v, customInputData);
  o.custompack0.xyzw = customInputData.pos;
  o.custompack1.xyz = customInputData.localPos;
  o.custompack2.xy = customInputData.texcoord;
  o.custompack3.xyz = customInputData.direction;
  o.pos = UnityObjectToClipPos(v.vertex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  float3 viewDirForLight = UnityWorldSpaceViewDir(worldPos);
  #ifndef DIRLIGHTMAP_OFF
  o.viewDir.x = dot(viewDirForLight, worldTangent);
  o.viewDir.y = dot(viewDirForLight, worldBinormal);
  o.viewDir.z = dot(viewDirForLight, worldNormal);
  #endif
  o.lmap.zw = 0;
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
	o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
	o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
	#if UNITY_SHOULD_SAMPLE_SH
	  o.sh = 0;
	  o.sh = ShadeSHPerVertex (worldNormal, o.sh);
	#endif
#endif
  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
	out half4 outGBuffer0 : SV_Target0,
	out half4 outGBuffer1 : SV_Target1,
	out half4 outGBuffer2 : SV_Target2,
	out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
	, out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.pos.x = 1.0;
  surfIN.localPos.x = 1.0;
  surfIN.texcoord.x = 1.0;
  surfIN.direction.x = 1.0;
  surfIN.pos = IN.custompack0.xyzw;
  surfIN.localPos = IN.custompack1.xyz;
  surfIN.texcoord = IN.custompack2.xy;
  surfIN.direction = IN.custompack3.xyz;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
	fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
	fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutputStandard o = (SurfaceOutputStandard)0;
  #else
  SurfaceOutputStandard o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Alpha = 0.0;
  o.Occlusion = 1.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);

  // call surface function
  surf (surfIN, o);
  o.Normal = -o.Normal;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
	giInput.lightmapUV = IN.lmap;
  #else
	giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH
	giInput.ambient = IN.sh;
  #else
	giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
	giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
	giInput.boxMax[0] = unity_SpecCube0_BoxMax;
	giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
	giInput.boxMax[1] = unity_SpecCube1_BoxMax;
	giInput.boxMin[1] = unity_SpecCube1_BoxMin;
	giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingStandard_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingStandard_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
	outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, float3(0, 0, 0));
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}


#endif


ENDCG

}

	// ---- end of surface shader generated code

#LINE 93


		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			Cull Off
 
			CGPROGRAM
#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
#line 99 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
			#pragma vertex vert
			#pragma fragment frag

			#pragma target 2.0

			#pragma multi_compile_shadowcaster

			#include "Core.cginc"

			#include "UnityCG.cginc"
			#include "UnityStandardShadow.cginc"

			#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
			#pragma multi_compile SHINE_ON SHINE_OFF
			#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
			#pragma multi_compile OCEAN_ON OCEAN_OFF
			#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4
 
			struct v2f_shadowCaster
			{
				V2F_SHADOW_CASTER;
			};
 
			v2f_shadowCaster vert(VertexInput v)
			{
				v2f_shadowCaster o;

				// Let's declare some data holders...
				float4 outputVertex = 0;
				float3 outputLocalVertex = 0;
				float2 outputTexcoord = 0;

				// Dublicate displacement work of main vertex shadeer...
				VERTEX_POSITION(v.vertex, v.uv0.xy, outputVertex, outputLocalVertex, outputTexcoord);

				// Pass displaced vertex to original data...
				v.vertex = float4(outputLocalVertex, 0.0);

				// Make the magic...
				TRANSFER_SHADOW_CASTER(o)

				return o;
			}
 
			float4 frag(v2f_shadowCaster i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG

#LINE 150

		}
	}
}