#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2018 Denis Ovchinnikov [zameran] 
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

namespace SpaceEngine.Debugging
{
    public sealed class DebugGUISwitcher : DebugSwitcher<DebugGUI>
    {
        public GUISkin GUISkin;

        public GUIStyle BoldLabelStyle { get; private set; }
        public GUIStyle ImageLabelStyle { get; private set; }

        public bool ShowAdditionalInfo = true;
        public bool ShowDebugGUIBounds = false;

        public Vector2 MousePosition { get; private set; }

        public bool AtLeastOneEnabled { get { return SwitchableComponents.Any((gui) => gui.enabled == true); } }

        public bool MouseOverGUIHotControl { get { return GUIUtility.hotControl != 0; } }

        public bool MouseOverGUIRect { get { return DrawAbleComponents.Any(gui => gui.isActiveAndEnabled && gui.debugInfoDrawBounds.Contains(MousePosition) && !ClickableThroughComponents.Contains(gui)); } }

        public bool MouseOverGUI { get { return MouseOverGUIHotControl || MouseOverGUIRect; } }

        protected override void Update()
        {
            base.Update();

            MousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        }

        private void OnGUI()
        {
            if (GUISkin != null)
            {
                var labelBoldStyle = GUI.skin.FindStyle("label_bold");
                var labelStyle = GUI.skin.FindStyle("label");
                var labelImage = GUI.skin.FindStyle("label_image");

                if (labelBoldStyle != null) BoldLabelStyle = labelBoldStyle;
                else if (labelStyle != null) BoldLabelStyle = labelStyle;

                if (labelImage != null) ImageLabelStyle = labelImage;
                else if (labelStyle != null) ImageLabelStyle = labelStyle;
            }

            if (ShowAdditionalInfo && !AtLeastOneEnabled)
            {
                GUILayoutExtensions.Vertical(() =>
                {
                    GUILayoutExtensions.LabelWithSpace(string.Format("Press {0} key to switch between debug GUI's...", SwitchKey.ToString()));
                    GUILayoutExtensions.LabelWithSpace(string.Format("Mouse Position: {0}", Input.mousePosition));
                });
            }
        }

        protected override KeyCode SwitchKey { get { return KeyCode.F5; } }
    }
}