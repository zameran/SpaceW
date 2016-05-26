using System;

using UnityEngine;

using ZFramework.Math;

namespace Experimental
{
    [Serializable]
    public class Orbit
    {
        private double drawResolution = 15.0;

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

        public Vector3 debugPos;
        public Vector3 debugVel;
        public Vector3 debugH;
        public Vector3 debugAN;
        public Vector3 debugEccVec;

        public double mag;
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

        public bool debug_returnFullEllipseTrajectory;

        public double semiMinorAxis
        {
            get
            {
                if (eccentricity < 1.0)
                    return semiMajorAxis * Math.Sqrt(1.0 - eccentricity * eccentricity);

                return semiMajorAxis * Math.Sqrt(eccentricity * eccentricity - 1.0);
            }
        }

        public double semiLatusRectum
        {
            get { return h.sqrMagnitude / referenceBody.gravParameter; }
        }

        public double PeR
        {
            get { return (1.0 - eccentricity) * semiMajorAxis; }
        }

        public double ApR
        {
            get { return (1.0 + eccentricity) * semiMajorAxis; }
        }

        public double PeA
        {
            get { return PeR - referenceBody.Radius; }
        }

        public double ApA
        {
            get { return ApR - referenceBody.Radius; }
        }

        public Orbit()
        {

        }

        public Orbit(double inclination,
                     double eccentricity,
                     double semiMajorAxis,
                     double LAN,
                     double argumentOfPeriapsis,
                     double meanAnomalyAtEpoch,
                     double epoch, CelestialBody referenceBody)
        {
            this.inclination = inclination;
            this.eccentricity = eccentricity;
            this.semiMajorAxis = semiMajorAxis;
            this.LAN = LAN;
            this.argumentOfPeriapsis = argumentOfPeriapsis;
            this.meanAnomalyAtEpoch = meanAnomalyAtEpoch;
            this.epoch = epoch;
            this.referenceBody = referenceBody;

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
            pos = (orbit.getTruePositionAtUT(UT) - toBody.GetTruePositionAtUT(UT)).xzy;
            vel = orbit.getOrbitalVelocityAtUT(UT) + orbit.referenceBody.GetFrameVelAtUT(UT) - toBody.GetFrameVelAtUT(UT);

            UpdateFromStateVectors(pos, vel, toBody, UT);
        }

