#region License And Usage
// TaskManager.cs
// Copyright (c) 2011, Ken Rockot  <k-e-n-@-REMOVE-CAPS-AND-HYPHENS-oz.gs>.  All rights reserved.
// Everyone is granted non-exclusive license to do anything at all with this code.
//
// This is a new coroutine interface for Unity.
//
// The motivation for this is twofold:
//
// 1. The existing coroutine API provides no means of stopping specific
//    coroutines; StopCoroutine only takes a string argument, and it stops
//    all coroutines started with that same string; there is no way to stop
//    coroutines which were started directly from an enumerator.  This is
//    not robust enough and is also probably pretty inefficient.
//
// 2. StartCoroutine and friends are MonoBehaviour methods.  This means
//    that in order to start a coroutine, a user typically must have some
//    component reference handy.  There are legitimate cases where such a
//    constraint is inconvenient.  This implementation hides that
//    constraint from the user.
//
// Example usage:
//
// ----------------------------------------------------------------------------
// IEnumerator MyAwesomeTask()
// {
//     while(true) {
//         Debug.Log("Logcat iz in ur consolez, spammin u wif messagez.");
//         yield return null;
//     }
// }
//
// IEnumerator TaskKiller(float delay, Task t)
// {
//     yield return new WaitForSeconds(delay);
//     t.Stop();
// }
//
// void SomeCodeThatCouldBeAnywhereInTheUniverse()
// {
//     Task spam = new Task(MyAwesomeTask());
//     new Task(TaskKiller(5, spam));
// }
// ----------------------------------------------------------------------------
//
// When SomeCodeThatCouldBeAnywhereInTheUniverse is called, the debug console
// will be spammed with annoying messages for 5 seconds.
//
// Simple, really.  There is no need to initialize or even refer to TaskManager.
// When the first Task is created in an application, a "TaskManager" GameObject
// will automatically be added to the scene root with the TaskManager component
// attached.  This component will be responsible for dispatching all coroutines
// behind the scenes.
//
// Task also provides an event that is triggered when the coroutine exits.
#endregion

using System.Collections;

namespace SpaceEngine.Managers
{
    /// <summary>
    /// A Task object represents a coroutine. Tasks can be started, paused, and stopped.
    /// It is an error to attempt to start a task that has been stopped or which has naturally terminated.
    /// </summary>
    public class Task
    {
        /// <summary>
        /// Returns true if and only if the coroutine is running. Paused tasks are considered to be running.
        /// </summary>
        public bool Running { get { return InternalTask.Running; } }

        /// <summary>
        /// Returns true if and only if the coroutine is currently paused.
        /// </summary>
        public bool Paused { get { return InternalTask.Paused; } }

        /// <summary>
        /// Delegate for termination subscribers.  manual is true if and only if the coroutine was stopped with an explicit call to <see cref="Task.Stop"/>.
        /// </summary>
        public delegate void FinishedHandler(bool manual);

        /// <summary>
        /// Termination event.  Triggered when the coroutine completes execution.
        /// </summary>
        public event FinishedHandler Finished;

        TaskManager.TaskState InternalTask;

        /// <summary>
        /// Creates a new Task object for the given coroutine.
        /// If <see cref="autoStart"/> is true (default) the task is automatically started upon construction.
        /// </summary>
        public Task(IEnumerator c, bool autoStart = true)
        {
            InternalTask = TaskManager.CreateTask(c);
            InternalTask.OnFinished += OnTaskFinished;

            if (autoStart) { Start(); }
        }

        /// <summary>
        /// Begins execution of the coroutine.
        /// </summary>
        public void Start()
        {
            InternalTask.Start();
        }

        /// <summary>
        /// Discontinues execution of the coroutine at its next yield.
        /// </summary>
        public void Stop()
        {
            InternalTask.Stop();
        }

        public void Pause()
        {
            InternalTask.Pause();
        }

        public void Unpause()
        {
            InternalTask.Unpause();
        }

        void OnTaskFinished(bool manual)
        {
            if (Finished != null)
                Finished(manual);
        }
    }

    public sealed class TaskManager : MonoSingleton<TaskManager>
    {
        public class TaskState
        {
            public bool Running { get; private set; }

            public bool Paused { get; private set; }

            public bool Stopped { get; private set; }

            public delegate void FinishedHandler(bool manual);

            public event FinishedHandler OnFinished;

            private IEnumerator Coroutine;

            public TaskState(IEnumerator coroutine)
            {
                this.Coroutine = coroutine;
            }

            public void Pause()
            {
                Paused = true;
            }

            public void Unpause()
            {
                Paused = false;
            }

            public void Start()
            {
                Running = true;

                Instance.StartCoroutine(CallWrapper());
            }

            public void Stop()
            {
                Stopped = true;
                Running = false;
            }

            private IEnumerator CallWrapper()
            {
                yield return null;

                var e = Coroutine;

                while (Running)
                {
                    if (Paused)
                        yield return null;
                    else
                    {
                        if (e != null && e.MoveNext())
                        {
                            yield return e.Current;
                        }
                        else
                        {
                            Running = false;
                        }
                    }
                }

                if (OnFinished != null)
                    OnFinished(Stopped);
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        public static TaskState CreateTask(IEnumerator coroutine)
        {
            return new TaskState(coroutine);
        }
    }
}