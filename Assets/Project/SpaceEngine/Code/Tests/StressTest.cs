using UnityEngine;

using ZFramework.Unity.Common.PerfomanceMonitor;

public class StressTest : MonoBehaviour
{
    private void Update()
    {
        Vector3 temp = Vector3.zero;

        using (new Timer("CalculatePatchCubeCenter"))
        {
            for (int i = 0; i < 1000000; i++)
            {
                BrainFuckMath.CalculatePatchCubeCenter(0, Vector3.zero, ref temp);
            }          
        }
    }
}