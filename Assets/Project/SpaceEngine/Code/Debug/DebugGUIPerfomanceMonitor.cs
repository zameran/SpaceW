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

using UnityEngine;

using ZFramework.Unity.Common.PerfomanceMonitor;

public sealed class DebugGUIPerfomanceMonitor : DebugGUI
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnGUI()
    {
        base.OnGUI();

        using (new Timer("Monitor GUI Draw"))
        {
            GUI.Window(0, debugInfoBounds, UI, "Perfomance Monitor (in milliseconds)");
        }
    }

    private void UI(int id)
    {
        const int ColumnZero = 10;
        const int ColumnOne = ColumnZero + 230;
        const int ColumnTwo = ColumnOne + 100;
        const int ColumnThree = ColumnTwo + 80;
        const int ColumnFour = ColumnThree + 60;
        const int ColumnFive = ColumnFour + 60;
        const int ColumnSix = ColumnFive + 80;

        //Collect our counters...
        var counters = PerformanceMonitor.Counters;

        debugInfoBounds.width = Screen.width - 20;
        debugInfoBounds.height = Mathf.Max(counters.Count * 20 + 5 + 60, 205);

        //Draw ui..
        //GUI.Box(new Rect(0, 0, Screen.width - 0, Mathf.Max(counters.Count * 20 + 5 + 60, 205)), "");
        //GUI.Label(new Rect(Screen.width / 2 - 125, 0, 500, 30), string.Format("<b>Performance Monitor</b> (in milliseconds)"));
        GUI.Label(new Rect(ColumnZero, 30, ColumnOne - ColumnZero, 30), string.Format("Name"), boldLabel);
        GUI.Label(new Rect(ColumnOne, 30, ColumnTwo - ColumnOne, 30), string.Format("Total"), boldLabel);
        GUI.Label(new Rect(ColumnTwo, 30, ColumnThree - ColumnTwo, 30), string.Format("Average"), boldLabel);
        GUI.Label(new Rect(ColumnThree, 30, ColumnFour - ColumnThree, 30), string.Format("Last"), boldLabel);
        GUI.Label(new Rect(ColumnFour, 30, ColumnFive - ColumnFour, 30), string.Format("Max"), boldLabel);
        GUI.Label(new Rect(ColumnFive, 30, ColumnSix - ColumnFive, 30), string.Format("Count"), boldLabel);

        var y = 60;

        foreach (var counter in counters)
        {
            GUI.Label(new Rect(ColumnZero, y, ColumnOne - ColumnZero, 30), counter.Name);
            GUI.Label(new Rect(ColumnOne, y, ColumnTwo - ColumnOne, 30), string.Format("{0}", counter.Time / 1000.0f));
            GUI.Label(new Rect(ColumnTwo, y, ColumnThree - ColumnTwo, 30), string.Format("{0:0.00}", counter.Average / 1000.0f));
            GUI.Label(new Rect(ColumnThree, y, ColumnFour - ColumnThree, 30), string.Format("{0:0.0}", counter.Last / 1000.0f));
            GUI.Label(new Rect(ColumnFour, y, ColumnFive - ColumnFour, 30), string.Format("{0:0.0}", counter.Max / 1000.0f));
            GUI.Label(new Rect(ColumnFive, y, ColumnSix - ColumnFive, 30), string.Format("{0}", counter.Count));

            y += 20;
        }
    }
}