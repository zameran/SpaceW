using System;
using UnityEngine;

namespace Experimental
{
    public class OrbitDriverD : MonoBehaviour
    {
        public enum UpdateMode
        {
            TRACK_Phys,
            UPDATE,
            IDLE
        }

        public delegate void CelestialBodyDelegate(CelestialBody body);

        public Vector3d pos;
        public Vector3d vel;

        public Vector3 startVel;
        public Vector3 localCoM;
        public Vector3 CoMoffset;

        private bool isHyperbolic;

        public Orbit orbit = new Orbit();

        public bool drawOrbit;
        public bool reverse;
        public bool frameShift;
        public bool QueuedUpdate;

        public OrbitDriverD.UpdateMode updateMode;
        public OrbitRenderer Renderer;

        private bool ready;

        public Vessel vessel;
        public CelestialBody celestialBody;

        public Color orbitColor = Color.grey;

        public float lowerCamVsSmaRatio = 0.03f;
        public float upperCamVsSmaRatio = 25f;

        public Transform driverTransform;

        public OrbitDriver.CelestialBodyDelegate OnReferenceBodyChange;

        public CelestialBody referenceBody
        {
            get
            {
                return orbit.referenceBody;
            }
            set
            {
                orbit.referenceBody = value;
            }
        }

        private void Awake()
        {
            driverTransform = transform;
            vessel = GetComponent<Vessel>();
            celestialBody = GetComponent<CelestialBody>();
        }

        private void Start()
        {
            switch (updateMode)
            {
                case OrbitDriverD.UpdateMode.TRACK_Phys:
                case OrbitDriverD.UpdateMode.IDLE:
                    if (!referenceBody)
                    {
                        referenceBody = FlightGlobals.getMainBody(driverTransform.position);
                    }
                    TrackRigidbody(referenceBody);
                    break;
                case OrbitDriverD.UpdateMode.UPDATE:
                    orbit.Init();
                    updateFromParameters();
                    break;
            }

            ready = true;
            //Planetarium.Orbits.Add(this);

            if (OnReferenceBodyChange != null) OnReferenceBodyChange(referenceBody);
        }

        private void OnDestroy()
        {
            //if (Planetarium.Orbits == null) return;

            //Planetarium.Orbits.Remove(this);
        }

        private void FixedUpdate()
        {
            if (QueuedUpdate) return;

            UpdateOrbit();
        }

        public void UpdateOrbit()
        {
            if (!ready) return;

            switch (updateMode)
            {
                case OrbitDriverD.UpdateMode.TRACK_Phys:
                case OrbitDriverD.UpdateMode.IDLE:
                    if (!(vessel == null) && !(vessel.rb == null))
                    {
                        TrackRigidbody(referenceBody);
                        CheckDominantBody(driverTransform.position);
                    }
                    break;
                case OrbitDriverD.UpdateMode.UPDATE:
                    if (vessel != null)
                    {
                        CheckDominantBody(referenceBody.position + pos);
                    }
                    updateFromParameters();
                    break;
            }

            if (isHyperbolic && orbit.eccentricity < 1.0)
            {
                isHyperbolic = false;

                if (vessel != null) { }
            }

            if (!isHyperbolic && orbit.eccentricity > 1.0)
            {
                isHyperbolic = true;

                if (vessel != null) { }
            }

            Debug.DrawRay(driverTransform.position, orbit.vel.xzy * Time.fixedDeltaTime, (updateMode != OrbitDriverD.UpdateMode.UPDATE) ? Color.white : Color.cyan);

            if (drawOrbit) orbit.DrawOrbit();
        }

        public void SetOrbitMode(OrbitDriverD.UpdateMode mode)
        {
            updateMode = mode;
        }

        private void CheckDominantBody(Vector3d refPos)
        {
            if (referenceBody != FlightGlobals.getMainBody(refPos))
            {
                RecalculateOrbit(FlightGlobals.getMainBody(refPos));
            }
        }

        private void TrackRigidbody(CelestialBody refBody)
        {
            localCoM = vessel.findLocalCenterOfMass();
            pos = (driverTransform.position + driverTransform.rotation * localCoM - (Vector3)refBody.position);
            pos = pos.xzy;

            if (updateMode == OrbitDriverD.UpdateMode.IDLE)
            {
                vel = orbit.GetRotFrameVel(referenceBody);
            }

            if (vessel != null && vessel.rb != null && !vessel.rb.isKinematic)
            {
                vel = vessel.rb.GetPointVelocity(driverTransform.TransformPoint(localCoM));// + Krakensbane.GetFrameVelocity();
                vel = vel.xzy + orbit.GetRotFrameVel(referenceBody);
            }

            vel = vel + referenceBody.GetFrameVel() - refBody.GetFrameVel();
            pos += vel * Time.fixedDeltaTime;

            orbit.UpdateFromStateVectors(this.pos, this.vel, refBody, Planetarium.GetUniversalTime());
        }