        public void UpdateFromStateVectors(Vector3d pos, Vector3d vel, CelestialBody refBody, double UT)
        {
            referenceBody = refBody;
            h = Vector3d.Cross(pos, vel);

            if (h.sqrMagnitude == 0.0)
            {
                inclination = Math.Acos(pos.z / pos.magnitude) * MathUtils.Rad2Deg;
                an = Vector3d.Cross(pos, Vector3d.forward);

                if (an.sqrMagnitude == 0.0)
                    an = Vector3d.right;
            }
            else
            {
                inclination = Math.Acos(h.z / h.magnitude) * MathUtils.Rad2Deg;
                an = Vector3d.Cross(Vector3d.forward, h);
            }

            eccVec = Vector3d.Cross(vel, h) / refBody.gravParameter - pos / pos.magnitude;
            eccentricity = eccVec.magnitude;
            orbitalEnergy = vel.sqrMagnitude / 2.0 - refBody.gravParameter / pos.magnitude;
            semiMajorAxis = eccentricity >= 1.0 ? -semiLatusRectum / (eccVec.sqrMagnitude - 1.0) : -refBody.gravParameter / (2.0 * orbitalEnergy);
            LAN = (an.y < 0.0 ? MathUtils.TwoPI - Math.Acos(an.x / an.magnitude) : Math.Acos(an.x / an.magnitude)) * MathUtils.Rad2Deg;
            argumentOfPeriapsis = Math.Acos(Vector3d.Dot(an, eccVec) / (an.magnitude * eccentricity));

            if (eccVec.z < 0.0) argumentOfPeriapsis = MathUtils.TwoPI - argumentOfPeriapsis;

            LAN = (LAN + Planetarium.InverseRotAngle) % 360.0;
            argumentOfPeriapsis *= MathUtils.Rad2Deg;
            period = MathUtils.TwoPI * Math.Sqrt(Math.Pow(Math.Abs(semiMajorAxis), 3.0) / refBody.gravParameter);
            trueAnomaly = Math.Acos(Vector3d.Dot(eccVec, pos) / (eccentricity * pos.magnitude));

            if (Vector3d.Dot(pos, vel) < 0.0) trueAnomaly = MathUtils.TwoPI - trueAnomaly;
            if (double.IsNaN(trueAnomaly)) trueAnomaly = Math.PI;

            eccentricAnomaly = GetEccentricAnomaly(trueAnomaly);
            meanAnomaly = GetMeanAnomaly(eccentricAnomaly, trueAnomaly);
            meanAnomalyAtEpoch = meanAnomaly;

            if (eccentricity < 1.0)
            {
                orbitPercent = meanAnomaly / (MathUtils.TwoPI);
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

            debugPos = pos;
            debugVel = vel;
            debugH = h;
            debugAN = an;
            debugEccVec = eccVec;
        }

        public void UpdateFromUT(double UT)
        {
            ObT = getObtAtUT(UT);
            an = Quat.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;

            if (!Planetarium.Pause)
            {
                mag = Vector3d.Cross(pos, vel).magnitude;
                h = Quat.AngleAxis(inclination, an) * Planetarium.Zup.Z * (!double.IsNaN(mag) ? Math.Max(mag, 1.0) : 1.0);
            }

            eccVec = Quat.AngleAxis(argumentOfPeriapsis, h) * an * eccentricity;

            if (eccentricity < 1.0)
            {
                meanAnomaly = ObT / period * MathUtils.TwoPI;
                eccentricAnomaly = eccentricity >= 0.9 ? solveEccentricAnomalyExtremeEcc(meanAnomaly, eccentricity, 8) : solveEccentricAnomalyStd(meanAnomaly, eccentricity, 1E-07);
                trueAnomaly = Math.Acos((Math.Cos(eccentricAnomaly) - eccentricity) / (1.0 - eccentricity * Math.Cos(eccentricAnomaly)));

                if (ObT > period / 2.0) trueAnomaly = MathUtils.TwoPI - trueAnomaly;

                radius = semiMajorAxis * (1.0 - eccentricity * eccentricity) / (1.0 + eccentricity * Math.Cos(trueAnomaly));
            }
            else
            {
                if (eccentricity == 1.0) eccentricity += 1E-10;

                meanAnomaly = MathUtils.TwoPI * Math.Abs(ObT) / period;

                if (ObT < 0.0) meanAnomaly *= -1.0;

                eccentricAnomaly = solveEccentricAnomalyHyp(Math.Abs(meanAnomaly), eccentricity, 1E-07);
                trueAnomaly = Math.Atan2(Math.Sqrt(eccentricity * eccentricity - 1.0) * Math.Sinh(eccentricAnomaly), eccentricity - Math.Cosh(eccentricAnomaly));

                if (ObT < 0.0) trueAnomaly = MathUtils.TwoPI - trueAnomaly;

                radius = -semiMajorAxis * (eccentricity * eccentricity - 1.0) / (1.0 + eccentricity * Math.Cos(trueAnomaly));
            }

            orbitPercent = meanAnomaly / MathUtils.TwoPI;

            pos = Quat.AngleAxis(argumentOfPeriapsis + trueAnomaly * MathUtils.Rad2Deg, h) * an * radius;
            vel = getOrbitalVelocityAtObT(ObT + Time.deltaTime);

            orbitalSpeed = vel.magnitude;
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

            debugPos = pos;
            debugVel = vel;
            debugH = h;
            debugAN = an;
            debugEccVec = eccVec;
        }

        public double GetDTforTrueAnomaly(double tA, double wrapAfterSeconds)
        {
            double eccentricAnomaly = GetEccentricAnomaly(tA);
            double meanAnomaly = GetMeanAnomaly(eccentricAnomaly, tA);
            double num = eccentricity >= 1.0 ? Math.Pow(Math.Pow(-semiMajorAxis, 3.0) / referenceBody.gravParameter, 0.5) * meanAnomaly : meanAnomaly / MathUtils.TwoPI * period;
            double d;

            if (eccentricity < 1.0)
            {
                d = tA >= 0.0 ? num - ObT : -ObT - num;

                if (d < -Math.Abs(wrapAfterSeconds)) d += period;
            }
            else
                d = tA >= 0.0 ? num - ObT : -ObT - num;

            if (double.IsNaN(d))
                Debug.Log(("[Orbit] : dT is NaN! tA: " + tA + ", E: " + eccentricAnomaly + ", M: " + meanAnomaly + ", T: " + num));

            return d;
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
            return getRelativePositionAtUT(UT).xzy + referenceBody.GetTruePositionAtUT(UT);
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

            if (eccentricity < 1.0)
            {
                if (double.IsInfinity(UT))
                {
                    Debug.Log(("[Orbit] : getObtAtUT infinite UT on elliptical orbit UT: " + UT.ToString() + ", returning NaN"));
                    return double.NaN;
                }

                Obt = (UT - epoch + ObTAtEpoch) % period;

                if (Obt < 0.0)
                    Obt += period;
            }
            else
            {
                if (double.IsInfinity(UT))
                    return UT;

                Obt = ObTAtEpoch + (UT - epoch);
            }

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
                return Quat.AngleAxis(inclination, Quat.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X) * Planetarium.Zup.Z;

            return h;
        }

        public Vector3d GetEccVector()
        {
            if (!Planetarium.FrameIsRotating()) return eccVec;

            Vector3d axis = Quat.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;

            return Quat.AngleAxis(argumentOfPeriapsis, Quat.AngleAxis(inclination, axis) * Planetarium.Zup.Z) * axis;
        }

        public Vector3d GetANVector()
        {
            if (Planetarium.FrameIsRotating()) return Quat.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;

            return an;
        }

        public Vector3d GetVel()
        {
            Vector3d vector3d = GetFrameVel() - (!(FlightGlobals.ActiveVessel) ? Vector3d.zero : FlightGlobals.ActiveVessel.orbitDriver.ReferenceBody.GetFrameVel());

            return new Vector3d(vector3d.x, vector3d.z, vector3d.y);
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
            return GetVel() - (!referenceBody.inverseRotation ? Vector3d.zero : referenceBody.GetRFrmVel(pos + referenceBody.Position));
        }

        public double GetTimeToPeriapsis()
        {
            if (eccentricity < 1.0 && patchEndTransition != PatchTransitionType.FINAL && StartUT + timeToPe > EndUT)
                return timeToPe - period;

            return timeToPe;
        }

        public double GetEccentricAnomaly(double tA)
        {
            double d;

            if (eccentricity < 1.0)
            {
                d = Math.Acos((eccentricity + Math.Cos(tA)) / (1.0 + eccentricity * Math.Cos(tA)));

                if (tA > Math.PI)
                    d = MathUtils.TwoPI - d;
            }
            else
            {
                d = Math.Abs(eccentricity * Math.Cos(tA) + 1.0) >= 1E-05 ? (eccentricity * Math.Cos(tA) >= -1.0 ? UtilMath.ACosh((eccentricity + Math.Cos(tA)) / (1.0 + eccentricity * Math.Cos(tA))) : double.NaN) : (tA >= Math.PI ? double.NegativeInfinity : double.PositiveInfinity);

                if (double.IsNaN(d))
                    Debug.Log(("[Orbit] : E is NaN! tA: " + tA + ", e = " + eccentricity));
            }

            return d;
        }

        public double GetMeanAnomaly(double E, double tA)
        {
            if (eccentricity < 1.0)
                return E - eccentricity * Math.Sin(E);

            if (double.IsInfinity(E))
                return E;

            return (eccentricity * Math.Sinh(E) - E) * (tA >= Math.PI ? -1.0 : 1.0);
        }

        public double RadiusAtTrueAnomaly(double tA)
        {
            if (eccentricity < 1.0)
                return semiLatusRectum * (1.0 / (1.0 + eccentricity * Math.Cos(tA)));

            return -semiMajorAxis * (eccentricity * eccentricity - 1.0) / (1.0 + eccentricity * Math.Cos(tA));
        }

        public double TrueAnomalyAtRadius(double R)
        {
            return trueAnomalyAtRadiusExtreme(R);
        }

        private double trueAnomalyAtRadiusExtreme(double R)
        {
            double extreme = Vector3d.Cross(getRelativePositionFromEccAnomaly(eccentricAnomaly), getOrbitalVelocityAtObT(ObT)).sqrMagnitude / referenceBody.gravParameter;
            double trueAnomaly;

            if (eccentricity < 1.0)
            {
                R = Math.Min(Math.Max(PeR, R), ApR);
                trueAnomaly = Math.Acos(extreme / (eccentricity * R) - 1.0 / eccentricity);
            }
            else
            {
                R = Math.Max(PeR, R);
                trueAnomaly = Math.PI - Math.Acos(semiMajorAxis * eccentricity / R - semiMajorAxis / (eccentricity * R) + 1.0 / eccentricity);
            }

            return trueAnomaly;
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
                double M = T / period * MathUtils.TwoPI;
                double d = eccentricity >= 0.9 ? solveEccentricAnomalyExtremeEcc(M, eccentricity, 8) : solveEccentricAnomalyStd(M, eccentricity, 1E-07);

                trueAnomaly = Math.Acos((Math.Cos(d) - eccentricity) / (1.0 - eccentricity * Math.Cos(d)));

                if (T > period / 2.0)
                    trueAnomaly = MathUtils.TwoPI - trueAnomaly;
            }
            else
            {
                double M = MathUtils.TwoPI * Math.Abs(T) / period;

                if (T < 0.0)
                    M *= -1.0;

                double d = solveEccentricAnomalyHyp(Math.Abs(M), eccentricity, 1E-07);

                if (double.IsPositiveInfinity(d))
                {
                    trueAnomaly = Math.Acos(-1.0 / eccentricity);
                }
                else if (double.IsNegativeInfinity(d))
                {
                    trueAnomaly = MathUtils.TwoPI - Math.Acos(-1.0 / eccentricity);
                }
                else
                {
                    trueAnomaly = Math.Atan2(Math.Sqrt(eccentricity * eccentricity - 1.0) * Math.Sinh(d), eccentricity - Math.Cosh(d));

                    if (T < 0.0)
                        trueAnomaly = MathUtils.TwoPI - trueAnomaly;
                }
            }

            return trueAnomaly;
        }

