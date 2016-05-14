namespace Experimental
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using ZFramework.Math;

    using UnityEngine;

    public class PatchedConics
    {
        public PatchedConics() { }

        public static bool CalculatePatch(Orbit p, Orbit nextPatch, double startEpoch, SolverParameters pars, CelestialBody targetBody)
        {
            double num;
            double num1;

            p.activePatch = true;
            p.nextPatch = nextPatch;
            p.patchEndTransition = Orbit.PatchTransitionType.FINAL;
            p.closestEncounterLevel = Orbit.EncounterSolutionLevel.NONE;

            if (Planetarium.Orbits == null) return false;

            foreach (OrbitDriver driver in Planetarium.Orbits)
            {
                Orbit orbit = driver.orbit;

                if (orbit != p)
                {
                    if (driver.celestialBody)
                    {
                        if (targetBody == null || driver.celestialBody == targetBody) p.closestTgtApprUT = 0;
                        if (driver.ReferenceBody != p.referenceBody) continue;

                        if (p.patchStartTransition != Orbit.PatchTransitionType.ESCAPE || !(driver.celestialBody == p.previousPatch.referenceBody))
                        {
                            if (Orbit.PeApIntersects(p, orbit, driver.celestialBody.sphereOfInfluence * 1.1))
                            {
                                p.closestEncounterLevel = Orbit.EncounterSolutionLevel.ORBIT_INTERSECT;
                                p.closestEncounterBody = driver.celestialBody;

                                Orbit.FindClosestPoints(p, orbit, ref p.ClEctr1, ref p.ClEctr2, ref p.FEVp, ref p.FEVs, ref p.SEVp, ref p.SEVs, 0.0001, pars.maxGeometrySolverIterations, ref pars.GeoSolverIterations);

                                p.FEVp = UtilMath.WrapAround(p.FEVp, 0, MathUtils.TwoPI);
                                p.SEVp = UtilMath.WrapAround(p.SEVp, 0, MathUtils.TwoPI);

                                if (Math.Min(p.ClEctr1, p.ClEctr2) <= driver.celestialBody.sphereOfInfluence)
                                {
                                    p.closestEncounterLevel = Orbit.EncounterSolutionLevel.SOI_INTERSECT_1;
                                    p.closestEncounterBody = driver.celestialBody;
                                    p.timeToTransition1 = p.GetDTforTrueAnomaly(p.FEVp, 0);
                                    p.secondaryPosAtTransition1 = orbit.getPositionAtUT(startEpoch + p.timeToTransition1);

                                    Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.referenceBody.transform.position), ScaledSpace.LocalToScaledSpace(p.secondaryPosAtTransition1), Color.yellow);

                                    p.timeToTransition2 = p.GetDTforTrueAnomaly(p.SEVp, 0);
                                    p.secondaryPosAtTransition2 = orbit.getPositionAtUT(startEpoch + p.timeToTransition2);

                                    Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.referenceBody.transform.position), ScaledSpace.LocalToScaledSpace(p.secondaryPosAtTransition2), Color.red);

                                    p.nearestTT = Math.Min(UtilMath.WrapAround(p.timeToTransition1, 0, p.period), UtilMath.WrapAround(p.timeToTransition2, 0, p.period));
                                    p.nextTT = UtilMath.WrapAround((p.nearestTT != p.timeToTransition1 ? p.timeToTransition1 : p.timeToTransition2), 0, p.period);

                                    if (double.IsNaN(p.nearestTT))
                                    {
                                        Debug.Log(string.Format("nearestTT is NaN! t1: {0}, t2: {1}, FEVp: {2}, SEVp: {3}",
                                                                p.timeToTransition1, 
                                                                p.timeToTransition2, 
                                                                p.FEVp, 
                                                                p.SEVp));
                                    }

                                    p.UTappr = startEpoch;

                                    if (p.eccentricity >= 1)
                                    {
                                        if (!double.IsInfinity(p.referenceBody.sphereOfInfluence))
                                        {
                                            num = p.TrueAnomalyAtRadius(p.referenceBody.sphereOfInfluence);
                                            num = Math.Min(num, MathUtils.TwoPI - num);
                                        }
                                        else if (orbit.eccentricity >= 1)
                                        {
                                            num = p.TrueAnomalyAtRadius(pars.outerReaches);
                                            num = Math.Min(num, MathUtils.TwoPI - num) * 0.999;
                                        }
                                        else
                                        {
                                            num = p.TrueAnomalyAtRadius(orbit.semiMajorAxis * 10);
                                            num = Math.Min(num, MathUtils.TwoPI - num);
                                        }

                                        double dTforTrueAnomaly = p.GetDTforTrueAnomaly(UtilMath.WrapAround(num, 0, MathUtils.TwoPI), 0);

                                        p.ClAppr = Orbit.SolveClosestApproach(p, orbit, ref p.UTappr, dTforTrueAnomaly * 0.5, 0, startEpoch, double.PositiveInfinity, 0.01, pars.maxTimeSolverIterations, ref pars.TimeSolverIterations1);
                                    }
                                    else
                                    {
                                        p.ClAppr = Orbit.SolveClosestApproach(p, orbit, ref p.UTappr, (startEpoch + p.nextTT - (startEpoch + p.nearestTT)) * 0.5, 0, startEpoch, startEpoch + p.period, 0.01, pars.maxTimeSolverIterations, ref pars.TimeSolverIterations1);
                                    }

                                    if (p.ClAppr >= driver.celestialBody.sphereOfInfluence || p.ClAppr == -1)
                                    {
                                        p.closestEncounterLevel = Orbit.EncounterSolutionLevel.SOI_INTERSECT_2;
                                        p.closestEncounterBody = driver.celestialBody;

                                        Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.getPositionAtUT(p.UTappr)), ScaledSpace.LocalToScaledSpace(orbit.getPositionAtUT(p.UTappr)), XKCDColors.Orange * 0.5f);

                                        p.UTappr = startEpoch + p.nearestTT;

                                        if (p.eccentricity >= 1)
                                        {
                                            if (!double.IsInfinity(p.referenceBody.sphereOfInfluence))
                                            {
                                                num1 = p.TrueAnomalyAtRadius(p.referenceBody.sphereOfInfluence);
                                                num1 = Math.Min(num1, MathUtils.TwoPI - num1);
                                            }
                                            else if (orbit.eccentricity >= 1)
                                            {
                                                num1 = p.TrueAnomalyAtRadius(pars.outerReaches);
                                                num1 = Math.Min(num1, MathUtils.TwoPI - num1) * 0.999;
                                            }
                                            else
                                            {
                                                num1 = p.TrueAnomalyAtRadius(orbit.semiMajorAxis * 10);
                                                num1 = Math.Min(num1, MathUtils.TwoPI - num1);
                                            }

                                            double dTforTrueAnomaly1 = p.GetDTforTrueAnomaly(UtilMath.WrapAround(num1, 0, MathUtils.TwoPI), 0);

                                            p.ClAppr = Orbit.SolveClosestApproach(p, orbit, ref p.UTappr, dTforTrueAnomaly1 * 0.5, 0, startEpoch, Double.PositiveInfinity, 0.01, pars.maxTimeSolverIterations, ref pars.TimeSolverIterations1);
                                        }
                                        else
                                        {
                                            p.ClAppr = Orbit.SolveClosestApproach(p, orbit, ref p.UTappr, (startEpoch + p.nextTT - (startEpoch + p.nearestTT)) * 0.5, 0, startEpoch, startEpoch + p.period, 0.01, pars.maxTimeSolverIterations, ref pars.TimeSolverIterations1);
                                        }

                                        if (p.ClAppr >= driver.celestialBody.sphereOfInfluence || p.ClAppr == -1)
                                        {
                                            Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.getPositionAtUT(p.UTappr)), ScaledSpace.LocalToScaledSpace(orbit.getPositionAtUT(p.UTappr)), XKCDColors.Orange);

                                            if (driver.celestialBody != targetBody) continue;

                                            p.closestTgtApprUT = p.UTappr;
                                        }
                                        else
                                        {
                                            Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.getPositionAtUT(p.UTappr)), ScaledSpace.LocalToScaledSpace(orbit.getPositionAtUT(p.UTappr)), XKCDColors.ElectricLime);

                                            p.UTsoi = p.UTappr;

                                            Orbit.SolveSOI_BSP(p, orbit, ref p.UTsoi, (p.UTappr - startEpoch) * 0.5, driver.celestialBody.sphereOfInfluence, startEpoch, p.UTappr, 0.01, pars.maxTimeSolverIterations, ref pars.TimeSolverIterations2);

                                            Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.getPositionAtUT(p.UTsoi)), ScaledSpace.LocalToScaledSpace(orbit.getPositionAtUT(p.UTsoi)), XKCDColors.AquaMarine);

                                            nextPatch.UpdateFromOrbitAtUT(p, p.UTsoi, driver.celestialBody);

                                            p.StartUT = startEpoch;
                                            p.EndUT = p.UTsoi;
                                            p.patchEndTransition = Orbit.PatchTransitionType.ENCOUNTER;

                                            break;
                                        }
                                    }
                                    else
                                    {
                                        Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.getPositionAtUT(p.UTappr)), ScaledSpace.LocalToScaledSpace(orbit.getPositionAtUT(p.UTappr)), XKCDColors.ElectricLime);

                                        p.UTsoi = p.UTappr;

                                        Orbit.SolveSOI_BSP(p, orbit, ref p.UTsoi, (p.UTappr - startEpoch) * 0.5, driver.celestialBody.sphereOfInfluence, startEpoch, p.UTappr, 0.01, pars.maxTimeSolverIterations, ref pars.TimeSolverIterations1);

                                        Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.getPositionAtUT(p.UTsoi)), ScaledSpace.LocalToScaledSpace(orbit.getPositionAtUT(p.UTsoi)), XKCDColors.AquaMarine);

                                        nextPatch.UpdateFromOrbitAtUT(p, p.UTsoi, driver.celestialBody);

                                        p.StartUT = startEpoch;
                                        p.EndUT = p.UTsoi;
                                        p.patchEndTransition = Orbit.PatchTransitionType.ENCOUNTER;

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (p.patchEndTransition == Orbit.PatchTransitionType.FINAL && !pars.debug_disableEscapeCheck)
            {
                if (p.ApR <= p.referenceBody.sphereOfInfluence && p.eccentricity < 1)
                {
                    p.StartUT = startEpoch;
                    p.EndUT = startEpoch + p.period;
                    p.patchEndTransition = Orbit.PatchTransitionType.FINAL;
                }
                else if (!double.IsInfinity(p.referenceBody.sphereOfInfluence))
                {
                    p.FEVp = p.TrueAnomalyAtRadius(p.referenceBody.sphereOfInfluence);
                    p.SEVp = MathUtils.TwoPI - p.FEVp;
                    p.FEVp = UtilMath.WrapAround(p.FEVp, 0, MathUtils.TwoPI);
                    p.SEVp = UtilMath.WrapAround(p.SEVp, 0, MathUtils.TwoPI);
                    p.timeToTransition1 = p.GetDTforTrueAnomaly(p.FEVp, 0);
                    p.timeToTransition2 = p.GetDTforTrueAnomaly(p.SEVp, 0);
                    p.UTsoi = startEpoch + p.timeToTransition1;

                    Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.referenceBody.transform.position), ScaledSpace.LocalToScaledSpace(p.getPositionFromTrueAnomaly(p.FEVp)), XKCDColors.Purple);
                    Debug.DrawLine(ScaledSpace.LocalToScaledSpace(p.referenceBody.transform.position), ScaledSpace.LocalToScaledSpace(p.getPositionFromTrueAnomaly(p.SEVp)), XKCDColors.LightPeriwinkle);

                    nextPatch.UpdateFromOrbitAtUT(p, p.UTsoi, p.referenceBody.ReferenceBody);

                    p.StartUT = startEpoch;
                    p.EndUT = p.UTsoi;
                    p.patchEndTransition = Orbit.PatchTransitionType.ESCAPE;
                }
                else
                {
                    p.FEVp = Math.Acos(-(1 / p.eccentricity));
                    p.SEVp = -p.FEVp;
                    p.StartUT = startEpoch;
                    p.EndUT = double.PositiveInfinity;
                    p.patchEndTransition = Orbit.PatchTransitionType.FINAL;
                }
            }

            nextPatch.StartUT = p.EndUT;
            nextPatch.patchStartTransition = p.patchEndTransition;
            nextPatch.previousPatch = p;

            return p.patchEndTransition != Orbit.PatchTransitionType.FINAL;
        }

        public static int CompareAnomalies(double a, double b)
        {
            if (b < 0 && a >= MathUtils.PI) a = a - MathUtils.TwoPI;
            if (a > b) return 1;
            if (a < b) return -1;

            return 0;
        }

        public static bool ScreenCast(Vector3 screenPos, List<PatchRendering> patchRenders, out PatchedConics.PatchCastHit hitInfo, float orbitPixelWidth = 10f, double maxUT = -1, bool clampToPatches = false)
        {
            PatchCastHit patchCastHit = new PatchCastHit();
            hitInfo = new PatchCastHit();
            bool flag = false;
            foreach (PatchRendering patchRender in patchRenders)
            {
                if (patchRender.enabled && patchRender.patch.activePatch)
                {
                    if (maxUT == -1 || patchRender.patch.StartUT < maxUT)
                    {
                        if (!ScreenCast(screenPos, patchRender, out patchCastHit, orbitPixelWidth, maxUT, clampToPatches))
                        {
                            continue;
                        }
                        hitInfo = patchCastHit;
                        flag = true;
                    }
                }
            }
            return flag;
        }

        public static bool ScreenCast(Vector3 screenPos, PatchRendering pr, out PatchCastHit hitInfo, float orbitPixelWidth = 10f, double maxUT = -1, bool clampToPatch = false)
        {
            float single;

            hitInfo = new PatchCastHit()
            {
                pr = pr
            };

            if (pr.relativityMode == PatchRendering.RelativityMode.RELATIVE)
            {
                hitInfo.UTatTA = pr.patch.StartUT;
                hitInfo.mouseTA = SolveRelativeTA_BSP(pr, screenPos, ref hitInfo.UTatTA, (pr.patch.EndUT - pr.patch.StartUT) * 0.5, pr.patch.StartUT, pr.patch.EndUT, 0.001);
                hitInfo.orbitOrigin = ScaledSpace.LocalToScaledSpace(pr.trajectory.ConvertPointToRelative(Vector3d.zero, hitInfo.UTatTA, pr.relativeTo));

                Vector3d pos = pr.patch.getRelativePositionAtUT(hitInfo.UTatTA);

                hitInfo.hitPoint = ScaledSpace.LocalToScaledSpace(pr.trajectory.ConvertPointToRelative(pos.xzy, hitInfo.UTatTA, pr.relativeTo)) - (Vector3d)hitInfo.orbitOrigin;
            }
            else if (pr.relativityMode == PatchRendering.RelativityMode.DYNAMIC)
            {
                hitInfo.UTatTA = pr.patch.StartUT;
                hitInfo.mouseTA = SolveDynamicTA_BSP(pr, screenPos, ref hitInfo.UTatTA, (pr.patch.EndUT - pr.patch.StartUT) * 0.5, pr.patch.StartUT, pr.patch.EndUT, 0.001);
                hitInfo.orbitOrigin = ScaledSpace.LocalToScaledSpace(pr.trajectory.ConvertPointToLerped(Vector3d.zero, hitInfo.UTatTA, pr.patch.referenceBody, pr.relativeTo, pr.patch.timeToPe, pr.patch.EndUT, pr.dynamicLinearity));

                Vector3d pos = pr.patch.getRelativePositionAtUT(hitInfo.UTatTA);

                hitInfo.hitPoint = ScaledSpace.LocalToScaledSpace(pr.trajectory.ConvertPointToLerped(pos.xzy, hitInfo.UTatTA, pr.patch.referenceBody, pr.relativeTo, pr.patch.timeToPe, pr.patch.EndUT, pr.dynamicLinearity)) - (Vector3d)hitInfo.orbitOrigin;
            }
            else if (pr.patch.eccentricity < 1)
            {
                Vector3d orbitNormal = pr.patch.GetOrbitNormal();

                hitInfo.orbitOrigin = pr.cb;

                Plane plane = new Plane(orbitNormal.xzy.normalized, hitInfo.orbitOrigin);

                Debug.DrawRay(hitInfo.orbitOrigin, plane.normal * 1000f, Color.cyan);

                Ray ray = Camera.main.ScreenPointToRay(screenPos);

                plane.Raycast(ray, out single);

                hitInfo.hitPoint = (ray.origin + (ray.direction * single)) - hitInfo.orbitOrigin;

                Vector3 vector3 = Quaternion.Inverse(Quaternion.LookRotation(-((pr.pe - pr.cb).normalized), orbitNormal.xzy)) * hitInfo.hitPoint;

                hitInfo.mouseTA = (MathUtils.PI - Mathf.Atan2(vector3.x, vector3.z));

                if (hitInfo.mouseTA < 0) hitInfo.mouseTA = MathUtils.TwoPI + hitInfo.mouseTA;

                hitInfo.UTatTA = pr.patch.StartUT + pr.patch.GetDTforTrueAnomaly(hitInfo.mouseTA, 0);
            }
            else
            {
                hitInfo.UTatTA = pr.patch.StartUT;
                hitInfo.mouseTA = SolveLocalTA_BSP(pr, screenPos, ref hitInfo.UTatTA, (pr.patch.EndUT - pr.patch.StartUT) * 0.5, pr.patch.StartUT, pr.patch.EndUT, 0.001);
                hitInfo.orbitOrigin = ScaledSpace.LocalToScaledSpace(pr.trajectory.ConvertPointToLocal(Vector3d.zero));

                Vector3d pos = pr.patch.getRelativePositionAtUT(hitInfo.UTatTA);

                hitInfo.hitPoint = ScaledSpace.LocalToScaledSpace(pr.trajectory.ConvertPointToLocal(pos.xzy)) - (Vector3d)hitInfo.orbitOrigin;
            }

            if (clampToPatch && !TAIsWithinPatchBounds(hitInfo.mouseTA, pr.patch))
            {
                Debug.DrawRay(hitInfo.orbitOrigin, hitInfo.hitPoint, Color.red);

                return false;
            }

            hitInfo.radiusAtTA = pr.patch.RadiusAtTrueAnomaly(hitInfo.mouseTA) * ScaledSpace.InverseScaleFactor;
            hitInfo.orbitPoint = (hitInfo.hitPoint.normalized * (float)hitInfo.radiusAtTA) + hitInfo.orbitOrigin;
            hitInfo.orbitScreenPoint = Camera.main.WorldToScreenPoint(hitInfo.orbitPoint);
            hitInfo.orbitScreenPoint = new Vector3(hitInfo.orbitScreenPoint.x, hitInfo.orbitScreenPoint.y, 0f);

            if (Vector3.Distance(hitInfo.orbitScreenPoint, Input.mousePosition) < orbitPixelWidth)
            {
                Debug.DrawLine(hitInfo.orbitOrigin, hitInfo.orbitPoint, Color.green);

                hitInfo.orbitScreenPoint = new Vector3(hitInfo.orbitScreenPoint.x, Screen.height - hitInfo.orbitScreenPoint.y, 0f);

                return true;
            }

            Debug.DrawLine(hitInfo.orbitOrigin, hitInfo.orbitPoint, Color.yellow);

            return false;
        }

        private static double SolveDynamicTA_BSP(PatchRendering pr, Vector3 screenPos, ref double UT, double dT, double MinUT, double MaxUT, double epsilon)
        {
            Vector3 screenPoint = new Vector3();

            UtilMath.BSPSolver(ref UT, dT, (double ut) => 
            {
                screenPoint = Camera.main.WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(pr.trajectory.ConvertPointToLerped(pr.patch.getRelativePositionAtUT(ut).xzy, ut, pr.patch.referenceBody, pr.relativeTo, pr.patch.timeToPe, pr.patch.EndUT, pr.dynamicLinearity)));
                screenPoint = new Vector3(screenPoint.x, screenPoint.y, 0f);
                return (double)(screenPoint - screenPos).sqrMagnitude;
            }, MinUT, MaxUT, epsilon, 50);

            return pr.patch.TrueAnomalyAtUT(UT);
        }

        private static double SolveLocalTA_BSP(PatchRendering pr, Vector3 screenPos, ref double UT, double dT, double MinUT, double MaxUT, double epsilon)
        {
            Vector3 screenPoint = new Vector3();

            UtilMath.BSPSolver(ref UT, dT, (double ut) => 
            {
                screenPoint = Camera.main.WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(pr.trajectory.ConvertPointToLocal(pr.patch.getRelativePositionAtUT(ut).xzy)));
                screenPoint = new Vector3(screenPoint.x, screenPoint.y, 0f);
                return (double)(screenPoint - screenPos).sqrMagnitude;
            }, MinUT, MaxUT, epsilon, 50);

            return pr.patch.TrueAnomalyAtUT(UT);
        }

        private static double SolveRelativeTA_BSP(PatchRendering pr, Vector3 screenPos, ref double UT, double dT, double MinUT, double MaxUT, double epsilon)
        {
            Vector3 screenPoint = new Vector3();

            UtilMath.BSPSolver(ref UT, dT, (double ut) => 
            {
                screenPoint = Camera.main.WorldToScreenPoint(ScaledSpace.LocalToScaledSpace(pr.trajectory.ConvertPointToRelative(pr.patch.getRelativePositionAtUT(ut).xzy, ut, pr.relativeTo)));
                screenPoint = new Vector3(screenPoint.x, screenPoint.y, 0f);
                return (double)(screenPoint - screenPos).sqrMagnitude;
            }, MinUT, MaxUT, epsilon, 50);

            return pr.patch.TrueAnomalyAtUT(UT);
        }

        public static bool TAIsWithinPatchBounds(double tA, Orbit patch)
        {
            if (patch.patchEndTransition != Orbit.PatchTransitionType.FINAL && patch.fromV != 0 && patch.toV != 0)
            {
                if (patch.fromV < patch.toV)
                {
                    if (CompareAnomalies(tA, patch.fromV) < 0 || CompareAnomalies(tA, patch.toV) > 0)
                    {
                        return false;
                    }
                }
                else if (CompareAnomalies(tA, patch.fromV) < 0 && CompareAnomalies(tA, patch.toV) > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public struct PatchCastHit
        {
            public Vector3 orbitOrigin;
            public Vector3 hitPoint;
            public Vector3 orbitPoint;
            public Vector3 orbitScreenPoint;

            public double mouseTA;
            public double radiusAtTA;
            public double UTatTA;

            public PatchRendering pr;

            public Vector3 GetScreenSpacePoint()
            {
                return Camera.main.WorldToScreenPoint(GetUpdatedOrbitPoint());
            }

            public Vector3 GetUpdatedOrbitPoint()
            {
                return pr.GetScaledSpacePointFromTA(mouseTA, UTatTA);
            }

            public Vector3 GetUpdatedOrigin()
            {
                if (pr.relativityMode == PatchRendering.RelativityMode.RELATIVE)
                {
                    return ScaledSpace.LocalToScaledSpace(pr.trajectory.ConvertPointToRelative(Vector3d.zero, UTatTA, pr.relativeTo));
                }

                if (pr.relativityMode != PatchRendering.RelativityMode.DYNAMIC)
                {
                    return pr.cb;
                }

                return ScaledSpace.LocalToScaledSpace(pr.trajectory.ConvertPointToLerped(Vector3d.zero, UTatTA, pr.patch.referenceBody, pr.relativeTo, pr.patch.timeToPe, pr.patch.EndUT, pr.dynamicLinearity));
            }
        }

        [Serializable]
        public class SolverParameters
        {
            public int maxGeometrySolverIterations = 25;
            public int maxTimeSolverIterations = 50;

            public int GeoSolverIterations;

            public int TimeSolverIterations1;
            public int TimeSolverIterations2;

            public bool FollowManeuvers;

            public bool debug_disableEscapeCheck;

            public double outerReaches = 1E+20;

            public SolverParameters() { }
        }
    }
}