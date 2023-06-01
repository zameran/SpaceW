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

using SpaceEngine.Core.Debugging;
using SpaceEngine.Tools;

using System;
using System.Collections;

using UnityEngine;

using Logger = SpaceEngine.Core.Debugging.Logger;

namespace SpaceEngine.Pluginator
{
    [UseLogger(LoggerCategory.Data)]
    public abstract class Loader : MonoBehaviour
    {
        private static Loader instance;

        public static Loader Instance
        {
            get
            {
                if (instance == null) { Logger.LogError("Loader.Instance.get: Loader Instance get fail!"); }
                return instance;
            }
            private set
            {
                if (value != null) instance = value;
                else Logger.LogError("Loader.Instance.set: Loader Instance set fail!");
            }
        }

        public static int Step { get; private set; }

        protected virtual void Start()
        {

        }

        protected virtual void Awake()
        {
            Instance = this;

            Step = 0;
        }

        protected virtual void Update()
        {

        }

        protected virtual void OnGUI()
        {

        }

        protected virtual void Pass()
        {
            Step++;
        }

        public void Delay(float waitTime, Action action)
        {
            Logger.Log($"Loader.Delay: Delay method invoked! Will wait for {waitTime} seconds...");

            StartCoroutine(DelayImpl(waitTime, action));
        }

        private IEnumerator DelayImpl(float waitTime, Action action)
        {
            yield return Yielders.Get(waitTime);

            action?.Invoke();
        }
    }
}