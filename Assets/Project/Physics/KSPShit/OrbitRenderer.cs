namespace Experimental
{
    using System;
    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.EventSystems;

    using Vectrosity;

    using ZFramework.Math;

    public class OrbitRenderer : MonoBehaviour
    {
        public AnimationCurve splineEccentricOffset;
        public Texture2D orbitTexture;
        public Material orbitMaterial;
        public OrbitDriver orbitDriver;
        public Vessel orbitVessel;
        public CelestialBody orbitCB;

        private GameObject vectorCanvas;

        public int lineSampleResolution = 15;
        public int lineSegments = 8;

        public float lineWidth = 2.5f;

        public Color orbitColor = Color.grey;

        public bool isFocused;
        public bool is3DLine;

        public DrawMode drawMode = DrawMode.REDRAW_AND_RECALCULATE;

        private double st;
        private double end;
        private double rng;
        private double itv;

        public double eccOffset;

        public float twkOffset;

        private Vector3d[] orbitPoints;

        private VectorLine orbitLine;

        protected Orbit orbit
        {
            get
            {
                return orbitDriver.orbit;
            }
        }

        private void Start()
        {
            orbitPoints = new Vector3d[360 / lineSampleResolution];

            MakeLine(ref orbitLine);

            if (orbitVessel == null)
            {
                DrawOrbit(DrawMode.REDRAW_ONLY);
            }
            else
            {
                DrawOrbit(DrawMode.REDRAW_AND_RECALCULATE);
            }
        }

        private void Update()
        {
            DrawOrbit(drawMode);
        }

        private void OnDestroy()
        {
            VectorLine.Destroy(ref orbitLine);
        }

        public void DrawOrbit(DrawMode mode)
        {
            if (orbitLine == null) return;

            orbitLine.active = true;

            if (mode != DrawMode.OFF)
            {
                orbitLine.SetColor(GetOrbitColour());

                switch (mode)
                {
                    case DrawMode.OFF:
                        {
                            break;
                        }
                    case DrawMode.REDRAW_ONLY:
                        {
                            DrawSpline();
                            break;
                        }
                    case DrawMode.REDRAW_AND_FOLLOW:
                        {
                            DrawSpline();
                            break;
                        }
                    case DrawMode.REDRAW_AND_RECALCULATE:
                        {
                            UpdateSpline();
                            DrawSpline();
                            break;
                        }
                    default:
                        {
                            goto case DrawMode.OFF;
                        }
                }
                return;
            }
        }

        private void DrawSpline()
        {
            if (orbit.eccentricity >= 1)
            {

            }
            else
            {
                double off = 0.0174532923847437;

                eccOffset = (orbit.eccentricAnomaly - off) % MathUtils.TwoPI / MathUtils.TwoPI;
                twkOffset = (float)eccOffset * GetEccOffset((float)eccOffset, (float)orbit.eccentricity, 4f);
            }

            MakeLine(ref orbitLine);
            UpdateSpline();

            if (is3DLine)
                orbitLine.Draw3D();
            else
                orbitLine.Draw();
        }

        private float GetEccOffset(float eccOffset, float ecc, float eccOffsetPower)
        {
            float spline = splineEccentricOffset.Evaluate(eccOffset);
            return 1f + (spline - 1f) * (Mathf.Pow(ecc, eccOffsetPower) / Mathf.Pow(0.9f, eccOffsetPower));
        }

        private Color GetOrbitColour()
        {
            return (!isFocused ? orbitColor : XKCDColors.ElectricLime);
        }

        private int GetSegmentCount(double sampleResolution, int lineSegments)
        {
            return 360 / (int)sampleResolution * lineSegments * 2;
        }

        private void MakeLine(ref VectorLine l)
        {
            if (l != null) VectorLine.Destroy(ref l);

            if (vectorCanvas == null && !is3DLine)
            {
                GameObject vectorCanvasScene = GameObject.Find("VectorCanvas");

                if (vectorCanvasScene != null)
                {
                    vectorCanvas = vectorCanvasScene;
                    vectorCanvas.hideFlags = HideFlags.HideInHierarchy;
                }
            }

            l = new VectorLine(string.Concat(name, "'s Orbit"), 
                new List<Vector3>(GetSegmentCount(lineSampleResolution, lineSegments)), 
                lineWidth, 
                LineType.Discrete);

            l.texture = orbitTexture;
            l.material = orbitMaterial;
            l.continuousTexture = true;
            l.UpdateImmediate = true;
            l.ContinuousTextureOffset = twkOffset;
            l.color = GetOrbitColour();
            l.rectTransform.gameObject.layer = 31;
            l.joins = Joins.Weld;
        }

        public OrbitDriver TargetCastSplines(out OrbitCastHit orbitHit)
        {
            orbitHit = new OrbitCastHit();
            OrbitCastHit orbitCastHit = new OrbitCastHit();

            foreach (OrbitDriver orbit in Planetarium.Orbits)
            {
                if (orbit.Renderer != null)
                {
                    if (!orbit.Renderer.OrbitCast(Input.mousePosition, out orbitCastHit, 18f))
                    {
                        continue;
                    }

                    orbitHit = orbitCastHit;

                    break;
                }
            }

            return orbitHit.driver;
        }

        public bool OrbitCast(Vector3 screenPos, out OrbitCastHit hitInfo, float orbitPixelWidth = 10f)
        {
            float enter;

            Vector3 pos;

            hitInfo = new OrbitCastHit()
            {
                or = this,
                driver = orbitDriver
            };

            if (EventSystem.current.IsPointerOverGameObject() || !orbitLine.active) return false;

            hitInfo.orbitOrigin = orbit.referenceBody.Position.LocalToScaledSpace();

            Plane plane = new Plane(orbit.h.xzy.normalized, hitInfo.orbitOrigin);

            Debug.DrawRay(hitInfo.orbitOrigin, plane.normal * 1000f, Color.cyan);

            Ray ray = Camera.main.ScreenPointToRay(screenPos);

            plane.Raycast(ray, out enter);

            hitInfo.hitPoint = (ray.origin + (ray.direction * enter)) - hitInfo.orbitOrigin;

            if (orbit.eccVec == Vector3d.zero)
            {
                pos = Quaternion.Inverse(
                      Quaternion.LookRotation(-orbit.getPositionFromTrueAnomaly(0).normalized, 
                                               orbit.h.xzy)) * hitInfo.hitPoint;
            }
            else
            {
                pos = Quaternion.Inverse(
                      Quaternion.LookRotation(-orbit.eccVec.xzy, orbit.h.xzy)) * hitInfo.hitPoint;
            }

            hitInfo.mouseTA = (MathUtils.PI - Mathf.Atan2(pos.x, pos.z));

            if (hitInfo.mouseTA < 0)
            {
                hitInfo.mouseTA = MathUtils.TwoPI - hitInfo.mouseTA;
            }

            hitInfo.radiusAtTA = orbit.RadiusAtTrueAnomaly(hitInfo.mouseTA) * ScaledSpace.InverseScaleFactor;
            hitInfo.orbitPoint = (hitInfo.hitPoint.normalized * (float)hitInfo.radiusAtTA) + hitInfo.orbitOrigin;
            hitInfo.UTatTA = orbit.GetUTforTrueAnomaly(hitInfo.mouseTA, 0);
            hitInfo.orbitScreenPoint = Camera.main.WorldToScreenPoint(hitInfo.orbitPoint);
            hitInfo.orbitScreenPoint = new Vector3(hitInfo.orbitScreenPoint.x, hitInfo.orbitScreenPoint.y, 0f);

            if (Vector3.Distance(hitInfo.orbitScreenPoint, Input.mousePosition) < orbitPixelWidth)
            {
                Debug.DrawLine(hitInfo.orbitOrigin, hitInfo.orbitPoint, Color.green);

                return true;
            }

            Debug.DrawRay(hitInfo.orbitOrigin, hitInfo.hitPoint, Color.yellow);

            return false;
        }

        private void UpdateSpline()
        {
            int i;

            if (orbit.eccentricity >= 1)
            {
                st = -Math.Acos(-(1 / orbit.eccentricity));
                end = Math.Acos(-(1 / orbit.eccentricity));

                rng = end - st;
                itv = rng / (orbitPoints.Length - 1);

                for (i = 0; i < orbitPoints.Length; i++)
                {
                    orbitPoints[i] = orbit.getPositionFromEccAnomaly(st + itv * i);
                }
            }
            else
            {
                int resolution = (int)Math.Floor(360 / (double)lineSampleResolution);

                for (i = 0; i < resolution; i++)
                {
                    orbitPoints[i] = orbit.getPositionFromEccAnomaly(i * lineSampleResolution * MathUtils.Deg2Rad);
                }
            }

            Vector3[] scaledOrbitPoints = new Vector3[orbitPoints.Length];

            for (i = 0; i < orbitPoints.Length; i++)
            {
                scaledOrbitPoints[i] = orbitPoints[i].LocalToScaledSpace();
            }

            orbitLine.MakeSpline(scaledOrbitPoints, orbit.eccentricity < 1);
        }

        public enum DrawMode
        {
            OFF,
            REDRAW_ONLY,
            REDRAW_AND_FOLLOW,
            REDRAW_AND_RECALCULATE
        }

        public struct OrbitCastHit
        {
            public Vector3 orbitOrigin;

            public Vector3 hitPoint;

            public Vector3 orbitPoint;

            public Vector3 orbitScreenPoint;

            public double mouseTA;

            public double radiusAtTA;

            public double UTatTA;

            public OrbitRenderer or;

            public OrbitDriver driver;

            public Vector3 GetUpdatedOrbitPoint()
            {
                if (driver.updateMode != OrbitDriver.UpdateMode.IDLE)
                    return or.orbit.getPositionFromTrueAnomaly(mouseTA).LocalToScaledSpace();

                return ScaledSpace.LocalToScaledSpace(or.transform.position);
            }
        }
    }
}