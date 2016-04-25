using System;
using UnityEngine;

namespace Experimental
{
    public class OrbitDriver : MonoBehaviour
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

        public Vector3 CoMLocal;
        public Vector3 CoMOffset;

        private bool isHyperbolic;

        public Orbit orbit = new Orbit();

        public bool drawOrbit;
        public bool reverse;
        public bool frameShift;
        public bool queuedUpdate;

        public UpdateMode updateMode;
        public OrbitRenderer Renderer;

        private bool ready;

        public Vessel vessel;
        public CelestialBody celestialBody;

        public Transform driverTransform;

        public CelestialBodyDelegate OnReferenceBodyChange;

        public CelestialBody ReferenceBody
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
                case UpdateMode.TRACK_Phys:
                case UpdateMode.IDLE:
                    if (!ReferenceBody)
                    {
                        ReferenceBody = FlightGlobals.GetMainBody(driverTransform.position);
                    }
                    TrackRigidbody(ReferenceBody);
                    break;
                case UpdateMode.UPDATE:
                    orbit.Init();
                    updateFromParameters();
                    break;
            }

            ready = true;
            //Planetarium.Orbits.Add(this);

            if (OnReferenceBodyChange != null) OnReferenceBodyChange(ReferenceBody);
        }

        private void OnDestroy()
        {
            //if (Planetarium.Orbits == null) return;

            //Planetarium.Orbits.Remove(this);
        }

        private void FixedUpdate()
        {
            if (queuedUpdate) return;

            UpdateOrbit();
        }

        public void UpdateOrbit()
        {
            if (!ready) return;

            switch (updateMode)
            {
                case UpdateMode.TRACK_Phys:
                case UpdateMode.IDLE:
                    if (!(vessel == null) && !(vessel.rb == null))
                    {
                        TrackRigidbody(ReferenceBody);
                        CheckDominantBody(driverTransform.position);
                    }
                    break;
                case UpdateMode.UPDATE:
                    if (vessel != null)
                    {
                        CheckDominantBody(ReferenceBody.Position + pos);
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

            Debug.DrawRay(driverTransform.position, orbit.vel.xzy * Time.fixedDeltaTime, (updateMode != OrbitDriver.UpdateMode.UPDATE) ? Color.white : Color.cyan);

            if (drawOrbit) orbit.DrawOrbit();
        }

        public void SetOrbitMode(UpdateMode mode)
        {
            updateMode = mode;
        }

        private void CheckDominantBody(Vector3d refPos)
        {
            if (ReferenceBody != FlightGlobals.GetMainBody(refPos))
            {
                RecalculateOrbit(FlightGlobals.GetMainBody(refPos));
            }
        }

        private void TrackRigidbody(CelestialBody refBody)
        {
            CoMLocal = vessel.findLocalCenterOfMass();
            pos = (driverTransform.position + driverTransform.rotation * CoMLocal - (Vector3)refBody.Position);
            pos = pos.xzy;

            if (updateMode == UpdateMode.IDLE)
            {
                vel = orbit.GetRotFrameVel(ReferenceBody);
            }

            if (vessel != null && vessel.rb != null && !vessel.rb.isKinematic)
            {
                vel = vessel.rb.GetPointVelocity(driverTransform.TransformPoint(CoMLocal));// + Krakensbane.GetFrameVelocity();
                vel = vel.xzy + orbit.GetRotFrameVel(ReferenceBody);
            }

            vel = vel + ReferenceBody.GetFrameVel() - refBody.GetFrameVel();
            pos += vel * Time.fixedDeltaTime;

            orbit.UpdateFromStateVectors(pos, vel, refBody, Planetarium.GetUniversalTime());
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
                    CoMOffset = driverTransform.rotation * CoMLocal;
                    vessel.SetPosition(ReferenceBody.Position + pos - (Vector3d)CoMOffset);
                }
                else if (celestialBody)
                {
                    celestialBody.Position = ReferenceBody.Position + pos;
                }
                else
                {
                    driverTransform.position = ReferenceBody.Position + pos;
                }
            }
            else
            {
                ReferenceBody.Position = ((!celestialBody) ? (Vector3d)driverTransform.position : celestialBody.Position) - pos;
            }
        }

        public void RecalculateOrbit(CelestialBody newReferenceBody)
        {
            if (frameShift) return;
            frameShift = true;

            CelestialBody referenceBody = this.ReferenceBody;

            if (updateMode == UpdateMode.UPDATE && Time.timeScale > 0f)
            {
                OnRailsSOITransition(orbit, newReferenceBody);
            }
            else
            {
                Debug.Log(string.Format("[OrbitDriver]: Recalculating orbit for {0}: rPos: {1}; rVel: {2} | {3}",
                                        name, referenceBody.gameObject.name, pos, vel, vel.magnitude));

                TrackRigidbody(newReferenceBody);

                Debug.Log(string.Format("[OrbitDriver]: Recalculated orbit for {0}: rPos: {1}; rVel: {2} | {3}",
                                        name, newReferenceBody.gameObject.name, pos, orbit.GetVel(), vel.magnitude));

                orbit.epoch = Planetarium.GetUniversalTime() - Time.fixedDeltaTime;
            }

            if (OnReferenceBodyChange != null) OnReferenceBodyChange(newReferenceBody);

            if (vessel != null)
            {
                //GameEvents.onVesselSOIChanged.Fire(new GameEvents.HostedFromToAction<Vessel, CelestialBody>(this.vessel, referenceBody, newReferenceBody));
                //ModuleTripLogger.ForceVesselLog(this.vessel, FlightLog.EntryType.Flyby, newReferenceBody);
            }

            Invoke("UnlockFrameSwitch", 1.0f);
        }

        public void OnRailsSOITransition(Orbit ownOrbit, CelestialBody to)
        {
            double TimeWarpCurrentRate = 1;
            double UT = Planetarium.GetUniversalTime();
            double time = UT;
            double vMin = UT - 1.0 * TimeWarpCurrentRate;
            double SOIsqr = 0.0;

            int bsp = 0;

            if (orbit.referenceBody.HasChild(to))
            {
                SOIsqr = to.sphereOfInfluence * to.sphereOfInfluence;
                bsp = UtilMath.BSPSolver(ref UT, 1.0 * TimeWarpCurrentRate, (double t) => Math.Abs((ownOrbit.getPositionAtUT(t) - to.GetPositionAtUT(t)).sqrMagnitude - SOIsqr), vMin, time, 0.01, 64);
            }
            else if (to.HasChild(orbit.referenceBody))
            {
                SOIsqr = orbit.referenceBody.sphereOfInfluence * orbit.referenceBody.sphereOfInfluence;
                bsp = UtilMath.BSPSolver(ref UT, 1.0 * TimeWarpCurrentRate, (double t) => Math.Abs(ownOrbit.getRelativePositionAtUT(t).sqrMagnitude - SOIsqr), vMin, time, 0.01, 64);
            }

            ownOrbit.UpdateFromOrbitAtUT(ownOrbit, UT, to);

            Debug.Log(string.Format("[OrbitDriver]: On-Rails SOI Transition from {0} to {1}. Transition UT Range: {2} - {3}. Transition UT: {4}. Iterations: {5}.",
                                    ReferenceBody.gameObject.name, to.gameObject.name,
                                    vMin.ToString("0.###"),
                                    time.ToString("0.###"),
                                    UT.ToString("0.###"),
                                    bsp));
        }

        private void UnlockFrameSwitch()
        {
            frameShift = false;
        }
    }
}