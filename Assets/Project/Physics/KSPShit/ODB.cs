using System;
using UnityEngine;

namespace Experimental
{
    public class OrbitDriverD : MonoBehaviour
    {
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

        public UpdateMode updateMode;

        private bool ready;

        public Vessel vessel;

        public CelestialBody celestialBody;

        public Transform driverTransform;

        public CelestialBodyDelegate OnReferenceBodyChange;

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

        public OrbitDriverD()
        {

        }

        private void Awake()
        {
            driverTransform = transform;
            vessel = GetComponent<Vessel>();
            celestialBody = GetComponent<CelestialBody>();
        }

        private void CheckDominantBody(Vector3d refPos)
        {
            if (referenceBody != FlightGlobals.getMainBody(refPos))
            {
                RecalculateOrbit(FlightGlobals.getMainBody(refPos));
            }
        }

        private void FixedUpdate()
        {
            if (QueuedUpdate) return;

            UpdateOrbit();
        }

        public void OnRailsSOITransition(Orbit ownOrbit, CelestialBody to)
        {
            double universalTime = Planetarium.GetUniversalTime();
            double currentRate = universalTime - 1 * 1;
            double UT = universalTime;
            double SOI = 0;

            if (orbit.referenceBody.HasChild(to))
            {
                SOI = to.sphereOfInfluence * to.sphereOfInfluence;
                UtilMath.BSPSolver(ref UT, 1 * 1, (double t) => Math.Abs((ownOrbit.getPositionAtUT(t) - to.getPositionAtUT(t)).sqrMagnitude - SOI), currentRate, universalTime, 0.01, 64);
            }
            else if (to.HasChild(orbit.referenceBody))
            {
                SOI = orbit.referenceBody.sphereOfInfluence * orbit.referenceBody.sphereOfInfluence;
                UtilMath.BSPSolver(ref UT, 1 * 1, (double t) => Math.Abs(ownOrbit.getRelativePositionAtUT(t).sqrMagnitude - SOI), currentRate, universalTime, 0.01, 64);
            }

            ownOrbit.UpdateFromOrbitAtUT(ownOrbit, UT, to);
        }

        public void RecalculateOrbit(CelestialBody newReferenceBody)
        {
            if (frameShift) return;

            frameShift = true;

            if (updateMode != UpdateMode.UPDATE || Time.timeScale <= 0f)
            {
                TrackRigidbody(newReferenceBody);
                orbit.epoch = Planetarium.GetUniversalTime() - Time.fixedDeltaTime;
            }
            else
            {
                OnRailsSOITransition(orbit, newReferenceBody);
            }

            if (OnReferenceBodyChange != null)
            {
                OnReferenceBodyChange(newReferenceBody);
            }

            Invoke("unlockFrameSwitch", 1f);
        }

        public void SetOrbitMode(UpdateMode mode)
        {
            updateMode = mode;
        }

        private void Start()
        {
            switch (updateMode)
            {
                case UpdateMode.TRACK_Phys:
                case UpdateMode.IDLE:
                    {
                        if (!referenceBody)
                        {
                            referenceBody = FlightGlobals.getMainBody(driverTransform.position);
                        }
                        TrackRigidbody(referenceBody);
                        break;
                    }
                case UpdateMode.UPDATE:
                    {
                        orbit.Init();
                        updateFromParameters();
                        break;
                    }
            }

            ready = true;
            //Planetarium.Orbits.Add(this);

            if (OnReferenceBodyChange != null)
            {
                OnReferenceBodyChange(referenceBody);
            }
        }

        private void TrackRigidbody(CelestialBody refBody)
        {
            localCoM = vessel.findLocalCenterOfMass();

            Vector3d position = (Vector3d)(driverTransform.position + (driverTransform.rotation * localCoM)) - refBody.position;

            pos = position.xzy;

            if (updateMode == UpdateMode.IDLE)
            {
                vel = orbit.GetRotFrameVel(referenceBody);
            }

            if (vessel != null && vessel.rb != null && !vessel.rb.isKinematic)
            {
                vel = vessel.velocity + vessel.rb.GetPointVelocity(driverTransform.TransformPoint(localCoM));
                vel = vel.xzy + orbit.GetRotFrameVel(referenceBody);
            }

            vel = (vel + referenceBody.GetFrameVel()) - refBody.GetFrameVel();

            pos = pos + (vel * Time.fixedDeltaTime);
            orbit.UpdateFromStateVectors(pos, vel, refBody, Planetarium.GetUniversalTime());
        }

        private void updateFromParameters()
        {
            Vector3d position;

            orbit.UpdateFromUT(Planetarium.GetUniversalTime());

            pos = orbit.pos.xzy;
            vel = orbit.vel;

            if (double.IsNaN(pos.x))
            {

            }

            if (reverse)
            {
                CelestialBody celestialBody = this.referenceBody;
                if (!this.celestialBody)
                {
                    position = this.driverTransform.position;
                }
                else
                {
                    position = this.celestialBody.position;
                }

                celestialBody.position = position - pos;
            }
            else if (vessel)
            {
                CoMoffset = driverTransform.rotation * localCoM;
                vessel.SetPosition((referenceBody.position + pos) - (Vector3d)CoMoffset);
            }
            else if (!this.celestialBody)
            {
                driverTransform.position = referenceBody.position + pos;
            }
            else
            {
                celestialBody.position = referenceBody.position + pos;
            }
        }

        public void UpdateOrbit()
        {
            if (!ready) return;

            switch (updateMode)
            {
                case UpdateMode.TRACK_Phys:
                case UpdateMode.IDLE:
                    {
                        if (vessel != null || vessel.rb != null)
                        {
                            TrackRigidbody(referenceBody);
                            CheckDominantBody(driverTransform.position);
                        }
                        break;
                    }
                case UpdateMode.UPDATE:
                    {
                        if (vessel != null)
                        {
                            CheckDominantBody(referenceBody.position + pos);
                        }
                        updateFromParameters();
                        break;
                    }
            }

            if (isHyperbolic && orbit.eccentricity < 1)
            {
                isHyperbolic = false;

                if (vessel != null) { }
            }

            if (!isHyperbolic && orbit.eccentricity > 1)
            {
                isHyperbolic = true;

                if (vessel != null) { }
            }

            Debug.DrawRay(driverTransform.position, orbit.vel.xzy * Time.deltaTime, (updateMode != UpdateMode.UPDATE ? Color.white : Color.cyan));

            if (drawOrbit)
            {
                orbit.DrawOrbit();
            }
        }

        public delegate void CelestialBodyDelegate(CelestialBody body);

        public enum UpdateMode
        {
            TRACK_Phys,
            UPDATE,
            IDLE
        }
    }
}