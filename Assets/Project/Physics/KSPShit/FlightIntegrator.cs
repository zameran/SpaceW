using UnityEngine;

namespace Experimental
{
    public class FlightIntegrator : MonoBehaviour
    {
        private void FixedUpdate()
        {
            if(FlightGlobals.fetch.vessels.Count > 0)
            {
                for (int i = 0; i < FlightGlobals.fetch.vessels.Count; i++)
                {
                    Integrate(FlightGlobals.fetch.vessels[i]);
                }
            }
        }

        protected virtual void Integrate(Vessel part)
        {
            Vector3 geeForce = FlightGlobals.getGeeForceAtPosition(part.centerOfMass.position);
            Vector3 centrifugalForce = FlightGlobals.getCentrifugalAcc(part.centerOfMass.position, part.orbitDriver.orbit.referenceBody);
            Vector3 coriolisForce = FlightGlobals.getCoriolisAcc(part.velocity, part.orbitDriver.orbit.referenceBody);

            if (part.rb != null)
            {
                //part.rb.mass = part.mass + part.resourceMass + this.GetPhysicslessChildMass(part);
                part.rb.centerOfMass = part.centerOfMass.position;
                part.rb.AddForce(geeForce, ForceMode.Acceleration);
                part.rb.AddForce(centrifugalForce, ForceMode.Acceleration);
                part.rb.AddForce(coriolisForce, ForceMode.Acceleration);
            }
        }
    }
}