using System;
using System.Collections.Generic;

using UnityEngine;

using ZFramework.Math;

namespace Experimental
{
    public class CelestialBody : MonoBehaviour
    {
        public string bodyName = "Unnamed";

        public string bodyDescription = "A mysterious uncharted celestial body.";

        public double GeeASL;

        public double Radius;

        public double Mass;

        public double Density;

        public double SurfaceArea;

        public double gravParameter;

        public double sphereOfInfluence = double.PositiveInfinity;

        public double hillSphere;

        public double gMagnitudeAtCenter;

        public double atmDensityASL;

        public double navballSwitchRadiusMult = 0.06;

        public bool use_The_InName;

        public bool isHomeWorld;

        public double atmosphereDepth;

        public double atmosphereTemperatureSeaLevel = 288.0;

        public double atmospherePressureSeaLevel = 101.325;

        public double atmosphereMolarMass = 0.0289644;

        public double atmosphereAdiabaticIndex = 1.4;

        public double atmosphereTemperatureLapseRate;

        public double atmosphereGasMassLapseRate;

        public bool atmosphereUseTemperatureCurve;

        public bool atmosphereTemperatureCurveIsNormalized;

        public FloatCurve atmosphereTemperatureCurve = new FloatCurve();

        public FloatCurve latitudeTemperatureBiasCurve = new FloatCurve(new Keyframe[]
        {
        new Keyframe(0f, 0f)
        });

        public FloatCurve latitudeTemperatureSunMultCurve = new FloatCurve(new Keyframe[]
        {
        new Keyframe(0f, 0f)
        });

        public FloatCurve axialTemperatureSunMultCurve = new FloatCurve(new Keyframe[]
        {
        new Keyframe(0f, 0f)
        });

        public FloatCurve axialTemperatureSunBiasCurve = new FloatCurve(new Keyframe[]
        {
        new Keyframe(0f, 0f)
        });

        public FloatCurve atmosphereTemperatureSunMultCurve = new FloatCurve(new Keyframe[]
        {
        new Keyframe(0f, 0f)
        });

        public double maxAxialDot;

        public FloatCurve eccentricityTemperatureBiasCurve = new FloatCurve(new Keyframe[]
        {
        new Keyframe(0f, 0f)
        });

        public double albedo = 0.35;

        public double emissivity = 0.65;

        public double coreTemperatureOffset;

        public double convectionMultiplier = 1.0;

        public double shockTemperatureMultiplier = 1.0;

        public bool atmosphereUsePressureCurve;

        public bool atmospherePressureCurveIsNormalized;

        public FloatCurve atmospherePressureCurve = new FloatCurve();

        public double radiusAtmoFactor = 1.0;

        private Vector3d _position;

        public QuaternionD rotation;

        public OrbitDriver orbitDriver;

        public GameObject scaledBody;

        public bool rotates;

        public double rotationPeriod;

        public double solarDayLength;

        public bool solarRotationPeriod;

        public double initialRotation;

        public double rotationAngle;

        public double directRotAngle;

        public Vector3d angularVelocity;

        public Vector3d zUpAngularVelocity;

        public bool tidallyLocked;

        public bool inverseRotation;

        public float inverseRotThresholdAltitude = 15000f;

        public double angularV;

        public float[] timeWarpAltitudeLimits;

        public Color atmosphericAmbientColor;

        public List<CelestialBody> orbitingBodies = new List<CelestialBody>();

        private QuaternionD newRot;

        private QuaternionD newZupRot;

        public Transform bodyTransform;

        private Vector3d rPos;

        private double rValue;

        public string theName
        {
            get
            {
                return ((!use_The_InName) ? string.Empty : "the ") + bodyName;
            }
        }

        public new string name
        {
            get
            {
                return bodyName;
            }
        }

        public int flightGlobalsIndex
        {
            get;
            set;
        }

