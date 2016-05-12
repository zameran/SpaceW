namespace Experimental
{
    using System;
    using UnityEngine;

    public class Trajectory
    {
        private Vector3d[] points;

        public double[] trueAnomalies;
        private float[] times;

        private Vector3d periapsis;
        private Vector3d apoapsis;
        private Vector3d patchStartPoint;
        private Vector3d patchEndPoint;
        private Vector3d refBodyPos;

        public CelestialBody referenceBody;

        public Orbit patch;

        public Trajectory(Vector3d[] points, float[] times, double[] trueAnomalies, Vector3d periapsis, Vector3d apoapsis, Vector3d patchStartPoint, Vector3d patchEndPoint, Vector3d refBodyPos, Orbit patch)
        {
            this.patch = patch;
            this.referenceBody = patch.referenceBody;
            this.points = points;
            this.times = times;
            this.trueAnomalies = trueAnomalies;
            this.apoapsis = apoapsis;
            this.periapsis = periapsis;
            this.patchStartPoint = patchStartPoint;
            this.patchEndPoint = patchEndPoint;
            this.refBodyPos = refBodyPos;
        }

        public Trajectory()
        {

        }

        public Vector3d ConvertPointToLerped(Vector3d point, double time, CelestialBody relativeFrom, CelestialBody relativeTo, double minUT, double escapeUT, float linearity)
        {
            Vector3d from = point + (Vector3d)relativeFrom.transform.position;
            Vector3d to = ((point + relativeFrom.GetTruePositionAtUT(time)) - relativeTo.GetTruePositionAtUT(time)) + (Vector3d)relativeTo.transform.position;

            return Vector3d.Lerp(from, to, Mathf.Pow(Mathf.InverseLerp((float)Math.Max(minUT, Planetarium.GetUniversalTime()), (float)escapeUT, (float)time), linearity));
        }

        public Vector3d ConvertPointToLocal(Vector3d point)
        {
            return point + (Vector3d)referenceBody.transform.position;
        }

        public Vector3d ConvertPointToLocalAtUT(Vector3d point, double atUT, CelestialBody relativeTo)
        {
            return ((point + referenceBody.GetTruePositionAtUT(atUT)) - relativeTo.GetTruePositionAtUT(atUT)) + (Vector3d)relativeTo.transform.position;
        }

        public Vector3d ConvertPointToRelative(Vector3d point, double time, CelestialBody relativeTo)
        {
            return ((point + referenceBody.GetTruePositionAtUT(time)) - relativeTo.GetTruePositionAtUT(time)) + (Vector3d)relativeTo.transform.position;
        }

        public Vector3d GetApoapsisLerped(CelestialBody relativeFrom, CelestialBody relativeTo, double minUT, double escapeUT, float linearity)
        {
            return ConvertPointToLerped(apoapsis, patch.StartUT + patch.timeToAp, relativeFrom, relativeTo, minUT, escapeUT, linearity);
        }

        public Vector3d GetApoapsisLocal()
        {
            return ConvertPointToLocal(apoapsis);
        }

        public Vector3d GetApoapsisLocalAtUT(CelestialBody relativeTo, double atUT)
        {
            return ConvertPointToLocalAtUT(apoapsis, atUT, relativeTo);
        }

        public Vector3d GetApoapsisRelative(CelestialBody relativeTo)
        {
            return ConvertPointToRelative(apoapsis, patch.StartUT + patch.timeToAp, relativeTo);
        }

        public Color[] GetColors(Color baseColor)
        {
            Color[] colorArray = new Color[points.Length];

            for (int i = points.Length - 1; i >= 0; i--)
            {
                if (patch.eccentricity >= 1 || patch.patchEndTransition == Orbit.PatchTransitionType.FINAL || i >= 2 && i < points.Length - 2)
                {
                    colorArray[i] = baseColor;
                }
                else
                {
                    colorArray[i] = Color.clear;
                }
            }

            return colorArray;
        }

        public Vector3d GetPatchEndLerped(CelestialBody relativeFrom, CelestialBody relativeTo, double minUT, double escapeUT, float linearity)
        {
            return ConvertPointToLerped(patchEndPoint, patch.EndUT, relativeFrom, relativeTo, minUT, escapeUT, linearity);
        }

        public Vector3d GetPatchEndLocal()
        {
            return ConvertPointToLocal(patchEndPoint);
        }

        public Vector3d GetPatchEndLocalAtUT(CelestialBody relativeTo, double atUT)
        {
            return ConvertPointToLocalAtUT(patchEndPoint, atUT, relativeTo);
        }

        public Vector3d GetPatchEndRelative(CelestialBody relativeTo)
        {
            return ConvertPointToRelative(patchEndPoint, patch.EndUT, relativeTo);
        }

        public Vector3d GetPatchStartLerped(CelestialBody relativeFrom, CelestialBody relativeTo, double minUT, double escapeUT, float linearity)
        {
            return ConvertPointToLerped(patchStartPoint, patch.StartUT, relativeFrom, relativeTo, minUT, escapeUT, linearity);
        }

        public Vector3d GetPatchStartLocal()
        {
            return ConvertPointToLocal(patchStartPoint);
        }

        public Vector3d GetPatchStartLocalAtUT(CelestialBody relativeTo, double atUT)
        {
            return ConvertPointToLocalAtUT(patchStartPoint, atUT, relativeTo);
        }

        public Vector3d GetPatchStartRelative(CelestialBody relativeTo)
        {
            return ConvertPointToRelative(patchStartPoint, patch.StartUT, relativeTo);
        }

        public Vector3d GetPeriapsisLerped(CelestialBody relativeFrom, CelestialBody relativeTo, double minUT, double escapeUT, float linearity)
        {
            if (patch.eccentricity < 1 && patch.patchEndTransition != Orbit.PatchTransitionType.FINAL && patch.StartUT + patch.timeToPe > patch.EndUT)
            {
                patch.timeToPe = patch.timeToPe - patch.period;
            }

            return ConvertPointToLerped(periapsis, patch.StartUT + patch.timeToPe, relativeFrom, relativeTo, minUT, escapeUT, linearity);
        }

        public Vector3d GetPeriapsisLocal()
        {
            return ConvertPointToLocal(periapsis);
        }

        public Vector3d GetPeriapsisLocalAtUT(CelestialBody relativeTo, double atUT)
        {
            return ConvertPointToLocalAtUT(periapsis, atUT, relativeTo);
        }

        public Vector3d GetPeriapsisRelative(CelestialBody relativeTo)
        {
            if (patch.eccentricity < 1 && patch.patchEndTransition != Orbit.PatchTransitionType.FINAL && this.patch.StartUT + this.patch.timeToPe > this.patch.EndUT)
            {
                patch.timeToPe = patch.timeToPe - patch.period;
            }

            return ConvertPointToRelative(periapsis, patch.StartUT + patch.timeToPe, relativeTo);
        }

        public Vector3d[] GetPoints()
        {
            return points;
        }

        public Vector3d[] GetPointsLerped(CelestialBody relativeFrom, CelestialBody relativeTo, double minUT, double escapeUT, float linearity)
        {
            Vector3d[] lerped = new Vector3d[points.Length];

            if (referenceBody != null)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    lerped[i] = ConvertPointToLerped(points[i], times[i], relativeFrom, relativeTo, minUT, escapeUT, linearity);
                }
            }

            return lerped;
        }

        public Vector3d[] GetPointsLocal()
        {
            Vector3d[] local = new Vector3d[points.Length];

            if (referenceBody != null)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    local[i] = ConvertPointToLocal(points[i]);
                }
            }

            return local;
        }

        public Vector3d[] GetPointsLocalAtUT(CelestialBody relativeTo, double atUT)
        {
            Vector3d[] localAtUT = new Vector3d[points.Length];

            if (referenceBody != null)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    localAtUT[i] = ConvertPointToLocalAtUT(points[i], atUT, relativeTo);
                }
            }

            return localAtUT;
        }

        public Vector3d[] GetPointsRelative(CelestialBody relativeTo)
        {
            Vector3d[] relative = new Vector3d[points.Length];

            if (referenceBody != null)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    relative[i] = ConvertPointToRelative(points[i], times[i], relativeTo);
                }
            }

            return relative;
        }

        public Vector3d GetRefBodyPosLerped(CelestialBody relativeFrom, CelestialBody relativeTo, double minUT, double escapeUT, float linearity)
        {
            if (patch.eccentricity < 1 && patch.patchEndTransition != Orbit.PatchTransitionType.FINAL && patch.StartUT + patch.timeToPe > patch.EndUT)
            {
                patch.timeToPe = patch.timeToPe - patch.period;
            }

            return ConvertPointToLerped(refBodyPos, patch.StartUT + patch.timeToPe, relativeFrom, relativeTo, minUT, escapeUT, linearity);
        }

        public Vector3d GetRefBodyPosLocal()
        {
            return ConvertPointToLocal(refBodyPos);
        }

        public Vector3d GetRefBodyPosLocalAtUT(CelestialBody relativeTo, double atUT)
        {
            return ConvertPointToLocalAtUT(refBodyPos, atUT, relativeTo);
        }

        public Vector3d GetRefBodyPosRelative(CelestialBody relativeTo)
        {
            if (patch.eccentricity < 1 && patch.patchEndTransition != Orbit.PatchTransitionType.FINAL && patch.StartUT + patch.timeToPe > patch.EndUT)
            {
                patch.timeToPe = patch.timeToPe - patch.period;
            }

            return ConvertPointToRelative(refBodyPos, patch.StartUT + patch.timeToPe, relativeTo);
        }

        public float[] GetTimes()
        {
            return times;
        }

        public Trajectory ReframeToLerped(CelestialBody relativeFrom, CelestialBody relativeTo, double minUT, double escapeUT, float linearity)
        {
            Trajectory trajectory = new Trajectory()
            {
                patch = this.patch,
                referenceBody = this.referenceBody,
                points = this.GetPointsLerped(relativeFrom, relativeTo, minUT, escapeUT, linearity),
                periapsis = this.ConvertPointToLerped(this.periapsis, this.patch.StartUT + this.patch.timeToPe, relativeFrom, relativeTo, minUT, escapeUT, linearity),
                apoapsis = this.ConvertPointToLerped(this.apoapsis, this.patch.StartUT + this.patch.timeToAp, relativeFrom, relativeTo, minUT, escapeUT, linearity),
                patchStartPoint = this.ConvertPointToLerped(this.patchStartPoint, this.patch.StartUT, relativeFrom, relativeTo, minUT, escapeUT, linearity),
                patchEndPoint = this.ConvertPointToLerped(this.patchEndPoint, this.patch.EndUT, relativeFrom, relativeTo, minUT, escapeUT, linearity),
                refBodyPos = this.ConvertPointToLerped(this.refBodyPos, this.patch.StartUT + this.patch.timeToPe, relativeFrom, relativeTo, minUT, escapeUT, linearity)
            };

            return trajectory;
        }

        public Trajectory ReframeToLocal()
        {
            Trajectory trajectory = new Trajectory()
            {
                patch = this.patch,
                referenceBody = this.referenceBody,
                points = this.GetPointsLocal(),
                periapsis = this.ConvertPointToLocal(this.periapsis),
                apoapsis = this.ConvertPointToLocal(this.apoapsis),
                patchStartPoint = this.ConvertPointToLocal(this.patchStartPoint),
                patchEndPoint = this.ConvertPointToLocal(this.patchEndPoint),
                refBodyPos = this.ConvertPointToLocal(this.refBodyPos)
            };

            return trajectory;
        }

        public Trajectory ReframeToLocalAtUT(CelestialBody relativeTo, double atUT)
        {
            Trajectory trajectory = new Trajectory()
            {
                patch = this.patch,
                referenceBody = this.referenceBody,
                points = this.GetPointsLocalAtUT(relativeTo, atUT),
                periapsis = this.ConvertPointToLocalAtUT(this.periapsis, atUT, relativeTo),
                apoapsis = this.ConvertPointToLocalAtUT(this.apoapsis, atUT, relativeTo),
                patchStartPoint = this.ConvertPointToLocalAtUT(this.patchStartPoint, atUT, relativeTo),
                patchEndPoint = this.ConvertPointToLocalAtUT(this.patchEndPoint, atUT, relativeTo),
                refBodyPos = this.ConvertPointToLocalAtUT(this.refBodyPos, atUT, relativeTo)
            };

            return trajectory;
        }

        public Trajectory ReframeToRelative(CelestialBody relativeTo)
        {
            Trajectory trajectory = new Trajectory()
            {
                patch = this.patch,
                referenceBody = this.referenceBody,
                points = this.GetPointsRelative(relativeTo),
                periapsis = this.ConvertPointToRelative(this.periapsis, this.patch.StartUT + this.patch.timeToPe, relativeTo),
                apoapsis = this.ConvertPointToRelative(this.apoapsis, this.patch.StartUT + this.patch.timeToAp, relativeTo),
                patchStartPoint = this.ConvertPointToRelative(this.patchStartPoint, this.patch.StartUT, relativeTo),
                patchEndPoint = this.ConvertPointToRelative(this.patchEndPoint, this.patch.EndUT, relativeTo),
                refBodyPos = this.ConvertPointToRelative(this.refBodyPos, this.patch.StartUT + this.patch.timeToPe, relativeTo)
            };

            return trajectory;
        }
    }
}