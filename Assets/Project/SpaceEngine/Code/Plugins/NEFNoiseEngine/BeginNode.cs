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

        protected internal override void NodeGUI()
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