        public double solveEccentricAnomaly(double M, double ecc, double maxError, int maxIterations)
        {
            if (eccentricity >= 1.0)
                return solveEccentricAnomalyHyp(M, eccentricity, maxError);

            if (eccentricity < 0.8)
                return solveEccentricAnomalyStd(M, eccentricity, maxError);

            return solveEccentricAnomalyExtremeEcc(M, eccentricity, maxIterations);
        }

        private double solveEccentricAnomalyStd(double M, double ecc, double maxError = 1E-07)
        {
            double i = 1.0;
            double eccentricAnomaly = M + ecc * Math.Sin(M) + 0.5 * ecc * ecc * Math.Sin(2.0 * M);

            while (Math.Abs(i) > maxError)
            {
                double num3 = eccentricAnomaly - ecc * Math.Sin(eccentricAnomaly);

                i = (M - num3) / (1.0 - ecc * Math.Cos(eccentricAnomaly));

                eccentricAnomaly += i;
            }

            return eccentricAnomaly;
        }

        private double solveEccentricAnomalyExtremeEcc(double M, double ecc, int iterations = 8)
        {
            double eccentricAnomaly = M + 0.85 * eccentricity * Math.Sign(Math.Sin(M));

            for (int index = 0; index < iterations; ++index)
            {
                double sina = ecc * Math.Sin(eccentricAnomaly);
                double cosa = ecc * Math.Cos(eccentricAnomaly);
                double x = eccentricAnomaly - sina - M;
                double y = 1.0 - cosa;
                double z = sina;

                eccentricAnomaly += -5.0 * x / (y + Math.Sign(y) * Math.Sqrt(Math.Abs(16.0 * y * y - 20.0 * x * z)));
            }

            return eccentricAnomaly;
        }

