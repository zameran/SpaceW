using System;
using UnityEngine;

public class FloatCurve
{
    private AnimationCurve fCurve;

    private float _minTime;
    private float _maxTime;

    private static int findCurveMinMaxInterations = 100;

    public AnimationCurve Curve
    {
        get
        {
            return fCurve;
        }
        set
        {
            fCurve = value;
        }
    }

    public float MinTime
    {
        get
        {
            return _minTime;
        }
    }

    public float MaxTime
    {
        get
        {
            return _maxTime;
        }
    }

    public FloatCurve()
    {
        fCurve = new AnimationCurve();

        _minTime = 3.40282347E+38f;
        _maxTime = -3.40282347E+38f;

        fCurve.postWrapMode = WrapMode.ClampForever;
        fCurve.preWrapMode = WrapMode.ClampForever;
    }

    public FloatCurve(Keyframe[] keyframes)
    {
        fCurve = new AnimationCurve(keyframes);

        _minTime = 3.40282347E+38f;
        _maxTime = -3.40282347E+38f;

        fCurve.postWrapMode = WrapMode.ClampForever;
        fCurve.preWrapMode = WrapMode.ClampForever;
    }

    public void Add(float time, float value)
    {
        fCurve.AddKey(time, value);

        _minTime = Mathf.Min(MinTime, time);
        _maxTime = Mathf.Max(MaxTime, time);
    }

    public void Add(float time, float value, float inTangent, float outTangent)
    {
        Keyframe key = new Keyframe();
        key.inTangent = inTangent;
        key.outTangent = outTangent;
        key.time = time;
        key.value = value;

        fCurve.AddKey(key);
        _minTime = Mathf.Min(MinTime, time);
        _maxTime = Mathf.Max(MaxTime, time);
    }

    public float Evaluate(float time)
    {
        return fCurve.Evaluate(time);
    }

    public void FindMinMaxValue(out float min, out float max)
    {
        min = 0;
        max = 0;

        if (fCurve == null) return;

        min = 3.40282347E+38f;
        max = -3.40282347E+38f;

        float minimum = 3.40282347E+38f;
        float maximum = -3.40282347E+38f;

        for (int i = 0; i < fCurve.keys.Length; i++)
        {
            if (fCurve.keys[i].time < minimum) minimum = fCurve.keys[i].time;
            if (fCurve.keys[i].time > maximum) maximum = fCurve.keys[i].time;
        }

        float range = (maximum - minimum) / findCurveMinMaxInterations;

        for (int j = 0; j < findCurveMinMaxInterations; j++)
        {
            float value = fCurve.Evaluate(minimum + j * range);

            if (value < min) min = value;
            if (value > max) max = value;
        }
    }

    public void FindMinMaxValue(out float min, out float max, out float tMin, out float tMax)
    {
        min = 0f;
        max = 0f;
        tMin = 0f;
        tMax = 0f;

        if (fCurve == null) return;

        min = 3.40282347E+38f;
        max = -3.40282347E+38f;

        float minimum = 3.40282347E+38f;
        float maximum = -3.40282347E+38f;

        for (int i = 0; i < fCurve.keys.Length; i++)
        {
            if (fCurve.keys[i].time < minimum) minimum = fCurve.keys[i].time;
            if (fCurve.keys[i].time > maximum) maximum = fCurve.keys[i].time;
        }

        float range = (maximum - minimum) / findCurveMinMaxInterations;

        for (int j = 0; j < findCurveMinMaxInterations; j++)
        {
            float tvalue = minimum + j * range;
            float value = fCurve.Evaluate(tvalue);

            if (value < min)
            {
                min = value;
                tMin = tvalue;
            }
            if (value > max)
            {
                max = value;
                tMax = tvalue;
            }
        }
    }
}
