#region License
/*
 * Procedural planet renderer.
 * Copyright (c) 2008-2011 INRIA
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Proland is distributed under a dual-license scheme.
 * You can obtain a specific license from Inria: proland-licensing@inria.fr.
 *
 * Authors: Justin Hawkins 2014.
 * Modified by Denis Ovchinnikov 2015-2016
 */
#endregion

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
            xmin = System.Math.Min(p.x, q.x);
            xmax = System.Math.Max(p.x, q.x);
            ymin = System.Math.Min(p.y, q.y);
            ymax = System.Math.Max(p.y, q.y);
            zmin = System.Math.Min(p.z, q.z);
            zmax = System.Math.Max(p.z, q.z);
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

        public Vector3d Center()
        {
            return new Vector3d((xmin + xmax) / 2.0, (ymin + ymax) / 2.0, (zmin + zmax) / 2.0);
        }

        //Returns the bounding box containing this box and the given point.
        public Box3d Enlarge(Vector3d p)
        {
            return new Box3d(System.Math.Min(xmin, p.x), System.Math.Max(xmax, p.x),
                                System.Math.Min(ymin, p.y), System.Math.Max(ymax, p.y),
                                System.Math.Min(zmin, p.z), System.Math.Max(zmax, p.z));
        }

        //Returns the bounding box containing this box and the given box.
        public Box3d Enlarge(Box3d r)
        {
            return new Box3d(System.Math.Min(xmin, r.xmin), System.Math.Max(xmax, r.xmax),
                                System.Math.Min(ymin, r.ymin), System.Math.Max(ymax, r.ymax),
                                System.Math.Min(zmin, r.zmin), System.Math.Max(zmax, r.zmax));
        }

        //Returns true if this bounding box contains the given point.
        public bool Contains(Vector3d p)
        {
            return (p.x >= xmin && p.x <= xmax && p.y >= ymin && p.y <= ymax && p.z >= zmin && p.z <= zmax);
        }
    } 
}