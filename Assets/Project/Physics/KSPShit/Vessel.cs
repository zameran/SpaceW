using UnityEngine;

namespace Experimental
{
    public class Vessel : MonoBehaviour
    {
        public Rigidbody rb;

        public Transform com;

        public CelestialBody db;

        public Vector3 velocity;

        public OrbitDriver orbitDriver;

        public Orbit tempOrbit;

        private bool rails = false;

        public Vector3 findLocalCenterOfMass()
        {
            return com.transform.position;
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

        public void GoOnRails()
        {
            orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.PLANET);

            tempOrbit = new Orbit(orbitDriver.orbit);

            rb.isKinematic = true;

            PauseVelocity();
        }

        public void GoOffRails()
        {
            orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.VESSEL);

            orbitDriver.orbit = new Orbit(tempOrbit);
            orbitDriver.updateFromParameters();

            rb.isKinematic = false;

            ResumeVelocity();
        }

        public void PauseVelocity()
        {
            this.velocity = Vector3d.zero;
        }

        public void ResumeVelocity()
        {
            Vector3 velocity = Vector3.zero;

            //velocity = orbitDriver.orbit.GetVel() - orbitDriver.referenceBody.getRFrmVel(transform.position); 
            velocity = orbitDriver.orbit.GetVel() - ((orbitDriver.orbit.referenceBody.inverseRotation) ? Vector3d.zero : orbitDriver.referenceBody.getRFrmVel(transform.position));

            this.velocity = velocity;
        }
    }
}