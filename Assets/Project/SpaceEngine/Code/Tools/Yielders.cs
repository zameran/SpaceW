using System;
using System.Collections.Generic;

using UnityEngine;

public static class Yielders
{
    private class FloatComparer : IEqualityComparer<float>
    {
        bool IEqualityComparer<float>.Equals(float x, float y)
        {
            return Math.Abs(x - y) < 0.001f;
        }

        int IEqualityComparer<float>.GetHashCode(float obj)
        {
            return obj.GetHashCode();
        }
    }

    private static readonly Dictionary<float, WaitForSeconds> TimeIntervals = new Dictionary<float, WaitForSeconds>(100, new FloatComparer());

    private static readonly WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();

    public static WaitForEndOfFrame EndOfFrame { get { return endOfFrame; } }

    private static readonly WaitForFixedUpdate fixedUpdate = new WaitForFixedUpdate();

    public static WaitForFixedUpdate FixedUpdate { get { return fixedUpdate; } }

    public static WaitForSeconds Get(float seconds)
    {
        WaitForSeconds wfs;

        if (!TimeIntervals.TryGetValue(seconds, out wfs))
            TimeIntervals.Add(seconds, wfs = new WaitForSeconds(seconds));

        return wfs;
    }
}