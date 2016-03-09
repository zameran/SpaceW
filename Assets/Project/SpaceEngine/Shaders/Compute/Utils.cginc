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