        private double solveEccentricAnomalyHyp(double M, double ecc, double maxError = 1E-07)
        {
            if (double.IsInfinity(M)) return M;

            double index = 1.0;
            double eccentricAnomaly = Math.Log(2.0 * M / ecc + 1.8);

            while (Math.Abs(index) > maxError)
            {
                index = (eccentricity * Math.Sinh(eccentricAnomaly) - eccentricAnomaly - M) / (eccentricity * Math.Cosh(eccentricAnomaly) - 1.0);
                eccentricAnomaly -= index;
            }

            return eccentricAnomaly;
        }

        public double getTrueAnomaly(double E)
        {
            double trueAnomaly;

            if (eccentricity < 1.0)
            {
                trueAnomaly = Math.Acos((Math.Cos(E) - eccentricity) / (1.0 - eccentricity * Math.Cos(E)));

                if (E > Math.PI) trueAnomaly = MathUtils.TwoPI - trueAnomaly;
                if (E < 0.0) trueAnomaly *= -1.0;
            }
            else
                trueAnomaly = Math.Atan2(Math.Sqrt(eccentricity * eccentricity - 1.0) * Math.Sinh(E), eccentricity - Math.Cosh(E));

            return trueAnomaly;
        }

        public double GetTrueAnomalyOfZupVector(Vector3d vector)
        {
            Vector3d zUpVector = !(eccVec != Vector3d.zero) ? (Vector3d)(Quaternion.Inverse(Quaternion.LookRotation(-getPositionFromTrueAnomaly(0.0).normalized, GetOrbitNormal().xzy)) * vector.xzy) : (Vector3d)(Quaternion.Inverse(Quaternion.LookRotation(-GetEccVector().xzy, GetOrbitNormal().xzy)) * vector.xzy);

            double trueAnomaly = Math.PI - Math.Atan2(zUpVector.x, zUpVector.z);

            if (trueAnomaly < 0.0) trueAnomaly = MathUtils.TwoPI - trueAnomaly;

            return trueAnomaly;
        }

        public Vector3d getPositionAtT(double T)
        {
            return referenceBody.Position + getRelativePositionAtT(T).xzy;
        }

        public Vector3d getRelativePositionAtT(double T)
        {
            double d1;
            double H;

            if (eccentricity < 1.0)
            {
                double M = T / period * MathUtils.TwoPI;
                double d2 = eccentricity >= 0.9 ? solveEccentricAnomalyExtremeEcc(M, eccentricity, 8) : solveEccentricAnomalyStd(M, eccentricity, 1E-07);

                d1 = Math.Acos((Math.Cos(d2) - eccentricity) / (1.0 - eccentricity * Math.Cos(d2)));

                if (T > period / 2.0) d1 = MathUtils.TwoPI - d1;

                H = semiMajorAxis * (1.0 - eccentricity * eccentricity) / (1.0 + eccentricity * Math.Cos(d1));
            }
            else
            {
                double M = MathUtils.TwoPI * Math.Abs(T) / period;

                if (T < 0.0)
                    M *= -1.0;

                double num3 = solveEccentricAnomalyHyp(Math.Abs(M), eccentricity, 1E-07);

                d1 = Math.Atan2(Math.Sqrt(eccentricity * eccentricity - 1.0) * Math.Sinh(num3), eccentricity - Math.Cosh(num3));

                if (T < 0.0) d1 = MathUtils.TwoPI - d1;

                H = -semiMajorAxis * (eccentricity * eccentricity - 1.0) / (1.0 + eccentricity * Math.Cos(d1));
            }

            Vector3d axis = Quat.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;
            Vector3d normal = Quat.AngleAxis(inclination, axis) * Planetarium.Zup.Z;

            return Quat.AngleAxis(argumentOfPeriapsis + d1 * MathUtils.Rad2Deg, normal) * axis * H;
        }

        public Vector3d getPositionFromMeanAnomaly(double M)
        {
            return referenceBody.Position + getRelativePositionFromMeanAnomaly(M).xzy;
        }

        public Vector3d getRelativePositionFromMeanAnomaly(double M)
        {
            return getRelativePositionFromEccAnomaly(solveEccentricAnomaly(M, eccentricity, 1E-05, 8));
        }

        public Vector3d getPositionFromEccAnomaly(double E)
        {
            return referenceBody.Position + getRelativePositionFromEccAnomaly(E).xzy;
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

            Vector3d position = new Vector3d(x, y, 0.0);
            Vector3d axis = Quat.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;

            return Quat.AngleAxis(argumentOfPeriapsis, Quat.AngleAxis(inclination, axis) * Planetarium.Zup.Z) * Quat.AngleAxis(inclination, axis) * Quat.AngleAxis(LAN - Planetarium.InverseRotAngle, Planetarium.Zup.Z) * position;
        }

        public Vector3d getPositionFromTrueAnomaly(double tA)
        {
            return referenceBody.Position + getRelativePositionFromTrueAnomaly(tA).xzy;
        }

