using SpaceEngine.AtmosphericScattering;

using System;

using UnityEngine;

public static class EventManager
{
    public static PlanetoidEvents PlanetoidEvents = new PlanetoidEvents();
}

public sealed class PlanetoidEvents
{
    public EventHolder<Planetoid, Atmosphere> OnAtmosphereBaked = new EventHolder<Planetoid, Atmosphere>();
    public EventHolder<Planetoid, Atmosphere> OnAtmospherePresetChanged = new EventHolder<Planetoid, Atmosphere>();

    public EventHolder<Planetoid, Quad> OnDispatchStarted = new EventHolder<Planetoid, Quad>();
    public EventHolder<Planetoid, Quad> OnDispatchEnd = new EventHolder<Planetoid, Quad>();
    public EventHolder<Planetoid, Quad> OnDispatchFinished = new EventHolder<Planetoid, Quad>();
}

#region Event Holders

public class EventHolder
{
    public event Action OnEvent;

    public void Invoke()
    {
        if (OnEvent != null)
            OnEvent();
    }

    public void SafeInvoke()
    {
        try
        {
            Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("EventManager: {0}:{1}", ex.GetType().Name, ex.Message));
        }
    }
}

public class EventHolder<T>
{
    public event Action<T> OnEvent;

    public void Invoke(T arg1)
    {
        if (OnEvent != null)
            OnEvent(arg1);
    }

    public void SafeInvoke(T arg1)
    {
        try
        {
            Invoke(arg1);
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("EventManager: {0}:{1}", ex.GetType().Name, ex.Message));
        }
    }
}

public class EventHolder<T1, T2>
{
    public event Action<T1, T2> OnEvent;

    public void Invoke(T1 arg1, T2 arg2)
    {
        if (OnEvent != null)
            OnEvent(arg1, arg2);
    }

    public void SafeInvoke(T1 arg1, T2 arg2)
    {
        try
        {
            Invoke(arg1, arg2);
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("EventManager: {0}:{1}", ex.GetType().Name, ex.Message));
        }
    }
}

public class EventHolder<T1, T2, T3>
{
    public event Action<T1, T2, T3> OnEvent;

    public void Invoke(T1 arg1, T2 arg2, T3 arg3)
    {
        if (OnEvent != null)
            OnEvent(arg1, arg2, arg3);
    }

    public void SafeInvoke(T1 arg1, T2 arg2, T3 arg3)
    {
        try
        {
            Invoke(arg1, arg2, arg3);
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("EventManager: {0}:{1}", ex.GetType().Name, ex.Message));
        }
    }
}

public class EventHolder<T1, T2, T3, T4>
{
    public event Action<T1, T2, T3, T4> OnEvent;

    public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        if (OnEvent != null)
            OnEvent(arg1, arg2, arg3, arg4);
    }

    public void SafeInvoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        try
        {
            Invoke(arg1, arg2, arg3, arg4);
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("EventManager: {0}:{1}", ex.GetType().Name, ex.Message));
        }
    }
}

#endregion