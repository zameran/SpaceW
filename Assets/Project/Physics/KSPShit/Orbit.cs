using System;

using UnityEngine;

using ZFramework.Math;

using Random = UnityEngine.Random;

namespace Experimental
{
    [Serializable]
    public class Orbit
    {
        /*
        public enum EncounterSolutionLevel
        {
            NONE,
            ESCAPE,
            ORBIT_INTERSECT,
            SOI_INTERSECT_2,
            SOI_INTERSECT_1
        }

        public enum PatchTransitionType
        {
            INITIAL,
            FINAL,
            ENCOUNTER,
            ESCAPE,
            MANEUVER,
            IMPACT
        }
        */

        public CelestialBody referenceBody;

        public double inclination;
        public double eccentricity;
        public double semiMajorAxis;
        public double LAN;
        public double argumentOfPeriapsis;
        public double epoch;

        public Vector3d pos;
        public Vector3d vel;

        public double orbitalEnergy;
        public double meanAnomaly;
        public double trueAnomaly;
        public double eccentricAnomaly;
        public double radius;
        public double altitude;
        public double orbitalSpeed;
        public double orbitPercent;
        public double ObT;
        public double ObTAtEpoch;
        public double timeToPe;
        public double timeToAp;

        public Vector3d h;
        public Vector3d eccVec;
        public Vector3d an;

        public double meanAnomalyAtEpoch;
        public double period;

        public double mag;

        private double drawResolution = 4.0;

        public double FEVp;
        public double FEVs;
        public double SEVp;
        public double SEVs;

        public double UTappr;
        public double UTsoi;

        public double ClAppr;
        public double CrAppr;

        public double ClEctr1;
        public double ClEctr2;

        public double timeToTransition1;
        public double timeToTransition2;

        public double nearestTT;
        public double nextTT;

        public Vector3d secondaryPosAtTransition1;
        public Vector3d secondaryPosAtTransition2;

        public double closestTgtApprUT;
        public double StartUT;
        public double EndUT;

        /*
        public bool activePatch;

        public Orbit closestEncounterPatch;

        public CelestialBody closestEncounterBody;

        public EncounterSolutionLevel closestEncounterLevel;
        public PatchTransitionType patchStartTransition;
        public PatchTransitionType patchEndTransition;

        public Orbit nextPatch;
        public Orbit previousPatch;

        public double fromE;
        public double toE;
        public double sampleInterval;
        public double E;
        public double V;
        public double fromV;
        public double toV;
        */

        public double semiMinorAxis
        {
            get
            {
                return (eccentricity >= 1.0) ? (semiMajorAxis * Math.Sqrt(eccentricity * eccentricity - 1.0)) : (semiMajorAxis * Math.Sqrt(1.0 - eccentricity * eccentricity));
            }
        }

        public double semiLatusRectum
        {
            get
            {
                return h.sqrMagnitude / referenceBody.gravParameter;
            }
        }

        public double PeR
        {
            get
            {
                return (1.0 - eccentricity) * semiMajorAxis;
            }
        }

        public double ApR
        {
            get
            {
                return (1.0 + eccentricity) * semiMajorAxis;
            }
        }

        public double PeA
        {
            get
            {
                return PeR - referenceBody.Radius;
            }
        }

        public double ApA
        {
            get
            {
                return ApR - referenceBody.Radius;
            }
        }

        public Orbit()
        {

        }

        public Orbit(double inclination, double eccentricity, double semiMajorAxis,
                     double LAN, double argumentOfPeriapsis, double meanAnomalyAtEpoch,
                     double epoch, CelestialBody referenceBody)
        {
            this.referenceBody = referenceBody;

            this.inclination = inclination;
            this.eccentricity = eccentricity;
            this.semiMajorAxis = semiMajorAxis;
            this.LAN = LAN;
            this.argumentOfPeriapsis = argumentOfPeriapsis;
            this.meanAnomalyAtEpoch = meanAnomalyAtEpoch;
            this.epoch = epoch;

            Init();
        }

        public Orbit(Orbit reference)
        {
            this.referenceBody = reference.referenceBody;

            this.inclination = reference.inclination;
            this.eccentricity = reference.eccentricity;
            this.semiMajorAxis = reference.semiMajorAxis;
            this.LAN = reference.LAN;
            this.argumentOfPeriapsis = reference.argumentOfPeriapsis;
            this.meanAnomalyAtEpoch = reference.meanAnomalyAtEpoch;
            this.epoch = reference.epoch;

            Init();
        }

        public void Init()
        {
            period = MathUtils.TwoPI * Math.Sqrt(Math.Pow(Math.Abs(semiMajorAxis), 3.0) / referenceBody.gravParameter);

            if (eccentricity < 1.0)
            {
                meanAnomaly = meanAnomalyAtEpoch;
                orbitPercent = meanAnomaly / MathUtils.TwoPI;
                ObTAtEpoch = orbitPercent * period;
            }
            else
            {
                meanAnomaly = meanAnomalyAtEpoch;
                ObT = Math.Pow(Math.Pow(Math.Abs(semiMajorAxis), 3.0) / referenceBody.gravParameter, 0.5) * meanAnomaly;
                ObTAtEpoch = ObT;
            }
        }

        public void UpdateFromOrbitAtUT(Orbit orbit, double UT, CelestialBody toBody)
        {
            pos = (orbit.getTruePositionAtUT(UT) - toBody.getTruePositionAtUT(UT)).xzy;
            vel = orbit.getOrbitalVelocityAtUT(UT) + orbit.referenceBody.GetFrameVelAtUT(UT) - toBody.GetFrameVelAtUT(UT);

            UpdateFromStateVectors(pos, vel, toBody, UT);
        }

