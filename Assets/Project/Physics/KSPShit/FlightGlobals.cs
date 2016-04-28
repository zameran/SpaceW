namespace Experimental
{
    using System;
    using System.Collections.Generic;

    using UnityEngine;

    public class FlightGlobals : MonoBehaviour
    {
        private static FlightGlobals instance;
        public static FlightGlobals Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (FlightGlobals)FindObjectOfType(typeof(FlightGlobals));
                }

                return instance;
            }
        }

        public Vessel activeVessel;
        public static Vessel ActiveVessel
        {
            get
            {
                return Instance.activeVessel;
            }
        }

        public List<Vessel> vessels = new List<Vessel>();
        public static List<Vessel> Vessels
        {
            get
            {
                return Instance.vessels;
            }
        }

        public List<CelestialBody> bodies = new List<CelestialBody>();
        public static List<CelestialBody> Bodies
        {
            get
            {
                return Instance.bodies;
            }
        }

        public static List<GameObject> physicalObjects = new List<GameObject>();

        public static Vector3d GetUpAxis(Vector3d position)
        {
            return (position - GetMainBody(position).Position).normalized;
        }

        public static Vector3d GetUpAxis(CelestialBody body, Vector3d position)
        {
            return (position - body.Position).normalized;
        }

        public static CelestialBody GetMainBody(Vector3d refPos)
        {
            return inSOI(refPos, Instance.bodies[0]);
        }

        private static CelestialBody inSOI(Vector3d pos, CelestialBody body)
        {
            int count = body.orbitingBodies.Count;

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    CelestialBody celestialBody = body.orbitingBodies[i];

                    if ((pos - celestialBody.Position).sqrMagnitude < celestialBody.sphereOfInfluence * celestialBody.sphereOfInfluence)
                    {
                        return inSOI(pos, celestialBody);
                    }
                }
            }

            return body;
        }

        public static Vector3d GetCentrifugalAcc(Vector3d pos, CelestialBody body)
        {
            if (!body.inverseRotation) return Vector3d.zero;

            pos = body.Position - pos;

            return Vector3d.Cross(body.angularVelocity, Vector3d.Cross(body.angularVelocity, pos));
        }

        public static Vector3d GetCoriolisAcc(Vector3d vel, CelestialBody body)
        {
            if (!body.inverseRotation) return Vector3d.zero;

            return -2 * Vector3d.Cross(body.angularVelocity, vel);
        }

        public static Vector3d GetGeeForceAtPosition(Vector3d pos, CelestialBody body)
        {
            Vector3d D = pos - body.Position;

            return D.Normalized() * -(body.gMagnitudeAtCenter / D.sqrMagnitude);
        }

        public static CelestialBody GetMainBody()
        {
            if (ActiveVessel)
            {
                return ActiveVessel.orbitDriver.ReferenceBody;
            }

            return GetMainBody(Vector3.zero);
        }

        public CelestialBody currentMainBody;

        public bool RefFrameIsRotating
        {
            get
            {
                return currentMainBody.rotates && currentMainBody.inverseRotation;
            }
        }
    }
}