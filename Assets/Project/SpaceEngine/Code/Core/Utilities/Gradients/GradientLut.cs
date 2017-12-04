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
//     notice, this list of conditions and the following disclaimer in the
//     documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//     contributors may be used to endorse or promote products derived from
//     this software without specific prior written permission.
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
// Creation Time: 1:57 AM
// Creator: zameran
#endregion

using UnityEngine;

namespace SpaceEngine.Core.Utilities.Gradients
{
    // NOTE : Dont't forget to clean up the Lut texture in OnDestroy!
    // NOTE : Gradients can be used as PropertyNotificationObject for particular generation of Lut in some dynamic cases...

    public abstract class GradientLut
    {
        public Gradient Gradient = new Gradient();

        /// <summary>
        /// The lut texture, generated from <see cref="Gradient"/>
        /// </summary>
        public Texture2D Lut { get; protected set; }

        /// <summary>
        /// Size of lut texture in pixels.
        /// <see cref="Vector2.x"/> is width.
        /// <see cref="Vector2.y"/> is height.
        /// </summary>
        protected abstract Vector2 Size { get; }

        public GradientLut()
        {
            
        }

        public GradientLut(Gradient gradient)
        {
            Gradient.SetKeys(gradient.colorKeys, gradient.alphaKeys);
        }

        /// <summary>
        /// Generates the <see cref="Lut"/>.
        /// </summary>
        public virtual void GenerateLut()
        {
            if (Lut == null || Lut.width != (int)Size.x || Lut.height != (int)Size.y)
            {
                DestroyLut();

                Lut = Helper.CreateTempTeture2D((int)Size.x, (int)Size.y, TextureFormat.ARGB32, false, false, false);
                Lut.wrapMode = TextureWrapMode.Repeat;
            }

            CalculateLut();
        }

        /// <summary>
        /// Calculates the <see cref="Gradient"/> data in to the <see cref="Lut"/>.
        /// </summary>
        public abstract void CalculateLut();

        /// <summary>
        /// Destroy the <see cref="Lut"/>.
        /// </summary>
        public abstract void DestroyLut();
    }
}