using UnityEngine;

namespace Experimental
{
    public class Vessel : MonoBehaviour
    {
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
            return centerOfMass.transform.position;
        }

        public void SetPosition(Vector3 position)
        {
            if (position.magnitude != float.NaN)
                transform.position = position;
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
                orbitDriver.UpdateOrbit();
        }

        public void GoOnRails()
        {
            if (alreadyOnRails) return;

            Debug.Log("Vessel now on rails!");

            orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.PLANET);

            //rb.isKinematic = true;
            alreadyOnRails = true;
            alreadyOffRails = false;

            PauseVelocity();
        }

        public void GoOffRails()
        {
            if (alreadyOffRails) return;

            Debug.Log("Vessel now off rails!");
                     
            orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.VESSEL);
           
            //rb.isKinematic = false;
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
            velocity = orbitDriver.orbit.GetVel() - (Vector3d)velocityLast - ((orbitDriver.orbit.referenceBody.inverseRotation) ? Vector3d.zero : orbitDriver.referenceBody.getRFrmVel(transform.position));
            rb.velocity = velocity;
        }
    }
}