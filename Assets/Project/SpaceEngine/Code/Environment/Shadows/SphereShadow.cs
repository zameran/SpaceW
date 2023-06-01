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

using SpaceEngine.Core.Utilities.Gradients;
using SpaceEngine.Helpers;
using UnityEngine;

namespace SpaceEngine.Environment.Shadows
{
    public class SphereShadow : Shadow
    {
        public float InnerRadius = 1.0f;
        public float OuterRadius = 2.0f;

        public PenumbraGradientLut Penumbra = new();
        private readonly float[] magnitudes = new float[3];

        private readonly Vector3[] vectors = new Vector3[3];

        protected virtual void Awake()
        {
            Penumbra.GenerateLut();
        }

        protected virtual void OnDestroy()
        {
            Helper.Destroy(Penumbra.Lut);

            Penumbra.DestroyLut();
        }

        #region Gizmos

        #if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (Helper.Enabled(this))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, transform.right * OuterRadius * 1.5f);
                Gizmos.DrawRay(transform.position, transform.up * OuterRadius * 1.5f);
                Gizmos.DrawRay(transform.position, transform.forward * OuterRadius * 1.5f);
                Gizmos.color = Color.white;

                Gizmos.matrix = transform.localToWorldMatrix;

                Gizmos.DrawWireSphere(Vector3.zero, InnerRadius);
                Gizmos.DrawWireSphere(Vector3.zero, OuterRadius);

                if (CalculateShadow())
                {
                    Gizmos.matrix = Matrix.inverse;

                    Gizmos.DrawWireCube(new Vector3(0, 0, 5), new Vector3(2, 2, 10));
                }
            }
        }
        #endif

        #endregion

        public override Texture GetTexture()
        {
            Penumbra.GenerateLut();

            return Penumbra.Lut;
        }

        public override bool CalculateShadow()
        {
            if (base.CalculateShadow())
            {
                var direction = default(Vector3);
                var position = default(Vector3);
                var color = default(Color);

                Helper.CalculateLight(Light, transform.position, null, null, ref position, ref direction, ref color);

                var rotation = Quaternion.FromToRotation(direction, Vector3.back);

                SetVector(0, rotation * transform.right * transform.lossyScale.x * OuterRadius);
                SetVector(1, rotation * transform.up * transform.lossyScale.y * OuterRadius);
                SetVector(2, rotation * transform.forward * transform.lossyScale.z * OuterRadius);

                SortVectors();

                var spin = Quaternion.LookRotation(Vector3.forward, new Vector2(-vectors[1].x, vectors[1].y)); // Orient the shadow ellipse
                var scale = Helper.Reciprocal3(new Vector3(magnitudes[0], magnitudes[1], 1.0f));

                var shadowT = Helper.Translation(-transform.position);
                var shadowR = Helper.Rotation(spin * rotation);
                var shadowS = Helper.Scaling(scale);

                Matrix = shadowS * shadowR * shadowT;
                Ratio = Helper.Divide(OuterRadius, OuterRadius - InnerRadius);

                return true;
            }

            return false;
        }

        private void SetVector(int index, Vector3 vector)
        {
            vectors[index] = vector;

            magnitudes[index] = new Vector2(vector.x, vector.y).magnitude;
        }

        private void SortVectors()
        {
            // Lowest is 0 or 2
            if (magnitudes[0] < magnitudes[1])
            {
                // Lowest is 0
                if (magnitudes[0] < magnitudes[2])
                {
                    vectors[0] = vectors[2];
                    magnitudes[0] = magnitudes[2];
                }
            }
            else // Lowest is 1 or 2
            {
                // Lowest is 1
                if (magnitudes[1] < magnitudes[2])
                {
                    vectors[1] = vectors[2];
                    magnitudes[1] = magnitudes[2];
                }
            }
        }
    }
}