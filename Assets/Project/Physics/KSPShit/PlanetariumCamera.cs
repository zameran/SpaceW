namespace Experimental
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class PlanetariumCamera : MonoBehaviour
    {
        private static PlanetariumCamera _fetch;

        public float minPitch = -0.95f;
        public float maxPitch = 0.95f;
        public float startDistance = 30f;
        public float maxDistance = 150000f;
        public float minDistance = 3f;
        public float orbitSensitivity = 0.5f;
        public float zoomScaleFactor = 1.2f;
        public float sharpness = 10f;
        public float camPitch;
        public float camHdg;

        public PlanetariumTarget initialTarget;
        public PlanetariumTarget target;
        public List<PlanetariumTarget> targets;

        public bool TabSwitchTargets;

        private static Camera camRef;

        private float minRadiusDistance;
        private float translateSmooth = 0.1f;
        private float targetHeading;
        private float endHeading;

        private Vector3 cameraVel;

        private float distance;

        private Vector3 endPos;

        private Quaternion testRot;
        private Quaternion endRot;

        private Transform pivot;

        [HideInInspector]
        private CelestialBody b;

        [HideInInspector]
        private double nearest;

        private CelestialBody nearestBody;

        public static Camera Camera
        {
            get
            {
                if (camRef == null)
                {
                    camRef = fetch.GetComponent<Camera>();
                }

                return camRef;
            }
        }

        public float Distance
        {
            get
            {
                return distance;
            }
        }

        public static PlanetariumCamera fetch
        {
            get
            {
                if (_fetch == null)
                {
                    _fetch = (PlanetariumCamera)FindObjectOfType(typeof(PlanetariumCamera));
                }

                return _fetch;
            }
        }

        public Quaternion pivotRotation
        {
            get
            {
                return pivot.rotation;
            }
        }

        private void Awake()
        {
            _fetch = this;

            CreatePivot();
        }

        private void CreatePivot()
        {
            GameObject gameObject = new GameObject("Scaled Camera Pivot");
            pivot = gameObject.transform;
            transform.parent = pivot;
        }

        private CelestialBody GetNearestBody(Vector3d cameraLocalSpacePos)
        {
            nearest = double.MaxValue;

            for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
            {
                b = FlightGlobals.Bodies[i];

                if ((b.Position - cameraLocalSpacePos).sqrMagnitude < nearest)
                {
                    nearest = (b.Position - cameraLocalSpacePos).sqrMagnitude;

                    nearestBody = b;
                }
            }

            return nearestBody;
        }

        public int GetTargetIndex(string targetName)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].gameObject.name == targetName)
                {
                    return i;
                }
            }

            return -1;
        }

        private void UpdateDistance()
        {
            nearestBody = GetNearestBody(ScaledSpace.ScaledToLocalSpace(transform.position));
            distance = (float)Math.Sqrt((transform.position - (Vector3)ScaledSpace.LocalToScaledSpace(nearestBody.Position)).sqrMagnitude);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (FlightGlobals.ActiveVessel == null)
                {
                    SetTarget(FlightGlobals.GetHomeBody());
                    SetDistance(Mathf.Clamp(distance, startDistance, (float)(target.celestialBody.sphereOfInfluence * 1.5 * ScaledSpace.InverseScaleFactor)));
                }
                else
                {
                    SetTarget(FlightGlobals.ActiveVessel.VesselTarget());
                    SetDistance(Mathf.Clamp(distance, Mathf.Max((float)target.vessel.orbitDriver.orbit.semiMajorAxis * ScaledSpace.InverseScaleFactor * 0.5f, minDistance), maxDistance));
                }
            }

            if (!EventSystem.current.IsPointerOverGameObject() && Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
            {
                Ray ray = Camera.ScreenPointToRay(Input.mousePosition);

                for (int i = targets.Count - 1; i >= 0; i--)
                {
                    Renderer component = targets[i].GetComponent<Renderer>();

                    if (component != null && component.bounds.IntersectRay(ray))
                    {
                        SetTarget(targets[i]);

                        return;
                    }

                    Vector3 screenPoint = Camera.WorldToScreenPoint(targets[i].transform.position);
                    screenPoint.z = 0f;

                    Vector3 mousePoint = Input.mousePosition;
                    mousePoint.z = 0f;

                    if ((screenPoint - mousePoint).sqrMagnitude < 144f)
                    {
                        SetTarget(targets[i]);
                        return;
                    }
                }
            }

            float scrollWheelAxis = Input.GetAxis("Mouse ScrollWheel");

            if (TabSwitchTargets && Input.GetKeyDown(KeyCode.Tab) && !Input.GetKey(KeyCode.LeftShift))
            {
                SetTarget(targets[(targets.IndexOf(target) + 1) % targets.Count]);
            }

            if (TabSwitchTargets && Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
            {
                SetTarget(targets[targets.IndexOf(target) - 1 + (targets.IndexOf(target) - 1 >= 0 ? 0 : targets.Count)]);
            }

            if (scrollWheelAxis != 0f && !EventSystem.current.IsPointerOverGameObject())
            {
                distance = Mathf.Clamp(distance * (1f - scrollWheelAxis * zoomScaleFactor), Mathf.Max(minDistance, minRadiusDistance), maxDistance);
            }

            if (Input.GetKey(KeyCode.Plus))
            {
                distance = Mathf.Clamp(distance / (1f + zoomScaleFactor * 0.04f), Mathf.Max(minDistance, minRadiusDistance), maxDistance);
            }

            if (Input.GetKey(KeyCode.Minus))
            {
                distance = Mathf.Clamp(distance * (1f + zoomScaleFactor * 0.04f), Mathf.Max(minDistance, minRadiusDistance), maxDistance);
            }

            if (Input.GetKey(KeyCode.Mouse2))
            {
                camHdg = camHdg + Input.GetAxis("Mouse X") * orbitSensitivity;
                camPitch = camPitch - Input.GetAxis("Mouse Y") * orbitSensitivity;
            }

            //camHdg = camHdg - GameSettings.AXIS_CAMERA_HDG.GetAxis() * orbitSensitivity;
            //camPitch = camPitch - GameSettings.AXIS_CAMERA_PITCH.GetAxis() * orbitSensitivity;

            if (Input.GetKey(KeyCode.UpArrow))
            {
                camPitch = camPitch + 1f * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                camPitch = camPitch - 1f * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                camHdg = camHdg + 1f * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                camHdg = camHdg - 1f * Time.deltaTime;
            }

            camPitch = Mathf.Clamp(camPitch, minPitch, maxPitch);
            endRot = Quaternion.AngleAxis(camHdg * 57.29578f + (float)Planetarium.InverseRotAngle, Vector3.up);

            pivot.rotation = endRot;
            pivot.Rotate(transform.right, camPitch * 57.29578f, Space.World);

            transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.back * distance, sharpness * Time.deltaTime);
            transform.localRotation = Quaternion.LookRotation(-transform.localPosition, Vector3.up);

            pivot.localPosition = Vector3.SmoothDamp(pivot.localPosition, endPos, ref cameraVel, translateSmooth);
        }

        private void ResetPivot()
        {
            if (pivot == null) CreatePivot();

            pivot.parent = ScaledSpace.Instance.transform;
            pivot.localRotation = Quaternion.identity;
        }

        public void SetDistance(float dist)
        {
            distance = Mathf.Clamp(dist, minDistance, maxDistance);
        }

        public int SetTarget(CelestialBody body)
        {
            return SetTarget(body.name);
        }

        public int SetTarget(string name)
        {
            return SetTarget(GetTargetIndex(name));
        }

        public int SetTarget(int tgtIdx)
        {
            if (tgtIdx >= targets.Count || tgtIdx < 0)
            {
                SetTarget(FlightGlobals.ActiveVessel.VesselTarget());

                return -1;
            }

            SetTarget(targets[tgtIdx]);

            return tgtIdx;
        }

        public void SetTarget(PlanetariumTarget tgt)
        {
            if (tgt == null) return;

            target = tgt;
            pivot.parent = tgt.transform;
            endPos = Vector3.zero;

            CelestialBody celestialBody = tgt.celestialBody;
            Vessel vessel = tgt.vessel;

            if (celestialBody == null)
            {
                minRadiusDistance = 0f;

                if (vessel != null)
                {
                    float inverseScaleFactor = (float)vessel.orbitDriver.orbit.semiMajorAxis * ScaledSpace.InverseScaleFactor * 0.5f;
                    distance = Mathf.Clamp(distance, Mathf.Max(inverseScaleFactor, minDistance), maxDistance);
                }
            }
            else
            {
                minRadiusDistance = (float)celestialBody.Radius * ScaledSpace.InverseScaleFactor * 2f;

                if (distance < minRadiusDistance) distance = minRadiusDistance;
            }

            cameraVel = Vector3.zero;
            distance = Mathf.Clamp(distance, Mathf.Max(minDistance, minRadiusDistance), maxDistance);
        }

        private void Start()
        {
            distance = startDistance;
            transform.localPosition = Vector3.back * distance;

            SetTarget(target);
        }
    }
}