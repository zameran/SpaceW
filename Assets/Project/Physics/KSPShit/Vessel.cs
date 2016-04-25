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

        public OrbitDriverD orbitDriver;

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
                        transform.rotation = Quaternion.LookRotation(orbitDriver.orbit.vel.xzy);
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
                orbitDriver.UpdateOrbit();
        }

        public void GoOnRails()
        {
            if (alreadyOnRails) return;

            Debug.Log("Vessel now on rails!");

            orbitDriver.SetOrbitMode(OrbitDriverD.UpdateMode.UPDATE);

            rb.isKinematic = true;
            alreadyOnRails = true;
            alreadyOffRails = false;

            PauseVelocity();
        }

        public void GoOffRails()
        {
            if (alreadyOffRails) return;

            Debug.Log("Vessel now off rails!");
                     
            orbitDriver.SetOrbitMode(OrbitDriverD.UpdateMode.TRACK_Phys);
           
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
            velocity = orbitDriver.orbit.GetVel() - (Vector3d)velocityLast - ((orbitDriver.orbit.referenceBody.inverseRotation) ? Vector3d.zero : orbitDriver.referenceBody.getRFrmVel(transform.position));
            rb.velocity = velocity;
        }
    }
}