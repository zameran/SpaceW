#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
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
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using System;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

[Obsolete("Do not use this class!")]
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