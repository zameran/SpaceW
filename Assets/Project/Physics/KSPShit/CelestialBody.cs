using System;
using System.Collections.Generic;

using UnityEngine;

using ZFramework.Math;

namespace Experimental
{
    public class CelestialBody : MonoBehaviour
    {
        public Vector3d rotvel = Vector3d.zero;

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
        public double maxAxialDot;
        public double albedo = 0.35;
        public double emissivity = 0.65;
        public double coreTemperatureOffset;
        public double convectionMultiplier = 1.0;
        public double shockTemperatureMultiplier = 1.0;

        private Vector3d position;
        private Quat rotation;

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

        public List<CelestialBody> orbitingBodies = new List<CelestialBody>();

        private Quat newRot;
        private Quat newZupRot;

        public Transform bodyTransform;

        public Vector3d Position
        {
            get { return position; }
            set { position = value; bodyTransform.position = value; }
        }

        public Quaternion Rotation
        {
            get { return rotation; }
            set { rotation = value; bodyTransform.rotation = value; }
        }

        public Orbit Orbit
        {
            get { return (!(orbitDriver != null)) ? null : orbitDriver.orbit; }
        }

        public CelestialBody ReferenceBody
        {
            get { return (!orbitDriver) ? this : Orbit.referenceBody; }
        }

        public Vector3d RotationAxis
        {
            get { return Vector3d.up; }
        }

        public void SetupConstants()
        {
            double R2 = Math.Pow(Radius, 2.0);

            Density = Mass / (4.1887902047863905 * R2 * Radius);
            SurfaceArea = 12.566370614359172 * R2;

            if (orbitDriver != null) maxAxialDot = Math.Sin((Orbit.inclination * MathUtils.Deg2Rad));
        }

        private void Awake()
        {
            bodyTransform = transform;
            position = bodyTransform.position;
            orbitDriver = GetComponent<OrbitDriver>();
            gMagnitudeAtCenter = GeeASL * 9.81 * Math.Pow(Radius, 2.0);
            Mass = Math.Pow(Radius, 2.0) * (GeeASL * 9.81) / 6.674E-11;
            gravParameter = Mass * 6.674E-11;

            if (orbitDriver)
                if (orbitDriver.ReferenceBody != this)
                    orbitDriver.ReferenceBody.orbitingBodies.Add(this);
        }