        public void UpdateFromStateVectors(Vector3d pos, Vector3d vel, CelestialBody refBody, double UT)
        {
            referenceBody = refBody;

            h = Vector3d.Cross(pos, vel);
            inclination = Math.Acos(h.z / h.magnitude) * MathUtils.Rad2Deg;

            if (inclination == 0.0)
            {
                vel += Vector3d.forward * 1e-10;

                h = Vector3d.Cross(pos, vel);
                inclination = Math.Acos(h.z / h.magnitude) * MathUtils.Rad2Deg;
            }

            eccVec = Vector3d.Cross(vel, h) / refBody.gravParameter - pos / pos.magnitude;
            eccentricity = eccVec.magnitude;

            orbitalEnergy = vel.sqrMagnitude / 2.0 - refBody.gravParameter / pos.magnitude;
            semiMajorAxis = ((eccentricity >= 1.0) ?
                            (-semiLatusRectum / (eccVec.sqrMagnitude - 1.0)) :
                            (-refBody.gravParameter / (2.0 * orbitalEnergy)));

            an = Vector3d.Cross(Vector3d.forward, h);

            LAN = ((an.y < 0.0) ? (MathUtils.TwoPI - Math.Acos(an.x / an.magnitude)) :
                                  Math.Acos(an.x / an.magnitude)) * MathUtils.Rad2Deg;

            argumentOfPeriapsis = Math.Acos(Vector3d.Dot(an, eccVec) / (an.magnitude * eccentricity));

            if (eccVec.z < 0.0) argumentOfPeriapsis = MathUtils.TwoPI - argumentOfPeriapsis;
            if (an == Vector3d.zero) { LAN = 0.0; argumentOfPeriapsis = Math.Acos(eccVec.x / eccentricity); }

            LAN = (LAN + Planetarium.InverseRotAngle) % 360.0;

            argumentOfPeriapsis *= MathUtils.Rad2Deg;

            period = MathUtils.TwoPI * Math.Sqrt(Math.Pow(Math.Abs(semiMajorAxis), 3.0) / refBody.gravParameter);
            trueAnomaly = Math.Acos(Vector3d.Dot(eccVec, pos) / (eccentricity * pos.magnitude));

            if (Vector3d.Dot(pos, vel) < 0.0) trueAnomaly = MathUtils.TwoPI - trueAnomaly;

            if (double.IsNaN(trueAnomaly)) trueAnomaly = MathUtils.PI;

            eccentricAnomaly = GetEccentricAnomaly(trueAnomaly);
            meanAnomaly = GetMeanAnomaly(eccentricAnomaly, trueAnomaly);

            meanAnomalyAtEpoch = meanAnomaly;

            if (eccentricity < 1.0)
            {
                orbitPercent = meanAnomaly / MathUtils.TwoPI;
                ObT = orbitPercent * period;

                timeToPe = period - ObT;
                timeToAp = timeToPe - period / 2.0;

                if (timeToAp < 0.0) timeToAp += period;

                ObTAtEpoch = ObT;
            }
            else
            {
                ObT = Math.Pow(Math.Pow(-semiMajorAxis, 3.0) / refBody.gravParameter, 0.5) * meanAnomaly;

                timeToPe = -ObT;
                ObTAtEpoch = ObT;
            }

            radius = pos.magnitude;
            altitude = radius - refBody.Radius;
            epoch = UT;

            this.pos = pos;
            this.vel = vel;
        }

        public void UpdateFromUT(double UT)
        {
            ObT = getObtAtUT(UT);
            an = QuaternionD.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;

            if (!Planetarium.Pause)
            {
                mag = Vector3d.Cross(pos, vel).magnitude;
                h = QuaternionD.AngleAxis(inclination, an) * Planetarium.Zup.Z * ((!double.IsNaN(mag)) ? Math.Max(mag, 1.0) : 1.0);
            }

            eccVec = QuaternionD.AngleAxis(argumentOfPeriapsis, h) * an * eccentricity;

            if (eccentricity < 1.0)
            {
                meanAnomaly = ObT / period * 2.0 * MathUtils.PI;
                eccentricAnomaly = ((eccentricity >= 0.9) ?
                                   solveEccentricAnomalyExtremeEcc(meanAnomaly, eccentricity, 8) :
                                   solveEccentricAnomalyStd(meanAnomaly, eccentricity, 1e-07));

                trueAnomaly = Math.Acos((Math.Cos(eccentricAnomaly) - eccentricity) / (1.0 - eccentricity * Math.Cos(eccentricAnomaly)));

                if (ObT > period / 2.0) trueAnomaly = MathUtils.TwoPI - trueAnomaly;

                radius = semiMajorAxis * (1.0 - eccentricity * eccentricity) / (1.0 + eccentricity * Math.Cos(trueAnomaly));
            }
            else
            {
                if (eccentricity == 1.0) eccentricity += 1e-10;

                meanAnomaly = MathUtils.TwoPI * Math.Abs(ObT) / period;

                if (ObT < 0.0) meanAnomaly *= -1.0;

                eccentricAnomaly = solveEccentricAnomalyHyp(Math.Abs(meanAnomaly), eccentricity, 1E-07);
                trueAnomaly = Math.Atan2(Math.Sqrt(eccentricity * eccentricity - 1.0) * Math.Sinh(eccentricAnomaly), eccentricity - Math.Cosh(eccentricAnomaly));

                if (ObT < 0.0) trueAnomaly = MathUtils.TwoPI - trueAnomaly;

                radius = -semiMajorAxis * (eccentricity * eccentricity - 1.0) / (1.0 + eccentricity * Math.Cos(trueAnomaly));
            }

            orbitPercent = meanAnomaly / MathUtils.TwoPI;
            pos = QuaternionD.AngleAxis(argumentOfPeriapsis + trueAnomaly * MathUtils.Rad2Deg, h) * an * radius;

            if (eccentricity > 1e-05 && eccentricity < 1.0)
            {
                Vector3d relativePositionAtT = getRelativePositionAtT(ObT + Time.deltaTime); //TimeWarp.deltaTime

                double magnitude = relativePositionAtT.magnitude;
                double axisMagn = magnitude / semiMajorAxis;
                double angle = Math.Acos((2.0 - 2.0 * (eccentricity * eccentricity)) / (axisMagn * (2.0 - axisMagn)) - 1.0);
                double T = (MathUtils.PI - angle) / 2.0;

                orbitalSpeed = Math.Sqrt(referenceBody.gravParameter * (2.0 / magnitude - 1.0 / semiMajorAxis));

                if (ObT > period / 2.0) T = angle + T;

                vel = QuaternionD.AngleAxis(T * MathUtils.Rad2Deg, h) * relativePositionAtT.normalized * orbitalSpeed;
            }
            else
            {
                Vector3d relativePositionAtT2 = getRelativePositionAtT(ObT + Time.deltaTime); //TimeWarp.deltaTime

                orbitalSpeed = Math.Sqrt(referenceBody.gravParameter * (2.0 / pos.magnitude - 1.0 / semiMajorAxis));
                vel = (relativePositionAtT2 - pos).normalized * orbitalSpeed;
            }

            orbitalEnergy = orbitalSpeed * orbitalSpeed / 2.0 - referenceBody.gravParameter / pos.magnitude;
            altitude = radius - referenceBody.Radius;

            if (eccentricity < 1.0)
            {
                timeToPe = period - ObT;
                timeToAp = timeToPe - period / 2.0;

                if (timeToAp < 0.0) timeToAp += period;
            }
            else
            {
                timeToPe = -ObT;
                timeToAp = 0.0;
            }
        }

