Shader "SpaceEngine/Lines/Colored Blended Simple" 
{
	SubShader 
	{ 
		Pass 
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off 
			Cull Off 
			Fog { Mode Off }

			BindChannels 
			{
				Bind "Vertex", vertex 
				Bind "Color", color 
			}
		}
	}
}