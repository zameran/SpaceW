namespace Experimental
{
    using System;
    using System.Collections.Generic;

    using UnityEngine;

    [RequireComponent(typeof(PatchedConicSolver))]
    public class PatchedConicRenderer : MonoBehaviour
    {
        public bool MapIsEnabled = true;
        public bool DrawAnyway = false;

        public int patchSamples = 48;
        public int interpolations = 4;

        public PatchedConicSolver solver;

        public List<PatchRendering> patchRenders;
        public List<PatchRendering> flightPlanRenders;

        public PatchRendering.RelativityMode relativityMode = PatchRendering.RelativityMode.RELATIVE;

        public CelestialBody relativeTo;

        public bool drawTimes;
        public bool renderEnabled = true;

        public OrbitDriver obtDriver
        {
            get
            {
                return solver.obtDriver;
            }
        }

        public Orbit orbit
        {
            get
            {
                return obtDriver.orbit;
            }
        }

        public Vessel vessel
        {
            get
            {
                return obtDriver.vessel;
            }
        }

        public PatchedConicRenderer() { }

        public void AddManeuverNode(double UT)
        {
            solver.AddManeuverNode(UT);
        }

        public void AddManeuverNode(double UT, Vector3d DeltaV)
        {
            solver.AddManeuverNode(UT, DeltaV);
        }

        public void RemoveManeuverNode(ManeuverNode node)
        {
            solver.RemoveManeuverNode(node);
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Add Maneuver"))
            {
                AddManeuverNode(Planetarium.GetUniversalTime());
            }

            if (GUILayout.Button("Add Maneuver With DeltaV"))
            {
                AddManeuverNode(Planetarium.GetUniversalTime(), obtDriver.orbit.vel / 10);
            }

            if (GUILayout.Button("Clear Maneuver's"))
            {
                if(solver.maneuverNodes.Count != 0)
                    RemoveManeuverNode(solver.maneuverNodes[solver.maneuverNodes.Count - 1]);
            }

            GUILayout.Label("Manuever Nodes Count in Solver: " + solver.maneuverNodes.Count);
        }

        private void Awake()
        {

        }

        private bool CanDrawAnyNode()
        {
            if (Planetarium.Pause || Time.timeScale == 0f) return false;

            return true;
        }

        public PatchRendering FindRenderingForPatch(Orbit patch)
        {
            PatchRendering patchRendering;

            foreach (PatchRendering flightPlanRender in flightPlanRenders)
            {
                if (flightPlanRender.patch != patch) continue;

                patchRendering = flightPlanRender;

                return patchRendering;
            }

            foreach (PatchRendering pr in patchRenders)
            {
                if (pr.patch != patch) continue;

                patchRendering = pr;

                return patchRendering;
            }

            return null;
        }

        private Color GetPatchColor(int index, Vessel vessel)
        {
            return (vessel != FlightGlobals.ActiveVessel ? 
                              MapView.TargetPatchColors[index % MapView.TargetPatchColors.Length] : 
                              MapView.PatchColors[index % MapView.PatchColors.Length]);
        }

        private void LateUpdate()
        {
            if (Planetarium.Pause || Time.timeScale == 0f || DrawAnyway) return;

            //if (obtDriver.updateMode == OrbitDriver.UpdateMode.IDLE)
            //{
            if (renderEnabled)
            {
                for (int i = 0; i < patchRenders.Count; i++)
                {
                    if (patchRenders[i].enabled) { patchRenders[i].DestroyVector(); }
                }

                renderEnabled = false;
            }

            //    return;
            //}

            renderEnabled = true;

            int reIndex = 0;
            for (int j = 0; j < patchRenders.Count; j++)
            {
                if (!solver.patches[j].activePatch || !MapIsEnabled)
                {
                    patchRenders[j].DestroyVector();
                }
                else
                {
                    if (j != 0)
                    {
                        patchRenders[j].patch = solver.patches[j];
                        patchRenders[j].currentMainBody = orbit.referenceBody;
                        patchRenders[j].relativityMode = relativityMode;
                        patchRenders[j].SetColor(GetPatchColor(j, vessel));
                        patchRenders[j].UpdatePR();
                    }
                }

                if (j < solver.flightPlan.Count && solver.flightPlan[j] == solver.patches[j])
                {
                    reIndex = j;
                }
            }

            for (int k = 0; k < solver.flightPlan.Count; k++)
            {
                if (flightPlanRenders.Count <= k)
                {
                    PatchRendering patchRendering = new PatchRendering(vessel.name + " flight plan " + flightPlanRenders.Count, patchSamples, interpolations, solver.flightPlan[k], MapView.PatchMaterial, 8.0f, false, this);

                    flightPlanRenders.Add(patchRendering);
                }

                if (!MapIsEnabled)
                {
                    flightPlanRenders[k].DestroyVector();
                }
                else
                {
                    if (k != 0)
                    {
                        flightPlanRenders[k].patch = solver.flightPlan[k];
                        flightPlanRenders[k].currentMainBody = orbit.referenceBody;
                        flightPlanRenders[k].relativityMode = relativityMode;
                        flightPlanRenders[k].SetColor(GetPatchColor(Math.Max(k - reIndex, 0), vessel));
                        flightPlanRenders[k].visible = k > reIndex;
                        flightPlanRenders[k].UpdatePR();
                    }
                }
            }

            while (flightPlanRenders.Count > solver.flightPlan.Count)
            {
                flightPlanRenders[flightPlanRenders.Count - 1].DestroyVector();
                flightPlanRenders.RemoveAt(flightPlanRenders.Count - 1);
            }

            for (int l = solver.maneuverNodes.Count - 1; l >= 0; l--)
            {
                ManeuverNode maneuverNode = solver.maneuverNodes[l];

                if (!MapIsEnabled)
                {
                    maneuverNode.DetachGizmo();
                }
                else
                {
                    PatchRendering nextPR = FindRenderingForPatch(maneuverNode.nextPatch);

                    if (nextPR != null)
                    {
                        Vector3d relativePositionAtUT = nextPR.patch.getRelativePositionAtUT(maneuverNode.UT).xzy;
                        Vector3d orbitalVelocityAtUT = nextPR.patch.getOrbitalVelocityAtUT(maneuverNode.UT).xzy;

                        maneuverNode.nodeRotation = Quaternion.LookRotation(orbitalVelocityAtUT, Vector3d.Cross(-relativePositionAtUT, orbitalVelocityAtUT));
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (solver != null)
            {
                for (int i = 0; i < solver.maneuverNodes.Count; i++)
                {
                    solver.maneuverNodes[i].RemoveSelf();
                    i--;
                }
            }

            if (patchRenders != null)
            {
                for (int j = 0; j < patchRenders.Count; j++)
                {
                    patchRenders[j].Terminate();
                }

                for (int k = 0; k < flightPlanRenders.Count; k++)
                {
                    flightPlanRenders[k].Terminate();
                }
            }
        }

        private void Start()
        {
            solver = GetComponent<PatchedConicSolver>();
            patchRenders = new List<PatchRendering>();
            flightPlanRenders = new List<PatchRendering>();
            relativeTo = FlightGlobals.Bodies[0];
            relativityMode = PatchRendering.RelativityMode.DYNAMIC;

            for (int i = 0; i < solver.maxTotalPatches; i++)
            {
                PatchRendering patchRendering = new PatchRendering(vessel.name + " patch " + patchRenders.Count, patchSamples, interpolations, solver.patches[i], MapView.PatchMaterial, 8.0f, true, this);

                patchRenders.Add(patchRendering);

                patchRendering.SetColor(GetPatchColor(i, vessel));
                patchRendering.relativityMode = relativityMode;
                patchRendering.relativeTo = relativeTo;
            }
        }
    }
}