#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2016 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using System.Linq;

using SpaceEngine.Startfield;

using UnityEngine;

namespace SpaceEngine.Debugging
{
    public sealed class DebugGUISettings : DebugGUI
    {
        public Planetoid[] Planetoids;
        public Starfield[] Starfields;

        public QuadLODDistanceMethod LODDistanceMethod = QuadLODDistanceMethod.ClosestAABBCorner;
        public QuadCullingMethod CullingMethod = QuadCullingMethod.Unity;
        public AtmosphereHDR HDRMode = AtmosphereHDR.ProlandOptimized;
        public QuadDrawAndCull DrawAndCull = QuadDrawAndCull.CullAfterDraw;

        public float LODDistanceMultiplier = 2.0f;

        public bool Eclipses = true;
        public bool Planetshine = true;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            GUILayout.Window(0, debugInfoBounds, UI, "Settings");
        }

        private void UI(int id)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
            Planetoids = FindObjectsOfType<Planetoid>();
            Starfields = FindObjectsOfType<Starfield>();

            GUILayout.BeginVertical();

            GUILayout.Label("Misc: ", boldLabel);

            GUILayoutExtensions.LabelWithSpace("Quads culling method: " + CullingMethod.ToString(), 0);
            CullingMethod = (QuadCullingMethod)GUILayout.SelectionGrid((int)CullingMethod, System.Enum.GetNames(typeof(QuadCullingMethod)), 3);

            GUILayoutExtensions.LabelWithSpace("Quads culling/draw method: " + DrawAndCull.ToString(), 0);
            DrawAndCull = (QuadDrawAndCull)GUILayout.SelectionGrid((int)DrawAndCull, System.Enum.GetNames(typeof(QuadDrawAndCull)), 3);

            GUILayoutExtensions.LabelWithSpace("Quads culling/draw method: " + LODDistanceMethod.ToString(), 0);
            LODDistanceMethod = (QuadLODDistanceMethod)GUILayout.SelectionGrid((int)LODDistanceMethod, System.Enum.GetNames(typeof(QuadLODDistanceMethod)), 3);

            GUILayout.Space(10);

            GUILayout.Label("LOD Distance Multiplier: " + LODDistanceMultiplier);
            GUILayout.BeginHorizontal();
            LODDistanceMultiplier = GUILayout.HorizontalSlider(LODDistanceMultiplier, 0.75f, 3.25f);
            if (GUILayout.Button("Reset")) LODDistanceMultiplier = 1.0f;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.Label("HDR: ");
            HDRMode = (AtmosphereHDR)GUILayout.SelectionGrid((int)HDRMode, System.Enum.GetNames(typeof(AtmosphereHDR)), 2);

            GUILayout.Space(10);

            Eclipses = GUILayout.Toggle(Eclipses, " - Eclipses?");
            Planetshine = GUILayout.Toggle(Planetshine, " - Planetshine?");

            GUILayout.EndVertical();

            if (Planetoids != null)
            {
                if (Planetoids.Length != 0)
                {
                    for (int i = 0; i < Planetoids.Length; i++)
                    {
                        if (Planetoids[i] != null)
                        {
                            Planetoids[i].CullingMethod = CullingMethod;
                            Planetoids[i].DrawAndCull = DrawAndCull;
                            Planetoids[i].LODDistanceMultiplier = LODDistanceMultiplier;
                            Planetoids[i].LODDistanceMethod = LODDistanceMethod;

                            if (Planetoids[i].Atmosphere != null)
                            {
                                Planetoids[i].Atmosphere.HDRMode = HDRMode;
                                Planetoids[i].Atmosphere.Eclipses = Eclipses;
                                Planetoids[i].Atmosphere.Planetshine = Planetshine;
                            }
                        }
                    }
                }
            }

            if (Starfields != null)
            {
                if (Starfields.Length != 0)
                {
                    for (int i = 0; i < Starfields.Length; i++)
                    {
                        if (Starfields[i] != null)
                        {
                            Starfields[i].HDRMode = HDRMode;
                        }
                    }
                }
            }

            GUILayout.EndScrollView();
        }
    }
}