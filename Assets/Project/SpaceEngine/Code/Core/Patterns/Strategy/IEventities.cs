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

namespace SpaceEngine.Core.Patterns.Strategy.Eventit
{
    /// <summary>
    /// Interface provides some strategy methods for a subscription/unsubscription to particular events.
    /// Use this to work with <see cref="EventManager"/>.
    /// </summary>
    public interface IEventit
    {
        bool isEventit { get; set; }

        void Eventit();

        void UnEventit();
    }

    /// <summary>
    /// Interface provides some generic strategy methods for a subscription/unsubscription to particular events.
    /// Use this to work with <see cref="EventManager"/>.
    /// </summary>
    /// <typeparam name="T">Generic.</typeparam>
    public interface IEventit<T> where T : class
    {
        bool isEventit { get; set; }

        void Eventit(T obj);

        void UnEventit(T obj);
    }

    /// <summary>
    /// Interface provides some several strategy generic methods for a subscription/unsubscription to particular events.
    /// Use this to work with <see cref="EventManager"/>.
    /// </summary>
    /// <typeparam name="T">Generic.</typeparam>
    /// <typeparam name="U"></typeparam>
    public interface IEventit<T, U> where T : class
        where U : class
    {
        bool isEventit { get; set; }

        void Eventit(T obj1, U obj2);

        void UnEventit(T obj1, U obj2);
    }
}