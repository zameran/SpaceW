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
                center = Vector3.zero;

                c = 0f;
            }

            public void Clear()
            {
                center.x = 0f;
                center.y = 0f;
                center.z = 0f;

                c = 0f;
            }
        }

        private CenterVectorHelper centerHelper = new CenterVectorHelper();

        public Rigidbody rb;

        public Transform centerOfMass;

        public Vector3 velocityLast;
        public Vector3 velocity;

        public Vector3 CoM;
        public Vector3 geeForce;
        public Vector3 centrifugalForce;
        public Vector3 coriolisForce;

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

        private void OnDrawGizmos()
        {
            Gizmos.color = XKCDColors.Liliac;
            Gizmos.DrawWireSphere(CoM, 50);
        }

        private void Start()
        {
            if (rb != null)
                rb.velocity = new Vector3(0, 0, 100);
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
            CoM = transform.position + findLocalCenterOfMass();

            if (orbitDriver != null)
            {
                orbitDriver.UpdateOrbit();

                Integrate();
            }
        }

        public void Integrate()
        {
            if (rails) return;

            if (rb != null)
            {
                CelestialBody rB = orbitDriver.orbit.referenceBody;

                geeForce = FlightGlobals.GetGeeForceAtPosition(CoM, rB);
                centrifugalForce = FlightGlobals.GetCentrifugalAcc(CoM, rB);
                coriolisForce = FlightGlobals.GetCoriolisAcc(rb.velocity, rB);

                rb.centerOfMass = CoM;

                rb.AddForce(geeForce, ForceMode.Acceleration);
                rb.AddForce(centrifugalForce, ForceMode.Acceleration);
                rb.AddForce(coriolisForce, ForceMode.Acceleration);

                Debug.DrawLine(CoM, coriolisForce, XKCDColors.Yellowish);
                Debug.DrawLine(CoM, centrifugalForce, XKCDColors.Bluegreen);
                Debug.DrawLine(CoM, geeForce, XKCDColors.Moss);

                Debug.DrawLine(rB.Position, geeForce, XKCDColors.Red);
            }
        }

        public void GoOnRails()
        {
            if (alreadyOnRails) return;

            orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.UPDATE);

            rb.isKinematic = true;
            alreadyOnRails = true;
            alreadyOffRails = false;

            PauseVelocity();
        }

        public void GoOffRails()
        {
            if (alreadyOffRails) return;

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

        public void ChangeWorldVelocity(Vector3d velOffset)
        {
            if (rails) return;

            if (rb != null) rb.AddForce(velOffset, ForceMode.VelocityChange);
        }
    }
}