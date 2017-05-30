using SpaceEngine.AtmosphericScattering.Sun;
using SpaceEngine.Core.Bodies;

using UnityEngine;

namespace SpaceEngine.Cameras
{
    [RequireComponent(typeof(Camera))]
    public class FloatingOrigin : MonoBehaviour
    {
        public bool Shift = true;

        public float Threshold = 25000.0f;

        private void Start()
        {

        }

        private void LateUpdate()
        {
            if (Shift)
            {
                var cameraPosition = transform.position;

                if (cameraPosition.magnitude > Threshold)
                {
                    var suns = FindObjectsOfType<AtmosphereSun>();
                    var bodies = FindObjectsOfType<CelestialBody>();

                    foreach (var sun in suns)
                    {
                        var sunTransform = sun.transform;

                        if (sunTransform.parent == null)
                        {
                            sun.transform.position -= cameraPosition;
                        }
                    }

                    foreach (var body in bodies)
                    {
                        var bodyTransform = body.transform;

                        if (bodyTransform.parent == null)
                        {
                            body.Origin -= cameraPosition;
                        }
                    }

                    if (transform.parent == null) transform.position -= cameraPosition;
                }
            }
        }
    }
}