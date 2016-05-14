namespace Experimental
{
    using System;
    using UnityEngine;

    [Serializable]
    public class ManeuverNode
    {
        public double UT;
        public Vector3d DeltaV;
        public Quaternion nodeRotation;

        //public ManeuverGizmo attachedGizmo;
        public PatchedConicSolver solver;

        //public MapObject scaledSpaceTarget;

        public Orbit patch;
        public Orbit nextPatch;

        public ManeuverNode() { }

        public void AttachGizmo(GameObject gizmoPrefab, PatchedConicRenderer renderer)
        {
            //attachedGizmo = GameObject.Instantiate<GameObject>(gizmoPrefab).GetComponent<ManeuverGizmo>();
            //attachedGizmo.gameObject.SetActive(true);
            //attachedGizmo.name = "Maneuver Node";
            //attachedGizmo.DeltaV = this.DeltaV;
            //attachedGizmo.UT = this.UT;
            //attachedGizmo.OnGizmoUpdated = new ManeuverGizmo.HandlesUpdatedCallback(this.OnGizmoUpdated);
            //attachedGizmo.OnMinimize = new Callback(this.DetachGizmo);
            //attachedGizmo.OnDelete = new Callback(this.RemoveSelf);
            //attachedGizmo.Setup(this, renderer);
        }

        public void DetachGizmo()
        {
            //if (attachedGizmo) attachedGizmo.Terminate();
        }

        public Vector3d GetBurnVector(Orbit currentOrbit)
        {
            if (currentOrbit.referenceBody == nextPatch.referenceBody)
            {
                return (nextPatch.getOrbitalVelocityAtUT(UT) - currentOrbit.getOrbitalVelocityAtUT(UT)).xzy;
            }

            return (nextPatch.getOrbitalVelocityAtUT(UT) - patch.getOrbitalVelocityAtUT(UT)).xzy;
        }

        public void OnGizmoUpdated(Vector3d dV, double ut)
        {
            DeltaV = dV;
            UT = ut;
            solver.UpdateFlightPlan();
        }

        public void RemoveSelf()
        {
            DetachGizmo();

            //if (scaledSpaceTarget)
            //{
            //    MapView.MapCamera.RemoveTarget(scaledSpaceTarget);
            //    scaledSpaceTarget.Terminate();
            //}

            solver.RemoveManeuverNode(this);
        }
    }
}
