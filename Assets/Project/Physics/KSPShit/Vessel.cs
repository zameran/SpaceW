using UnityEngine;

namespace Experimental
{
    public class Vessel : MonoBehaviour
    {
        public Rigidbody rb;

        public Transform com;

        public CelestialBody db;

        public Vector3 velocity;

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
                GetComponent<OrbitDriver>().ro();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                rails = !rails;

                if (rails)
                    GoOnRails();
                else
                    GoOffRails();
            }
        }

        public void GoOnRails()
        {
            GetComponent<OrbitDriver>().SetOrbitMode(OrbitDriver.UpdateMode.UPDATE);
        }

        public void GoOffRails()
        {
            GetComponent<OrbitDriver>().SetOrbitMode(OrbitDriver.UpdateMode.TRACK_Phys);
        }

        public void ChangeWorldVelocity(Vector3d velOffset)
        {
            rb.AddForce(velOffset, ForceMode.VelocityChange);
        }
    }
}