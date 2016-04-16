using System.Collections.Generic;

using UnityEngine;

using Vectrosity;

namespace Experimental
{
    public class OrbitRenderer : MonoBehaviour
    {
        public enum DrawMode
        {
            OFF,
            REDRAW_ONLY,
            REDRAW_AND_FOLLOW,
            REDRAW_AND_RECALCULATE
        }

        public DrawMode drawMode = DrawMode.REDRAW_AND_RECALCULATE;

        private static double sampleResolution;

        private static int lineSegments;

        public Orbit orbit;

        public Vessel vessel;

        public OrbitDriver driver;

        private VectorLine orbitLine;

        private int GetSegmentCount(double sampleResolution, int lineSegments)
        {
            return 360 / (int)sampleResolution * lineSegments * 2;
        }

        protected virtual Color GetOrbitColour()
        {
            return XKCDColors.ElectricLime;
        }
    }
}