        public Vector3d getRelativePositionFromTrueAnomaly(double tA)
        {
            double H = eccentricity >= 1.0 ? -semiMajorAxis * (eccentricity * eccentricity - 1.0) / (1.0 + this.eccentricity * Math.Cos(tA)) : semiLatusRectum * (1.0 / (1.0 + eccentricity * Math.Cos(tA)));

            Vector3d axis = Quat.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;
            Vector3d normal = Quat.AngleAxis(inclination, axis) * Planetarium.Zup.Z;

            return Quat.AngleAxis(argumentOfPeriapsis + tA * MathUtils.Rad2Deg, normal) * axis * H;
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
            return getOrbitalSpeedAtDistance((referenceBody.Position - pos).magnitude);
        }

        public double getOrbitalSpeedAtDistance(double d)
        {
            return Math.Sqrt(referenceBody.gravParameter * (2.0 / d - 1.0 / semiMajorAxis));
        }

        public Vector3d getOrbitalVelocityAtObT(double ObT)
        {
            return getOrbitalVelocityAtTrueAnomaly(TrueAnomalyAtT(ObT));
        }

        public Vector3d getOrbitalVelocityAtTrueAnomaly(double tA)
        {
            double cosa = Math.Cos(tA);
            double sina = Math.Sin(tA);
            double x = Math.Sqrt(referenceBody.gravParameter / (semiMajorAxis * (1.0 - eccentricity * eccentricity)));
            double y = -sina * x;
            double z = (cosa + eccentricity) * x;

            Vector3d axis = Quat.AngleAxis(LAN, Planetarium.Zup.Z) * Planetarium.Zup.X;
            Vector3d normal = Quat.AngleAxis(inclination, axis) * Planetarium.Zup.Z;

            return Quat.AngleAxis(argumentOfPeriapsis, normal) * axis * y + Quat.AngleAxis(argumentOfPeriapsis + 90.0, normal) * axis * z;
        }

        public void DrawOrbit()
        {
            if (eccentricity < 1.0)
            {
                double tA = 0.0;

                while (tA < MathUtils.TwoPI)
                {
                    Vector3 vector3_1 = getPositionFromTrueAnomaly(tA % MathUtils.TwoPI);
                    Vector3 vector3_2 = getPositionFromTrueAnomaly((tA + drawResolution * MathUtils.Deg2Rad) % MathUtils.TwoPI);

                    Debug.DrawLine(ScaledSpace.LocalToScaledSpace(vector3_1), ScaledSpace.LocalToScaledSpace(vector3_2), Color.Lerp(Color.yellow, Color.green, Mathf.InverseLerp((float)getOrbitalSpeedAtDistance(PeR), (float)getOrbitalSpeedAtDistance(ApR), (float)getOrbitalSpeedAtPos(vector3_1))));

                    tA += drawResolution * MathUtils.Deg2Rad;
                }
            }
            else
            {
                double tA = -Math.Acos(-(1.0 / eccentricity)) + drawResolution * MathUtils.Deg2Rad;

                while (tA < Math.Acos(-(1.0 / eccentricity)) - drawResolution * MathUtils.Deg2Rad)
                {
                    Debug.DrawLine(ScaledSpace.LocalToScaledSpace(getPositionFromTrueAnomaly(tA)), ScaledSpace.LocalToScaledSpace(getPositionFromTrueAnomaly(Math.Min(Math.Acos(-(1.0 / eccentricity)), tA + drawResolution * MathUtils.Deg2Rad))), Color.green);

                    tA += drawResolution * MathUtils.Deg2Rad;
                }
            }

            Debug.DrawLine(ScaledSpace.LocalToScaledSpace(getPositionAtT(ObT)), ScaledSpace.LocalToScaledSpace(referenceBody.Position), Color.green);
            Debug.DrawRay(ScaledSpace.LocalToScaledSpace(getPositionAtT(ObT)), (vel.xzy * 0.00999999977648258), Color.white);
            Debug.DrawLine(ScaledSpace.LocalToScaledSpace(referenceBody.Position), ScaledSpace.LocalToScaledSpace(referenceBody.Position.ToVector3() + (Vector3)(an.xzy * radius)), Color.cyan);
            Debug.DrawLine(ScaledSpace.LocalToScaledSpace(referenceBody.Position), ScaledSpace.LocalToScaledSpace(getPositionAtT(0.0)), Color.magenta);
            Debug.DrawRay(ScaledSpace.LocalToScaledSpace(referenceBody.Position), ScaledSpace.LocalToScaledSpace(h.xzy), Color.blue);
        }

        public static bool PeApIntersects(Orbit primary, Orbit secondary, double threshold)
        {
            if (primary.eccentricity >= 1.0) return primary.PeR < secondary.ApR + threshold;
            if (secondary.eccentricity >= 1.0) return secondary.PeR < primary.ApR + threshold;

            return Math.Max(primary.PeR, secondary.PeR) - Math.Min(primary.ApR, secondary.ApR) <= threshold;
        }

