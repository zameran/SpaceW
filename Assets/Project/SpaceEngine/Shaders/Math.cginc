#define MATH

#if !defined (M_PI)
#define M_PI 3.14159265358
#endif

#if !defined (M_PI2)
#define M_PI2 6.28318530716
#endif

float IntersectInnerSphere(float3 p1, float3 d, float3 p3, float r)
{
	float a = dot(d, d);
	float b = 2.0 * dot(d, p1 - p3);
	float c = dot(p3, p3) + dot(p1, p1) - 2.0 * dot(p3, p1) - r * r;
	float test = b * b - 4.0 * a * c;

	if (test < 0) return -1.0;

	float u = (-b - sqrt(test)) / (2.0 * a);	
								
	return u;
}

float IntersectOuterSphere(float3 p1, float3 d, float3 p3, float r)
{
	// p1 starting point
	// d look direction
	// p3 is the sphere center

	float a = dot(d, d);
	float b = 2.0 * dot(d, p1 - p3);
	float c = dot(p3, p3) + dot(p1, p1) - 2.0 * dot(p3, p1) - r * r;
	float test = b * b - 4.0 * a * c;

	if (test < 0) return -1.0;

	float u = (-b - sqrt(test)) / (2.0 * a);

	u = (u < 0) ? (-b + sqrt(test)) / (2.0 * a) : u;
			
	return u;
}

float IntersectOuterSphereInverted(float3 p1, float3 d, float3 p3, float r)
{
	// p1 starting point
	// d look direction
	// p3 is the sphere center

	float a = dot(d, d);
	float b = 2.0 * dot(d, p1 - p3);
	float c = dot(p3, p3) + dot(p1, p1) - 2.0 * dot(p3, p1) - r * r;
	float test = b * b - 4.0 * a * c;

	if (test < 0) return -1.0;
			
	return (-b + sqrt(test)) / (2.0 * a);
}