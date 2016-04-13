namespace Experimental
{
    using System;
    using System.Collections.Generic;

    using UnityEngine;

    public class FlightGlobals : MonoBehaviour
    {
        private static FlightGlobals _fetch;
        public static FlightGlobals fetch
        {
            get
            {
                if (_fetch == null)
                {
                    _fetch = (FlightGlobals)FindObjectOfType(typeof(FlightGlobals));
                }

                return _fetch;
            }
        }

        public Vessel activeVessel;
        public static Vessel ActiveVessel
        {
            get
            {
                return fetch.activeVessel;
            }
        }

        public List<Vessel> vessels = new List<Vessel>();
        public static List<Vessel> Vessels
        {
            get
            {
                return FlightGlobals.fetch.vessels;
            }
        }

        public List<CelestialBody> bodies = new List<CelestialBody>();
        public static List<CelestialBody> Bodies
        {
            get
            {
                return FlightGlobals.fetch.bodies;
            }
        }

        public static List<GameObject> physicalObjects = new List<GameObject>();
    }
}