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
// Creation Date: 2017.05.02
// Creation Time: 5:41 PM
// Creator: zameran

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpaceEngine.Core.Debugging
{
    /// <summary>
    ///     Class for method execution time measuring.
    /// </summary>
    public sealed class PerformanceMonitor
    {
        /// <summary>
        ///     Counter dictionary.
        /// </summary>
        private static readonly Dictionary<string, Counter> counters;

        /// <summary>
        ///     Constructor.
        /// </summary>
        static PerformanceMonitor()
        {
            counters = new Dictionary<string, Counter>();
        }

        /// <summary>
        ///     Returns all counters in List.
        /// </summary>
        public static List<Counter> Counters
        {
            get
            {
                lock (counters)
                {
                    var list = counters.Values.OrderBy(counter => counter.Name).ToList();

                    return list;
                }
            }
        }

        /// <summary>
        ///     Incr. of counter with defined name to defined ms.
        /// </summary>
        /// <param name="name">Name of counter inct. to.</param>
        /// <param name="ms">Count of ms to incr..</param>
        public static void IncrementCounter(string name, long ms)
        {
            lock (counters)
            {
                if (counters.TryGetValue(name, out var counter))
                {
                    counter.Increment(ms);
                }
                else
                {
                    counters.Add(name, new Counter(name, ms));
                }
            }
        }

        /// <summary>
        ///     Dictionary of counter clean up method.
        /// </summary>
        public static void Clear()
        {
            lock (counters)
            {
                counters.Clear();
            }
        }

        /// <summary>
        ///     Microstopwatch class represents a Stopwatch analog with some additions.
        /// </summary>
        public class MicroStopwatch : Stopwatch
        {
            /// <summary>
            ///     Tic.
            /// </summary>
            private readonly double MicroSecPerTick = 1000000D / Frequency;

            /// <summary>
            ///     Constructor.
            /// </summary>
            public MicroStopwatch()
            {
                if (!IsHighResolution)
                {
                    throw new Exception("On this system the high-resolution - performance counter is not available");
                }
            }

            /// <summary>
            ///     Wasted time.
            /// </summary>
            public long ElapsedMicroseconds => (long)(ElapsedTicks * MicroSecPerTick);
        }

        /// <summary>
        ///     Simple timer class.
        /// </summary>
        /// <example>
        ///     <code>
        /// using (new Timer("Timer Name"))
        /// {
        ///     //...
        /// }
        /// </code>
        /// </example>
        public sealed class Timer : MicroStopwatch, IDisposable
        {
            /// <summary>
            ///     Name of timer.
            /// </summary>
            private readonly string name;

            /// <summary>
            ///     Constructor.
            /// </summary>
            /// <param name="name">Name of timer.</param>
            public Timer(string name)
            {
                this.name = name;

                Start();
            }

            public void Dispose()
            {
                IncrementCounter(name, ElapsedMicroseconds);
            }
        }

        /// <summary>
        ///     Counter class.
        /// </summary>
        public class Counter : IComparable<Counter>
        {
            /// <summary>
            ///     Constructor of counter class.
            /// </summary>
            /// <param name="name">Name.</param>
            /// <param name="time">Time.</param>
            public Counter(string name, long time)
            {
                Name = name;

                Time = time;
                Last = time;
                Max = time;
                Count = 1;
            }

            /// <summary>
            ///     Name of counter.
            /// </summary>
            public string Name { get; }

            /// <summary>
            ///     Time of counter.
            /// </summary>
            public long Time { get; private set; }

            /// <summary>
            ///     Last time of counter.
            /// </summary>
            public long Last { get; private set; }

            /// <summary>
            ///     Max value of counter.
            /// </summary>
            public long Max { get; private set; }

            /// <summary>
            ///     Count of counters.
            /// </summary>
            public int Count { get; private set; }

            /// <summary>
            ///     Average time of counter.
            /// </summary>
            public float Average => Time / (float)Count;

            /// <summary>
            ///     Comparer.
            /// </summary>
            /// <param name="other">Counter compare to.</param>
            /// <returns></returns>
            public int CompareTo(Counter other)
            {
                return string.Compare(Name, other.Name, StringComparison.Ordinal);
            }

            /// <summary>
            ///     Counter increment im ms.
            /// </summary>
            /// <param name="ms"></param>
            public void Increment(long ms)
            {
                Time += ms;
                Last = ms;

                if (ms > Max)
                {
                    Max = ms;
                }

                Count++;
            }
        }
    }
}