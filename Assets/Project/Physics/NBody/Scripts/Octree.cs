using System;

using UnityEngine;

namespace NBody
{
    /// <summary>
    /// Represents the spatial tree structure in the Barnes-Hut algorithm. 
    /// </summary>
    public class Octree
    { 
        /// <summary>
        /// The tolerance of the mass grouping approximation in the simulation. A 
        /// body is only accelerated when the ratio of the tree's width to the 
        /// distance (from the tree's center of mass to the body) is less than this.
        /// </summary>
        private const double Tolerance = 0.5;

        /// <summary>
        /// The softening factor for the acceleration equation. This dampens the 
        /// the slingshot effect during close encounters of bodies. 
        /// </summary>
        private const double Epsilon = 700;

        /// <summary>
        /// The minimum width of a tree. Subtrees are not created when if their width 
        /// would be smaller than this value. 
        /// </summary>
        private const double MinimumWidth = 1;

        /// <summary>
        /// The number of bodies in the tree. 
        /// </summary>
        public int BodyCount = 0;

        /// <summary>
        /// The total mass of the bodies contained in the tree. 
        /// </summary>
        public double Mass = 0;

        /// <summary>
        /// The collection of subtrees for the tree. 
        /// </summary>
        private Octree[] _subtrees = null;

        /// <summary>
        /// The location of the center of the tree's bounds. 
        /// </summary>
        private Vector _location;

        /// <summary>
        /// The width of the tree's bounds. 
        /// </summary>
        private double _width = 0;

        /// <summary>
        /// The location of the center of mass of the bodies contained in the tree. 
        /// </summary>
        private Vector _centerOfMass = Vector.Zero;

        /// <summary>
        /// The first body added to the tree. This is used when the first Body must 
        /// be added to subtrees at a later time. 
        /// </summary>
        private Body _firstBody = null;

        /// <summary>
        /// Constructs a tree with the given width located about the origin.
        /// </summary>
        /// <param name="width">The width of the new tree.</param>
        public Octree(double width)
        {
            _width = width;
        }

        /// <summary>
        /// Constructs a tree with the given location and width.
        /// </summary>
        /// <param name="location">The location of the center of the new tree.</param>
        /// <param name="width">The width of the new tree.</param>
        public Octree(Vector location, double width)
            : this(width)
        {
            _location = _centerOfMass = location;
        }

        /// <summary>
        /// Adds a body to the tree and subtrees if appropriate. 
        /// </summary>
        /// <param name="body">The body to add to the tree.</param>
        public void Add(Body body)
        {
            _centerOfMass = (Mass * _centerOfMass + body.Mass * body.Location) / (Mass + body.Mass);
            Mass += body.Mass;
            BodyCount++;

            if (BodyCount == 1)
                _firstBody = body;
            else
            {
                AddToSubtree(body);
                if (BodyCount == 2)
                    AddToSubtree(_firstBody);
            }
        }

        /// <summary>
        /// Adds a body to the appropriate subtree based on its spatial location. The 
        /// subtree collection and individual subtrees are initialized as necessary. 
        /// </summary>
        /// <param name="body">The body to add to a subtree.</param>
        private void AddToSubtree(Body body)
        {
            double subtreeWidth = _width / 2;

            // Don't create subtrees if it violates the width limit.
            if (subtreeWidth < MinimumWidth)
                return;

            if (_subtrees == null)
                _subtrees = new Octree[8];

            // Determine which subtree the body belongs in and add it to that subtree. 
            int subtreeIndex = 0;

            for (int i = -1; i <= 1; i += 2)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    for (int k = -1; k <= 1; k += 2)
                    {
                        Vector subtreeLocation = _location + (subtreeWidth / 2) * new Vector(i, j, k);

                        // Determine if the body is contained within the bounds of the subtree under 
                        // consideration. 
                        if (Math.Abs(subtreeLocation.X - body.Location.X) <= subtreeWidth / 2
                         && Math.Abs(subtreeLocation.Y - body.Location.Y) <= subtreeWidth / 2
                         && Math.Abs(subtreeLocation.Z - body.Location.Z) <= subtreeWidth / 2)
                        {

                            if (_subtrees[subtreeIndex] == null)
                                _subtrees[subtreeIndex] = new Octree(subtreeLocation, subtreeWidth);
                            _subtrees[subtreeIndex].Add(body);
                            return;
                        }
                        subtreeIndex++;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the acceleration of a body based on the properties of the tree. 
        /// </summary>
        /// <param name="body">The body to accelerate.</param>
        public void Accelerate(Body body)
        {
            double dx = _centerOfMass.X - body.Location.X;
            double dy = _centerOfMass.Y - body.Location.Y;
            double dz = _centerOfMass.Z - body.Location.Z;
            double dSquared = dx * dx + dy * dy + dz * dz;

            // Case 1. The tree contains only one body and it is not the one in the 
            //         tree so we can perform the acceleration. 
            //
            // Case 2. The width to distance ratio is within the defined tolerance so 
            //         we consider the tree to be effectively a single massive body and 
            //         perform the acceleration. 
            if ((BodyCount == 1 && body != _firstBody) || (_width * _width < Tolerance * Tolerance * dSquared))
            {
                // Calculate a normalized acceleration value and multiply it with the 
                // displacement in each coordinate to get that coordinate's acceleration 
                // component. 
                double distance = Math.Sqrt(dSquared + Epsilon * Epsilon);
                double normAcc = World.G * Mass / (distance * distance * distance);

                body.Acceleration.X += normAcc * dx;
                body.Acceleration.Y += normAcc * dy;
                body.Acceleration.Z += normAcc * dz;
            }

            // Case 3. More granularity is needed so we accelerate at the subtrees. 
            else if (_subtrees != null)
                foreach (Octree subtree in _subtrees)
                    if (subtree != null)
                        subtree.Accelerate(body);
        }

        /// <summary>
        /// Draws the tree and its subtrees. 
        /// </summary>
        public void Draw()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_location, 
                                new Vector3((float)_width, (float)_width, (float)_width));

            if (_subtrees != null)
                foreach (Octree subtree in _subtrees)
                    if (subtree != null)
                        subtree.Draw();
        }
    }
}