        public static void FindClosestPoints(Orbit p, Orbit s, ref double CD, ref double CCD, ref double FFp, ref double FFs, ref double SFp, ref double SFs, double epsilon, int maxIterations, ref int iterationCount)
        {
            double num1 = p.inclination * MathUtils.Deg2Rad;
            double num2 = s.inclination * MathUtils.Deg2Rad;
            double num3 = num1 - num2;

            Vector3d vector3d = Vector3d.Cross(s.h, p.h);

            Debug.DrawRay(ScaledSpace.LocalToScaledSpace(p.referenceBody.Position), (Vector3)(vector3d.xzy * 1000.0), Color.white);

            double x1 = 1.0 / Math.Sin(num3) * (Math.Sin(num1) * Math.Cos(num2) - Math.Sin(num2) * Math.Cos(num1) * Math.Cos(p.LAN * MathUtils.Deg2Rad - s.LAN * MathUtils.Deg2Rad));
            double num4 = Math.Atan2(1.0 / Math.Sin(num3) * (Math.Sin(num2) * Math.Sin(p.LAN * MathUtils.Deg2Rad - s.LAN * MathUtils.Deg2Rad)), x1);
            double x2 = 1.0 / Math.Sin(num3) * (Math.Sin(num1) * Math.Cos(num2) * Math.Cos(p.LAN * MathUtils.Deg2Rad - s.LAN * MathUtils.Deg2Rad) - Math.Sin(num2) * Math.Cos(num1));
            double num5 = Math.Atan2(1.0 / Math.Sin(num3) * (Math.Sin(num1) * Math.Sin(p.LAN * MathUtils.Deg2Rad - s.LAN * MathUtils.Deg2Rad)), x2);

            FFp = num4 - p.argumentOfPeriapsis * MathUtils.Deg2Rad;
            FFs = num5 - s.argumentOfPeriapsis * MathUtils.Deg2Rad;

            if (p.eccentricity == 0.0 && s.eccentricity == 0.0)
            {
                CD = Vector3d.Distance(p.getPositionFromTrueAnomaly(FFp), s.getPositionFromTrueAnomaly(FFs));
                CCD = CD;
            }

            CD = SolveClosestBSP(ref FFp, ref FFs, num3, Math.PI, p, s, 0.0001, maxIterations, ref iterationCount);

            Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.referenceBody.Position), ScaledSpace.LocalToScaledSpace(p.getPositionFromTrueAnomaly(FFp)), Color.green);
            Debug.DrawLine(ScaledSpace.LocalToScaledSpace(s.referenceBody.Position), ScaledSpace.LocalToScaledSpace(s.getPositionFromTrueAnomaly(FFs)), Color.grey);

            SFp = FFp + Math.PI;
            SFs = FFs + Math.PI;

