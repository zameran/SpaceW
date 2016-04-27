using UnityEngine;

namespace Experimental
{
    public class Vessel : MonoBehaviour
    {
        private class CenterVectorHelper
        {
            public Vector3 center;

            public float c;

            public CenterVectorHelper()
            {
                this.center = Vector3.zero;
                this.c = 0f;
            }

            public void Clear()
            {
                this.center.x = 0f;
                this.center.y = 0f;
                this.center.z = 0f;
                this.c = 0f;
            }
        }

        private CenterVectorHelper centerHelper = new CenterVectorHelper();

        public Rigidbody rb;

        public Transform centerOfMass;

        public Vector3 velocityLast;
        public Vector3 velocity;

        public OrbitDriver orbitDriver;

        public bool rails = false;

        public bool alreadyOnRails = false;
        public bool alreadyOffRails = false;

        public Vector3 findLocalCenterOfMass()
        {
            return transform.InverseTransformPoint(centerOfMass.transform.position);
        }

        public void SetPosition(Vector3 position)
        {
            if (position.x != float.NaN && position.y != float.NaN && position.z != float.NaN)
                transform.position = position;
        }

        private void recurseCoMs(Vessel part)
        {
            if (part.transform == null)
            {

            }

            if (part.rb != null)
            {
                centerHelper.center = centerHelper.center + (part.rb.worldCenterOfMass * part.rb.mass);
                centerHelper.c = centerHelper.c + part.rb.mass;
            }
        }

        private void Start()
        {
            if (rb != null)
                rb.velocity = new Vector3(0, 0, 50);
        }

        private void Update()
        {
            if (orbitDriver != null)
            {
                if (orbitDriver.orbit != null)
                {
                    if (orbitDriver.orbit.vel.xzy != Vector3d.zero)
                    {
                        //transform.rotation = Quaternion.LookRotation(orbitDriver.orbit.vel.xzy);
                    }
                }
            }

            Debug.DrawLine(transform.position, transform.position + transform.forward * 200, XKCDColors.Adobe);
        }

        public void UpdateRails()
        {
            if (rails)
                GoOnRails();
            else
                GoOffRails();
        }

        private void FixedUpdate()
        {
            if (orbitDriver != null)
            {
                orbitDriver.UpdateOrbit();

                Integrate();

                if (!rails)
                    if (rb != null)
                        rb.velocity = orbitDriver.vel.xzy;
            }
        }

        public void Integrate()
        {
            if (rails) return;

            if(rb != null)
            {
                CelestialBody rB = orbitDriver.orbit.referenceBody;

                Vector3d CoM = findLocalCenterOfMass();

                Vector3 geeForce = FlightGlobals.GetGeeForceAtPosition(CoM) / 100000;
                Vector3 centrifugalForce = FlightGlobals.GetCentrifugalAcc(CoM, rB) / 100000;
                Vector3 coriolisForce = FlightGlobals.GetCoriolisAcc(rb.velocity, rB) / 100000;

                rb.centerOfMass = CoM;
                //rb.AddForce(geeForce, ForceMode.Acceleration);
                //rb.AddForce(centrifugalForce, ForceMode.Acceleration);
                //rb.AddForce(coriolisForce, ForceMode.Acceleration);

                Debug.DrawLine(transform.position, geeForce * 100000, XKCDColors.Amber);
                Debug.DrawLine(transform.position, centrifugalForce * 100000, XKCDColors.Bluegreen);
                Debug.DrawLine(transform.position, coriolisForce * 100000, XKCDColors.Yellowish);
            }
        }

        public void GoOnRails()
        {
            if (alreadyOnRails) return;

            Debug.Log("Vessel now on rails!");

            orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.UPDATE);

            rb.isKinematic = true;
            alreadyOnRails = true;
            alreadyOffRails = false;

            PauseVelocity();
        }

        public void GoOffRails()
        {
            if (alreadyOffRails) return;

            Debug.Log("Vessel now off rails!");
                     
            orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.IDLE);
           
            rb.isKinematic = false;
            alreadyOnRails = false;
            alreadyOffRails = true;

            ResumeVelocity();
        }

        public void PauseVelocity()
        {
            velocityLast = velocity;
            velocity = Vector3.zero;
            rb.velocity = Vector3.zero;
        }

        public void ResumeVelocity()
        {
            velocityLast = Vector3.zero;
            velocity = orbitDriver.orbit.GetVel() - (Vector3d)velocityLast - ((orbitDriver.orbit.referenceBody.inverseRotation) ? Vector3d.zero : orbitDriver.ReferenceBody.GetRFrmVel(transform.position));
            rb.velocity = velocity;
        }
    }
}