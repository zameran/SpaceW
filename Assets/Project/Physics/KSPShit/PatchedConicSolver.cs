namespace Experimental
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using UnityEngine;

    [RequireComponent(typeof(OrbitDriver))]
    public class PatchedConicSolver : MonoBehaviour
    {
        public OrbitDriver obtDriver;

        public int patchLimit = 2;
        public int maxTotalPatches = 12;

        public List<Orbit> patches;
        public List<Orbit> flightPlan;

        public List<ManeuverNode> maneuverNodes;

        public bool MorePatchesAhead;

        public int patchesAhead;

        public PatchedConics.SolverParameters parameters;

        public CelestialBody targetBody;

        public Orbit orbit
        {
            get
            {
                return obtDriver.orbit;
            }
        }

        public ManeuverNode AddManeuverNode(double UT)
        {
            ManeuverNode maneuverNode = new ManeuverNode()
            {
                solver = this,
                DeltaV = Vector3d.zero,
                UT = UT
            };

            maneuverNodes.Add(maneuverNode);

            UpdateFlightPlan();

            return maneuverNode;
        }

        public ManeuverNode AddManeuverNode(double UT, Vector3d DeltaV)
        {
            ManeuverNode maneuverNode = new ManeuverNode()
            {
                solver = this,
                DeltaV = DeltaV,
                UT = UT
            };

            maneuverNodes.Add(maneuverNode);

            UpdateFlightPlan();

            return maneuverNode;
        }

        private void Awake()
        {
            obtDriver = GetComponent<OrbitDriver>();
            obtDriver.OnReferenceBodyChange += new OrbitDriver.CelestialBodyDelegate(OnReferenceBodyChange);
            patches = new List<Orbit>();
            flightPlan = new List<Orbit>();
            maneuverNodes = new List<ManeuverNode>();

            for (int i = 0; i < maxTotalPatches; i++)
            {
                if (i != 0) patches.Add(new Orbit());
                else patches.Add(obtDriver.orbit);
            }

            parameters = new PatchedConics.SolverParameters();
        }

        private void CheckNextManeuver(int nodeIdx, Orbit nodePatch, int patchesAhead)
        {
            flightPlan.Add(nodePatch);

            Orbit orbit = new Orbit();

            bool path = false;

            if (nodeIdx != 0)
            {
                path = PatchedConics.CalculatePatch(nodePatch, orbit, nodePatch.epoch, parameters, targetBody);
            }

            if (nodeIdx < maneuverNodes.Count)
            {
                ManeuverNode item = maneuverNodes[nodeIdx];

                if (item.UT > nodePatch.StartUT && item.UT < nodePatch.EndUT || nodePatch.patchEndTransition == Orbit.PatchTransitionType.FINAL)
                {
                    item.patch = nodePatch;

                    Vector3d relativePositionAtUT = nodePatch.getRelativePositionAtUT(item.UT).xzy;
                    Vector3d orbitalVelocityAtUT = nodePatch.getOrbitalVelocityAtUT(item.UT).xzy;

                    item.nodeRotation = Quaternion.LookRotation(orbitalVelocityAtUT, Vector3d.Cross(-relativePositionAtUT, orbitalVelocityAtUT));

                    Vector3d deltaV = item.nodeRotation * item.DeltaV;

                    item.nextPatch = orbit;

                    orbit.UpdateFromStateVectors(relativePositionAtUT.xzy, orbitalVelocityAtUT.xzy + deltaV.xzy, nodePatch.referenceBody, item.UT);
                    orbit.patchStartTransition = Orbit.PatchTransitionType.MANEUVER;
                    orbit.StartUT = item.UT;
                    orbit.DrawOrbit();

                    if (nodeIdx != 0)
                    {
                        nodePatch.patchEndTransition = Orbit.PatchTransitionType.MANEUVER;
                        nodePatch.EndUT = item.UT;
                        nodePatch.activePatch = true;
                    }

                    CheckNextManeuver(nodeIdx + 1, orbit, 1);

                    return;
                }
            }

            if (path && (patchesAhead < patchLimit || nodeIdx < maneuverNodes.Count))
            {
                CheckNextManeuver(nodeIdx, orbit, patchesAhead + 1);
            }
        }

        public Orbit FindFirstPatch(double UT)
        {
            int i = 0;

            while (i < patches.Count)
            {
                Orbit patch = patches[i];

                if (patch.activePatch)
                {
                    if (UT >= patch.StartUT && UT <= patch.EndUT || patch.patchEndTransition == Orbit.PatchTransitionType.FINAL)
                    {
                        return patch;
                    }

                    i++;
                }
                else break;
            }

            return null;
        }

        private void OnDestroy()
        {
            if (obtDriver != null)
            {
                obtDriver.OnReferenceBodyChange -= new OrbitDriver.CelestialBodyDelegate(OnReferenceBodyChange);
            }
        }

        private void OnReferenceBodyChange(CelestialBody body)
        {
            
        }

        public void RemoveManeuverNode(ManeuverNode node)
        {
            if (!maneuverNodes.Contains(node))
            {
                Debug.LogWarning("Patched Conics Solver: Cannot remove a node not found on the nodes list", gameObject);

                return;
            }

            maneuverNodes.Remove(node);

            if (maneuverNodes.Count != 0)
            {
                UpdateFlightPlan();
            }
            else
            {
                flightPlan.Clear();
            }
        }

        private int SortNodesByDate(ManeuverNode m1, ManeuverNode m2)
        {
            return m1.UT.CompareTo(m2.UT);
        }

        private void Start()
        {

        }

        public void Update()
        {
            if (Planetarium.Pause || Time.timeScale == 0f) return;

            patchLimit = Mathf.Max(Mathf.Min(maxTotalPatches - 1, patchLimit), 1);

            for (int j = 0; j < patches.Count; j++)
            {
                patches[j].activePatch = false;
            }

            //if (obtDriver.updateMode == OrbitDriver.UpdateMode.IDLE) return;

            if (obtDriver.vessel == null) return;

            patches[0] = orbit;
            patches[0].patchStartTransition = Orbit.PatchTransitionType.INITIAL;
            patches[0].StartUT = Planetarium.GetUniversalTime();

            int i = 0;
            do
            {
                MorePatchesAhead = PatchedConics.CalculatePatch(patches[i], patches[i + 1], (i != 0 ? patches[i].epoch : Planetarium.GetUniversalTime()), parameters, targetBody);
                patchesAhead = i;

                i++;

                if (MorePatchesAhead) continue;

                break;
            }
            while (i < patchLimit || maneuverNodes.Count > 0 && FindFirstPatch(maneuverNodes[0].UT) == null);

            UpdateFlightPlan();
        }

        public void UpdateFlightPlan()
        {
            if (maneuverNodes.Count > 0)
            {
                maneuverNodes.Sort(new Comparison<ManeuverNode>(SortNodesByDate));

                flightPlan.Clear();

                Orbit orbit = FindFirstPatch(maneuverNodes[0].UT);

                if (orbit == null) return;

                int i = 0;

                while (i < patches.Count)
                {
                    if (patches[i] != orbit)
                    {
                        flightPlan.Add(patches[i]);

                        i++;
                    }
                    else break;
                }

                CheckNextManeuver(0, orbit, 1);
            }
        }
    }
}