        private void Start()
        {
            if (orbitDriver)
            {
                sphereOfInfluence = Orbit.semiMajorAxis * Math.Pow(Mass / Orbit.referenceBody.Mass, 0.4);
                hillSphere = Orbit.semiMajorAxis * (1.0 - Orbit.eccentricity) * Math.Pow(Mass / Orbit.referenceBody.Mass, 0.33333333333333331);
                //orbitDriver.QueuedUpdate = true;

                if (solarRotationPeriod)
                {
                    double period = rotationPeriod;
                    double rotation = MathUtils.TwoPI * Math.Sqrt(Math.Pow(Math.Abs(Orbit.semiMajorAxis), 3.0) / Orbit.referenceBody.gravParameter);
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
            gMagnitudeAtCenter = GeeASL * 9.81 * Math.Pow(Radius, 2.0);
            Mass = Math.Pow(Radius, 2.0) * (GeeASL * 9.81) / 6.674E-11;
            gravParameter = Mass * 6.674E-11;

            if (rotates && rotationPeriod != 0.0 && (!tidallyLocked || (Orbit != null && Orbit.period != 0.0)))
            {
                if (!tidallyLocked)
                {
                    angularVelocity = Vector3d.down * (MathUtils.TwoPI / rotationPeriod);
                    zUpAngularVelocity = Vector3d.back * (MathUtils.TwoPI / rotationPeriod);
                }
                else if (orbitDriver)
                {
                    rotationPeriod = Orbit.period;
                    angularVelocity = Vector3d.down * (MathUtils.TwoPI / Orbit.period);
                    zUpAngularVelocity = Vector3d.back * (MathUtils.TwoPI / Orbit.period);
                }

                rotationAngle = (initialRotation + 360.0 / rotationPeriod * Planetarium.GetUniversalTime()) % 360.0;
                angularV = angularVelocity.magnitude;

                if (!inverseRotation)
                {
                    directRotAngle = (rotationAngle - Planetarium.InverseRotAngle) % 360.0;
                    newRot = Quat.AngleAxis(directRotAngle, Vector3d.down);
                    newZupRot = Quat.AngleAxis(directRotAngle, Vector3d.back);
                    bodyTransform.rotation = newRot;
                    rotation = newRot;
                }
                else
                {
                    Planetarium.InverseRotAngle = (rotationAngle - directRotAngle) % 360.0;

                    newRot = Quat.AngleAxis(Planetarium.InverseRotAngle, Vector3d.down);
                    newZupRot = Quat.AngleAxis(Planetarium.InverseRotAngle, Vector3d.back);

                    Planetarium.Rotation = Quaternion.Inverse(newRot);
                    Planetarium.ZupRotation = newZupRot;
                }
            }

            if (orbitDriver)
            {
                orbitDriver.UpdateOrbit();

                rotvel = GetFrameVel();
            }

            CelestialBody currentCelestialBody = this;
            CelestialBody sunCelestialBody;

            if (Planetarium.fetch != null) sunCelestialBody = Planetarium.fetch.Sun;
            else sunCelestialBody = currentCelestialBody;//FlightGlobals.Bodies[0];

            while (currentCelestialBody.ReferenceBody != sunCelestialBody && currentCelestialBody.ReferenceBody != null)
            {
                currentCelestialBody = currentCelestialBody.ReferenceBody;
            }

            if (currentCelestialBody.Orbit != null)
            {
                double num = currentCelestialBody.Orbit.period - rotationPeriod;

                if (num == 0.0) solarDayLength = 1.7976931348623157E+308;
                else solarDayLength = currentCelestialBody.Orbit.period * rotationPeriod / num;
            }
            else solarDayLength = 1.0;
        }

        private void OnDrawGizmos()
        {
            if (sphereOfInfluence != 0.0)
            {
                Gizmos.color = Color.gray;
                Gizmos.DrawWireSphere(Position.LocalToScaledSpace(), (float)sphereOfInfluence * ScaledSpace.InverseScaleFactor);
            }

            if(Radius != 0.0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(Position.LocalToScaledSpace(), (float)Radius * ScaledSpace.InverseScaleFactor);
            }
        }

        public Vector3d GetFrameVel()
        {
            if (orbitDriver != null && orbitDriver.enabled)
            {
                return Orbit.vel + Orbit.referenceBody.GetFrameVel();
            }
            else
            {
                return Vector3d.zero;
            }
        }

        public Vector3d GetFrameVelAtUT(double UT)
        {
            if (orbitDriver != null && orbitDriver.enabled)
            {
                return Orbit.getOrbitalVelocityAtUT(UT) + Orbit.referenceBody.GetFrameVelAtUT(UT);
            }

            return Vector3d.zero;
        }

        public Vector3d GetRFrmVel(Vector3d worldPos)
        {
            return Vector3d.Cross(angularVelocity, worldPos - Position);
        }

        public Vector3d GetTruePositionAtUT(double UT)
        {
            return (!orbitDriver || !orbitDriver.enabled) ? Position : (Orbit.getRelativePositionAtUT(UT).xzy + Orbit.referenceBody.GetTruePositionAtUT(UT));
        }

        public Vector3d GetPositionAtUT(double UT)
        {
            return (!orbitDriver || !orbitDriver.enabled) ? Position : (Orbit.getRelativePositionAtUT(UT).xzy + Orbit.referenceBody.Position);
        }

        public static Vector3d GetRSrfNVector(double lat, double lon)
        {
            return Quat.AngleAxis(lon, Vector3d.down) * Quat.AngleAxis(lat, Vector3d.forward) * Vector3d.right;
        }

        public Vector3d GetRelSurfaceNVector(double lat, double lon)
        {
            return Quat.AngleAxis(lon, Vector3d.down) * Quat.AngleAxis(lat, Vector3d.forward) * Vector3d.right;
        }

        public Vector3d GetSurfaceNVector(double lat, double lon)
        {
            return Quat.AngleAxis(directRotAngle, Vector3d.down) * Quat.AngleAxis(lon, Vector3d.down) * Quat.AngleAxis(lat, Vector3d.forward) * Vector3d.right;
        }

        public Vector3d GetRelSurfacePosition(double lat, double lon, double alt)
        {
            return GetSurfaceNVector(lat, lon) * (Radius + alt);
        }

        public Vector3d GetRelSurfacePosition(Vector3d worldPosition)
        {
            return Quat.AngleAxis(directRotAngle, Vector3d.down) * worldPosition;
        }

        public Vector3d GetWorldSurfacePosition(double lat, double lon, double alt)
        {
            return GetRelSurfacePosition(lat, lon, alt) + Position;
        }

        public double GetLatitude(Vector3d worldPos)
        {
            Vector3d rPos = (worldPos - Position).normalized;
            double rValue = Math.Asin(rPos.y) * MathUtils.Rad2Deg;

            return (!double.IsNaN(rValue)) ? rValue : 0.0;
        }

        public double GetLongitude(Vector3d worldPos)
        {
            Vector3d rPos = (worldPos - Position).normalized;
            double rValue = Math.Atan2(rPos.z, rPos.x) * MathUtils.Rad2Deg - directRotAngle;

            return (!double.IsNaN(rValue)) ? rValue : 0.0;
        }

        public double GetAltitude(Vector3d worldPos)
        {
            return (worldPos - Position).magnitude - Radius;
        }

        public bool HasParent(CelestialBody body)
        {
            return ReferenceBody == body || (!(ReferenceBody == this) && ReferenceBody.HasParent(body));
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