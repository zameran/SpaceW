#region License
//
// Procedural planet renderer.
// Copyright (c) 2008-2011 INRIA
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// Proland is distributed under a dual-license scheme.
// You can obtain a specific license from Inria: proland-licensing@inria.fr.
//
// Authors: Justin Hawkins 2014.
// Modified by Denis Ovchinnikov 2015-2017
#endregion

using System;

namespace UnityEngine
{
    public class Box3d
    {
        public double xmin, xmax, ymin, ymax, zmin, zmax;

        public Box3d()
        {
            xmin = double.PositiveInfinity;
            xmax = double.NegativeInfinity;
            ymin = double.PositiveInfinity;
            ymax = double.NegativeInfinity;
            zmin = double.PositiveInfinity;
            zmax = double.NegativeInfinity;
        }

        public Box3d(double xmin, double xmax, double ymin, double ymax, double zmin, double zmax)
        {
            this.xmin = xmin;
            this.xmax = xmax;
            this.ymin = ymin;
            this.ymax = ymax;
            this.zmin = zmin;
            this.zmax = zmax;
        }

        public Box3d(Vector3d p, Vector3d q)
        {
            xmin = Math.Min(p.x, q.x);
            xmax = Math.Max(p.x, q.x);
            ymin = Math.Min(p.y, q.y);
            ymax = Math.Max(p.y, q.y);
            zmin = Math.Min(p.z, q.z);
            zmax = Math.Max(p.z, q.z);
        }

        public Box3d(Vector3 min, Vector3 max)
        {
            xmin = min.x;
            xmax = max.x;
            ymin = min.y;
            ymax = max.y;
            zmin = min.z;
            zmax = max.z;
        }

        /// <summary>
        /// Calculate the center of this bounding box.
        /// </summary>
        /// <returns>Returns the center point of this bounding box.</returns>
        public Vector3d Center()
        {
            return new Vector3d((xmin + xmax) / 2.0, (ymin + ymax) / 2.0, (zmin + zmax) / 2.0);
        }

        /// <summary>
        /// Enlarge the bounding box.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <returns>Returns the bounding box containing this box and the given point.</returns>
        public Box3d Enlarge(Vector3d p)
        {
            return new Box3d(Math.Min(xmin, p.x), Math.Max(xmax, p.x),
                             Math.Min(ymin, p.y), Math.Max(ymax, p.y),
                             Math.Min(zmin, p.z), Math.Max(zmax, p.z));
        }

        /// <summary>
        /// Enlarge the bounding box.
        /// </summary>
        /// <param name="bb">The bounding box.</param>
        /// <returns>Returns the bounding box containing this box and the given box.</returns>
        public Box3d Enlarge(Box3d bb)
        {
            return new Box3d(Math.Min(xmin, bb.xmin), Math.Max(xmax, bb.xmax),
                             Math.Min(ymin, bb.ymin), Math.Max(ymax, bb.ymax),
                             Math.Min(zmin, bb.zmin), Math.Max(zmax, bb.zmax));
        }

        /// <summary>
        /// Is the bounding box contains the giveen point?
        /// </summary>
        /// <param name="p">The point.</param>
        /// <returns>Returns true if this bounding box contains the given point.</returns>
        public bool Contains(Vector3d p)
        {
            return (p.x >= xmin && p.x <= xmax && p.y >= ymin && p.y <= ymax && p.z >= zmin && p.z <= zmax);
        }
    }
}