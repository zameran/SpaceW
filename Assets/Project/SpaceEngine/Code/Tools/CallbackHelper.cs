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