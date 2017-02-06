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

using UnityEngine;

[ExecuteInEditMode]
public class RingShadow : Shadow
{
    private CachedComponent<Ring> RingCachedComponent = new CachedComponent<Ring>();

    public Ring RingComponent { get { return RingCachedComponent.Component; } }

    [HideInInspector]
    public Texture Texture;

    [HideInInspector]
    public float InnerRadius = 1.0f;

    [HideInInspector]
    public float OuterRadius = 2.0f;

    public override Texture GetTexture()
    {
        return RingComponent.MainTex ?? Texture;
    }

    public override bool CalculateShadow()
    {
        if (RingComponent == null)
        {
            RingCachedComponent.TryInit(this);
        }

        if (base.CalculateShadow() == true)
        {
            if (RingComponent == null) return false;

            if (Texture != null)
            {
                if (Helper.Enabled(RingComponent) == true)
                {
                    InnerRadius = RingComponent.InnerRadius;
                    OuterRadius = RingComponent.OuterRadius;
                }

                var direction = default(Vector3);
                var position = default(Vector3);
                var color = default(Color);

                Helper.CalculateLight(Light, transform.position, null, null, ref position, ref direction, ref color);

                var rotation = Quaternion.FromToRotation(direction, Vector3.back);
                var squash = Vector3.Dot(direction, transform.up); // Find how squashed the ellipse is based on light direction
                var width = transform.lossyScale.x * OuterRadius;
                var length = transform.lossyScale.z * OuterRadius;
                var axis = rotation * transform.up; // Find the transformed up axis
                var spin = Quaternion.LookRotation(Vector3.forward, new Vector2(-axis.x, axis.y)); // Orient the shadow ellipse
                var scale = Helper.Reciprocal3(new Vector3(width, length * Mathf.Abs(squash), 1.0f));
                var skew = Mathf.Tan(Helper.Acos(-squash));

                var shadowT = MatrixHelper.Translation(-transform.position);
                var shadowR = MatrixHelper.Rotation(spin * rotation); // Spin the shadow so lines up with its tilt
                var shadowS = MatrixHelper.Scaling(scale); // Scale the ring into an oval
                var shadowK = MatrixHelper.ShearingZ(new Vector2(0.0f, skew)); // Skew the shadow so it aligns with the ring plane

                Matrix = shadowS * shadowK * shadowR * shadowT;
                Ratio = Helper.Divide(OuterRadius, OuterRadius - InnerRadius);

                return true;
            }
        }

        return false;
    }

    protected override void Start()
    {
        RingCachedComponent.TryInit(this);

        base.Start();
    }

    private void OnDestroy()
    {

    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        if (Helper.Enabled(this) == true)
        {
            Gizmos.DrawRay(transform.position, transform.up * OuterRadius);

            Gizmos.matrix = transform.localToWorldMatrix;

            Helper.DrawCircle(Vector3.zero, Vector3.right * InnerRadius, Vector3.forward * InnerRadius);
            Helper.DrawCircle(Vector3.zero, Vector3.right * OuterRadius, Vector3.forward * OuterRadius);

            if (CalculateShadow() == true)
            {
                Gizmos.matrix = Matrix.inverse;

                Gizmos.DrawWireCube(new Vector3(0, 0, 5), new Vector3(2, 2, 10));
            }
        }
    }
#endif
}