using System;
using System.Collections;
using UnityEngine;

using ZFramework.Extensions;
using ZFramework.Unity.Common;

using Logger = ZFramework.Unity.Common.Logger;

[UseLogger(Category.Data)]
[UseLoggerFile("Loader")]
public abstract class Loader : MonoBehaviour
{
    public bool ShouldDontDestroyOnLoad = true;

    private static Loader instance;
    public static Loader Instance
    {
        get { if (instance == null) { Logger.Log("Loader Instance get fail!"); } return instance; }
        private set { if (value != null) instance = value; else Logger.Log("Loader Instance set fail!"); }
    }

    protected virtual void Start()
    {

    }

    protected virtual void Awake()
    {
        Instance = this;
        if(ShouldDontDestroyOnLoad) DontDestroyOnLoad(this);
    }

    protected virtual void Update()
    {

    }

    protected virtual void OnGUI()
    {

    }

    protected virtual void OnLevelWasLoaded(int level)
    {

    }

    protected abstract void Pass();

    public void Delay(float waitTime, Action action)
    {
        Logger.Log(string.Format("Delay method invoked. Will wait for {0} seconds. BUGAGA!", waitTime));

        StartCoroutine(DelayImpl(waitTime, action));
    }

    IEnumerator DelayImpl(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);

        if (action != null) action();
    }
}