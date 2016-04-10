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

using UnityEngine;

public sealed class DebugDrawQuadBox : DebugDraw
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void OnPostRender()
    {
        base.OnPostRender();
    }

    protected override void CreateLineMaterial()
    {
        base.CreateLineMaterial();
    }

    protected override void Draw()
    {
        #if UNITY_EDITOR
        if (UnityEditor.SceneView.currentDrawingSceneView != null) return; //Do not draw at Scene tab in editor.
        #endif

        for (int i = 0; i < Planet.Quads.Count; i++)
        {
            Quad q = Planet.Quads[i];

            if (q.Generated && q.ShouldDraw)
            {
                Color lineColor = Color.blue;

                int[,] ORDER = new int[,] { { 1, 0 }, { 2, 3 }, { 0, 2 }, { 3, 1 } };

                Vector3[] verts = q.GetVolumeBox(q.Planetoid.TerrainMaxHeight * 3);

                GL.PushMatrix();
                GL.LoadIdentity();
                GL.MultMatrix(Camera.main.worldToCameraMatrix * q.Planetoid.transform.localToWorldMatrix);
                GL.LoadProjectionMatrix(Camera.main.projectionMatrix);

                lineMaterial.renderQueue = 5000;
                lineMaterial.SetPass(0);

                GL.Begin(GL.LINES);
                GL.Color(lineColor);

                for (int j = 0; j < 4; j++)
                {
                    //Draw bottom quad
                    GL.Vertex3(verts[ORDER[j, 0]].x, verts[ORDER[j, 0]].y, verts[ORDER[j, 0]].z);
                    GL.Vertex3(verts[ORDER[j, 1]].x, verts[ORDER[j, 1]].y, verts[ORDER[j, 1]].z);

                    //Draw top quad
                    GL.Vertex3(verts[ORDER[j, 0] + 4].x, verts[ORDER[j, 0] + 4].y, verts[ORDER[j, 0] + 4].z);
                    GL.Vertex3(verts[ORDER[j, 1] + 4].x, verts[ORDER[j, 1] + 4].y, verts[ORDER[j, 1] + 4].z);

                    //Draw verticals
                    GL.Vertex3(verts[ORDER[j, 0]].x, verts[ORDER[j, 0]].y, verts[ORDER[j, 0]].z);
                    GL.Vertex3(verts[ORDER[j, 0] + 4].x, verts[ORDER[j, 0] + 4].y, verts[ORDER[j, 0] + 4].z);
                }

                GL.End();
                GL.PopMatrix();
            }
        }
    }
}