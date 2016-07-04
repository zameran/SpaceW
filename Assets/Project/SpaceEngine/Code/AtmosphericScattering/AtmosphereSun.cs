#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2016 Denis Ovchinnikov [zameran] 
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

public sealed class AtmosphereSun : MonoBehaviour
{
    [Range(1, 4)]
    public int sunID = 1;

    static readonly Vector3 Z_AXIS = new Vector3(0, 0, 1);

    Vector3 m_startSunDirection = Z_AXIS;

    private bool HasMoved = false;

    public float Radius = 250000;

    public Matrix4x4 WorldToLocalRotation
    {
        get
        {
            Quaternion q = Quaternion.FromToRotation(GetDirection(), Z_AXIS);

            return Matrix4x4.TRS(Vector3.zero + Origin, q, Vector3.one);
        }
        private set { WorldToLocalRotation = value; }
    }

    public Matrix4x4 LocalToWorldRotation
    {
        get
        {
            return WorldToLocalRotation.inverse;
        }
        private set { LocalToWorldRotation = value; }
    }

    public float SunIntensity = 100.0f;

    public Vector3 Origin;

    public Vector3 GetDirection()
    {
        return -transform.forward;
    }

    public bool GetHasMoved()
    {
        return HasMoved;
    }

    private void Start()
    {
        if (m_startSunDirection.magnitude < Mathf.Epsilon)
            m_startSunDirection = Z_AXIS;

        transform.forward = m_startSunDirection.normalized;
    }

    public void UpdateNode()
    {
        if ((sunID == 1 && Input.GetKey(KeyCode.RightControl)) || (sunID == 2 && Input.GetKey(KeyCode.RightShift)))
        {
            HasMoved = false;

            float h = Input.GetAxis("HorizontalArrows") * 0.75f;
            float v = Input.GetAxis("VerticalArrows") * 0.75f;

            transform.Rotate(new Vector3(v, h, 0), UnityEngine.Space.World);

            HasMoved = true;
        }
    }

    public void SetUniforms(MaterialPropertyBlock block)
    {
        if (block == null) return;

        block.SetFloat("_Sun_Intensity", SunIntensity);

        switch (sunID)
        {
            case 1:
                block.SetVector("_Sun_WorldSunDir_1", GetDirection());
                block.SetMatrix("_Sun_WorldToLocal_1", WorldToLocalRotation);
                break;
            case 2:
                block.SetVector("_Sun_WorldSunDir_2", GetDirection());
                block.SetMatrix("_Sun_WorldToLocal_2", WorldToLocalRotation);
                break;
            case 3:
                block.SetVector("_Sun_WorldSunDir_3", GetDirection());
                block.SetMatrix("_Sun_WorldToLocal_3", WorldToLocalRotation);
                break;
            case 4:
                block.SetVector("_Sun_WorldSunDir_4", GetDirection());
                block.SetMatrix("_Sun_WorldToLocal_4", WorldToLocalRotation);
                break;
            default:
                block.SetVector("_Sun_WorldSunDir", GetDirection());
                block.SetMatrix("_Sun_WorldToLocal", WorldToLocalRotation);
                break;
        }

        block.SetVector("_Sun_Position", transform.position);
    }

    public void SetUniforms(Material mat)
    {
        if (mat == null) return;

        mat.SetFloat("_Sun_Intensity", SunIntensity);

        switch (sunID)
        {
            case 1:
                mat.SetVector("_Sun_WorldSunDir_1", GetDirection());
                mat.SetMatrix("_Sun_WorldToLocal_1", WorldToLocalRotation);
                break;
            case 2:
                mat.SetVector("_Sun_WorldSunDir_2", GetDirection());
                mat.SetMatrix("_Sun_WorldToLocal_2", WorldToLocalRotation);
                break;
            case 3:
                mat.SetVector("_Sun_WorldSunDir_3", GetDirection());
                mat.SetMatrix("_Sun_WorldToLocal_3", WorldToLocalRotation);
                break;
            case 4:
                mat.SetVector("_Sun_WorldSunDir_4", GetDirection());
                mat.SetMatrix("_Sun_WorldToLocal_4", WorldToLocalRotation);
                break;
            default:
                mat.SetVector("_Sun_WorldSunDir", GetDirection());
                mat.SetMatrix("_Sun_WorldToLocal", WorldToLocalRotation);
                break;
        }

        mat.SetVector("_Sun_Position", transform.position);
    }
}