using System;
using System.Collections.Generic;

using UnityEngine;

public class ParallelManager
{
    private static readonly int ThreadCount = (2 * 8);

    public static void For(int fromInclusive, int toExclusive, Action<int> body)
    {
        object indexLock = new object();

        int step = Math.Max(1, (toExclusive - fromInclusive) / (10 * ThreadCount));
        int index = fromInclusive;

        Action action = delegate
        {
            while (true)
            {
                int tempIndex;

                lock (indexLock)
                {
                    tempIndex = index;
                    index += step;
                }

                for (int m = tempIndex; m < (tempIndex + step); m++)
                {
                    if(m >= toExclusive)
                    {
                        return;
                    }

                    body(m);
                }
            }
        };

        IAsyncResult[] resultArray = new IAsyncResult[ThreadCount];

        for (int j = 0; j < resultArray.Length; j++)
        {
            resultArray[j] = action.BeginInvoke(null, null);
        }

        for (int k = 0; k < resultArray.Length; k++)
        {
            action.EndInvoke(resultArray[k]);
        }
    }

    public static void ForEach<T>(IList<T> source, Action<T> action)
    {
        For(0, source.Count, i => action(source[i]));
    }

    public static void Invoke(params Action[] body)
    {
        For(0, body.Length, i => body[i]());
    }
}