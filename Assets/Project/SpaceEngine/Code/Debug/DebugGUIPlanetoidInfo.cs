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

using UnityEngine;

public sealed class DebugGUIPlanetoidInfo : DebugGUI
{
    public Planetoid Planetoid;

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

        GUILayout.Window(0, debugInfoBounds, UI, "Planetoid Info");
    }

    public double CalculateTexturesVMU(int quadsCount)
    {
        int size = QuadSettings.nVertsPerEdgeSub * QuadSettings.nVertsPerEdgeSub;

        double sizeInBytes = size * 8; //8 bit per channel.
        double sizeInMegabytes = (sizeInBytes / 1024.0) / 1024.0;

        return sizeInMegabytes * quadsCount;
    }

    private void UI(int id)
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

        if (Planetoid != null)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Planetoid stats: ", boldLabel);

            GUILayoutExtensions.LabelWithSpace((Planetoid.gameObject.name + ": " + (Planetoid.Working ? "Generating..." : "Idle...")), -8);

            if (Planetoid.CullingMethod == QuadCullingMethod.Custom)
            {
                int quadsCount = Planetoid.Quads.Count;
                int quadsCulledCount = Planetoid.GetCulledQuadsCount();
                int vertsRendered = (quadsCount - quadsCulledCount) * QuadSettings.nVerts;

                double quadsTexturesVideoMemoryUsage = CalculateTexturesVMU(quadsCount);

                GUILayoutExtensions.LabelWithSpace("Quads count: " + quadsCount, -8);
                GUILayoutExtensions.LabelWithSpace("Quads culled count: " + quadsCulledCount, -8);
                GUILayoutExtensions.LabelWithSpace("Quads textures VMU (MB): " + quadsTexturesVideoMemoryUsage.ToString("0.00"), -8);
                GUILayoutExtensions.LabelWithSpace("Verts rendered per frame (Only Quads): " + vertsRendered, -8);
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical();

            GUILayout.Space(10);

            GUILayout.Label("Planetoid parameters: ", boldLabel);
 
            if (GUILayout.Button("Resetup")) Planetoid.ReSetupQuads();

            GUILayout.EndVertical();

            if (Planetoid.Atmosphere != null)
            {
                GUILayout.Space(10);

                GUILayout.BeginVertical();

                GUILayout.Label("Atmosphere parameters: ", boldLabel);

                GUILayout.Label("Preset: ");
                Planetoid.Atmosphere.AtmosphereBase = (AtmosphereBase)GUILayout.SelectionGrid((int)Planetoid.Atmosphere.AtmosphereBase, System.Enum.GetNames(typeof(AtmosphereBase)), 2);

                GUILayout.Space(10);

                GUILayout.Label("Density: ");
                float.TryParse(GUILayout.TextField(Planetoid.Atmosphere.Density.ToString("0.0")), out Planetoid.Atmosphere.Density);

                GUILayout.Label("Radius: ");
                float.TryParse(GUILayout.TextField(Planetoid.Atmosphere.Radius.ToString("0.0")), out Planetoid.Atmosphere.Radius);

                GUILayout.Label("Height: ");
                float.TryParse(GUILayout.TextField(Planetoid.Atmosphere.Height.ToString("0.0")), out Planetoid.Atmosphere.Height);

                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical();

                GUILayoutExtensions.LabelWithSpace("No Atmosphere!?", -8);

                GUILayout.EndVertical();
            }
        }
        else
        {
            GUILayout.BeginVertical();

            GUILayoutExtensions.LabelWithSpace("No Planetoid!?", -8);

            GUILayout.EndVertical();
        }

        GUILayout.EndScrollView();
    }
}