#region License

// Procedural planet generator.
// 
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
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

#if NODE_EDITOR_FRAMEWORK
using System;
using System.Text;

using UnityEngine;

namespace NodeEditorFramework.NoiseEngine
{
    [System.Serializable]
    [Node(false, "NoiseEngine/Begin")]
    public class BeginNode : Node, IPreviewable
    {
        public string Content;

        public const string ID = "ne_begin";
        public override string GetID { get { return ID; } }

        private Texture2D preview = null;

        public Texture2D Preview
        {
            get
            {
                if (preview == null)
                    preview = CalculatePreview();

                return preview;
            }

            set
            {
                preview = value;
            }
        }

        public override Node Create(Vector2 pos)
        {
            BeginNode node = CreateInstance<BeginNode>();

            node.name = "Begin Node";
            node.rect = new Rect(pos.x, pos.y, 200, 163);

            node.CreateOutput("Output 1", "Shader");

            return node;
        }

        protected override void NodeGUI() //protected internal override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            GUILayout.Box(Preview);

            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            Outputs[0].DisplayLayout();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (GUI.changed)
                NodeEditor.RecalculateFrom(this);
        }

        public override bool Calculate()
        {
            Content = string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(ShaderStrings.TCCommonFileInclude);
            sb.AppendLine("");
            sb.AppendLine("float GetValue()");
            sb.AppendLine("{");
            sb.AppendLine("");
            sb.AppendLine("}");
            Content = sb.ToString();

            Outputs[0].SetValue<string>(Content);

            return true;
        }

        public Texture2D CalculatePreview(int size = 128)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false, true);

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    tex.SetPixel(i, j, Color.red);
                }
            }

            return tex;
        }
    }
}
#endif