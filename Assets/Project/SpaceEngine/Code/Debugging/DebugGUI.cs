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

using SpaceEngine.Core;
using UnityEngine;

namespace SpaceEngine.Debugging
{
    /// <summary>
    ///     This class provides default behaviour of all <see cref="GUI" />-based debug panels.
    /// </summary>
    public abstract class DebugGUI : MonoBehaviour, IDebug
    {
        public Rect debugInfoBounds = new(10, 10, 500, 500);

        // NOTE : Use this variable for runtime 'debugInfoBounds' manipulation...
        [HideInInspector]
        public Rect debugInfoDrawBounds = new(10, 10, 500, 500);

        [HideInInspector]
        public Vector2 ScrollPosition = Vector2.zero;

        private readonly CachedComponent<DebugGUISwitcher> SwitcherCachedComponent = new();

        public DebugGUISwitcher SwitcherComponent => SwitcherCachedComponent.Component;

        public GUISkin GUISkin
        {
            get
            {
                if (SwitcherComponent != null)
                {
                    if (SwitcherComponent.GUISkin != null)
                    {
                        return SwitcherComponent.GUISkin;
                    }
                }

                return GUI.skin;
            }
        }

        public GUIStyle BoldLabelStyle
        {
            get
            {
                if (SwitcherComponent != null)
                {
                    return SwitcherComponent.BoldLabelStyle;
                }

                return null;
            }
        }

        public GUIStyle ImageLabelStyle
        {
            get
            {
                if (SwitcherComponent != null)
                {
                    return SwitcherComponent.ImageLabelStyle;
                }

                return null;
            }
        }

        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
            SwitcherCachedComponent.TryInit(this);
        }

        protected virtual void OnGUI()
        {
            GUI.skin = GUISkin;
            GUI.depth = -100;

            if (SwitcherComponent != null)
            {
                if (SwitcherComponent.ShowDebugGUIBounds)
                {
                    var guiColor = GUI.color;

                    GUI.color = Color.red;
                    GUI.Box(debugInfoBounds, "");

                    GUI.color = Color.green;
                    GUI.Box(debugInfoDrawBounds, "");

                    GUI.color = guiColor;
                }
            }
        }

        protected abstract void UI(int id);
    }
}