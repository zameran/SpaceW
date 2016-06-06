#if NODE_EDITOR_FRAMEWORK
using System;

using UnityEngine;

namespace NodeEditorFramework.NoiseEngine
{
    public class ShaderContentType : IConnectionTypeDeclaration
    {
        public string Identifier { get { return "Shader"; } }
        public Type Type { get { return typeof(string); } }
        public Color Color { get { return Color.red; } }
        public string InKnobTex { get { return "Textures/In_Knob.png"; } }
        public string OutKnobTex { get { return "Textures/Out_Knob.png"; } }
    }
}
#endif