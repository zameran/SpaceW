using UnityEngine;

using ZFramework.Unity.Common.PerfomanceMonitor;

public class PerfomanceMonitorGUI : DebugGUI
{
    void OnGUI()
    {
        GUI.depth = -100;

        using (new Timer("Monitor GUI Draw"))
        {
            ShowPerformanceCounters();
        }
    }
    
    private void ShowPerformanceCounters()
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

        //Draw ui..
        GUI.Box(new Rect(0, 0, Screen.width - 0, Mathf.Max(counters.Count * 20 + 5 + 60, 205)), "");
        GUI.Label(new Rect(Screen.width / 2 - 125, 0, 500, 30), string.Format("<b>Performance Monitor</b> (in milliseconds)"));
        GUI.Label(new Rect(ColumnZero, 30, ColumnOne - ColumnZero, 30), string.Format("Name"));
        GUI.Label(new Rect(ColumnOne, 30, ColumnTwo - ColumnOne, 30), string.Format("Total"));
        GUI.Label(new Rect(ColumnTwo, 30, ColumnThree - ColumnTwo, 30), string.Format("Average"));
        GUI.Label(new Rect(ColumnThree, 30, ColumnFour - ColumnThree, 30), string.Format("Last"));
        GUI.Label(new Rect(ColumnFour, 30, ColumnFive - ColumnFour, 30), string.Format("Max"));
        GUI.Label(new Rect(ColumnFive, 30, ColumnSix - ColumnFive, 30), string.Format("Count"));
        var y = 60;

        foreach (var counter in counters)
        {
            GUI.Label(new Rect(ColumnZero, y, ColumnOne - ColumnZero, 30), counter.Name);
            GUI.Label(new Rect(ColumnOne, y, ColumnTwo - ColumnOne, 30), string.Format("{0}", counter.Time / 1000));
            GUI.Label(new Rect(ColumnTwo, y, ColumnThree - ColumnTwo, 30), string.Format("{0:0.00}", counter.Average / 1000f));
            GUI.Label(new Rect(ColumnThree, y, ColumnFour - ColumnThree, 30), string.Format("{0:0.0}", counter.Last / 1000f));
            GUI.Label(new Rect(ColumnFour, y, ColumnFive - ColumnFour, 30), string.Format("{0:0.0}", counter.Max / 1000f));
            GUI.Label(new Rect(ColumnFive, y, ColumnSix - ColumnFive, 30), string.Format("{0}", counter.Count));

            y += 20;
        }
    }
}