        public double GetDTforTrueAnomaly(double tA, double wrapAfterSeconds)
        {
            double eccentricAnomaly = GetEccentricAnomaly(tA);
            double meanAnomaly = GetMeanAnomaly(eccentricAnomaly, tA);
            double t = (eccentricity >= 1.0) ?
                       (Math.Pow(Math.Pow(-semiMajorAxis, 3.0) / referenceBody.gravParameter, 0.5) * meanAnomaly) :
                       (meanAnomaly / MathUtils.TwoPI * period);

            double dt;

            if (eccentricity < 1.0)
            {
                if (tA < 0.0) dt = -ObT - t;
                else dt = t - ObT;

                if (dt < -Math.Abs(wrapAfterSeconds)) dt += period;
            }
            else if (tA < 0.0) dt = -ObT - t;
            else dt = t - ObT;

            if (double.IsNaN(dt))
                Debug.Log(string.Format("dt is NaN! tA: {0}, E: {1}, M: {2}, T: {3}", tA, eccentricAnomaly, meanAnomaly, t));

            return dt;
        }

        public double GetUTforTrueAnomaly(double tA, double wrapAfterSeconds)
        {
            return Planetarium.GetUniversalTime() + GetDTforTrueAnomaly(tA, wrapAfterSeconds);
        }

        public Vector3d getPositionAtUT(double UT)
        {
            return getPositionAtT(getObtAtUT(UT));
        }

        public Vector3d getTruePositionAtUT(double UT)
        {
            return getRelativePositionAtUT(UT).xzy + referenceBody.getTruePositionAtUT(UT);
        }

        public Vector3d getRelativePositionAtUT(double UT)
        {
            return getRelativePositionAtT(getObtAtUT(UT));
        }

        public Vector3d getOrbitalVelocityAtUT(double UT)
        {
            return getOrbitalVelocityAtObT(getObtAtUT(UT));
        }

        public double getObtAtUT(double UT)
        {
            double Obt;

            if (eccentricity < 1.0) Obt = (UT - epoch + ObTAtEpoch) % period;
            else Obt = ObTAtEpoch + (UT - epoch);

            if (Obt < 0.0 && eccentricity < 1.0) Obt += period;

            if (double.IsNaN(Obt)) Debug.Log("getObtAtUT() result is NaN! UT: " + UT.ToString());

            return Obt;
        }

        public double getObTAtMeanAnomaly(double M)
        {
            if (eccentricity < 1.0) return meanAnomaly / MathUtils.TwoPI * period;

            return Math.Pow(Math.Pow(-semiMajorAxis, 3.0) / referenceBody.gravParameter, 0.5) * M;
        }

        public Vector3d GetOrbitNormal()
        {
            if (Planetarium.FrameIsRotating())
            {
                Vector3d axis = QuaternionD.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;

                return QuaternionD.AngleAxis(inclination, axis) * Planetarium.Zup.Z;
            }

            return h;
        }

        public void GetOrbitNormal(ref Vector3 axis, ref Vector3d normal)
        {
            if (Planetarium.FrameIsRotating())
            {
                axis = QuaternionD.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;
                normal = QuaternionD.AngleAxis(inclination, axis) * Planetarium.Zup.Z;
            }
        }

        public Vector3d GetEccVector()
        {
            if (Planetarium.FrameIsRotating())
            {
                Vector3d axis = QuaternionD.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;
                Vector3d normal = QuaternionD.AngleAxis(inclination, axis) * Planetarium.Zup.Z;

                return QuaternionD.AngleAxis(argumentOfPeriapsis, normal) * axis;
            }

            return eccVec;
        }

        public Vector3d GetANVector()
        {
            if (Planetarium.FrameIsRotating())
                return QuaternionD.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;

            return an;
        }

        public Vector3d GetVel()
        {
            Vector3d vel = GetFrameVel() - ((!FlightGlobals.ActiveVessel) ? Vector3d.zero : FlightGlobals.ActiveVessel.orbitDriver.referenceBody.GetFrameVel());

            return vel.xzy;
        }

        public Vector3d GetRelativeVel()
        {
            return vel.xzy;
        }

        public Vector3d GetRotFrameVel(CelestialBody refBody)
        {
            if (refBody.rotates && refBody.inverseRotation)
                return Vector3d.Cross(refBody.zUpAngularVelocity, -pos);

            return Vector3d.zero;
        }

        public Vector3d GetFrameVel()
        {
            return vel + referenceBody.GetFrameVel();
        }

        public Vector3d GetFrameVelAtUT(double UT)
        {
            return getOrbitalVelocityAtUT(UT) + referenceBody.GetFrameVelAtUT(UT);
        }

        public Vector3d GetWorldSpaceVel()
        {
            return GetVel() - ((!referenceBody.inverseRotation) ? Vector3d.zero : referenceBody.getRFrmVel(pos + referenceBody.position));
        }

        public double GetTimeToPeriapsis()
        {
            //if (eccentricity < 1.0 && patchEndTransition != PatchTransitionType.FINAL && StartUT + timeToPe > EndUT)
            if (eccentricity < 1.0 && StartUT + timeToPe > EndUT)
                return timeToPe - period;

            return timeToPe;
        }

        public static double ACosh(double x)
        {
            return Math.Log(x + Math.Sqrt(x * x - 1.0));
        }

        public double GetEccentricAnomaly(double tA)
        {
            double eccentricAnomaly;

            if (eccentricity < 1.0)
            {
                eccentricAnomaly = Math.Acos((eccentricity + Math.Cos(tA)) / (1.0 + eccentricity * Math.Cos(tA)));

                if (tA > MathUtils.PI) eccentricAnomaly = MathUtils.TwoPI - eccentricAnomaly;
            }
            else
            {
                eccentricAnomaly = ACosh((eccentricity + Math.Cos(tA)) / (1.0 + eccentricity * Math.Cos(tA)));

                if (double.IsNaN(eccentricAnomaly)) eccentricAnomaly = MathUtils.PI;

                if (double.IsInfinity(eccentricAnomaly))
                {
                    Debug.Log(string.Format("E is Infinity! tA: {0}, e: {1}", tA, eccentricAnomaly));
                }
            }

            return eccentricAnomaly;
        }

