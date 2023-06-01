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
// Creation Date: 2017.05.03
// Creation Time: 5:24 PM
// Creator: zameran
#endregion

using System.Collections.Generic;
using System.Linq;
using SpaceEngine.Core.Patterns.Singleton;
using UnityEngine;

namespace SpaceEngine.Debugging
{
    public abstract class DebugSwitcher<T> : MonoSingleton<DebugSwitcher<T>>, IDebugSwitcher where T : MonoBehaviour
    {
        public List<T> SwitchableComponents = new List<T>(255);
        public List<T> DrawAbleComponents = new List<T>(255);
        public List<T> ClickableThroughComponents = new List<T>(255);

        public bool DisableAllOnStart = true;

        private int State;

        protected abstract KeyCode SwitchKey { get; }

        protected void Awake()
        {
            Instance = this;
        }

        protected void Start()
        {
            if (SwitchableComponents == null || SwitchableComponents.Count == 0)
            {
                SwitchableComponents = GetComponents<T>().ToList();
            }

            if (DrawAbleComponents == null || DrawAbleComponents.Count == 0)
            {
                DrawAbleComponents = GetComponents<T>().ToList();
            }

            if (ClickableThroughComponents == null || ClickableThroughComponents.Count == 0)
            {
                ClickableThroughComponents = GetComponents<T>().ToList();
            }

            // NOTE : If GUI element implements IDebugAlwaysVisible interface - remove it from switchables...
            SwitchableComponents.RemoveAll(x => typeof(IDebugAlwaysVisible).IsInstanceOfType(x));

            // NOTE : If GUI element implements IDebugClickableThrough interface - leave it here, in special list... 
            ClickableThroughComponents.RemoveAll(x => !typeof(IDebugClickableThrough).IsInstanceOfType(x));

            if (DisableAllOnStart) ToogleAll(SwitchableComponents, false);
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(SwitchKey))
            {
                if (State == SwitchableComponents.Count)
                {
                    State = 0;
                    ToogleAll(SwitchableComponents, false);
                    return;
                }

                ToogleAll(SwitchableComponents, false);
                State++;
                ToogleAt(SwitchableComponents, true, State);
            }
        }

        #region API

        public void Toogle(T component, bool state)
        {
            component.enabled = state;
        }

        public void ToogleAt(List<T> components, bool state, int index)
        {
            components[index - 1].enabled = state;
        }

        public void ToogleAll(List<T> components, bool state)
        {
            for (var i = 0; i < components.Count; i++)
            {
                components[i].enabled = state;
            }
        }

        #endregion
    }
}