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
        public Texture2D orbitFadeTexture;
        public Material orbitMaterial;
        public OrbitDriver orbitDriver;
        public Vessel orbitVessel;
        public CelestialBody orbitCB;

        private GameObject vectorCanvas;

        public int lineSampleResolution = 16;
        public int lineSegments = 8;

        public float lineWidth = 2.5f;

        public Color orbitColor = Color.grey;

        public bool isJunk;
        public bool isFocused;
        public bool isOffsettable;
        public bool is3DLine;
        public bool UseEccOrbitData;

        public DrawMode drawMode = DrawMode.REDRAW_AND_RECALCULATE;

        private double eccOffset;

        private float twkOffset;
        private float textureOffset;

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
            if (orbitLine != null) VectorLine.Destroy(ref orbitLine);
            if (orbitMaterial != null) orbitMaterial.SetTextureOffset("_MainTex", new Vector2(0, 0));
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
                            UpdateSpline();
                            DrawSpline();
                            break;
                        }
                    case DrawMode.REDRAW_AND_RECALCULATE:
                        {
                            MakeLine(ref orbitLine);
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
                textureOffset = 0;
            }
            else
            {
                eccOffset = (orbit.eccentricAnomaly - MathUtils.Deg2Rad * 2) % MathUtils.TwoPI / MathUtils.TwoPI;
                twkOffset = (float)eccOffset * GetEccOffset((float)eccOffset, (float)orbit.eccentricity, 4.0f);

                textureOffset = 1.0f - twkOffset;
            }

            if (is3DLine)
                orbitLine.Draw3D();
            else
                orbitLine.Draw();
        }

        private float GetEccOffset(float eccOffset, float ecc, float eccOffsetPower)
        {
            float spline = splineEccentricOffset.Evaluate(eccOffset);
            return 1.0f + (spline - 1.0f) * (Mathf.Pow(ecc, eccOffsetPower) / Mathf.Pow(0.9f, eccOffsetPower));
        }

        private Color GetOrbitColour()
        {
            return (!isFocused ? (isJunk ? XKCDColors.OffWhite : orbitColor) : XKCDColors.ElectricLime);
        }

        private int GetSegmentCount(double sampleResolution, int lineSegments)
        {
            return 360 / (int)sampleResolution * lineSegments * 2;
        }

        private void MakeLine(ref VectorLine l)
        {
            if (l != null) VectorLine.Destroy(ref l);

            string orbitName = string.Concat(name, "'s Orbit");

            l = new VectorLine(orbitName,
                new List<Vector3>(GetSegmentCount(lineSampleResolution, lineSegments)),
                lineWidth,
                LineType.Continuous);

            l.texture = isOffsettable ? orbitFadeTexture : orbitTexture;
            l.material = orbitMaterial;
            l.textureOffset = textureOffset / textureOffset - 1.0f;
            l.continuousTexture = true;
            l.color = GetOrbitColour();
            l.rectTransform.gameObject.layer = 31;
            l.rectTransform.gameObject.hideFlags = HideFlags.HideInHierarchy;
            l.joins = Joins.Weld;
        }

        private void OnGUI()
        {
            OrbitCastHit orbitCastHit;
            OrbitDriver hitDriver = TargetCastSplines(out orbitCastHit, lineWidth);

            if(hitDriver != null && orbitLine != null)
            {
                Vector2 position = orbitCastHit.orbitScreenPoint;
                Vector2 dimension = new Vector2(200, 50);

                GUILayout.BeginArea(new Rect(position, dimension));

                GUILayout.Window(0, new Rect(position, dimension), (x) => 
                {
                    GUILayout.Label("Velocity: " + hitDriver.orbit.vel.magnitude.ToString("0.0") + " m/s");
                    GUILayout.Label("Altitude: " + hitDriver.orbit.altitude.ToString("N0") + " m");
                }, orbitLine.name + " Info");

                GUILayout.EndArea();
            }
        }

        public OrbitDriver TargetCastSplines(out OrbitCastHit orbitHit, float orbitPixelWidth = 18f)
        {
            orbitHit = new OrbitCastHit();
            OrbitCastHit tempOrbitHit = new OrbitCastHit();

            foreach (OrbitDriver orbit in Planetarium.Orbits)
            {
                if (orbit.Renderer != null)
                {
                    if (!orbit.Renderer.OrbitCast(Input.mousePosition, out tempOrbitHit, orbitPixelWidth))
                    {
                        continue;
                    }

                    orbitHit = tempOrbitHit;

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

            if(EventSystem.current != null && orbitLine != null)
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

                hitInfo.orbitScreenPoint = new Vector3(hitInfo.orbitScreenPoint.x, Screen.height - hitInfo.orbitScreenPoint.y, 0f);

                return true;
            }

            Debug.DrawRay(hitInfo.orbitOrigin, hitInfo.hitPoint, Color.yellow);

            return false;
        }

        private void UpdateSpline()
        {
            if (orbit.eccentricity >= 1)
            {
                double st;
                double end;
                double rng;
                double itv;

                st = -Math.Acos(-(1 / orbit.eccentricity));
                end = Math.Acos(-(1 / orbit.eccentricity));

                rng = end - st;
                itv = rng / (orbitPoints.Length - 1);

                for (int i = 0; i < orbitPoints.Length; i++)
                {
                    if (UseEccOrbitData)
                        orbitPoints[i] = orbit.getPositionFromEccAnomaly(st + itv * i);
                    else
                        orbitPoints[i] = orbit.getPositionFromTrueAnomaly(st + itv * i);
                }

                if (!UseEccOrbitData)
                {
                    int fixCount = 2;

                    for (int i = 0; i < fixCount; i++)
                    {
                        orbitPoints[i] = orbitPoints[fixCount + 1];
                        orbitPoints[orbitPoints.Length - i - 1] = orbitPoints[orbitPoints.Length - (fixCount + 1) - 1];
                    }
                }
            }
            else
            {
                int resolution = (int)Math.Floor(360 / (double)lineSampleResolution);

                for (int i = 0; i < resolution; i++)
                {
                    if (UseEccOrbitData)
                        orbitPoints[i] = orbit.getPositionFromEccAnomaly(orbitDriver.orbit.eccentricAnomaly + i * lineSampleResolution * MathUtils.Deg2Rad);
                    else
                        orbitPoints[i] = orbit.getPositionFromTrueAnomaly(orbitDriver.orbit.trueAnomaly + i * lineSampleResolution * MathUtils.Deg2Rad);
                }
            }

            Vector3[] scaledOrbitPoints = new Vector3[orbitPoints.Length];

            for (int i = 0; i < orbitPoints.Length; i++)
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
                    return ScaledSpace.LocalToScaledSpace(or.orbit.getPositionFromTrueAnomaly(mouseTA));

                if (or.orbitCB)
                    return ScaledSpace.LocalToScaledSpace(or.orbitCB.transform.position);

                return ScaledSpace.LocalToScaledSpace(or.transform.position);
            }
        }
    }
}