        public double GetMeanAnomaly(double E, double tA)
        {
            if (eccentricity < 1.0) return E - eccentricity * Math.Sin(E);

            return (eccentricity * Math.Sinh(E) - E) * ((tA >= 3.1415926535897931) ? -1.0 : 1.0);
        }

        public double RadiusAtTrueAnomaly(double tA)
        {
            return (eccentricity >= 1.0) ?
                   (-semiMajorAxis * (eccentricity * eccentricity - 1.0) / (1.0 + eccentricity * Math.Cos(tA))) :
                   (semiLatusRectum * (1.0 / (1.0 + eccentricity * Math.Cos(tA))));
        }

        public double TrueAnomalyAtRadius(double R)
        {
            return trueAnomalyAtRadiusExtreme(R);
        }

        private double trueAnomalyAtRadiusExtreme(double R)
        {
            double result;

            if (eccentricity < 1.0)
            {
                R = Math.Min(Math.Max(PeR, R), ApR);
                result = Math.Acos((Vector3d.Cross(getRelativePositionFromEccAnomaly(eccentricAnomaly), getOrbitalVelocityAtObT(ObT)).sqrMagnitude / referenceBody.gravParameter) / (this.eccentricity * R) - 1.0 / this.eccentricity);
            }
            else
            {
                R = Math.Max(PeR, R);
                result = MathUtils.PI - Math.Acos(semiMajorAxis * eccentricity / R - semiMajorAxis / (eccentricity * R) + 1.0 / eccentricity);
            }

            return result;
        }

        public double TrueAnomalyAtUT(double UT)
        {
            return TrueAnomalyAtT(getObtAtUT(UT));
        }

        public double TrueAnomalyAtT(double T)
        {
            double trueAnomaly;

            if (eccentricity < 1.0)
            {
                double meanAnomaly = T / period * 2.0 * MathUtils.PI;
                double eccentricAnomaly = (eccentricity >= 0.9) ? solveEccentricAnomalyExtremeEcc(meanAnomaly, eccentricity, 8) :
                                                                  solveEccentricAnomalyStd(meanAnomaly, eccentricity, 1e-07);

                trueAnomaly = Math.Acos((Math.Cos(eccentricAnomaly) - eccentricity) / (1.0 - eccentricity * Math.Cos(eccentricAnomaly)));

                if (T > period / 2.0) trueAnomaly = MathUtils.TwoPI - trueAnomaly;
            }
            else
            {
                double meanAnomaly = MathUtils.TwoPI * Math.Abs(T) / period;

                if (T < 0.0) meanAnomaly *= -1.0;

                double eccentricAnomaly = solveEccentricAnomalyHyp(Math.Abs(meanAnomaly), eccentricity, 1e-07);

                trueAnomaly = Math.Atan2(Math.Sqrt(eccentricity * eccentricity - 1.0) * Math.Sinh(eccentricAnomaly), eccentricity - Math.Cosh(eccentricAnomaly));

                if (T < 0.0) trueAnomaly = MathUtils.TwoPI - trueAnomaly;
            }

            return trueAnomaly;
        }

        public double solveEccentricAnomaly(double M, double ecc, double maxError, int maxIterations)
        {
            return (eccentricity >= 1.0) ? solveEccentricAnomalyHyp(M, eccentricity, maxError) :
                   ((eccentricity >= 0.8) ? solveEccentricAnomalyExtremeEcc(M, eccentricity, maxIterations) :
                                            solveEccentricAnomalyStd(M, eccentricity, maxError));
        }

        private double solveEccentricAnomalyStd(double M, double ecc, double maxError = 1E-07)
        {
            double s = 1.0;
            double std = M + ecc * Math.Sin(M) + 0.5 * ecc * ecc * Math.Sin(2.0 * M);

            while (Math.Abs(s) > maxError)
            {
                double num3 = std - ecc * Math.Sin(std);

                s = (M - num3) / (1.0 - ecc * Math.Cos(std));
                std += s;
            }

            return std;
        }

        private double solveEccentricAnomalyExtremeEcc(double M, double ecc, int iterations = 8)
        {
            double s = M + 0.85 * eccentricity * Math.Sign(Math.Sin(M));

            for (int i = 0; i < iterations; i++)
            {
                double sins = ecc * Math.Sin(s);
                double coss = ecc * Math.Cos(s);
                double aC = s - sins - M;
                double aB = 1.0 - coss;

                s += -5.0 * aC / (aB + Math.Sign(aB) * Math.Sqrt(Math.Abs(16.0 * aB * aB - 20.0 * aC * sins)));
            }

            return s;
        }

        private double solveEccentricAnomalyHyp(double M, double ecc, double maxError = 1E-07)
        {
            double anomaly = 1.0;
            double hyp = Math.Log(2.0 * M / ecc + 1.8);

            while (Math.Abs(anomaly) > maxError)
            {
                anomaly = (eccentricity * Math.Sinh(hyp) - hyp - M) / (eccentricity * Math.Cosh(hyp) - 1.0);
                hyp -= anomaly;
            }

            return hyp;
        }

        public double getTrueAnomaly(double E)
        {
            double trueAnomaly;

            if (eccentricity < 1.0)
            {
                trueAnomaly = Math.Acos((Math.Cos(E) - eccentricity) / (1.0 - eccentricity * Math.Cos(E)));

                if (E > MathUtils.PI) trueAnomaly = MathUtils.TwoPI - trueAnomaly;
                if (E < 0.0) trueAnomaly *= -1.0;
            }
            else
            {
                trueAnomaly = Math.Atan2(Math.Sqrt(eccentricity * eccentricity - 1.0) * Math.Sinh(E), eccentricity - Math.Cosh(E));
            }

            return trueAnomaly;
        }

        public double GetTrueAnomalyOfZupVector(Vector3d vector)
        {
            Vector3 axis;

            if (eccVec != Vector3d.zero)
            {
                axis = Quaternion.Inverse(Quaternion.LookRotation(-GetEccVector().xzy.ToVector3(), GetOrbitNormal().xzy.ToVector3())) * vector.xzy.ToVector3();
            }
            else
            {
                axis = Quaternion.Inverse(Quaternion.LookRotation(-getPositionFromTrueAnomaly(0.0).normalized.ToVector3(), GetOrbitNormal().xzy.ToVector3())) * vector.xzy.ToVector3();
            }

            double trueAnomaly = MathUtils.PI - Math.Atan2(axis.x, axis.z);

            if (trueAnomaly < 0.0) trueAnomaly = MathUtils.TwoPI - trueAnomaly;

            return trueAnomaly;
        }

