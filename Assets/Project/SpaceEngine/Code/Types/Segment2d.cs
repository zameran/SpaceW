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
    [Serializable]
    public struct Segment2d
    {
        /// <summary>
        /// One of the segment extremity.
        /// </summary>
        public Vector2d a;

        /// <summary>
        /// The vector joining <see cref="Segment2d.a"/> to the other segment extremity.
        /// </summary>
        public Vector2d ab;

        /// <summary>
        /// Creates a new segment with the given extremities.
        /// </summary>
        /// <param name="a">A segment extremity.</param>
        /// <param name="b">The other segment extremity.</param>
        public Segment2d(Vector2d a, Vector2d b)
        {
            this.a = new Vector2d(a);
            this.ab = b - a;
        }

        /// <summary>
        /// The square distance between the given point and the line, defined by this segment.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <returns>Returns the square distance between the given point and the line, defined by this segment.</returns>
        public double LineDistanceSqr(Vector2d p)
        {
            var ap = p - a;
            var dotprod = ab.Dot(ap);
            var projLenSq = dotprod * dotprod / ab.SqrMagnitude();

            return ap.SqrMagnitude() - projLenSq;
        }

        /// <summary>
        /// The square distance between the given point and this segment.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <returns>Returns the square distance between the given point and this segment.</returns>
        public double SegmentDistanceSqr(Vector2d p)
        {
            var ap = p - a;
            var dotprod = ab.Dot(ap);

            double projlenSq;

            if (dotprod <= 0.0)
            {
                projlenSq = 0.0;
            }
            else
            {
                ap = ab - ap;
                dotprod = ab.Dot(ap);

                if (dotprod <= 0.0)
                {
                    projlenSq = 0.0;
                }
                else
                {
                    projlenSq = dotprod * dotprod / ab.SqrMagnitude();
                }
            }

            return ap.SqrMagnitude() - projlenSq;
        }

        /// <summary>
        /// Is this segment interesects the given segment?
        /// </summary>
        /// <param name="s">The segment</param>
        /// <returns>Returns true if this segment intersects the given segment.</returns>
        public bool Intersects(Segment2d s)
        {
            var aa = s.a - a;
            var det = Vector2d.Cross(ab, s.ab);
            var t0 = Vector2d.Cross(aa, s.ab) / det;

            if (t0 > 0.0 && t0 < 1.0)
            {
                var t1 = Vector2d.Cross(aa, ab) / det;

                return t1 > 0 && t1 < 1;
            }

            return false;
        }

        /// <summary>
        /// Is this segment intersects the given segment? If any, an intersection will be returned as vector.
        /// </summary>
        /// <param name="s">The segment.</param>
        /// <param name="i">Where to store the intersection point, if any.</param>
        /// <returns>Returns true if this segment intersects the given segment.</returns>
        public bool Intersects(Segment2d s, ref Vector2d i)
        {
            var aa = s.a - a;
            var det = Vector2d.Cross(ab, s.ab);
            var t0 = Vector2d.Cross(aa, s.ab) / det;

            if (t0 > 0.0 && t0 < 1.0)
            {
                i = a + ab * t0;

                var t1 = Vector2d.Cross(aa, ab) / det;

                return t1 > 0 && t1 < 1;
            }

            return false;
        }

        /// <summary>
        /// Is this segment is inside or intersects the given bounding box?
        /// </summary>
        /// <param name="r">The bounding box.</param>
        /// <returns>Returns true if this segment is inside or intersects the given bounding box.</returns>
        public bool Intersects(Box2d r)
        {
            var b = a + ab;

            if (r.Contains(a) || r.Contains(b)) { return true; }

            var t = new Box2d(a, b);

            if (t.xmin > r.xmax || t.xmax < r.xmin || t.ymin > r.ymax || t.ymax < r.ymin) { return false; }

            var p0 = Vector2d.Cross(ab, new Vector2d(r.xmin, r.ymin) - a);
            var p1 = Vector2d.Cross(ab, new Vector2d(r.xmax, r.ymin) - a);

            if (p1 * p0 <= 0.0) { return true; }

            var p2 = Vector2d.Cross(ab, new Vector2d(r.xmin, r.ymax) - a);

            if (p2 * p0 <= 0.0) { return true; }

            var p3 = Vector2d.Cross(ab, new Vector2d(r.xmax, r.ymax) - a);

            if (p3 * p0 <= 0.0) { return true; }

            return false;
        }

        /// <summary>
        /// Is this segment, with the given width, contains the given point?
        /// More precisely this method returns true if the stroked path defined from this segment, with a cap "butt" style, contains the given point.
        /// </summary>
        /// <param name="p">The point.</param>
        /// <param name="w">The width of this segment.</param>
        /// <returns>Returns true if this segment, with the given width, contains the given point. </returns>
        public bool Contains(Vector2d p, double w)
        {
            var ap = p - a;
            var dotprod = ab.Dot(ap);

            if (dotprod <= 0.0)
            {
                return false;
            }
            else
            {
                ap = ab - ap;
                dotprod = ab.Dot(ap);

                if (dotprod <= 0.0)
                {
                    return false;
                }
                else
                {
                    return ap.SqrMagnitude() - (dotprod * dotprod / ab.SqrMagnitude()) < w * w;
                }
            }
        }
    }
}