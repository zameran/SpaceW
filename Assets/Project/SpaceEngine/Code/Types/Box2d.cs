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
    public class Box2d
    {
        public double xmin, xmax, ymin, ymax;

        public Box2d()
        {
            xmin = double.PositiveInfinity;
            xmax = double.NegativeInfinity;
            ymin = double.PositiveInfinity;
            ymax = double.NegativeInfinity;
        }

        public Box2d(double xmin, double xmax, double ymin, double ymax)
        {
            this.xmin = xmin;
            this.xmax = xmax;
            this.ymin = ymin;
            this.ymax = ymax;
        }

        public Box2d(Vector2d p, Vector2d q)
        {
            xmin = Math.Min(p.x, q.x);
            xmax = Math.Max(p.x, q.x);
            ymin = Math.Min(p.y, q.y);
            ymax = Math.Max(p.y, q.y);
        }

        /// <summary>
        /// Calculate the center of this bounding box.
        /// </summary>
        /// <returns>Returns the center point of this bounding box.</returns>
        public Vector2d Center()
        {
            return new Vector2d((xmin + xmax) / 2.0, (ymin + ymax) / 2.0);
        }

        public double Width()
        {
            return xmax - xmin;
        }

        public double Height()
        {
            return ymax - ymin;
        }

        public double Area()
        {
            return (xmax - xmin) * (ymax - ymin);
        }

        /// <summary>
        /// Enlarge the bounding box.
        /// </summary>
        /// <param name="w">The border.</param>
        /// <returns>Returns the bounding box containing this box and the given border.</returns>
        public Box2d Enlarge(double w)
        {
            return new Box2d(xmin - w, xmax + w, ymin - w, ymax + w);
        }

        /// <summary>
        /// Enlarge the bounding box.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <returns>Returns the bounding box containing this box and the given point.</returns>
        public Box2d Enlarge(Vector2d p)
        {
            return new Box2d(Math.Min(xmin, p.x), Math.Max(xmax, p.x), Math.Min(ymin, p.y), Math.Max(ymax, p.y));
        }

        /// <summary>
        /// Enlarge the bounding box.
        /// </summary>
        /// <param name="bb">The bounding box.</param>
        /// <returns>Returns the bounding box containing this box and the given box.</returns>
        public Box2d Enlarge(Box2d bb)
        {
            return new Box2d(Math.Min(xmin, bb.xmin), Math.Max(xmax, bb.xmax), Math.Min(ymin, bb.ymin), Math.Max(ymax, bb.ymax));
        }

        /// <summary>
        /// Is the bounding box contains the giveen point?
        /// </summary>
        /// <param name="p">The point.</param>
        /// <returns>Returns true if this bounding box contains the given point.</returns>
        public bool Contains(Vector2d p)
        {
            return (p.x >= xmin && p.x <= xmax && p.y >= ymin && p.y <= ymax);
        }

        /// <summary>
        /// Is the bounding box contains the giveen bounding box?
        /// </summary>
        /// <param name="bb">The bounding box.</param>
        /// <returns>Returns true if this bounding box contains the given bounding box.</returns>
        public bool Contains(Box2d bb)
        {
            return (bb.xmin >= xmin && bb.xmax <= xmax && bb.ymin >= ymin && bb.ymax <= ymax);
        }

        /// <summary>
        /// Is the bounding box intersects the given box?
        /// </summary>
        /// <param name="bb">The bounding box.</param>
        /// <returns>Returns true if this bounding box intersects the given bounding box.</returns>
        public bool Intersects(Box2d bb)
        {
            return (bb.xmax >= xmin && bb.xmin <= xmax && bb.ymax >= ymin && bb.ymin <= ymax);
        }

        /// <summary>
        /// Calculate the nearest point to a contained in the bounding box.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <returns>Returns the nearest point to a contained in the bounding box.</returns>
        public Vector2d NearestInnerPoint(Vector2d p)
        {
            var nearest = new Vector2d(p);

            if (p.x < xmin) { nearest.x = xmin; }
            else if (p.x > xmax) { nearest.x = xmax; }

            if (p.y < ymin) { nearest.y = ymin; }
            else if (p.y > ymax) { nearest.y = ymax; }

            return nearest;
        }
    }
}