        public Vector3d getPositionAtT(double T)
        {
            return referenceBody.position + getRelativePositionAtT(T).xzy;
        }

        public Vector3d getRelativePositionAtT(double T)
        {
            double trueAnomaly;
            double d;

            if (eccentricity < 1.0)
            {
                double meanAnomaly = T / period * 2.0 * MathUtils.PI;
                double eccentricAnomaly = (eccentricity >= 0.9) ? solveEccentricAnomalyExtremeEcc(meanAnomaly, eccentricity, 8) :
                                                                  solveEccentricAnomalyStd(meanAnomaly, eccentricity, 1e-07);

                trueAnomaly = Math.Acos((Math.Cos(eccentricAnomaly) - eccentricity) / (1.0 - eccentricity * Math.Cos(eccentricAnomaly)));

                if (T > period / 2.0) trueAnomaly = MathUtils.TwoPI - trueAnomaly;

                d = semiMajorAxis * (1.0 - eccentricity * eccentricity) / (1.0 + eccentricity * Math.Cos(trueAnomaly));
            }
            else
            {
                double meanAnomaly = MathUtils.TwoPI * Math.Abs(T) / period;

                if (T < 0.0) meanAnomaly *= -1.0;

                double eccentricAnomaly = solveEccentricAnomalyHyp(Math.Abs(meanAnomaly), eccentricity, 1E-07);

                trueAnomaly = Math.Atan2(Math.Sqrt(eccentricity * eccentricity - 1.0) * Math.Sinh(eccentricAnomaly), eccentricity - Math.Cosh(eccentricAnomaly));

                if (T < 0.0) trueAnomaly = MathUtils.TwoPI - trueAnomaly;

                d = -semiMajorAxis * (eccentricity * eccentricity - 1.0) / (1.0 + eccentricity * Math.Cos(trueAnomaly));
            }

            Vector3d normal = QuaternionD.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;
            Vector3d axis = QuaternionD.AngleAxis(inclination, normal) * Planetarium.Zup.Z;

            return QuaternionD.AngleAxis(argumentOfPeriapsis + trueAnomaly * MathUtils.Rad2Deg, axis) * normal * d;
        }

        public Vector3d getPositionFromMeanAnomaly(double M)
        {
            return referenceBody.position + getRelativePositionFromMeanAnomaly(M).xzy;
        }

        public Vector3d getRelativePositionFromMeanAnomaly(double M)
        {
            return getRelativePositionFromEccAnomaly(solveEccentricAnomaly(M, eccentricity, 1e-05, 8));
        }

        public Vector3d getPositionFromEccAnomaly(double E)
        {
            return referenceBody.position + getRelativePositionFromEccAnomaly(E).xzy;
        }

        public Vector3d getRelativePositionFromEccAnomaly(double E)
        {
            E *= -1.0;

            double x;
            double y;

            if (eccentricity < 1.0)
            {
                x = semiMajorAxis * (Math.Cos(E) - eccentricity);
                y = semiMajorAxis * Math.Sqrt(1.0 - eccentricity * eccentricity) * -Math.Sin(E);
            }
            else if (eccentricity > 1.0)
            {
                x = -semiMajorAxis * (eccentricity - Math.Cosh(E));
                y = -semiMajorAxis * Math.Sqrt(eccentricity * eccentricity - 1.0) * -Math.Sinh(E);
            }
            else
            {
                x = 0.0;
                y = 0.0;
            }

            Vector3d pos = new Vector3d(x, y, 0.0);
            Vector3d normal = QuaternionD.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;
            Vector3d axis = QuaternionD.AngleAxis(inclination, normal) * Planetarium.Zup.Z;

            QuaternionD rotation = QuaternionD.AngleAxis(argumentOfPeriapsis, axis) *
                                   QuaternionD.AngleAxis(inclination, normal) *
                                   QuaternionD.AngleAxis(LAN - Planetarium.InverseRotAngle, Planetarium.Zup.Z);

            pos = rotation * pos;

            return pos;
        }

        public Vector3d getPositionFromTrueAnomaly(double tA)
        {
            return referenceBody.position + getRelativePositionFromTrueAnomaly(tA).xzy;
        }

        public Vector3d getRelativePositionFromTrueAnomaly(double tA)
        {
            double d = (eccentricity >= 1.0) ?
                       (-semiMajorAxis * (eccentricity * eccentricity - 1.0) / (1.0 + eccentricity * Math.Cos(tA))) :
                       (semiLatusRectum * (1.0 / (1.0 + eccentricity * Math.Cos(tA))));

            Vector3d normal = QuaternionD.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;
            Vector3d axis = QuaternionD.AngleAxis(inclination, normal) * Planetarium.Zup.Z;

            return QuaternionD.AngleAxis(argumentOfPeriapsis + tA * MathUtils.Rad2Deg, axis) * normal * d;
        }

        public double getOrbitalSpeedAt(double time)
        {
            return getOrbitalSpeedAtDistance(getRelativePositionAtT(time).magnitude);
        }

        public double getOrbitalSpeedAtRelativePos(Vector3d relPos)
        {
            return getOrbitalSpeedAtDistance(relPos.magnitude);
        }

        public double getOrbitalSpeedAtPos(Vector3d pos)
        {
            return getOrbitalSpeedAtDistance((referenceBody.position - pos).magnitude);
        }

        public double getOrbitalSpeedAtDistance(double d)
        {
            return Math.Sqrt(referenceBody.gravParameter * (2.0 / d - 1.0 / semiMajorAxis));
        }