        private void updateFromParameters()
        {
            orbit.UpdateFromUT(Planetarium.GetUniversalTime());
            pos = orbit.pos.xzy;
            vel = orbit.vel;

            if (double.IsNaN(pos.x))
            {
                Debug.Log(string.Format("[OrbitDriver]: ObT : {0} nM : {1} nE : {2} nV : {3} nRadius : {4} nVel {5} nAN : {6} nPeriod : {7}",
                                        orbit.ObT, orbit.meanAnomaly, orbit.eccentricAnomaly,
                                        orbit.trueAnomaly, orbit.radius, vel, orbit.an, orbit.period));

                if (vessel)
                {
                    Debug.LogWarning("[OrbitDriver Warning!]: " + vessel.gameObject.name + " had a NaN Orbit and was removed.");

                    //vessel.Unload();

                    Destroy(vessel.gameObject);
                }
            }

            if (!reverse)
            {
                if (vessel)
                {
                    CoMoffset = driverTransform.rotation * localCoM;
                    vessel.SetPosition(referenceBody.position + pos - (Vector3d)CoMoffset);
                }
                else if (celestialBody)
                {
                    celestialBody.position = referenceBody.position + pos;
                }
                else
                {
                    driverTransform.position = referenceBody.position + pos;
                }
            }
            else
            {
                referenceBody.position = ((!celestialBody) ? (Vector3d)driverTransform.position : celestialBody.position) - pos;
            }
        }

        public void RecalculateOrbit(CelestialBody newReferenceBody)
        {
            if (frameShift) return;
            frameShift = true;

            CelestialBody referenceBody = this.referenceBody;

            if (updateMode == OrbitDriverD.UpdateMode.UPDATE && Time.timeScale > 0f)
            {
                OnRailsSOITransition(orbit, newReferenceBody);
            }
            else
            {
                Debug.Log(string.Format("[OrbitDriver]: Recalculating orbit for {0}: rPos: {1}; rVel: {2} | {3}",
                                        name, referenceBody.name, pos, vel, vel.magnitude));

                TrackRigidbody(newReferenceBody);

                Debug.Log(string.Format("[OrbitDriver]: Recalculated orbit for {0}: rPos: {1}; rVel: {2} | {3}",
                                        name, newReferenceBody.name, pos, orbit.GetVel(), vel.magnitude));

                orbit.epoch = Planetarium.GetUniversalTime() - Time.fixedDeltaTime;
            }

            if (OnReferenceBodyChange != null) OnReferenceBodyChange(newReferenceBody);

            if (vessel != null)
            {
                //GameEvents.onVesselSOIChanged.Fire(new GameEvents.HostedFromToAction<Vessel, CelestialBody>(this.vessel, referenceBody, newReferenceBody));
                //ModuleTripLogger.ForceVesselLog(this.vessel, FlightLog.EntryType.Flyby, newReferenceBody);
            }

            Invoke("unlockFrameSwitch", 1.0f);
        }

        public void OnRailsSOITransition(Orbit ownOrbit, CelestialBody to)
        {
            double TimeWarpCurrentRate = 1;
            double universalTime = Planetarium.GetUniversalTime();
            double vMin = universalTime - 1.0 * TimeWarpCurrentRate;
            double uT = universalTime;
            double SOIsqr = 0.0;

            int bsp = 0;

            if (orbit.referenceBody.HasChild(to))
            {
                SOIsqr = to.sphereOfInfluence * to.sphereOfInfluence;
                bsp = UtilMath.BSPSolver(ref uT, 1.0 * TimeWarpCurrentRate, (double t) => Math.Abs((ownOrbit.getPositionAtUT(t) - to.getPositionAtUT(t)).sqrMagnitude - SOIsqr), vMin, universalTime, 0.01, 64);
            }
            else if (to.HasChild(orbit.referenceBody))
            {
                SOIsqr = orbit.referenceBody.sphereOfInfluence * orbit.referenceBody.sphereOfInfluence;
                bsp = UtilMath.BSPSolver(ref uT, 1.0 * TimeWarpCurrentRate, (double t) => Math.Abs(ownOrbit.getRelativePositionAtUT(t).sqrMagnitude - SOIsqr), vMin, universalTime, 0.01, 64);
            }

            ownOrbit.UpdateFromOrbitAtUT(ownOrbit, uT, to);

            Debug.Log(string.Format("[OrbitDriver]: On-Rails SOI Transition from {0} to {1}. Transition UT Range: {2} - {3}. Transition UT: {4}. Iterations: {5}.",
                                    referenceBody.name, to.name,
                                    vMin.ToString("0.###"),
                                    universalTime.ToString("0.###"),
                                    uT.ToString("0.###"),
                                    bsp));
        }

        private void unlockFrameSwitch()
        {
            frameShift = false;
        }
    }
}