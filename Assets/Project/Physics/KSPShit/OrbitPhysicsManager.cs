namespace Experimental
{
    using UnityEngine;

    public class OrbitPhysicsManager : MonoBehaviour
    {
        private static OrbitPhysicsManager fetch;

        public CelestialBody dominantBody;

        public bool toggleDominantBodyRotation;
        public bool holdVesselUnpack;

        private int releaseUnpackIn;

        private bool started;

        public static CelestialBody DominantBody
        {
            get
            {
                if (!fetch) return null;
                else return fetch.dominantBody;
            }
        }

        public OrbitPhysicsManager()
        {
        }

        private void Awake()
        {
            if (fetch)
            {
                Destroy(this);

                return;
            }

            fetch = this;
        }

        public void checkReferenceFrame()
        {
            if (dominantBody != FlightGlobals.ActiveVessel.orbitDriver.orbit.referenceBody)
            {
                setDominantBody(FlightGlobals.ActiveVessel.orbitDriver.orbit.referenceBody);
            }

            if (!fetch.dominantBody.rotates) return;

            if (FlightGlobals.getAltitudeAtPos(FlightGlobals.ActiveVessel.transform.position) < dominantBody.inverseRotThresholdAltitude && !dominantBody.inverseRotation)
            {
                setRotatingFrame(true);
            }

            if (FlightGlobals.getAltitudeAtPos(FlightGlobals.ActiveVessel.transform.position) > dominantBody.inverseRotThresholdAltitude && dominantBody.inverseRotation)
            {
                setRotatingFrame(false);
            }
        }

        public static void CheckReferenceFrame()
        {
            fetch.checkReferenceFrame();
        }

        private void FixedUpdate()
        {
            if (!started) return;

            checkReferenceFrame();

            if (holdVesselUnpack)
            {
                if (releaseUnpackIn <= 0)
                {
                    holdVesselUnpack = false;
                }
                else
                {
                    releaseUnpackIn = releaseUnpackIn - 1;
                }
            }
        }

        public static void HoldVesselUnpack(int releaseAfter = 1)
        {
            fetch.holdVesselUnpack = true;
            fetch.releaseUnpackIn = releaseAfter;
        }

        private void LateUpdate()
        {
            if (!started) return;

            /*
            for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
            {
                Vessel item = FlightGlobals.Vessels[i];

                if (item.orbitDriver != null)
                {
                    if (!item.rails)
                    {
                        item.rails = true;
                    }
                    else if (item == FlightGlobals.ActiveVessel && !holdVesselUnpack && item.rails)
                    {
                        item.rails = false;
                    }

                    item.UpdateRails();

                    if (item != FlightGlobals.ActiveVessel)
                    {
                        if (item != null)
                        {
                            if (Vector3.Distance(item.transform.position, FlightGlobals.ActiveVessel.transform.position) > item.D1)
                            {
                                if (!item.rails)
                                {
                                    item.rails = true;
                                }
                            }
                            else if (Vector3.Distance(item.transform.position, FlightGlobals.ActiveVessel.transform.position) < item.D2 && item.rails && !holdVesselUnpack)
                            {
                                item.rails = false;
                            }

                            item.UpdateRails();
                        }
                    }
                }
            }
            */
        }

        private void setDominantBody(CelestialBody body)
        {
            dominantBody = body;

            Vector3[] component = new Vector3[FlightGlobals.physicalObjects.Count];

            Debug.Log(string.Format("[OrbitPhysicsManager] : Setting new dominant body: {0} \nFlightGlobals.mainBody: {1}", dominantBody.name, 
                                                                                                    Planetarium.fetch.CurrentMainBody.name));

            foreach (OrbitDriver orbit in Planetarium.Orbits)
            {
                orbit.reverse = false;
            }

            if (dominantBody.orbitDriver)
            {
                SetReverseOrbit(dominantBody.orbitDriver);
            }

            if (FlightGlobals.physicalObjects.Count > 0)
            {
                for (int i = 0; i < FlightGlobals.physicalObjects.Count; i++)
                {
                    if (FlightGlobals.physicalObjects[i] != null)
                    {
                        if (FlightGlobals.physicalObjects[i].GetComponent<Rigidbody>() != null)
                        {
                            component[i] = FlightGlobals.physicalObjects[i].GetComponent<Rigidbody>().velocity - (Vector3)(FlightGlobals.ActiveVessel.orbitDriver.orbit.GetVel() - Krakensbane.GetFrameVelocity());
                        }
                    }
                }
            }

            for (int j = 0; j < FlightGlobals.Vessels.Count; j++)
            {
                Vessel vessel = FlightGlobals.Vessels[j];
                Vector3d velocity = vessel.orbitDriver.orbit.GetVel() - Krakensbane.GetFrameVelocity();

                if (!vessel.rails)
                {
                    if (vessel.GetComponent<Rigidbody>() != null)
                    {
                        vessel.GetComponent<Rigidbody>().velocity = vessel.orbitDriver.orbit.GetVel() - Krakensbane.GetFrameVelocity();
                    }

                    Debug.Log(string.Format("[OrbitPhysicsManager] : Vessel {0} velocity resumed. Reference body: {1}; Velocity: {2}", vessel.name,
                                                                                                               vessel.orbitDriver.orbit.referenceBody.name,
                                                                                                               velocity));
                }
            }

            if (FlightGlobals.physicalObjects.Count > 0)
            {
                for (int l = 0; l < FlightGlobals.physicalObjects.Count; l++)
                {
                    if (FlightGlobals.physicalObjects[l] != null)
                    {
                        if (FlightGlobals.physicalObjects[l].GetComponent<Rigidbody>() != null)
                        {
                            FlightGlobals.physicalObjects[l].GetComponent<Rigidbody>().velocity = (Vector3)(FlightGlobals.ActiveVessel.orbitDriver.orbit.GetVel() - Krakensbane.GetFrameVelocity()) + component[l];
                        }
                    }
                }
            }
        }

        public static void SetDominantBody(CelestialBody body)
        {
            if (!fetch) return;

            fetch.setDominantBody(body);
        }

        private void SetReverseOrbit(OrbitDriver orbit)
        {
            orbit.reverse = true;

            if (orbit.ReferenceBody.orbitDriver != null)
            {
                SetReverseOrbit(orbit.ReferenceBody.orbitDriver);
            }
        }

        private void setRotatingFrame(bool rotatingFrameState)
        {
            dominantBody.inverseRotation = rotatingFrameState;

            Debug.Log(string.Format("[OrbitPhysicsManager] : Reference Frame: {0}", (!rotatingFrameState ? "Inertial" : "Rotating")));

            for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
            {
                Vessel item = FlightGlobals.Vessels[i];

                if (!item.rails)
                {
                    if (!item.rails && !(item.GetComponent<Rigidbody>() == null))
                    {
                        if (!rotatingFrameState)
                        {
                            item.GetComponent<Rigidbody>().velocity = item.GetComponent<Rigidbody>().velocity + (Vector3)dominantBody.GetRFrmVel(item.transform.position);
                        }
                        else
                        {
                            item.GetComponent<Rigidbody>().velocity = item.GetComponent<Rigidbody>().velocity - (Vector3)dominantBody.GetRFrmVel(item.transform.position);
                        }
                    }
                }
            }
        }

        private void Start()
        {
            dominantBody = Planetarium.fetch.CurrentMainBody;
            started = true;
        }
    }
}