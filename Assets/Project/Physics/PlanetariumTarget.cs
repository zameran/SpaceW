using System;

using UnityEngine;

namespace Experimental
{
    public class PlanetariumTarget : MonoBehaviour
    {
        public CelestialBody celestialBody;
        public Vessel vessel;

        private void Awake()
        {
            celestialBody = GetComponent<CelestialBody>();
            vessel = GetComponent<Vessel>();
        }
    }

    public static class TargetExtensions
    {
        public static PlanetariumTarget VesselTarget(this Vessel vessel)
        {
            return vessel.gameObject.GetComponent<PlanetariumTarget>();
        }
    }
}