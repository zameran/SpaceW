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

        public Vector3 findLocalCenterOfMass()
        {
            return centerOfMass.transform.position;
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        private void Start()
        {

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
            Debug.Log("Vessel now on rails!");

            orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.PLANET);

            rb.isKinematic = true;

            PauseVelocity();
        }

        public void GoOffRails()
        {
            Debug.Log("Vessel now off rails!");
                     
            orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.VESSEL);
           
            rb.isKinematic = false;

            ResumeVelocity();
        }

        public void PauseVelocity()
        {
            velocityLast = velocity;
            velocity = Vector3.zero;
        }

        public void ResumeVelocity()
        {
            velocityLast = Vector3.zero;
            velocity = orbitDriver.orbit.GetVel() - ((orbitDriver.orbit.referenceBody.inverseRotation) ? Vector3d.zero : orbitDriver.referenceBody.getRFrmVel(transform.position));
        }
    }
}