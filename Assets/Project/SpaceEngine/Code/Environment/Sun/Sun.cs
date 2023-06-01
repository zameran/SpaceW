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

namespace SpaceEngine.Environment.Sun
{
    public sealed class Sun : Node<Sun>
    {
        [Range(1, 4)]
        public int Index = 1;

        public float Radius = 250000;
        public float Intensity = 100.0f;

        #region Node

        protected override void InitNode()
        {
        }

        protected override void UpdateNode()
        {
            if ((Index == 1 && Input.GetKey(KeyCode.RightControl)) ||
                (Index == 2 && Input.GetKey(KeyCode.RightShift)) ||
                (Index == 3 && Input.GetKey(KeyCode.LeftControl)) ||
                (Index == 4 && Input.GetKey(KeyCode.LeftShift)))
            {
                var h = Input.GetAxis("HorizontalArrows") * 0.75f;
                var v = Input.GetAxis("VerticalArrows") * 0.75f;

                transform.RotateAround(new Vector3(0, 0, 0), new Vector3(0, 1, 0), h);
                transform.RotateAround(new Vector3(0, 0, 0), new Vector3(1, 0, 0), v);
            }
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion
    }
}