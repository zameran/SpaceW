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

using System;
using System.Collections;

using UnityEngine;

using ZFramework.Unity.Common.Messenger;

public static class CallbackHelper
{
    public static void Clear(this Callback callback)
    {
        callback = () => { };
    }

    public static void Clear<T>(this Callback<T> callback)
    {
        callback = (T t) => { };
    }

    public static void Clear<T, U>(this Callback<T, U> callback)
    {
        callback = (T t, U u) => { };
    }

    public static void Clear<T, U, V>(this Callback<T, U, V> callback)
    {
        callback = (T t, U u, V v) => { };
    }

    public static void Clear<T, U, V, N>(this Callback<T, U, V, N> callback)
    {
        callback = (T t, U u, V v, N n) => { };
    }

    public static IEnumerator DoUntil(Callback callback, Func<bool> condition)
    {
        while (!condition())
        {
            if (callback != null) callback();

            yield return null;
        }
    }

    public static IEnumerator HoldUntil(Func<bool> condition)
    {
        while (!condition())
        {
            yield return null;
        }
    }

    public static IEnumerator WaitUntil(Func<bool> condition, Callback callback)
    {
        while (!condition())
        {
            yield return null;
        }

        if (callback != null) callback();
    }
}