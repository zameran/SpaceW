//-----------------------------------------------------------------------------
struct QuadGenerationConstants
{
	float planetRadius;
	float spacing;
	float spacingreal;
	float spacingsub;
	float terrainMaxHeight;
	float lodLevel;
	float orientation;

	float3 cubeFaceEastDirection;
	float3 cubeFaceNorthDirection;
	float3 patchCubeCenter;
};

struct OutputStruct
{
	float noise;

	float3 patchCenter;
	
	float4 vcolor;
	float4 pos;
	float4 cpos;
};
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 FindBiTangent(float3 normal, float epsilon, float3 dir)
{
	float refVectorSign = sign(1.0 - abs(normal.x) - epsilon);

	float3 refVector = refVectorSign * dir;
	float3 biTangent = refVectorSign * cross(normal, refVector);

	return biTangent;
}

float3 FindTangent(float3 normal, float epsilon, float3 dir)
{
	float refVectorSign = sign(1.0 - abs(normal.x) - epsilon);

	float3 refVector = refVectorSign * dir;
	float3 biTangent = refVectorSign * cross(normal, refVector);

	return cross(-normal, biTangent);
}
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
float3 CubeCoord(QuadGenerationConstants constants, float VerticesPerSide, uint3 id, int mod, float spacing)
{
	//32 : 1;
	//64 : 3;
	//128 : 7;
	//256 : 17;
	//512 : 41;
	//TODO modifier calculation. Seems to 1,3,7 are normal, but bigger... fail.
	float eastValue = (id.x - ((VerticesPerSide - mod) / 2.0)) * spacing;
	float northValue = (id.y - ((VerticesPerSide - mod) / 2.0)) * spacing;

	float3 cubeCoordEast = constants.cubeFaceEastDirection * eastValue;
	float3 cubeCoordNorth = constants.cubeFaceNorthDirection * northValue;

	float3 cubeCoord = cubeCoordEast + cubeCoordNorth + constants.patchCubeCenter;

	return cubeCoord;
}
//-----------------------------------------------------------------------------

float3x3 TBN(float3 normal)
{
	float3 n = normal;
	
	float3 t; 

	float3 c1 = cross(n, float3(0.0, 0.0, 1.0)); 
	float3 c2 = cross(n, float3(0.0, 1.0, 0.0)); 

	if(length(c1) > length(c2))
	{
		t = c1;	
	}
	else
	{
		t = c2;	
	}

	t = normalize(t);

	float3 b = cross(t, n);

	return float3x3(t, b, n);
}