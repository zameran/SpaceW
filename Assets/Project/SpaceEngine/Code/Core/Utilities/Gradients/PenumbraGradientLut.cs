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
// Creation Date: 2017.05.31
// Creation Time: 5:16 PM
// Creator: zameran
#endregion

using System;

using UnityEngine;

namespace SpaceEngine.Core.Utilities.Gradients
{
    [Serializable]
    public class PenumbraGradientLut : GradientLut
    {
        public Gradient BrighnessGradient = new Gradient();

        /// <summary>
        /// Should <see cref="GradientLut.Lut"/> be inverted along X axis?
        /// </summary>
        public bool ReverseX = false;

        /// <inheritdoc />
        protected override Vector2 Size { get { return new Vector2(1, 64); } }

        public PenumbraGradientLut() : base() { }

        public PenumbraGradientLut(Gradient gradient) : base(gradient) { }

        /// <inheritdoc />
        public override void CalculateLut()
        {
            for (var y = 0; y < Lut.height; y++)
            {
                var t = y / (float)Lut.height;
                var a = BrighnessGradient.Evaluate(ReverseX ? (1.0f - t) : t);
                var b = Gradient.Evaluate(ReverseX ? (1.0f - t) : t);
                var c = a * b;

                c.a = c.grayscale;

                Lut.SetPixel(0, y, c);
            }

            // Make sure the last pixel is white...
            Lut.SetPixel(0, Lut.height - 1, Color.white);

            // Make sure the wrapMode is good for penumbra...
            Lut.wrapMode = TextureWrapMode.Clamp;

            Lut.Apply();
        }

        /// <inheritdoc />
        public override void DestroyLut()
        {
            if (Lut != null) Helper.Destroy(Lut);
        }
    }
}