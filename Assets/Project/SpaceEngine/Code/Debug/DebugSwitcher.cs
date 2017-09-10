#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//     notice, this list of conditions and the following disclaimer.
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

using UnityEngine;

namespace SpaceEngine.Debugging
{
    public abstract class DebugSwitcher<T> : MonoSingleton<DebugSwitcher<T>>, IDebugSwitcher where T : MonoBehaviour
    {
        public List<T> DebugComponents = new List<T>(255);

        public bool DisableAllOnStart = true;

        private int State;

        protected abstract KeyCode SwitchKey { get; }

        protected void Awake()
        {
            Instance = this;
        }

        protected void Start()
        {
            if (DebugComponents == null || DebugComponents.Count == 0)
            {
                DebugComponents = GetComponents<T>().ToList();
            }

            if (DisableAllOnStart) ToogleAll(DebugComponents, false);
        }

        protected void Update()
        {
            if (Input.GetKeyDown(SwitchKey))
            {
                if (State == DebugComponents.Count)
                {
                    State = 0;
                    ToogleAll(DebugComponents, false);
                    return;
                }

                ToogleAll(DebugComponents, false);
                State++;
                ToogleAt(DebugComponents, true, State);
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
            for (byte i = 0; i < components.Count; i++)
            {
                components[i].enabled = state;
            }
        }

        #endregion
    }
}