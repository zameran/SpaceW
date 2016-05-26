namespace Experimental
{
    using System;
    using System.Collections.Generic;

    using UnityEngine;

    using ZFramework.Math;

    using Vectrosity;

    [Serializable]
    public class PatchRendering
    {
        public enum RelativityMode
        {
            LOCAL_TO_BODIES,
            LOCAL_AT_SOI_ENTRY_UT,
            LOCAL_AT_SOI_EXIT_UT,
            RELATIVE,
            DYNAMIC
        }

        public AnimationCurve splineEccentricOffset = new AnimationCurve(new Keyframe(0.0f, 0.0f),
                                                                         new Keyframe(1.0f, 0.0f));

        private PatchedConicRenderer pcr;

        public CelestialBody currentMainBody;
        public CelestialBody relativeTo;

        public Orbit patch;

        public Trajectory trajectory;

        public RelativityMode relativityMode = RelativityMode.RELATIVE;

        public Vector3d pe;
        public Vector3d ap;
        public Vector3d st;
        public Vector3d end;
        public Vector3d cb;

        public double UTpe;
        public double UTap;
        public double UTcb;

        public Vector3d[] tPoints;
        public Vector3[] scaledTPoints;
        public List<Vector3> vectorPoints;

        public Color[] colors;
        public Color patchColor;
        public Color nodeColor;

        private List<Color32> vectorColours;

        private VectorLine vectorLine;

        public Material lineMaterial;

        public bool is3DLine = false;

        public int samples;
        public int interpolations;

        public float dynamicLinearity = 2f;

        public string vectorName;

        public float lineWidth = 8;

        private bool smoothLineTexture;

        public bool visible;

        private double trailOffset;
        private double eccOffset;
        private float twkOffset;

        public bool enabled
        {
            get
            {
                return vectorLine != null;
            }
        }

        public List<Color32> VectorColours
        {
            get
            {
                if (vectorColours == null)
                {
                    vectorColours = new List<Color32>(samples * interpolations);

                    for (int i = 0; i < samples; i++)
                    {
                        for (int j = 0; j < interpolations; j++)
                        {
                            vectorColours.Add(patchColor);
                        }
                    }
                }

                return vectorColours;
            }
        }

        public PatchRendering(string name, int nSamples, int nInterpolations, Orbit patchRef, Material lineMat, float LineWidth, bool smoothTexture, PatchedConicRenderer pcr)
        {
            tPoints = new Vector3d[nSamples];
            scaledTPoints = new Vector3[nSamples];
            vectorPoints = new List<Vector3>(samples * interpolations * 2);
            colors = new Color[nSamples];
            samples = nSamples;
            interpolations = nInterpolations;
            patch = patchRef;
            lineMaterial = lineMat;
            relativeTo = patchRef.referenceBody;
            vectorName = name;
            lineWidth = LineWidth;
            smoothLineTexture = smoothTexture;
            visible = true;

            this.pcr = pcr;
        }

        private bool CanDrawAnyNode()
        {
            if (enabled && patch.activePatch)
            {
                return true;
            }

            return false;
        }

        public void DestroyVector()
        {
            if (vectorLine != null)
            {
                VectorLine.Destroy(ref vectorLine);

                vectorLine = null;
            }
        }

        private void DrawSplines()
        {
            if (!smoothLineTexture) vectorLine.textureScale = 1f;
            else if (patch.eccentricity >= 1 || patch.patchEndTransition != Orbit.PatchTransitionType.FINAL)
            {
                vectorLine.textureOffset = 0.0f;
            }
            else
            {
                eccOffset = (patch.eccentricAnomaly - MathUtils.Deg2Rad * 2) % MathUtils.TwoPI / MathUtils.TwoPI;
                twkOffset = (float)eccOffset * GetEccOffset((float)eccOffset, (float)patch.eccentricity, 4f);
                vectorLine.textureOffset = 1.0f - twkOffset;
            }

            MakeVector();
            UpdateSpline();

            if (is3DLine) vectorLine.Draw3D();
            else vectorLine.Draw();
        }

        private float GetEccOffset(float eccOffset, float ecc, float eccOffsetPower)
        {
            float spline = splineEccentricOffset.Evaluate(eccOffset);
            return 1.0f + (spline - 1.0f) * (Mathf.Pow(ecc, eccOffsetPower) / Mathf.Pow(0.9f, eccOffsetPower));
        }

        public Vector3 GetScaledSpacePointFromTA(double TA, double UT)
        {
            Vector3d local;

            if (trajectory == null)
            {
                Debug.Log("PatchRendering Trajectory is Null");

                return Vector3.zero;
            }

            Vector3d pos = patch.getRelativePositionFromTrueAnomaly(TA);

            switch (relativityMode)
            {
                case RelativityMode.LOCAL_TO_BODIES:
                    {
                        local = trajectory.ConvertPointToLocal(pos.xzy);
                        break;
                    }
                case RelativityMode.LOCAL_AT_SOI_ENTRY_UT:
                    {
                        local = trajectory.ConvertPointToLocalAtUT(pos.xzy, patch.StartUT, relativeTo);
                        break;
                    }
                case RelativityMode.LOCAL_AT_SOI_EXIT_UT:
                    {
                        local = trajectory.ConvertPointToLocalAtUT(pos.xzy, patch.EndUT, relativeTo);
                        break;
                    }
                case RelativityMode.RELATIVE:
                    {
                        local = trajectory.ConvertPointToRelative(pos.xzy, UT, relativeTo);
                        break;
                    }
                case RelativityMode.DYNAMIC:
                    {
                        local = trajectory.ConvertPointToLerped(pos.xzy, UT, patch.referenceBody, relativeTo, Planetarium.GetUniversalTime() + patch.timeToPe, patch.EndUT, dynamicLinearity);
                        break;
                    }
                default:
                    {
                        goto case RelativityMode.LOCAL_TO_BODIES;
                    }
            }

            return ScaledSpace.LocalToScaledSpace(local);
        }

        public void MakeVector()
        {
            if (vectorLine != null) DestroyVector();

            vectorLine = new VectorLine(vectorName, vectorPoints, lineWidth, LineType.Discrete)
            {
                texture = lineMaterial.mainTexture,
                material = lineMaterial,
                continuousTexture = smoothLineTexture
            };

            vectorLine.SetColor(patchColor);
            vectorLine.smoothColor = true;
            vectorLine.rectTransform.gameObject.layer = 31;
            vectorLine.rectTransform.gameObject.hideFlags = HideFlags.HideInHierarchy;
            vectorLine.joins = Joins.Weld;
        }

        public void SetColor(Color color)
        {
            nodeColor = color;
            patchColor = (nodeColor * 0.5f).A(nodeColor.a);
        }

        private List<Color32> SetColorSegments(Color[] pointColors, int interpolations)
        {
            for (int i = samples - 1; i >= 0; i--)
            {
                for (int j = interpolations - 1; j >= 0; j--)
                {
                    VectorColours[i * interpolations + j] = pointColors[i];
                }
            }

            return VectorColours;
        }

        public void Terminate()
        {
            DestroyVector();
        }

        public void UpdatePR()
        {
            if (pcr == null) { }

            if (samples != tPoints.Length || vectorPoints.Count != samples * interpolations * 2)
            {
                tPoints = new Vector3d[samples];
                scaledTPoints = new Vector3[samples];
                vectorPoints = new List<Vector3>(samples * interpolations * 2);
                colors = new Color[samples];

                MakeVector();
            }

            if (vectorLine != null) { vectorLine.active = (!patch.activePatch ? false : visible); }
            else MakeVector();

            if (!patch.activePatch) return;

            trajectory = patch.GetPatchTrajectory(samples);
            relativeTo = patch.referenceBody.ReferenceBody;

            if (relativityMode == RelativityMode.RELATIVE && patch.patchEndTransition == Orbit.PatchTransitionType.FINAL)
            {
                relativityMode = RelativityMode.LOCAL_AT_SOI_ENTRY_UT;
            }

            if (currentMainBody.HasParent(patch.referenceBody))
            {
                relativityMode = RelativityMode.LOCAL_TO_BODIES;
            }

            if (patch.referenceBody == currentMainBody)
            {
                relativityMode = RelativityMode.LOCAL_TO_BODIES;
            }

            if (patch.referenceBody == PlanetariumCamera.fetch.target.GetReferenceBody() || patch.referenceBody.HasChild(PlanetariumCamera.fetch.target.GetReferenceBody()))
            {
                relativityMode = RelativityMode.LOCAL_TO_BODIES;
            }

            double UT = Planetarium.GetUniversalTime();

            UTap = patch.StartUT + patch.timeToAp;
            UTpe = patch.StartUT + patch.GetTimeToPeriapsis();

            switch (relativityMode)
            {
                case RelativityMode.LOCAL_TO_BODIES:
                    {
                        tPoints = trajectory.GetPointsLocal();
                        pe = trajectory.GetPeriapsisLocal();
                        ap = trajectory.GetApoapsisLocal();
                        st = trajectory.GetPatchStartLocal();
                        end = trajectory.GetPatchEndLocal();
                        cb = trajectory.GetRefBodyPosLocal();
                        UTcb = UT;

                        break;
                    }
                case RelativityMode.LOCAL_AT_SOI_ENTRY_UT:
                    {
                        tPoints = trajectory.GetPointsLocalAtUT(relativeTo, patch.StartUT);
                        pe = trajectory.GetPeriapsisLocalAtUT(relativeTo, patch.StartUT);
                        ap = trajectory.GetApoapsisLocalAtUT(relativeTo, patch.StartUT);
                        st = trajectory.GetPatchStartLocalAtUT(relativeTo, patch.StartUT);
                        end = trajectory.GetPatchEndLocalAtUT(relativeTo, patch.StartUT);
                        cb = trajectory.GetRefBodyPosLocalAtUT(relativeTo, patch.StartUT);
                        UTcb = patch.StartUT;

                        break;
                    }
                case RelativityMode.LOCAL_AT_SOI_EXIT_UT:
                    {
                        tPoints = trajectory.GetPointsLocalAtUT(relativeTo, patch.EndUT);
                        pe = trajectory.GetPeriapsisLocalAtUT(relativeTo, patch.EndUT);
                        ap = trajectory.GetApoapsisLocalAtUT(relativeTo, patch.EndUT);
                        st = trajectory.GetPatchStartLocalAtUT(relativeTo, patch.EndUT);
                        end = trajectory.GetPatchEndLocalAtUT(relativeTo, patch.EndUT);
                        cb = trajectory.GetRefBodyPosLocalAtUT(relativeTo, patch.EndUT);
                        UTcb = patch.EndUT;

                        break;
                    }
                case RelativityMode.RELATIVE:
                    {
                        tPoints = trajectory.GetPointsRelative(relativeTo);
                        pe = trajectory.GetPeriapsisRelative(relativeTo);
                        ap = trajectory.GetApoapsisRelative(relativeTo);
                        st = trajectory.GetPatchStartRelative(relativeTo);
                        end = trajectory.GetPatchEndRelative(relativeTo);
                        cb = trajectory.GetRefBodyPosRelative(relativeTo);
                        UTcb = UTpe;

                        break;
                    }
                case RelativityMode.DYNAMIC:
                    {
                        tPoints = trajectory.GetPointsLerped(patch.referenceBody, relativeTo, Math.Max(UT, patch.StartUT), patch.EndUT, dynamicLinearity);
                        pe = trajectory.GetPeriapsisLerped(patch.referenceBody, relativeTo, Math.Max(UT, patch.StartUT), patch.EndUT, dynamicLinearity);
                        ap = trajectory.GetApoapsisLerped(patch.referenceBody, relativeTo, Math.Max(UT, patch.StartUT), patch.EndUT, dynamicLinearity);
                        st = trajectory.GetPatchStartLerped(patch.referenceBody, relativeTo, Math.Max(UT, patch.StartUT), patch.EndUT, dynamicLinearity);
                        end = trajectory.GetPatchEndLerped(patch.referenceBody, relativeTo, Math.Max(UT, patch.StartUT), patch.EndUT, dynamicLinearity);
                        cb = trajectory.GetRefBodyPosLerped(patch.referenceBody, relativeTo, Math.Max(UT, patch.StartUT), patch.EndUT, dynamicLinearity);
                        UTcb = UTpe;

                        break;
                    }
            }

            pe = ScaledSpace.LocalToScaledSpace(pe);
            ap = ScaledSpace.LocalToScaledSpace(ap);
            st = ScaledSpace.LocalToScaledSpace(st);
            end = ScaledSpace.LocalToScaledSpace(end);
            cb = ScaledSpace.LocalToScaledSpace(cb);

            for (int i = tPoints.Length - 1; i >= 0; i--)
            {
                scaledTPoints[i] = ScaledSpace.LocalToScaledSpace(tPoints[i]);
            }

            colors = trajectory.GetColors(patchColor);

            if (vectorLine != null)
            {
                UpdateSpline();
                DrawSplines();
            }
        }

        private void UpdateSpline()
        {
            vectorLine.MakeSpline(scaledTPoints, (patch.eccentricity >= 1 ? false : patch.patchEndTransition == Orbit.PatchTransitionType.FINAL));
            vectorLine.SetColors(SetColorSegments(colors, interpolations));
        }
    }
}