        public Vector3d getOrbitalVelocityAtObT(double ObT)
        {
            if (eccentricity > 1e-05 && eccentricity < 1.0)
            {
                Vector3d relativePositionAtT = getRelativePositionAtT(ObT + Time.fixedDeltaTime);

                double magnitude = relativePositionAtT.magnitude;
                double d = Math.Sqrt(referenceBody.gravParameter * (2.0 / magnitude - 1.0 / semiMajorAxis));
                double axisMagn = magnitude / semiMajorAxis;
                double angle = Math.Acos((2.0 - 2.0 * (eccentricity * eccentricity)) / (axisMagn * (2.0 - axisMagn)) - 1.0);
                double T = (MathUtils.PI - angle) / 2.0;

                if (ObT > period / 2.0) T = angle + T;

                Vector3d normal = QuaternionD.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;
                Vector3d axis = QuaternionD.AngleAxis(inclination, normal) * Planetarium.Zup.Z;

                return QuaternionD.AngleAxis(T * MathUtils.Rad2Deg, axis) * relativePositionAtT.normalized * d;
            }

            Vector3d relativePositionAtT2 = getRelativePositionAtT(ObT);
            Vector3d relativePositionAtT3 = getRelativePositionAtT(ObT - Time.fixedDeltaTime);

            double d2 = Math.Sqrt(referenceBody.gravParameter * (2.0 / relativePositionAtT2.magnitude - 1.0 / semiMajorAxis));

            Vector3d result = (relativePositionAtT2 - relativePositionAtT3).normalized * d2;

            if (double.IsNaN(result.x))
            {
                Debug.Log(string.Format("Problem! \n {0} - {1} - {2} - {3} - {4}", relativePositionAtT2.ToString(),
                                                                                   relativePositionAtT3.ToString(),
                                                                                   d2,
                                                                                   result.ToString(),
                                                                                   ObT));
            }

            return result;
        }

        public void DrawOrbit()
        {
            if (eccentricity < 1.0)
            {
                for (double i = 0.0; i < MathUtils.TwoPI; i += drawResolution * MathUtils.Deg2Rad)
                {
                    Vector3 v = getPositionFromTrueAnomaly(i % MathUtils.TwoPI).ToVector3();
                    Vector3 v2 = getPositionFromTrueAnomaly((i + drawResolution * MathUtils.Deg2Rad) % MathUtils.TwoPI).ToVector3();

                    Debug.DrawLine(v, v2, Color.Lerp(Color.yellow, Color.green, Mathf.InverseLerp((float)getOrbitalSpeedAtDistance(PeR), (float)getOrbitalSpeedAtDistance(ApR), (float)getOrbitalSpeedAtPos(v))));
                }
            }
            else
            {
                for (double i = -Math.Acos(-(1.0 / eccentricity)) + drawResolution * MathUtils.Deg2Rad; i < Math.Acos(-(1.0 / eccentricity)) - drawResolution * MathUtils.Deg2Rad; i += drawResolution * MathUtils.Deg2Rad)
                {
                    Debug.DrawLine(getPositionFromTrueAnomaly(i), getPositionFromTrueAnomaly(Math.Min(Math.Acos(-(1.0 / eccentricity)), i + drawResolution * MathUtils.Deg2Rad)), Color.green);
                }
            }

            Debug.DrawLine(getPositionAtT(ObT), referenceBody.position, Color.white);
            Debug.DrawLine(referenceBody.position, referenceBody.position + an.xzy * radius, Color.cyan);
            Debug.DrawLine(referenceBody.position, getPositionAtT(0.0), Color.magenta);

            Debug.DrawRay(getPositionAtT(ObT), vel.xzy * 0.0099999997764825821, Color.white);
            Debug.DrawRay(referenceBody.position, h.xzy, Color.blue);
        }

        public static bool PeApIntersects(Orbit primary, Orbit secondary, double threshold)
        {
            if (primary.eccentricity >= 1.0) return primary.PeR < secondary.ApR + threshold;
            if (secondary.eccentricity >= 1.0) return secondary.PeR < primary.ApR + threshold;

            return Math.Max(primary.PeR, secondary.PeR) - Math.Min(primary.ApR, secondary.ApR) <= threshold;
        }

        public static void FindClosestPoints(Orbit p, Orbit s, ref double CD, ref double CCD, ref double FFp, ref double FFs, ref double SFp, ref double SFs, double epsilon, int maxIterations, ref int iterationCount)
        {
            double pInc = p.inclination * MathUtils.Deg2Rad;
            double sInc = s.inclination * MathUtils.Deg2Rad;
            double dInc = pInc - sInc;

            Vector3d vector3d = Vector3d.Cross(s.h, p.h);

            Debug.DrawRay(ScaledSpace.LocalToScaledSpace(p.referenceBody.position), vector3d.xzy * 1000.0, Color.white);

            double num4 = 1.0 / Math.Sin(dInc) * (Math.Sin(pInc) * Math.Cos(sInc) - Math.Sin(sInc) * Math.Cos(pInc) * Math.Cos(p.LAN * MathUtils.Deg2Rad - s.LAN * MathUtils.Deg2Rad));
            double num5 = 1.0 / Math.Sin(dInc) * (Math.Sin(sInc) * Math.Sin(p.LAN * MathUtils.Deg2Rad - s.LAN * MathUtils.Deg2Rad));

            double num6 = Math.Atan2(num5, num4);

            double num7 = 1.0 / Math.Sin(dInc) * (Math.Sin(pInc) * Math.Cos(sInc) * Math.Cos(p.LAN * MathUtils.Deg2Rad - s.LAN * MathUtils.Deg2Rad) - Math.Sin(sInc) * Math.Cos(pInc));
            double num8 = 1.0 / Math.Sin(dInc) * (Math.Sin(pInc) * Math.Sin(p.LAN * MathUtils.Deg2Rad - s.LAN * MathUtils.Deg2Rad));

            double num9 = Math.Atan2(num8, num7);

            FFp = num6 - p.argumentOfPeriapsis * MathUtils.Deg2Rad;
            FFs = num9 - s.argumentOfPeriapsis * MathUtils.Deg2Rad;

            if (p.eccentricity == 0.0 && s.eccentricity == 0.0)
            {
                CD = Vector3d.Distance(p.getPositionFromTrueAnomaly(FFp), s.getPositionFromTrueAnomaly(FFs));
                CCD = CD;
            }

            CD = SolveClosestBSP(ref FFp, ref FFs, dInc, MathUtils.PI, p, s, 0.0001, maxIterations, ref iterationCount);

            Debug.DrawLine(p.referenceBody.position, p.getPositionFromTrueAnomaly(FFp), Color.green);
            Debug.DrawLine(s.referenceBody.position, s.getPositionFromTrueAnomaly(FFs), Color.grey);

            SFp = FFp + MathUtils.PI;
            SFs = FFs + MathUtils.PI;

            CCD = SolveClosestBSP(ref SFp, ref SFs, dInc, MathUtils.PIOver2, p, s, 0.0001, maxIterations, ref iterationCount);

            Debug.DrawLine(p.referenceBody.position, p.getPositionFromTrueAnomaly(SFp), Color.cyan);
            Debug.DrawLine(s.referenceBody.position, s.getPositionFromTrueAnomaly(SFs), Color.magenta);

            CD = Math.Sqrt(CD);
            CCD = Math.Sqrt(CCD);
        }

