using UnityEngine;

using ZFramework.Unity.Common;
using ZFramework.Unity.Common.Threading;
using Logger = ZFramework.Unity.Common.Logger;

using System.Collections;
using System.Collections.Generic;

[UseLoggerFile("StressTest")]
[UseLogger(Category.Other)]
public class LoggerStressTest : MonoBehaviour
{
    private void Start()
    {
        Test1(false);
        Test2(false);
        Test1(true);
        Test2(true);
    }

    private void Test1(bool c)
    {
        for (int i = 0; i < 100; i++)
        {
            if (!c)
                Spam("Test1_" + i);
            else
                StartCoroutine(SpamC("Test1_" + i));
        }
    }

    private void Test2(bool c)
    {
        for (int i = 0; i < 100; i++)
        {
            if (!c)
                Spam("Test2_" + i);
            else
                StartCoroutine(SpamC("Test2_" + i));
        }
    }

    private void Spam(string msg)
    {
        Logger.Log("00" + msg);
        Logger.Log("01" + msg);
    }

    private IEnumerator SpamC(string msg)
    {
        Logger.Log("00" + msg);
        yield return new WaitForSeconds(0.01f);
        Logger.Log("01" + msg);
    }
}