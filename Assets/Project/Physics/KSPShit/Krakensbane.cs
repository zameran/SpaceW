namespace Experimental
{
    using System;
    using System.Collections.Generic;

    using UnityEngine;

    public class Krakensbane : MonoBehaviour
    {
        private static Krakensbane fetch;

        public float MaxV = 750f;

        public Vector3d FrameVel;

        public Vector3d totalVel;

        public Vector3d RBVel;

        public Vector3d excessV;

        public Vector3d lastCorrection;

        public static float SqrThreshold
        {
            get
            {
                return Krakensbane.fetch.MaxV * Krakensbane.fetch.MaxV;
            }
        }

        public static float Threshold
        {
            get
            {
                return Krakensbane.fetch.MaxV;
            }
        }

        public Krakensbane()
        {

        }

        private void Awake()
        {
            if (Krakensbane.fetch)
            {
                Destroy(this);

                return;
            }

            Krakensbane.fetch = this;
        }

        private void FixedUpdate()
        {
            if (FlightGlobals.ActiveVessel == null || !FlightGlobals.ActiveVessel.GetComponent<Rigidbody>())
            {
                return;
            }

            RBVel = FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().velocity;
            lastCorrection = Vector3d.zero;

            if (FlightGlobals.ActiveVessel.rails)
            {
                FrameVel = Vector3d.zero;
                excessV = Vector3d.zero;
            }
            else if ((RBVel + FrameVel).sqrMagnitude > (MaxV * MaxV))
            {
                excessV = RBVel;
                lastCorrection = -excessV;

                FrameVel += excessV;

                int count = FlightGlobals.Vessels.Count;
                for (int i = 0; i < count; i++)
                {
                    Vessel item = FlightGlobals.Vessels[i];

                    if (!item.rails)
                    {
                        item.ChangeWorldVelocity(-excessV);
                    }
                }

                int num = FlightGlobals.physicalObjects.Count;
                for (int j = 0; j < num; j++)
                {
                    GameObject gameObject = FlightGlobals.physicalObjects[j];
                    if (gameObject != null)
                    {
                        if (gameObject.GetComponent<Rigidbody>() != null)
                        {
                            gameObject.GetComponent<Rigidbody>().AddForce(-excessV, ForceMode.VelocityChange);
                        }
                    }
                }
            }
            else if (!FrameVel.IsZero())
            {
                lastCorrection = FrameVel;

                int count1 = FlightGlobals.Vessels.Count;
                for (int k = 0; k < count1; k++)
                {
                    Vessel vessel = FlightGlobals.Vessels[k];
                    if (!vessel.rails)
                    {
                        vessel.ChangeWorldVelocity(FrameVel);
                    }
                }

                int num1 = FlightGlobals.physicalObjects.Count;
                for (int l = 0; l < num1; l++)
                {
                    GameObject item1 = FlightGlobals.physicalObjects[l];
                    if (item1 != null)
                    {
                        item1.GetComponent<Rigidbody>().AddForce(FrameVel, ForceMode.VelocityChange);
                    }
                }

                FrameVel = Vector3d.zero;
            }

            if (!FrameVel.IsZero())
            {
                //setOffset(FrameVel * Time.fixedDeltaTime);
            }
        }

        public static Vector3d GetFrameVelocity()
        {
            return Krakensbane.fetch.FrameVel;
        }

        public static Vector3 GetFrameVelocityV3f()
        {
            return Krakensbane.fetch.FrameVel;
        }

        public static Vector3d GetLastCorrection()
        {
            return Krakensbane.fetch.lastCorrection;
        }

        public static void ResetVelocityFrame()
        {
            Krakensbane.fetch.FrameVel.Zero();
        }
        /*
        public void setOffset(Vector3d offset)
        {
            int count = FlightGlobals.Bodies.Count;
            for (int i = 0; i < count; i++)
            {
                CelestialBody item = FlightGlobals.Bodies[i];
                item.position = item.position - offset;
            }
            int num = FlightGlobals.Vessels.Count;
            for (int j = 0; j < num; j++)
            {
                Vessel vessel = FlightGlobals.Vessels[j];
                if (vessel.state != Vessel.State.DEAD)
                {
                    int count1 = vessel.parts.Count;
                    for (int k = 0; k < count1; k++)
                    {
                        Part part = vessel.parts[k];
                        int num1 = part.fxGroups.Count;
                        for (int l = 0; l < num1; l++)
                        {
                            FXGroup fXGroup = part.fxGroups[l];
                            if (fXGroup.Active)
                            {
                                int count2 = fXGroup.fxEmitters.Count;
                                for (int m = 0; m < count2; m++)
                                {
                                    ParticleEmitter particleEmitter = fXGroup.fxEmitters[m];
                                    if (particleEmitter.useWorldSpace)
                                    {
                                        Particle[] particleArray = particleEmitter.particles;
                                        for (int n = 0; n < (int)particleArray.Length; n++)
                                        {
                                            particleArray[n].position = particleArray[n].position - offset;
                                        }
                                        particleEmitter.particles = particleArray;
                                    }
                                }
                            }
                        }
                    }
                    if (!vessel.loaded || vessel.packed)
                    {
                        vessel.SetPosition(vessel.transform.position - offset);
                    }
                }
            }
            EffectBehaviour.OffsetParticles(offset);
        }
        */
        private void Start()
        {
            this.FrameVel = Vector3d.zero;
        }
    }
}