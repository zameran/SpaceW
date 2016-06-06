#if NODE_EDITOR_FRAMEWORK
using System;
using System.Text;

using UnityEngine;

namespace NodeEditorFramework.NoiseEngine
{
    [System.Serializable]
    [Node(false, "NoiseEngine/Variable")]
    public class VariableNode : Node
    {
        public string Content;

        public const string ID = "ne_variable";
        public override string GetID { get { return ID; } }

        public override Node Create(Vector2 pos)
        {
            VariableNode node = CreateInstance<VariableNode>();

            node.name = "Variable Node";
            node.rect = new Rect(pos.x, pos.y, 200, 163);

            node.CreateInput("Input 1", "Shader");
            node.CreateOutput("Output 1", "Shader");

            return node;
        }

        protected override void NodeGUI() //protected internal override void NodeGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            GUILayout.Label("Body");
            InputKnob(0);

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
            return true;
        }
    }
}
#endif