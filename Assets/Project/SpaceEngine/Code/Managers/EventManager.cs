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

using SpaceEngine.Core.Bodies;
using SpaceEngine.Core.Terrain;
using SpaceEngine.Enums;
using SpaceEngine.Environment.Atmospheric;
using SpaceEngine.UI;

using System;

using UnityEngine;
using UnityEngine.SceneManagement;

public static class EventManager
{
    public static BaseEvents BaseEvents = new BaseEvents();

    public static BodyEvents BodyEvents = new BodyEvents();

    public static UIEvents UIEvents = new UIEvents();
}

public sealed class BaseEvents
{
    public EventHolder<EntryPoint, LoadSceneMode> OnSceneWillBeLoadedNow = new EventHolder<EntryPoint, LoadSceneMode>();
    public EventHolder<EntryPoint, LoadSceneMode> OnSceneWillBeLoaded = new EventHolder<EntryPoint, LoadSceneMode>();
    public EventHolder<EntryPoint, LoadSceneMode> OnSceneLoaded = new EventHolder<EntryPoint, LoadSceneMode>();
}

public sealed class BodyEvents
{
    public EventHolder<Body, Atmosphere> OnAtmosphereBaked = new EventHolder<Body, Atmosphere>();
    public EventHolder<Body, Atmosphere, AtmosphereBase> OnAtmospherePresetChanged = new EventHolder<Body, Atmosphere, AtmosphereBase>();

    public EventHolder<Body, TerrainNode> OnSamplersChanged = new EventHolder<Body, TerrainNode>();
}

public sealed class UIEvents
{
    public EventHolder AllAdditiveUILoaded = new EventHolder();
    public EventHolder<UICore> UIRemixed = new EventHolder<UICore>();
}

#region Event Holders

public class EventHolder
{
    public event Action OnEvent;

    public void Invoke()
    {
        OnEvent?.Invoke();
    }

    public void SafeInvoke()
    {
        try
        {
            Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"EventManager: {ex.GetType().Name}:{ex.Message}");
        }
    }
}

public class EventHolder<T>
{
    public event Action<T> OnEvent;

    public void Invoke(T arg1)
    {
        OnEvent?.Invoke(arg1);
    }

    public void SafeInvoke(T arg1)
    {
        try
        {
            Invoke(arg1);
        }
        catch (Exception ex)
        {
            Debug.LogError($"EventManager: {ex.GetType().Name}:{ex.Message}");
        }
    }
}

public class EventHolder<T1, T2>
{
    public event Action<T1, T2> OnEvent;

    public void Invoke(T1 arg1, T2 arg2)
    {
        OnEvent?.Invoke(arg1, arg2);
    }

    public void SafeInvoke(T1 arg1, T2 arg2)
    {
        try
        {
            Invoke(arg1, arg2);
        }
        catch (Exception ex)
        {
            Debug.LogError($"EventManager: {ex.GetType().Name}:{ex.Message}");
        }
    }
}

public class EventHolder<T1, T2, T3>
{
    public event Action<T1, T2, T3> OnEvent;

    public void Invoke(T1 arg1, T2 arg2, T3 arg3)
    {
        OnEvent?.Invoke(arg1, arg2, arg3);
    }

    public void SafeInvoke(T1 arg1, T2 arg2, T3 arg3)
    {
        try
        {
            Invoke(arg1, arg2, arg3);
        }
        catch (Exception ex)
        {
            Debug.LogError($"EventManager: {ex.GetType().Name}:{ex.Message}");
        }
    }
}

public class EventHolder<T1, T2, T3, T4>
{
    public event Action<T1, T2, T3, T4> OnEvent;

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        OnEvent?.Invoke(arg1, arg2, arg3, arg4);
    }

    public void SafeInvoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        try
        {
            Invoke(arg1, arg2, arg3, arg4);
        }
        catch (Exception ex)
        {
            Debug.LogError($"EventManager: {ex.GetType().Name}:{ex.Message}");
        }
    }
}

#endregion