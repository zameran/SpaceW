#region License
/* Procedural planet generator.
 *
 * Copyright (C) 2015-2016 Denis Ovchinnikov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

using System;

using UnityEngine;

namespace NBody
{
    /// <summary>
    /// Specifies the system type to generate. 
    /// </summary>
    [Serializable]
    public enum SystemType { None, SlowParticles, FastParticles, MassiveBody, OrbitalSystem, BinarySystem, PlanetarySystem, DistributionTest };

    /// <summary>
    /// Represents the world of the simulation. 
    /// </summary>
    public class World : MonoBehaviour
    {
        /// <summary>
        /// The gravitational constant. 
        /// </summary>
        public static double G = 6.6740831;

        /// <summary>
        /// The maximum speed. 
        /// </summary>
        public static double C = 299792458;

        public static double S = 1;

        /// <summary>
        /// The number of bodies allocated in the simulation. 
        /// </summary>
        public int BodyAllocationCount
        {
            get
            {
                return _bodies.Length;
            }
            set
            {
                if (_bodies.Length != value)
                {
                    _bodies = new Body[value];
                }
            }
        }

        /// <summary>
        /// The number of bodies that exist in the simulation. 
        /// </summary>
        public int BodyCount
        {
            get
            {
                return _tree == null ? 0 : _tree.BodyCount;
            }
        }

        /// <summary>
        /// The total mass of the bodies that exist in the simulation. 
        /// </summary>
        public double TotalMass
        {
            get
            {
                return _tree == null ? 0 : _tree.Mass;
            }
        }

        /// <summary>
        /// Determines whether the simulation is active or paused. 
        /// </summary>
        public bool Active
        {
            get;
            set;
        }

        /// <summary>
        /// Determines whether to draw the tree structure for calculating forces. 
        /// </summary>
        public bool DrawTree = false;

        /// <summary>
        /// The collection of bodies in the simulation. 
        /// </summary>
        private Body[] _bodies = new Body[32];

        /// <summary>
        /// The tree for calculating forces. 
        /// </summary>
        private Octree _tree;

        private Octree _predictionTree;

        private void Start()
        {
            Generate(SystemType.PlanetarySystem);
        }

        private void Update()
        {
            Simulate();
        }

        private void OnDrawGizmos()
        {
            Draw();
        }

        /// <summary>
        /// Constructs the world and starts the simulation. 
        /// </summary>
        public World()
        {
            Active = true;
        }

        /// <summary>
        /// Advances the simulation by one frame if it is active. 
        /// </summary>
        public void Simulate()
        {
            if (Active)
            {
                // Update the bodies and determine the required tree width. 
                double halfWidth = 0;

                for (int i = 0; i < _bodies.Length; i++)
                {
                    if (_bodies[i] != null)
                    {
                        _bodies[i].Update();
                        halfWidth = Math.Max(Math.Abs(_bodies[i].Location.X), halfWidth);
                        halfWidth = Math.Max(Math.Abs(_bodies[i].Location.Y), halfWidth);
                        halfWidth = Math.Max(Math.Abs(_bodies[i].Location.Z), halfWidth);
                    }
                }

                // Initialize the root tree and add the bodies. The root tree needs to be 
                // slightly larger than twice the determined half width. 
                _tree = new Octree(2.1 * halfWidth);

                for (int i = 0; i < _bodies.Length; i++)
                {
                    if(_bodies[i] != null)
                    {
                        _tree.Add(_bodies[i]);
                        _tree.Accelerate(_bodies[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Generates the specified gravitational system. 
        /// </summary>
        /// <param name="type">The system type to generate.</param>
        public void Generate(SystemType type)
        {
            switch (type)
            {
                // Clear bodies. 
                case SystemType.None:
                    Array.Clear(_bodies, 0, _bodies.Length);
                    break;

                // Generate slow particles. 
                case SystemType.SlowParticles:
                    {
                        for (int i = 0; i < _bodies.Length; i++)
                        {
                            double distance = PseudoRandom.Double(1e6);
                            double angle = PseudoRandom.Double(Math.PI * 2);
                            pVector3d location = new pVector3d(Math.Cos(angle) * distance, PseudoRandom.Double(-2e5, 2e5), Math.Sin(angle) * distance);
                            double mass = PseudoRandom.Double(1e6) + 3e4;
                            pVector3d velocity = PseudoRandom.Vector(5);
                            _bodies[i] = new Body(location, mass, velocity);
                        }
                    }
                    break;

                // Generate fast particles. 
                case SystemType.FastParticles:
                    {
                        for (int i = 0; i < _bodies.Length; i++)
                        {
                            double distance = PseudoRandom.Double(1e6);
                            double angle = PseudoRandom.Double(Math.PI * 2);
                            pVector3d location = new pVector3d(Math.Cos(angle) * distance, PseudoRandom.Double(-2e5, 2e5), Math.Sin(angle) * distance);
                            double mass = PseudoRandom.Double(1e6) + 3e4;
                            pVector3d velocity = PseudoRandom.Vector(5e3);
                            _bodies[i] = new Body(location, mass, velocity);
                        }
                    }
                    break;

                // Generate massive body demonstration. 
                case SystemType.MassiveBody:
                    {
                        _bodies[0] = new Body(pVector3d.Zero, 1e10);

                        pVector3d location1 = PseudoRandom.Vector(8e3) + new pVector3d(-3e5, 1e5 + _bodies[0].Radius, 0);
                        double mass1 = 1e6;
                        pVector3d velocity1 = new pVector3d(2e3, 0, 0);
                        _bodies[1] = new Body(location1, mass1, velocity1);

                        for (int i = 2; i < _bodies.Length; i++)
                        {
                            double distance = PseudoRandom.Double(2e5) + _bodies[1].Radius;
                            double angle = PseudoRandom.Double(Math.PI * 2);
                            double vertical = Math.Min(2e8 / distance, 2e4);
                            pVector3d location = (new pVector3d(Math.Cos(angle) * distance, PseudoRandom.Double(-vertical, vertical), Math.Sin(angle) * distance) + _bodies[1].Location);
                            double mass = PseudoRandom.Double(5e5) + 1e5;
                            double speed = Math.Sqrt(_bodies[1].Mass * _bodies[1].Mass * G / ((_bodies[1].Mass + mass) * distance));
                            pVector3d velocity = pVector3d.Cross(location, pVector3d.YAxis).Unit() * speed + velocity1;
                            location = location.Rotate(0, 0, 0, 1, 1, 1, Math.PI * 0.1);
                            velocity = velocity.Rotate(0, 0, 0, 1, 1, 1, Math.PI * 0.1);
                            _bodies[i] = new Body(location, mass, velocity);
                        }
                    }
                    break;

                // Generate orbital system. 
                case SystemType.OrbitalSystem:
                    {
                        _bodies[0] = new Body(1e10);

                        for (int i = 1; i < _bodies.Length; i++)
                        {
                            double distance = PseudoRandom.Double(1e6) + _bodies[0].Radius;
                            double angle = PseudoRandom.Double(Math.PI * 2);
                            pVector3d location = new pVector3d(Math.Cos(angle) * distance, PseudoRandom.Double(-2e4, 2e4), Math.Sin(angle) * distance);
                            double mass = PseudoRandom.Double(1e6) + 3e4;
                            double speed = Math.Sqrt(_bodies[0].Mass * _bodies[0].Mass * G / ((_bodies[0].Mass + mass) * distance));
                            pVector3d velocity = pVector3d.Cross(location, pVector3d.YAxis).Unit() * speed;
                            _bodies[i] = new Body(location, mass, velocity);
                        }
                    }
                    break;

                // Generate binary system. 
                case SystemType.BinarySystem:
                    {
                        double mass1 = PseudoRandom.Double(9e9) + 1e9;
                        double mass2 = PseudoRandom.Double(9e9) + 1e9;
                        double angle0 = PseudoRandom.Double(Math.PI * 2);
                        double distance0 = PseudoRandom.Double(1e5) + 3e4;
                        double distance1 = distance0 / 2;
                        double distance2 = distance0 / 2;
                        pVector3d location1 = new pVector3d(Math.Cos(angle0) * distance1, 0, Math.Sin(angle0) * distance1);
                        pVector3d location2 = new pVector3d(-Math.Cos(angle0) * distance2, 0, -Math.Sin(angle0) * distance2);
                        double speed1 = Math.Sqrt(mass2 * mass2 * G / ((mass1 + mass2) * distance0));
                        double speed2 = Math.Sqrt(mass1 * mass1 * G / ((mass1 + mass2) * distance0));
                        pVector3d velocity1 = pVector3d.Cross(location1, pVector3d.YAxis).Unit() * speed1;
                        pVector3d velocity2 = pVector3d.Cross(location2, pVector3d.YAxis).Unit() * speed2;
                        _bodies[0] = new Body(location1, mass1, velocity1);
                        _bodies[1] = new Body(location2, mass2, velocity2);

                        for (int i = 2; i < _bodies.Length; i++)
                        {
                            double distance = PseudoRandom.Double(1e6);
                            double angle = PseudoRandom.Double(Math.PI * 2);
                            pVector3d location = new pVector3d(Math.Cos(angle) * distance, PseudoRandom.Double(-2e4, 2e4), Math.Sin(angle) * distance);
                            double mass = PseudoRandom.Double(1e6) + 3e4;
                            double speed = Math.Sqrt((mass1 + mass2) * (mass1 + mass2) * G / ((mass1 + mass2 + mass) * distance));
                            speed /= distance >= distance0 / 2 ? 1 : (distance0 / 2 / distance);
                            pVector3d velocity = pVector3d.Cross(location, pVector3d.YAxis).Unit() * speed;
                            _bodies[i] = new Body(location, mass, velocity);
                        }
                    }
                    break;

                // Generate planetary system. 
                case SystemType.PlanetarySystem:
                    {
                        _bodies[0] = new Body(1e10);
                        int planets = PseudoRandom.Int32(10) + 5;
                        int planetsWithRings = PseudoRandom.Int32(1) + 1;
                        int k = 1;
                        for (int i = 1; i < planets + 1 && k < _bodies.Length; i++)
                        {
                            int planetK = k;
                            double distance = PseudoRandom.Double(2e6) + 1e5 + _bodies[0].Radius;
                            double angle = PseudoRandom.Double(Math.PI * 2);
                            pVector3d location = new pVector3d(Math.Cos(angle) * distance, PseudoRandom.Double(-2e4, 2e4), Math.Sin(angle) * distance);
                            double mass = PseudoRandom.Double(1e8) + 1e7;
                            double speed = Math.Sqrt(_bodies[0].Mass * _bodies[0].Mass * G / ((_bodies[0].Mass + mass) * distance));
                            pVector3d velocity = pVector3d.Cross(location, pVector3d.YAxis).Unit() * speed;
                            _bodies[k++] = new Body(location, mass, velocity);

                            // Generate rings.
                            const int RingParticles = 100;
                            if (--planetsWithRings >= 0 && k < _bodies.Length - RingParticles)
                            {
                                for (int j = 0; j < RingParticles; j++)
                                {
                                    double ringDistance = PseudoRandom.Double(1e1) + 1e4 + _bodies[planetK].Radius;
                                    double ringAngle = PseudoRandom.Double(Math.PI * 2);
                                    pVector3d ringLocation = location + new pVector3d(Math.Cos(ringAngle) * ringDistance, 0, Math.Sin(ringAngle) * ringDistance);
                                    double ringMass = PseudoRandom.Double(1e3) + 1e3;
                                    double ringSpeed = Math.Sqrt(_bodies[planetK].Mass * _bodies[planetK].Mass * G / ((_bodies[planetK].Mass + ringMass) * ringDistance));
                                    pVector3d ringVelocity = pVector3d.Cross(location - ringLocation, pVector3d.YAxis).Unit() * ringSpeed + velocity;
                                    _bodies[k++] = new Body(ringLocation, ringMass, ringVelocity);
                                }
                                continue;
                            }

                            // Generate moons. 
                            int moons = PseudoRandom.Int32(4);
                            while (moons-- > 0 && k < _bodies.Length)
                            {
                                double moonDistance = PseudoRandom.Double(1e4) + 5e3 + _bodies[planetK].Radius;
                                double moonAngle = PseudoRandom.Double(Math.PI * 2);
                                pVector3d moonLocation = location + new pVector3d(Math.Cos(moonAngle) * moonDistance, PseudoRandom.Double(-2e3, 2e3), Math.Sin(moonAngle) * moonDistance);
                                double moonMass = PseudoRandom.Double(1e6) + 1e5;
                                double moonSpeed = Math.Sqrt(_bodies[planetK].Mass * _bodies[planetK].Mass * G / ((_bodies[planetK].Mass + moonMass) * moonDistance));
                                pVector3d moonVelocity = pVector3d.Cross(moonLocation - location, pVector3d.YAxis).Unit() * moonSpeed + velocity;
                                _bodies[k++] = new Body(moonLocation, moonMass, moonVelocity);
                            }
                        }

                        // Generate asteroid belt.
                        while (k < _bodies.Length)
                        {
                            double asteroidDistance = PseudoRandom.Double(4e5) + 1e6;
                            double asteroidAngle = PseudoRandom.Double(Math.PI * 2);
                            pVector3d asteroidLocation = new pVector3d(Math.Cos(asteroidAngle) * asteroidDistance, PseudoRandom.Double(-1e3, 1e3), Math.Sin(asteroidAngle) * asteroidDistance);
                            double asteroidMass = PseudoRandom.Double(1e6) + 3e4;
                            double asteroidSpeed = Math.Sqrt(_bodies[0].Mass * _bodies[0].Mass * G / ((_bodies[0].Mass + asteroidMass) * asteroidDistance));
                            pVector3d asteroidVelocity = pVector3d.Cross(asteroidLocation, pVector3d.YAxis).Unit() * asteroidSpeed;
                            _bodies[k++] = new Body(asteroidLocation, asteroidMass, asteroidVelocity);
                        }
                    }
                    break;

                // Generate distribution test. 
                case SystemType.DistributionTest:
                    {
                        Array.Clear(_bodies, 0, _bodies.Length);
                        double distance = 4e4;
                        double mass = 5e6;

                        int side = (int)Math.Pow(_bodies.Length, 1.0 / 3);
                        int k = 0;
                        for (int a = 0; a < side; a++)
                            for (int b = 0; b < side; b++)
                                for (int c = 0; c < side; c++)
                                    _bodies[k++] = new Body(distance * (new pVector3d(a - side / 2, b - side / 2, c - side / 2)), mass);
                    }
                    break;
            }
        }

        /// <summary>
        /// Rotates the world by calling the bodies' rotate methods. 
        /// </summary>
        /// <param name="point">The starting point for the axis of rotation.</param>
        /// <param name="direction">The direction for the axis of rotation</param>
        /// <param name="angle">The angle to rotate by.</param>
        public void Rotate(pVector3d point, pVector3d direction, double angle)
        {
            foreach (Body b in _bodies)
            {
                if (b != null)
                    b.Rotate(point, direction, angle);
            }
        }

        /// <summary>
        /// Draws the bodies in the world. 
        /// </summary>
        /// <param name="g">The graphics surface to draw on.</param>
        public void Draw()
        {
            for (int i = 0; i < _bodies.Length; i++)
            {
                if (_bodies[i] != null)
                {
                    Body body = _bodies[i];

                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(new Vector3((float)body.Location.X, (float)body.Location.Y, (float)body.Location.Z), (float)body.Radius);
                }
            }

            if (DrawTree)
                if (_tree != null)
                    _tree.Draw();
        }
    }
}