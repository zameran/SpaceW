#region License
/* Procedural planet generator.
 *
 * Copyright (C) 2015-2016 Denis Ovchinnikov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holders nor the names of its
 *    contributors may be used to endorse or promote products derived from
 *    this software without specific prior written permission.

 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */
#endregion

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