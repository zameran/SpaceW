using System;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

public class ThreadScheduler : MonoBehaviour
{
    private static bool initialized = false;

    private static int totalThreads = 16;

    public int TotalThreads
    {
        get { return totalThreads; }
    }

    private static int numThreads = 0;
    public int NumThreads
    {
        get { return numThreads; }
    }

    public int ActionsCount
    {
        get { return actions.Count; }
    }

    public int CurrentActionsCount
    {
        get { return currentActions.Count; }
    }

    private List<Action> actions = new List<Action>();
    private List<Action> currentActions = new List<Action>();

    #region Instance

    private static ThreadScheduler instance;
    public static ThreadScheduler Instance
    {
        get
        {
            Initialize();
            return instance;
        }
    }

    void OnDisable()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    #endregion

    #region Initialization

    void Awake()
    {
        instance = this;
        initialized = true;
    }

    public static void Initialize()
    {
        if (!initialized)
        {
            if (!Application.isPlaying)
                return;

            initialized = true;

            GameObject go = new GameObject("ThreadScheduler");
            instance = go.AddComponent<ThreadScheduler>();
        }
    }

    #endregion

    #region Runtime

    public static void RunOnMainThread(Action action)
    {
        Initialize();

        lock (Instance.actions)
        {
            Instance.actions.Add(action);
        }
    }

    public static Thread RunOnThread(Action action)
    {
        Initialize();

        while (numThreads >= totalThreads)
        {
            Thread.Sleep(100);
        }

        Interlocked.Increment(ref numThreads);

        ThreadPool.QueueUserWorkItem(RunAction, action);

        return null;
    }

    public static Thread RunOnThreadIfPlaying(Action action)
    {
        if (Application.isPlaying)
        {
            return RunOnThread(action);
        }
        else
        {
            if (action != null)
                action();

            return null;
        }
    }

    private static void RunAction(object action)
    {
        try
        {
            ((Action)action)();
        }
        catch { }

        finally
        {
            Interlocked.Decrement(ref numThreads);
        }

    }

    void Update()
    {
        lock (actions)
        {
            currentActions.Clear();
            currentActions.AddRange(actions);
            actions.Clear();
        }

        for (int i = 0; i < currentActions.Count; i++)
            currentActions[i]();
    }
    #endregion
}