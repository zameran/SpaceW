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

        private bool rails = false;

        public Vector3 findLocalCenterOfMass()
        {
            return centerOfMass.transform.position;
        }

        public void SetPosition(Vector3 position)
        {
            this.transform.position = position;
        }

        private void Start()
        {

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                orbitDriver.ro();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                rails = !rails;

                if (rails)
                    GoOnRails();
                else
                    GoOffRails();
            }

            if (Input.GetKey(KeyCode.G))
            {
                orbitDriver.updateFromParameters();
            }
        }

        private void FixedUpdate()
        {

        }

        public void GoOnRails()
        {
            Debug.Log("Vessel now on rails!");

            orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.PLANET);

            rb.isKinematic = true;

            PauseVelocity();
        }

        public void GoOffRails()
        {
            Debug.Log("Vessel now off rails!");
                     
            orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.VESSEL_ACTIVE);
           
            rb.isKinematic = false;

            ResumeVelocity();
        }

        public void PauseVelocity()
        {
            this.velocityLast = velocity;
            this.velocity = Vector3.zero;
        }

        public void ResumeVelocity()
        {
            this.velocityLast = Vector3.zero;
            this.velocity = orbitDriver.orbit.GetVel() - ((orbitDriver.orbit.referenceBody.inverseRotation) ? Vector3d.zero : orbitDriver.referenceBody.getRFrmVel(transform.position));
        }
    }
}