        public Vector3d position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                bodyTransform.position = value;
            }
        }

        public Orbit orbit
        {
            get
            {
                return (!(orbitDriver != null)) ? null : orbitDriver.orbit;
            }
        }

        public CelestialBody referenceBody
        {
            get
            {
                return (!orbitDriver) ? this : orbit.referenceBody;
            }
        }

        public Vector3d RotationAxis
        {
            get
            {
                return Vector3d.up;
            }
        }

        public void SetupConstants()
        {
            //if (atmosphere)
            //{
            //    atmosphereTemperatureLapseRate = atmosphereTemperatureSeaLevel / atmosphereDepth;
            //    atmosphereGasMassLapseRate = GeeASL * 9.81 * atmosphereMolarMass / (PhysicsGlobals.idealGasConstant * atmosphereTemperatureLapseRate);
            //    radiusAtmoFactor = Radius / atmosphereDepth * -Math.Log(1E-06);
            //    atmDensityASL = GetDensity(GetPressure(0.0), GetTemperature(0.0));
            //}

            double R2 = Radius * Radius;

            Density = Mass / (4.1887902047863905 * R2 * Radius);
            SurfaceArea = 12.566370614359172 * R2;

            if (orbitDriver != null) maxAxialDot = Math.Sin((orbit.inclination * MathUtils.Deg2Rad));
        }

        public double GetPressure(double altitude)
        {
            if (altitude >= atmosphereDepth) return 0.0;

            if (!atmosphereUsePressureCurve)
                return atmospherePressureSeaLevel * Math.Pow(1.0 - atmosphereTemperatureLapseRate * altitude / atmosphereTemperatureSeaLevel, atmosphereGasMassLapseRate);

            if (atmospherePressureCurveIsNormalized)
                return Mathf.Lerp(0f, (float)atmospherePressureSeaLevel, atmospherePressureCurve.Evaluate((float)(altitude / atmosphereDepth)));

            return atmospherePressureCurve.Evaluate((float)altitude);
        }

        public double GetTemperature(double altitude)
        {
            if (altitude >= atmosphereDepth) return 0.0;

            if (atmosphereUseTemperatureCurve)
            {
                if (atmosphereTemperatureCurveIsNormalized)
                {
                    return MathUtils.Lerp(PhysicsGlobals.spaceTemperature, atmosphereTemperatureSeaLevel, atmosphereTemperatureCurve.Evaluate((float)(altitude / this.atmosphereDepth)));
                }
                else
                {
                    return atmosphereTemperatureCurve.Evaluate((float)altitude);
                }
            }
            else
            {
                return atmosphereTemperatureSeaLevel - atmosphereTemperatureLapseRate * altitude;
            }
        }

        public double GetDensity(double pressure, double temperature)
        {
            if (pressure <= 0.0 || temperature <= 0.0) return 0.0;

            return pressure * 1000.0 * atmosphereMolarMass / (PhysicsGlobals.idealGasConstant * temperature);
        }

        /*
        public double GetSpeedOfSound(double pressure, double density)
        {
            if (pressure <= 0.0 || density <= 0.0)
            {
                return 0.0;
            }
            return Math.Sqrt(this.atmosphereAdiabaticIndex * (pressure * 1000.0 / density));
        }
        
        public double GetSolarPowerFactor(double density)
        {
            double num = 1.225;
            if (Planetarium.fetch != null && Planetarium.fetch.Home.atmosphereDepth > 0.0)
            {
                num = Planetarium.fetch.Home.atmDensityASL;
            }
            double num2 = (1.0 - PhysicsGlobals.SolarInsolationAtHome) * num;
            return num2 / (num2 + density * PhysicsGlobals.SolarInsolationAtHome);
        }
        */

        public bool testBool = true;

        private void Awake()
        {
            bodyTransform = transform;
            _position = bodyTransform.position;
            orbitDriver = GetComponent<OrbitDriver>();
            gMagnitudeAtCenter = GeeASL * 9.81 * Math.Pow(Radius, 2.0);
            Mass = Radius * Radius * (GeeASL * 9.81) / 6.674E-11;
            gravParameter = Mass * 6.674E-11;

            if(testBool)
                if (orbitDriver)
                    if (orbitDriver.referenceBody != this)
                        orbitDriver.referenceBody.orbitingBodies.Add(this);
        }

        private void Start()
        {
            if (orbitDriver)
            {
                sphereOfInfluence = orbit.semiMajorAxis * Math.Pow(Mass / orbit.referenceBody.Mass, 0.4);
                hillSphere = orbit.semiMajorAxis * (1.0 - orbit.eccentricity) * Math.Pow(Mass / orbit.referenceBody.Mass, 0.33333333333333331);
                //orbitDriver.QueuedUpdate = true;

                if (solarRotationPeriod)
                {
                    double period = rotationPeriod;
                    double rotation = MathUtils.TwoPI * Math.Sqrt(Math.Pow(Math.Abs(orbit.semiMajorAxis), 3.0) / orbit.referenceBody.gravParameter);
                    double time = period * rotation / (rotation + period);

                    rotationPeriod = time;
                }
            }
            else
            {
                sphereOfInfluence = double.PositiveInfinity;
                hillSphere = double.PositiveInfinity;
            }

            SetupConstants();
        }

        public void Update()
        {
            gMagnitudeAtCenter = GeeASL * 9.81 * Radius * Radius;
            Mass = Radius * Radius * (GeeASL * 9.81) / 6.674E-11;
            gravParameter = Mass * 6.674E-11;

            if (rotates && rotationPeriod != 0.0 && (!tidallyLocked || (orbit != null && orbit.period != 0.0)))
            {
                if (!tidallyLocked)
                {
                    angularVelocity = Vector3d.down * (MathUtils.TwoPI / rotationPeriod);
                    zUpAngularVelocity = Vector3d.back * (MathUtils.TwoPI / rotationPeriod);
                }
                else if (orbitDriver)
                {
                    rotationPeriod = orbit.period;
                    angularVelocity = Vector3d.down * (MathUtils.TwoPI / orbit.period);
                    zUpAngularVelocity = Vector3d.back * (MathUtils.TwoPI / orbit.period);
                }

                rotationAngle = (initialRotation + 360.0 / rotationPeriod * Planetarium.GetUniversalTime()) % 360.0;
                angularV = angularVelocity.magnitude;

                if (!inverseRotation)
                {
                    directRotAngle = (rotationAngle - Planetarium.InverseRotAngle) % 360.0;
                    newRot = QuaternionD.AngleAxis(directRotAngle, Vector3d.down);
                    newZupRot = QuaternionD.AngleAxis(directRotAngle, Vector3d.back);
                    bodyTransform.rotation = newRot;
                    rotation = newRot;
                }
                else
                {
                    Planetarium.InverseRotAngle = (rotationAngle - directRotAngle) % 360.0;

                    newRot = QuaternionD.AngleAxis(Planetarium.InverseRotAngle, Vector3d.down);
                    newZupRot = QuaternionD.AngleAxis(Planetarium.InverseRotAngle, Vector3d.back);

                    Planetarium.Rotation = Quaternion.Inverse(newRot);
                    Planetarium.ZupRotation = newZupRot;
                }
            }

            if (orbitDriver) orbitDriver.UpdateOrbit();

            CelestialBody currentCelestialBody = this;
            CelestialBody sunCelestialBody;

            if (Planetarium.fetch != null) sunCelestialBody = Planetarium.fetch.Sun;
            else sunCelestialBody = currentCelestialBody;//FlightGlobals.Bodies[0];

            while (currentCelestialBody.referenceBody != sunCelestialBody && currentCelestialBody.referenceBody != null)
            {
                currentCelestialBody = currentCelestialBody.referenceBody;
            }

            if (currentCelestialBody.orbit != null)
            {
                double num = currentCelestialBody.orbit.period - rotationPeriod;

                if (num == 0.0) solarDayLength = 1.7976931348623157E+308;
                else solarDayLength = currentCelestialBody.orbit.period * rotationPeriod / num;
            }
            else solarDayLength = 1.0;
        }

        public Bounds getBounds()
        {
            return new Bounds(position, Vector3d.one * Radius * 2.0);
        }

        private void OnDrawGizmos()
        {
            if (sphereOfInfluence != 0.0)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(position, (float)sphereOfInfluence);
            }
        }

        public Vector3d GetFrameVel()
        {
            if (orbitDriver && orbitDriver.enabled)
            {
                return orbit.vel + orbit.referenceBody.GetFrameVel();
            }

            return Vector3d.zero;
        }

        public Vector3d GetFrameVelAtUT(double UT)
        {
            if (orbitDriver && orbitDriver.enabled)
            {
                return orbit.getOrbitalVelocityAtUT(UT) + orbit.referenceBody.GetFrameVelAtUT(UT);
            }

            return Vector3d.zero;
        }

        public Vector3d getRFrmVel(Vector3d worldPos)
        {
            return Vector3d.Cross(angularVelocity, worldPos - position);
        }

        public Vector3d getTruePositionAtUT(double UT)
        {
            return (!orbitDriver || !orbitDriver.enabled) ? position : (orbit.getRelativePositionAtUT(UT).xzy + orbit.referenceBody.getTruePositionAtUT(UT));
        }

        public Vector3d getPositionAtUT(double UT)
        {
            return (!orbitDriver || !orbitDriver.enabled) ? position : (orbit.getRelativePositionAtUT(UT).xzy + orbit.referenceBody.position);
        }

        public static Vector3d GetRSrfNVector(double lat, double lon)
        {
            return QuaternionD.AngleAxis(lon, Vector3d.down) * QuaternionD.AngleAxis(lat, Vector3d.forward) * Vector3d.right;
        }

        public Vector3d GetRelSurfaceNVector(double lat, double lon)
        {
            return QuaternionD.AngleAxis(lon, Vector3d.down) * QuaternionD.AngleAxis(lat, Vector3d.forward) * Vector3d.right;
        }

        public Vector3d GetSurfaceNVector(double lat, double lon)
        {
            return QuaternionD.AngleAxis(directRotAngle, Vector3d.down) * QuaternionD.AngleAxis(lon, Vector3d.down) * QuaternionD.AngleAxis(lat, Vector3d.forward) * Vector3d.right;
        }

        public Vector3d GetRelSurfacePosition(double lat, double lon, double alt)
        {
            return GetSurfaceNVector(lat, lon) * (Radius + alt);
        }

        public Vector3d GetRelSurfacePosition(Vector3d worldPosition)
        {
            return QuaternionD.AngleAxis(directRotAngle, Vector3d.down) * worldPosition;
        }

        public Vector3d GetWorldSurfacePosition(double lat, double lon, double alt)
        {
            return GetRelSurfacePosition(lat, lon, alt) + position;
        }

        public double GetLatitude(Vector3d worldPos)
        {
            rPos = (worldPos - position).normalized;
            rValue = Math.Asin(rPos.y) * MathUtils.Rad2Deg;

            return (!double.IsNaN(rValue)) ? rValue : 0.0;
        }

        public double GetLongitude(Vector3d worldPos)
        {
            rPos = (worldPos - position).normalized;
            rValue = Math.Atan2(rPos.z, rPos.x) * MathUtils.Rad2Deg - directRotAngle;

            return (!double.IsNaN(rValue)) ? rValue : 0.0;
        }

        public double GetAltitude(Vector3d worldPos)
        {
            return (worldPos - position).magnitude - Radius;
        }

        public bool HasParent(CelestialBody body)
        {
            return referenceBody == body || (!(referenceBody == this) && referenceBody.HasParent(body));
        }

        public bool HasChild(CelestialBody body)
        {
            for (int i = 0; i < orbitingBodies.Count; i++)
            {
                CelestialBody celestialBody = orbitingBodies[i];

                if (celestialBody == body) return true;
                if (celestialBody.HasChild(body)) return true;
            }

            return false;
        }
    }
}