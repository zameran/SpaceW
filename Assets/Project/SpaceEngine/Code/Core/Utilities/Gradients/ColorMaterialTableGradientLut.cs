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
// Creation Date: 2017.05.17
// Creation Time: 2:03 AM
// Creator: zameran

#endregion

using System;
using SpaceEngine.Helpers;
using UnityEngine;

namespace SpaceEngine.Core.Utilities.Gradients
{
    [Serializable]
    public class ColorMaterialTableGradientLut : GradientLut
    {
        /// <summary>
        ///     Should <see cref="GradientLut.Lut" /> be inverted along X axis?
        /// </summary>
        public bool ReverseX;

        public ColorMaterialTableGradientLut()
        {
        }

        public ColorMaterialTableGradientLut(Gradient gradient) : base(gradient)
        {
        }

        /// <inheritdoc />
        protected override Vector2 Size => new(256, 32);

        /// <inheritdoc />
        public override void CalculateLut()
        {
            for (ushort x = 0; x < Lut.width; x++)
            {
                for (ushort y = 0; y < Lut.height; y++)
                {
                    var t = x / Size.x;
                    var gradientColor = Gradient.Evaluate(ReverseX ? 1.0f - t : t);

                    Lut.SetPixel(x, y, gradientColor);
                }
            }

            Lut.Apply();
        }

        /// <inheritdoc />
        public override void DestroyLut()
        {
            if (Lut != null)
            {
                Helper.Destroy(Lut);
            }
        }
    }
}