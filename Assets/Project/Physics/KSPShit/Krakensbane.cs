namespace Experimental
{
    using System;
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

        public static float Threshold
        {
            get
            {
                return fetch.MaxV;
            }
        }

        public static float SqrThreshold
        {
            get
            {
                return fetch.MaxV * fetch.MaxV;
            }
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

        private void Start()
        {
            FrameVel = Vector3d.zero;
        }

        private void FixedUpdate()
        {
            RBVel = FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().velocity;
            lastCorrection.Zero();

            //if (!FlightGlobals.ActiveVessel.packed)
            //{
                //if ((RBVel + FrameVel).sqrMagnitude > (MaxV * MaxV) && !FlightGlobals.ActiveVessel.LandedOrSplashed && FlightGlobals.ActiveVessel.state != Vessel.State.DEAD)
                if ((RBVel + FrameVel).sqrMagnitude > (MaxV * MaxV))
                {
                    excessV = RBVel;
                    lastCorrection = -excessV;

                    if (FrameVel.IsZero())
                    {
                        //GameEvents.onKrakensbaneEngage.Fire(-this.excessV);
                    }

                    FrameVel += excessV;

                    for (int i = 0; i < FlightGlobals.Vessels.Count; i++)
                    {
                        Vessel vessel = FlightGlobals.Vessels[i];

                        //if (vessel.loaded && !vessel.packed && vessel.state != Vessel.State.DEAD)
                        //{
                            vessel.ChangeWorldVelocity(-this.excessV);
                        //}
                    }

                    for (int j = 0; j < FlightGlobals.physicalObjects.Count; j++)
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

                    //GameEvents.onKrakensbaneDisengage.Fire(this.FrameVel);

                    for (int k = 0; k < FlightGlobals.Vessels.Count; k++)
                    {
                        Vessel vessel2 = FlightGlobals.Vessels[k];

                        //if (vessel2.loaded && !vessel2.packed && vessel2.state != Vessel.State.DEAD)
                        //{
                            vessel2.ChangeWorldVelocity(FrameVel);
                        //}
                    }

                    for (int l = 0; l < FlightGlobals.physicalObjects.Count; l++)
                    {
                        GameObject gameObject2 = FlightGlobals.physicalObjects[l];

                        if (!(gameObject2 == null))
                        {
                            gameObject2.GetComponent<Rigidbody>().AddForce(FrameVel, ForceMode.VelocityChange);
                        }
                    }

                    FrameVel.Zero();
                }
            //}
            //else
            //{
            //    FrameVel.Zero();
            //    excessV.Zero();
            //}

            if (!FrameVel.IsZero())
            {
                setOffset(FrameVel * Time.fixedDeltaTime);
            }
        }

        public void setOffset(Vector3d offset)
        {
            for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
            {
                FlightGlobals.Bodies[i].position -= offset;
            }

            for (int j = 0; j < FlightGlobals.Vessels.Count; j++)
            {
                Vessel vessel = FlightGlobals.Vessels[j];
                //if (vessel.state != Vessel.State.DEAD)
                //{
                    //if (!vessel.loaded || vessel.packed)
                    //{
                        vessel.SetPosition((Vector3d)vessel.transform.position - offset);
                    //}
                //}
            }

            //EffectBehaviour.OffsetParticles(offset);
        }

        public static Vector3d GetFrameVelocity()
        {
            return fetch.FrameVel;
        }

        public static Vector3 GetFrameVelocityV3f()
        {
            return fetch.FrameVel;
        }

        public static Vector3d GetLastCorrection()
        {
            return fetch.lastCorrection;
        }

        public static void ResetVelocityFrame()
        {
            fetch.FrameVel.Zero();
        }
    }
}