        private static double SolveClosestBSP(ref double Fp, ref double Fs, double Ir, double dF, Orbit p, Orbit s, double epsilon, int maxIterations, ref int iterationCount)
        {
            double DF = dF;

            if (Math.Abs(Ir) % MathUtils.PI * 2.0 > MathUtils.PIOver2) DF *= -1.0;

            iterationCount = 0;

            double num3 = (p.getRelativePositionFromTrueAnomaly(Fp) - s.getRelativePositionFromTrueAnomaly(Fs)).sqrMagnitude;

            while (DF > 0.0001 && iterationCount < maxIterations)
            {
                double sqrMagnitude1 = (p.getRelativePositionFromTrueAnomaly(Fp + DF) - s.getRelativePositionFromTrueAnomaly(Fs + DF)).sqrMagnitude;
                double sqrMagnitude2 = (p.getRelativePositionFromTrueAnomaly(Fp - DF) - s.getRelativePositionFromTrueAnomaly(Fs - DF)).sqrMagnitude;

                num3 = Math.Min(num3, Math.Min(sqrMagnitude1, sqrMagnitude2));

                if (num3 == sqrMagnitude1)
                {
                    Fp += DF;
                    Fs += DF;
                }
                else if (num3 == sqrMagnitude2)
                {
                    Fp -= DF;
                    Fs -= DF;
                }

                DF *= 0.5;

                iterationCount++;
            }

            return num3;
        }

        public static double SolveClosestApproach(Orbit p, Orbit s, ref double UT, double dT, double threshold, double MinUT, double MaxUT, double epsilon, int maxIterations, ref int iterationCount)
        {
            if (UT < MinUT) return -1.0;
            if (UT > MaxUT) return -1.0;

            iterationCount = 0;

            double pM = Math.Abs((p.getPositionAtUT(UT) - s.getPositionAtUT(UT)).sqrMagnitude);

            while (dT > epsilon && iterationCount < maxIterations)
            {
                double sqrMagnitude1 = (p.getPositionAtUT(UT + dT) - s.getPositionAtUT(UT + dT)).sqrMagnitude;
                double sqrMagnitude2 = (p.getPositionAtUT(UT - dT) - s.getPositionAtUT(UT - dT)).sqrMagnitude;

                if (UT - dT < MinUT) sqrMagnitude2 = 1.7976931348623157E+308;
                if (UT + dT > MaxUT) sqrMagnitude1 = 1.7976931348623157E+308;

                pM = Math.Min(pM, Math.Min(sqrMagnitude1, sqrMagnitude2));

                if (pM == sqrMagnitude2) UT -= dT;
                else if (pM == sqrMagnitude1) UT += dT;

                dT /= 2.0;

                iterationCount++;

                Debug.DrawLine(p.referenceBody.position, p.getPositionAtUT(UT), Color.yellow * 0.5f);
            }

            return Math.Sqrt(pM);
        }

        public static bool SolveSOI_BSP(Orbit p, Orbit s, ref double UT, double dT, double Rsoi, double MinUT, double MaxUT, double epsilon, int maxIterations, ref int iterationCount)
        {
            if (UT < MinUT) return false;
            if (UT > MaxUT) return false;

            iterationCount = 0;

            bool result = false;

            double r = Rsoi * Rsoi;
            double pM = Math.Abs((p.getPositionAtUT(UT) - s.getPositionAtUT(UT)).sqrMagnitude - r);

            while (dT > epsilon && iterationCount < maxIterations)
            {
                double sqrMagnitude1 = (p.getPositionAtUT(UT + dT) - s.getPositionAtUT(UT + dT)).sqrMagnitude - r;
                double sqrMagnitude2 = (p.getPositionAtUT(UT - dT) - s.getPositionAtUT(UT - dT)).sqrMagnitude - r;

                if (UT - dT < MinUT) sqrMagnitude2 = 1.7976931348623157E+308;
                if (UT + dT > MaxUT) sqrMagnitude1 = 1.7976931348623157E+308;

                if (pM < 0.0 || sqrMagnitude1 < 0.0 || sqrMagnitude2 < 0.0) result = true;

                sqrMagnitude1 = Math.Abs(sqrMagnitude1);
                sqrMagnitude2 = Math.Abs(sqrMagnitude2);

                pM = Math.Min(pM, Math.Min(sqrMagnitude1, sqrMagnitude2));

                if (pM == sqrMagnitude2) UT -= dT;
                else if (pM == sqrMagnitude1) UT += dT;

                dT /= 2.0;
                iterationCount++;

                Debug.DrawLine(p.referenceBody.position, p.getPositionAtUT(UT), Color.magenta * 0.5f);
            }
            return result;
        }

