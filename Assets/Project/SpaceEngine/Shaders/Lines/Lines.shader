Shader "SpaceEngine/Lines/Colored Blended" 
{
	Properties 
	{
		_Color ("Color", Color) = (0, 0, 0, 0)
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader 
	{ 
		Pass 
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			ZWrite On 
			Lighting Off
			Cull Off 
			Fog { Mode Off }

			BindChannels 
			{
				Bind "Vertex", vertex 
				Bind "Color", color
				Bind "TexCoord", texcoord
			}

			SetTexture [_MainTex] 
			{
				combine texture * primary
			}

			SetTexture [_MainTex] 
			{
				constantColor [_Color]
				combine previous lerp (previous) constant DOUBLE
			}
		} 
	} 
}