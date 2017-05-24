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

namespace SpaceEngine.Core.Patterns.Strategy.Uniformed
{
    public interface IUniformed
    {
        void InitUniforms();

        void SetUniforms();

        void InitSetUniforms();
    }

    /// <summary>
    /// This interface should be implemented in all things, that's gonna manipulate with 
    /// <see cref="UnityEngine.Material"/> and <see cref="UnityEngine.Shader"/>, or <see cref="UnityEngine.MaterialPropertyBlock"/> uniforms.
    /// Target for manipulations is a generic type.
    /// </summary>
    /// <typeparam name="T">Generic. <example><see cref="UnityEngine.Material"/> or <see cref="UnityEngine.Shader"/>.</example></typeparam>
    public interface IUniformed<in T> where T : class
    {
        void InitUniforms(T target);

        void SetUniforms(T target);

        void InitSetUniforms();
    }

    /// <summary>
    /// This interface should be implemented in all things, that's gonna manipulate with 
    /// <see cref="UnityEngine.Material"/> and <see cref="UnityEngine.Shader"/>, or <see cref="UnityEngine.MaterialPropertyBlock"/> uniforms.
    /// Targets for manipulations is a generic type.
    /// Two targets at the same time!
    /// <remarks>
    /// This interface slightly different: <see cref="IUniformed{T}.InitSetUniforms"/>. It's done, 
    /// because class can implement sequence of <see cref="IUniformed{T}"/> and <see cref="IUniformed{T, U}"/>
    /// </remarks>
    /// </summary>
    /// <typeparam name="T">Generic. <example><see cref="UnityEngine.Material"/> or <see cref="UnityEngine.Shader"/>.</example></typeparam>
    /// <typeparam name="U">Generic. <example><see cref="UnityEngine.Material"/> or <see cref="UnityEngine.Shader"/>.</example></typeparam>
    public interface IUniformed<T, U> where T : class where U : class
    {
        void InitUniforms(T target0, U target1);

        void SetUniforms(T target0, U target1);
    }
}