        /*      
        public bool debug_returnFullEllipseTrajectory;

        public Trajectory GetPatchTrajectory(int sampleCount)
        {
            Vector3d[] array = new Vector3d[sampleCount];
            double[] array2 = new double[sampleCount];
            float[] array3 = new float[sampleCount];
            if (this.eccentricity < 1.0)
            {
                if (this.patchEndTransition == Orbit.PatchTransitionType.FINAL || this.debug_returnFullEllipseTrajectory)
                {
                    this.sampleInterval = MathUtils.TwoPI / (double)sampleCount;
                    for (int i = 0; i < sampleCount; i++)
                    {
                        this.E = (double)i * this.sampleInterval;
                        this.V = this.getTrueAnomaly(this.E);
                        array3[i] = (float)(this.StartUT + this.GetDTforTrueAnomaly(this.V, 1.7976931348623157E+308));
                        array[i] = this.getRelativePositionFromEccAnomaly(this.E).xzy;
                    }
                }
                else
                {
                    this.fromV = this.TrueAnomalyAtUT(this.StartUT);
                    this.toV = this.TrueAnomalyAtUT(this.EndUT);
                    this.fromE = this.GetEccentricAnomaly(this.fromV);
                    this.toE = this.GetEccentricAnomaly(this.toV);
                    if (this.fromV > this.toV)
                    {
                        this.fromE = -(MathUtils.TwoPI - this.fromE);
                    }
                    this.sampleInterval = (this.toE - this.fromE) / (double)(sampleCount - 5);
                    this.fromE -= this.sampleInterval * 2.0;
                    double dTforTrueAnomaly = this.GetDTforTrueAnomaly(this.fromV, 0.0);
                    for (int j = 0; j < sampleCount; j++)
                    {
                        this.E = this.fromE + this.sampleInterval * (double)j;
                        this.V = this.getTrueAnomaly(this.E);
                        array2[j] = this.V;
                        array3[j] = (float)(this.StartUT + this.GetDTforTrueAnomaly(this.V, dTforTrueAnomaly));
                        array[j] = this.getRelativePositionFromEccAnomaly(this.E).xzy;
                    }
                }
            }
            else
            {
                this.fromV = this.TrueAnomalyAtUT(this.StartUT);
                this.toV = this.TrueAnomalyAtUT(this.EndUT);
                this.fromE = this.GetEccentricAnomaly(this.fromV);
                this.toE = this.GetEccentricAnomaly(this.toV);
                if (this.fromV > 3.1415926535897931)
                {
                    this.fromE = -this.fromE;
                }
                this.sampleInterval = (this.toE - this.fromE) / (double)(sampleCount - 1);
                for (int k = 0; k < sampleCount; k++)
                {
                    this.E = this.fromE + this.sampleInterval * (double)k;
                    this.V = this.getTrueAnomaly(this.E);
                    array3[k] = (float)(this.StartUT + this.GetDTforTrueAnomaly(this.V, 1.7976931348623157E+308));
                    array[k] = this.getRelativePositionFromEccAnomaly(this.E).xzy;
                }
            }
            Vector3d pe;
            Vector3d ap;
            if (this.eccentricity < 1.0)
            {
                pe = this.getRelativePositionAtT(0.0).xzy;
                ap = this.getRelativePositionAtT(this.period * 0.5).xzy;
            }
            else
            {
                pe = this.GetEccVector().xzy.normalized * (-this.semiMajorAxis * (this.eccentricity - 1.0));
                ap = Vector3d.zero;
            }
            Vector3d xzy = this.getRelativePositionAtUT(this.StartUT).xzy;
            Vector3d xzy2 = this.getRelativePositionAtUT(this.EndUT).xzy;
            return new Trajectory(array, array3, array2, pe, ap, xzy, xzy2, Vector3d.zero, this);
        }
        */

        public static Orbit CreateRandomOrbitAround(CelestialBody body)
        {
            Orbit orbit = new Orbit();
            orbit.referenceBody = body;
            orbit.eccentricity = Random.Range(0.0001f, 0.01f);
            orbit.semiMajorAxis = Random.Range((float)body.atmosphereDepth, (float)body.sphereOfInfluence);
            orbit.inclination = Random.Range(-0.001f, 0.001f);
            orbit.LAN = Random.Range(0.999f, 1.001f);
            orbit.argumentOfPeriapsis = Random.Range(0.999f, 1.001f);
            orbit.meanAnomalyAtEpoch = Random.Range(0.999f, 1.001f);
            orbit.epoch = Random.Range(0.999f, 1.001f);

            orbit.Init();

            return orbit;
        }

        public static Orbit CreateRandomOrbitAround(CelestialBody body, double minAltitude, double maxAltitude)
        {
            Orbit orbit = new Orbit();
            orbit.referenceBody = body;
            orbit.eccentricity = Random.Range(0.0001f, 0.01f);
            orbit.semiMajorAxis = Random.Range((float)minAltitude, (float)maxAltitude);
            orbit.inclination = Random.Range(-0.001f, 0.001f);
            orbit.LAN = Random.Range(0.999f, 1.001f);
            orbit.argumentOfPeriapsis = Random.Range(0.999f, 1.001f);
            orbit.meanAnomalyAtEpoch = Random.Range(0.999f, 1.001f);
            orbit.epoch = Random.Range(0.999f, 1.001f);

            orbit.Init();

            return orbit;
        }

        public static Orbit CreateRandomOrbitNearby(Orbit baseOrbit)
        {
            Orbit orbit = new Orbit();
            orbit.eccentricity = baseOrbit.eccentricity + Random.Range(0.0001f, 0.01f);
            orbit.semiMajorAxis = baseOrbit.semiMajorAxis * Random.Range(0.999f, 1.001f);
            orbit.inclination = baseOrbit.inclination + Random.Range(-0.001f, 0.001f);
            orbit.LAN = baseOrbit.LAN * Random.Range(0.999f, 1.001f);
            orbit.argumentOfPeriapsis = baseOrbit.argumentOfPeriapsis * Random.Range(0.999f, 1.001f);
            orbit.meanAnomalyAtEpoch = baseOrbit.meanAnomalyAtEpoch * Random.Range(0.999f, 1.001f);
            orbit.epoch = baseOrbit.epoch;
            orbit.referenceBody = baseOrbit.referenceBody;

            orbit.Init();

            return orbit;
        }

        public static Orbit CreateRandomOrbitFlyBy(CelestialBody tgtBody, double daysToClosestApproach)
        {
            double periapsis = Math.Max(tgtBody.Radius * 3.0, tgtBody.sphereOfInfluence * Random.Range(0f, 1.1f));
            double deltaVatPeriapsis = Random.Range(100f, 500f);

            return CreateRandomOrbitFlyBy(tgtBody.orbit, daysToClosestApproach * 24.0 * 60.0 * 60.0, periapsis, deltaVatPeriapsis);
        }

        public static Orbit CreateRandomOrbitFlyBy(Orbit targetOrbit, double timeToPeriapsis, double periapsis, double deltaVatPeriapsis)
        {
            double universalTime = Planetarium.GetUniversalTime();

            Vector3d relativePositionAtUT = targetOrbit.getRelativePositionAtUT(universalTime + timeToPeriapsis);
            Vector3d orbitalVelocityAtUT = targetOrbit.getOrbitalVelocityAtUT(universalTime + timeToPeriapsis);

            Orbit orbit = new Orbit();

            Vector3d pos = relativePositionAtUT.ToVector3() + Random.onUnitSphere * (float)periapsis;
            Vector3d vel = orbitalVelocityAtUT.ToVector3() + (orbitalVelocityAtUT.normalized.ToVector3() + Random.onUnitSphere) * (float)deltaVatPeriapsis;

            orbit.UpdateFromStateVectors(pos, vel, targetOrbit.referenceBody, universalTime + timeToPeriapsis);

            return orbit;
        }
    }
}