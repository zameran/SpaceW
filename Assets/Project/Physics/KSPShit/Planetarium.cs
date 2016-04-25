namespace Experimental
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    using UnityEngine;

    public class Planetarium : MonoBehaviour
    {
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public struct ZupVectors
        {
            public Vector3d X
            {
                get
                {
                    return ZupRotation * new Vector3d(1.0, 0.0, 0.0);
                }
            }

            public Vector3d Y
            {
                get
                {
                    return ZupRotation * new Vector3d(0.0, 1.0, 0.0);
                }
            }

            public Vector3d Z
            {
                get
                {
                    return ZupRotation * new Vector3d(0.0, 0.0, 1.0);
                }
            }
        }

        public List<OrbitDriver> orbits;

        public double time;

        public double timeScale = 1.0;

        public bool pause;

        public static Planetarium fetch;

        public QuaternionD rotation;

        public QuaternionD zUpRotation;

        public double inverseRotAngle;

        public static ZupVectors Zup;

        public CelestialBody Sun;

        public CelestialBody Home;

        public CelestialBody CurrentMainBody;

        public double fixedDeltaTime = 0.02;

        public static List<OrbitDriver> Orbits
        {
            get
            {
                return (!fetch) ? null : fetch.orbits;
            }
        }

        public static double TimeScale
        {
            get
            {
                return (!fetch) ? 1.0 : fetch.timeScale;
            }
            set
            {
                if (fetch)
                {
                    fetch.timeScale = value;
                }
            }
        }

        public static bool Pause
        {
            get
            {
                return fetch && fetch.pause;
            }
        }

        public static QuaternionD Rotation
        {
            get
            {
                return fetch.rotation;
            }
            set
            {
                fetch.rotation = value;
            }
        }

        public static QuaternionD ZupRotation
        {
            get
            {
                return fetch.zUpRotation;
            }
            set
            {
                fetch.zUpRotation = value;
            }
        }

        public static double InverseRotAngle
        {
            get
            {
                return fetch.inverseRotAngle;
            }
            set
            {
                fetch.inverseRotAngle = value;
            }
        }

        public static Vector3d up
        {
            get
            {
                return Rotation * new Vector3d(0.0, 1.0, 0.0);
            }
        }

        public static Vector3d forward
        {
            get
            {
                return Rotation * new Vector3d(0.0, 0.0, 1.0);
            }
        }

        public static Vector3d right
        {
            get
            {
                return Rotation * new Vector3d(1.0, 0.0, 0.0);
            }
        }

        private void Awake()
        {
            if (fetch != null)
            {
                throw new Exception("Don't try to instantiate the singleton Planetarium class more than once!");
            }

            fetch = this;

            rotation = Quaternion.Inverse(QuaternionD.AngleAxis(inverseRotAngle, Vector3d.down));
            zUpRotation = Quaternion.Inverse(QuaternionD.AngleAxis(inverseRotAngle, Vector3d.back));
        }

        private void FixedUpdate()
        {
            if (!pause)
            {
                time += fixedDeltaTime * timeScale;

                UpdateCBs();
            }
        }

        public void UpdateCBs()
        {
            if (Sun != null)
            {
                //CurrentMainBody = FindRootBody(Sun);
                //UpdateCBsRecursive(CurrentMainBody);
            }
        }

        private CelestialBody FindRootBody(CelestialBody cb)
        {
            for (int i = 0; i < cb.orbitingBodies.Count; i++)
            {
                if (cb.orbitingBodies[i] != null) continue;

                CelestialBody celestialBody = cb.orbitingBodies[i];

                if (celestialBody.orbitDriver && celestialBody.orbitDriver.reverse)
                {
                    return FindRootBody(celestialBody);
                }
            }

            return cb;
        }

        private void UpdateCBsRecursive(CelestialBody cb)
        {
            cb.Update();

            int count = cb.orbitingBodies.Count;

            for (int i = 0; i < count; i++)
            {
                if (cb.orbitingBodies[i] != null) continue;

                CelestialBody celestialBody = cb.orbitingBodies[i];

                if (!celestialBody.orbitDriver || !celestialBody.orbitDriver.reverse) UpdateCBsRecursive(celestialBody);
            }

            if (cb.orbitDriver && cb.orbitDriver.reverse) UpdateCBsRecursive(cb.referenceBody);
        }

        public static double GetUniversalTime()
        {
            return (!fetch) ? 0.0 : fetch.time; //HighLogic.CurrentGame.UniversalTime
        }

        public static void SetUniversalTime(double t)
        {
            if (fetch)
            {
                fetch.time = t;
            }
        }

        public static bool FrameIsRotating()
        {
            return findRotatingBodiesRecursive(fetch.Sun);
        }

        private static bool findRotatingBodiesRecursive(CelestialBody cb)
        {
            if (cb.rotates && cb.inverseRotation) return true;

            for (int i = 0; i < cb.orbitingBodies.Count; i++)
            {
                if (cb.orbitingBodies[i] != null) continue;
                if (findRotatingBodiesRecursive(cb.orbitingBodies[i])) return true;
            }

            return false;
        }
    }
}