            CCD = SolveClosestBSP(ref SFp, ref SFs, num3, Math.PI / 2.0, p, s, 0.0001, maxIterations, ref iterationCount);

            Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.referenceBody.Position), ScaledSpace.LocalToScaledSpace(p.getPositionFromTrueAnomaly(SFp)), Color.cyan);
            Debug.DrawLine(ScaledSpace.LocalToScaledSpace(s.referenceBody.Position), ScaledSpace.LocalToScaledSpace(s.getPositionFromTrueAnomaly(SFs)), Color.magenta);

            CD = Math.Sqrt(CD);
            CCD = Math.Sqrt(CCD);
        }

        private static double SolveClosestBSP(ref double Fp, ref double Fs, double Ir, double dF, Orbit p, Orbit s, double epsilon, int maxIterations, ref int iterationCount)
        {
            double dFp = dF;
            double dFs = dF;

            if (Math.Abs(Ir) % Math.PI * 2.0 > Math.PI / 2.0) dFs *= -1.0;

            iterationCount = 0;

            double bsp = (p.getRelativePositionFromTrueAnomaly(Fp) - s.getRelativePositionFromTrueAnomaly(Fs)).sqrMagnitude;

            while (dFp > 0.0001 && iterationCount < maxIterations)
            {
                double sqrMagnitude1 = (p.getRelativePositionFromTrueAnomaly(Fp + dFp) - s.getRelativePositionFromTrueAnomaly(Fs + dFs)).sqrMagnitude;
                double sqrMagnitude2 = (p.getRelativePositionFromTrueAnomaly(Fp - dFp) - s.getRelativePositionFromTrueAnomaly(Fs - dFs)).sqrMagnitude;

                bsp = Math.Min(bsp, Math.Min(sqrMagnitude1, sqrMagnitude2));

                if (bsp == sqrMagnitude1)
                {
                    Fp = Fp + dFp;
                    Fs = Fs + dFs;
                }
                else if (bsp == sqrMagnitude2)
                {
                    Fp = Fp - dFp;
                    Fs = Fs - dFs;
                }

                dFp *= 0.5;
                dFs *= 0.5;

                iterationCount = iterationCount + 1;
            }

            return bsp;
        }

        public static double SolveClosestApproach(Orbit p, Orbit s, ref double UT, double dT, double threshold, double MinUT, double MaxUT, double epsilon, int maxIterations, ref int iterationCount)
        {
            if (UT < MinUT || UT > MaxUT) return -1.0;

            iterationCount = 0;

            double closestAproach = Math.Abs((p.getPositionAtUT(UT) - s.getPositionAtUT(UT)).sqrMagnitude);

            while (dT > epsilon && iterationCount < maxIterations)
            {
                double val1 = (p.getPositionAtUT(UT + dT) - s.getPositionAtUT(UT + dT)).sqrMagnitude;
                double val2 = (p.getPositionAtUT(UT - dT) - s.getPositionAtUT(UT - dT)).sqrMagnitude;

                if (UT - dT < MinUT) val2 = double.MaxValue;
                if (UT + dT > MaxUT) val1 = double.MaxValue;

                closestAproach = Math.Min(closestAproach, Math.Min(val1, val2));

                if (closestAproach == val2) UT = UT - dT;
                else if (closestAproach == val1) UT = UT + dT;

                dT /= 2.0;

                iterationCount = iterationCount + 1;

                Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.referenceBody.Position), ScaledSpace.LocalToScaledSpace(p.getPositionAtUT(UT)), XKCDColors.Lime * 0.5f);
            }

            return Math.Sqrt(closestAproach);
        }

        public static bool SolveSOI_BSP(Orbit p, Orbit s, ref double UT, double dT, double Rsoi, double MinUT, double MaxUT, double epsilon, int maxIterations, ref int iterationCount)
        {
            if (UT < MinUT || UT > MaxUT) return false;

            iterationCount = 0;

            double SOI = Rsoi * Rsoi;
            double BSP = Math.Abs((p.getPositionAtUT(UT) - s.getPositionAtUT(UT)).sqrMagnitude - SOI);

            while (dT > epsilon && iterationCount < maxIterations)
            {
                double num2 = (p.getPositionAtUT(UT + dT) - s.getPositionAtUT(UT + dT)).sqrMagnitude - SOI;
                double num3 = (p.getPositionAtUT(UT - dT) - s.getPositionAtUT(UT - dT)).sqrMagnitude - SOI;

                if (UT - dT < MinUT) num3 = double.MaxValue;
                if (UT + dT > MaxUT) num2 = double.MaxValue;
                if (BSP < 0.0 || num2 < 0.0 || num3 < 0.0) return true;

                double val1_2 = Math.Abs(num2);
                double val2 = Math.Abs(num3);

                BSP = Math.Min(BSP, Math.Min(val1_2, val2));

                if (BSP == val2) UT = UT - dT;
                else if (BSP == val1_2) UT = UT + dT;

                dT /= 2.0;

                iterationCount = iterationCount + 1;

                Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.referenceBody.Position), ScaledSpace.LocalToScaledSpace(p.getPositionAtUT(UT)), XKCDColors.LightMagenta * 0.5f);
            }

            return false;
        }

        public Trajectory GetPatchTrajectory(int sampleCount)
        {
            Vector3d[] points = new Vector3d[sampleCount];

            double[] trueAnomalies = new double[sampleCount];

            float[] times = new float[sampleCount];

            if (eccentricity < 1.0)
            {
                if (patchEndTransition == PatchTransitionType.FINAL || debug_returnFullEllipseTrajectory)
                {
                    sampleInterval = MathUtils.TwoPI / sampleCount;

                    for (int index = 0; index < sampleCount; ++index)
                    {
                        E = index * sampleInterval;
                        V = getTrueAnomaly(E);

                        times[index] = (float)(StartUT + GetDTforTrueAnomaly(V, double.MaxValue));

                        points[index] = getRelativePositionFromEccAnomaly(E).xzy;
                    }
                }
                else
                {
                    fromV = TrueAnomalyAtUT(StartUT);
                    toV = TrueAnomalyAtUT(EndUT);
                    fromE = GetEccentricAnomaly(fromV);
                    toE = GetEccentricAnomaly(toV);

                    if (fromV > toV)
                    {
                        fromE = -(MathUtils.TwoPI - fromE);
                        fromV = -(MathUtils.TwoPI - fromV);
                    }

                    sampleInterval = (toE - fromE) / (sampleCount - 5);
                    fromE -= sampleInterval * 2.0;

                    double dTForTrueAnomaly = GetDTforTrueAnomaly(fromV, 0.0);

                    for (int index = 0; index < sampleCount; ++index)
                    {
                        E = fromE + sampleInterval * index;
                        V = getTrueAnomaly(E);

                        trueAnomalies[index] = V;

                        times[index] = (float)(StartUT + GetDTforTrueAnomaly(V, dTForTrueAnomaly));

                        points[index] = getRelativePositionFromEccAnomaly(E).xzy;
                    }
                }
            }
            else
            {
                fromV = TrueAnomalyAtUT(StartUT);
                toV = TrueAnomalyAtUT(EndUT);
                fromE = GetEccentricAnomaly(fromV);
                toE = GetEccentricAnomaly(toV);

                if (double.IsInfinity(fromE))
                {
                    if (fromE > 0.0) fromV *= 0.95;
                    else fromV = 0.95 * (fromV - MathUtils.TwoPI);

                    fromE = GetEccentricAnomaly(fromV);
                }

                if (double.IsInfinity(toE))
                {
                    if (toE > 0.0) toV *= 0.95;
                    else toV = 0.95 * (toV - MathUtils.TwoPI);

                    toE = GetEccentricAnomaly(toV);
                }

                if (fromV > Math.PI) fromE = -fromE;

                sampleInterval = (toE - fromE) / (sampleCount - 1);

                for (int index = 0; index < sampleCount; ++index)
                {
                    E = fromE + sampleInterval * index;
                    V = getTrueAnomaly(E);

                    times[index] = (float)(StartUT + GetDTforTrueAnomaly(V, double.MaxValue));

                    points[index] = getRelativePositionFromEccAnomaly(E).xzy;
                }
            }

            Vector3d pe;
            Vector3d ap;

            if (eccentricity < 1.0)
            {
                pe = getRelativePositionAtT(0.0).xzy;
                ap = getRelativePositionAtT(period * 0.5).xzy;
            }
            else
            {
                pe = GetEccVector().xzy.normalized * (-semiMajorAxis * (eccentricity - 1.0));
                ap = Vector3d.zero;
            }

            Vector3d patchStartPoint = getRelativePositionAtUT(StartUT).xzy;
            Vector3d patchEndPoint = getRelativePositionAtUT(EndUT).xzy;

            return new Trajectory(points, times, trueAnomalies, pe, ap, patchStartPoint, patchEndPoint, Vector3d.zero, this);
        }

        public static Orbit CreateRandomOrbitAround(CelestialBody body)
        {
            Orbit orbit = new Orbit();

            orbit.referenceBody = body;
            orbit.eccentricity = UnityEngine.Random.Range(0.0001f, 0.01f);
            orbit.semiMajorAxis = UnityEngine.Random.Range((float)body.Radius, (float)body.sphereOfInfluence);
            orbit.inclination = UnityEngine.Random.Range(-1f / 1000f, 1f / 1000f);
            orbit.LAN = UnityEngine.Random.Range(0.999f, 1.001f);
            orbit.argumentOfPeriapsis = UnityEngine.Random.Range(0.999f, 1.001f);
            orbit.meanAnomalyAtEpoch = UnityEngine.Random.Range(0.999f, 1.001f);
            orbit.epoch = UnityEngine.Random.Range(0.999f, 1.001f);

            orbit.Init();

            return orbit;
        }

        public static Orbit CreateRandomOrbitAround(CelestialBody body, double minAltitude, double maxAltitude)
        {
            Orbit orbit = new Orbit();

            orbit.referenceBody = body;
            orbit.eccentricity = UnityEngine.Random.Range(0.0001f, 0.01f);
            orbit.semiMajorAxis = UnityEngine.Random.Range((float)minAltitude, (float)maxAltitude);
            orbit.inclination = UnityEngine.Random.Range(-1f / 1000f, 1f / 1000f);
            orbit.LAN = UnityEngine.Random.Range(0.999f, 1.001f);
            orbit.argumentOfPeriapsis = UnityEngine.Random.Range(0.999f, 1.001f);
            orbit.meanAnomalyAtEpoch = UnityEngine.Random.Range(0.999f, 1.001f);
            orbit.epoch = UnityEngine.Random.Range(0.999f, 1.001f);

            orbit.Init();

            return orbit;
        }

        public static Orbit CreateRandomOrbitNearby(Orbit baseOrbit)
        {
            Orbit orbit = new Orbit();

            orbit.eccentricity = baseOrbit.eccentricity + UnityEngine.Random.Range(0.0001f, 0.01f);
            orbit.semiMajorAxis = baseOrbit.semiMajorAxis * UnityEngine.Random.Range(0.999f, 1.001f);
            orbit.inclination = baseOrbit.inclination + UnityEngine.Random.Range(-1f / 1000f, 1f / 1000f);
            orbit.LAN = baseOrbit.LAN * UnityEngine.Random.Range(0.999f, 1.001f);
            orbit.argumentOfPeriapsis = baseOrbit.argumentOfPeriapsis * UnityEngine.Random.Range(0.999f, 1.001f);
            orbit.meanAnomalyAtEpoch = baseOrbit.meanAnomalyAtEpoch * UnityEngine.Random.Range(0.999f, 1.001f);
            orbit.epoch = baseOrbit.epoch;
            orbit.referenceBody = baseOrbit.referenceBody;

            orbit.Init();

            return orbit;
        }

        public static Orbit CreateRandomOrbitFlyBy(CelestialBody tgtBody, double daysToClosestApproach)
        {
            double periapsis = Math.Max(tgtBody.Radius * 3.0, tgtBody.sphereOfInfluence * UnityEngine.Random.Range(0.0f, 1.1f));
            double deltaVatPeriapsis = UnityEngine.Random.Range(100f, 500f);

            return CreateRandomOrbitFlyBy(tgtBody.Orbit, daysToClosestApproach * 24.0 * 60.0 * 60.0, periapsis, deltaVatPeriapsis);
        }

        public static Orbit CreateRandomOrbitFlyBy(Orbit targetOrbit, double timeToPeriapsis, double periapsis, double deltaVatPeriapsis)
        {
            double universalTime = Planetarium.GetUniversalTime();

            Vector3d relativePositionAtUt = targetOrbit.getRelativePositionAtUT(universalTime + timeToPeriapsis);
            Vector3d orbitalVelocityAtUt = targetOrbit.getOrbitalVelocityAtUT(universalTime + timeToPeriapsis);

            Orbit orbit = new Orbit();

            Vector3d pos = relativePositionAtUt + (Vector3d)UnityEngine.Random.onUnitSphere * periapsis;
            Vector3d vel = orbitalVelocityAtUt + (orbitalVelocityAtUt.normalized + (Vector3d)UnityEngine.Random.onUnitSphere) * deltaVatPeriapsis;

            orbit.UpdateFromStateVectors(pos, vel, targetOrbit.referenceBody, universalTime + timeToPeriapsis);

            return orbit;
        }

        public enum ObjectType
        {
            VESSEL,
            SPACE_DEBRIS,
            CELESTIAL_BODIES,
            UNKNOWN_MISC,
            KERBAL,
        }

        public enum EncounterSolutionLevel
        {
            NONE,
            ESCAPE,
            ORBIT_INTERSECT,
            SOI_INTERSECT_2,
            SOI_INTERSECT_1,
        }

        public enum PatchTransitionType
        {
            INITIAL,
            FINAL,
            ENCOUNTER,
            ESCAPE,
            MANEUVER,
            IMPACT,
        }
    }
}