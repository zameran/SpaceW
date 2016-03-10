uniform float3 v3Translate;			// The objects world pos
uniform float3 v3LightPos;			// The direction vector to the light source
uniform float3 v3InvWavelength;		// 1 / pow(wavelength, 4) for the red, green, and blue channels
uniform float fOuterRadius;			// The outer (atmosphere) radius
uniform float fOuterRadius2;		// fOuterRadius^2
uniform float fInnerRadius;			// The inner (planetary) radius
uniform float fInnerRadius2;		// fInnerRadius^2
uniform float fKrESun;				// Kr * ESun
uniform float fKmESun;				// Km * ESun
uniform float fKr4PI;				// Kr * 4 * PI
uniform float fKm4PI;				// Km * 4 * PI
uniform float fScale;				// 1 / (fOuterRadius - fInnerRadius)
uniform float fScaleDepth;			// The scale depth (i.e. the altitude at which the atmosphere's average density is found)
uniform float fScaleOverScaleDepth;	// fScale / fScaleDepth
uniform float fHdrExposure;			// HDR exposure
uniform float g;					// The Mie phase asymmetry factor
uniform float g2;					// The Mie phase asymmetry factor squared

float3 hdr(float3 L) 
{
    L = L * fHdrExposure;

    L.r = L.r < 1.413 ? pow(L.r * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.r);
    L.g = L.g < 1.413 ? pow(L.g * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.g);
    L.b = L.b < 1.413 ? pow(L.b * 0.38317, 1.0 / 2.2) : 1.0 - exp(-L.b);

    return L;
}

float GetSamplesCount()
{
	return 8;
}

float GetMiePhase(float fCos, float fCos2, float g, float g2)
{
	return 1.5 * ((1.0 - g2) / (2.0 + g2)) * (1.0 + fCos2) / pow(1.0 + g2 - 2.0 * g * fCos, 1.5);
}

float GetRayleighPhase(float fCos2)
{
	return 0.75 + 0.75 * fCos2;
}

float Scale(float fCos, float fScaleDepth)
{
	float x = 1.0 - fCos;

	return fScaleDepth * exp(-0.00287 + x * (0.459 + x * (3.83 + x * (-6.80 + x * 5.25))));
}