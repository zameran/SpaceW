using System;
using System.Collections;
using System.Threading;

using UnityEngine;

namespace SpaceEngine.Core.Utilities
{
    public static class NinjaMonoBehaviourExtensions
    {   
        /// <summary>
        /// Start a co-routine on a background thread.
        /// </summary>
        /// <param name="behaviour">Target.</param>
        /// <param name="routine">Co-routine to start.</param>
        /// <param name="task">Gets a task object with more control on the background thread.</param>
        /// <returns>Returns <see cref="Coroutine"/> of <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"/></returns>
        public static Coroutine StartCoroutineAsync(this MonoBehaviour behaviour, IEnumerator routine, out Task task)
        {
            task = new Task(routine);

            return behaviour.StartCoroutine(task);
        }

        /// <summary>
        /// Start a co-routine on a background thread.
        /// </summary>
        /// <param name="behaviour">Target.</param>
        /// <param name="routine">Co-routine to start.</param>
        public static Coroutine StartCoroutineAsync(this MonoBehaviour behaviour, IEnumerator routine)
        {
            Task t;

            return StartCoroutineAsync(behaviour, routine, out t);
        }
    }

    /// <summary>
    /// Ninja jumps between threads.
    /// </summary>
    public static class ThreadNinja
    {
        /// <summary>
        /// Yield return it to switch to Unity main thread.
        /// </summary>
        public static readonly object JumpToUnity;

        /// <summary>
        /// Yield return it to switch to background thread.
        /// </summary>
        public static readonly object JumpBack;

        static ThreadNinja()
        {
            JumpToUnity = new object();
            JumpBack = new object();
        }
    }

    /// <summary>
    /// Running state of a task.
    /// </summary>
    public enum TaskState
    {
        /// <summary>
        /// Task has been created, but has not begun.
        /// </summary>
        Init,

        /// <summary>
        /// Task is running.
        /// </summary>
        Running,

        /// <summary>
        /// Task has finished properly.
        /// </summary>
        Done,

        /// <summary>
        /// Task has been cancelled.
        /// </summary>
        Cancelled,

        /// <summary>
        /// Task terminated by errors.
        /// </summary>
        Error
    }

    /// <summary>
    /// Represents an async task.
    /// </summary>
    public class Task : IEnumerator
    {
        #region IEnumerator Interface

        /// <summary>
        /// The current iterator yield return value.
        /// </summary>
        public object Current { get; private set; }

        /// <summary>
        /// Runs next iteration.
        /// </summary>
        /// <returns><code>true</code> for continue, otherwise <code>false</code>.</returns>
        public bool MoveNext()
        {
            return OnMoveNext();
        }

        public void Reset()
        {
            throw new NotSupportedException("Not support calling Reset() on iterator.");
        }

        #endregion

        /// <summary>
        /// Inner running state used by state machine.
        /// </summary>
        private enum RunningState
        {
            Init,
            RunningAsync,
            PendingYield,
            ToBackground,
            RunningSync,
            CancellationRequested,
            Done,
            Error
        }

        /// <summary>
        /// Routine user want to run.
        /// </summary>
        private readonly IEnumerator innerRoutine;

        /// <summary>
        /// Current running state.
        /// </summary>
        private RunningState currentState;

        /// <summary>
        /// Last running state.
        /// </summary>
        private RunningState previousState;

        private object pendingCurrent;

        /// <summary>
        /// Gets state of the task.
        /// </summary>
        public TaskState State
        {
            get
            {
                switch (currentState)
                {
                    case RunningState.CancellationRequested:
                        return TaskState.Cancelled;
                    case RunningState.Done:
                        return TaskState.Done;
                    case RunningState.Error:
                        return TaskState.Error;
                    case RunningState.Init:
                        return TaskState.Init;
                    default:
                        return TaskState.Running;
                }
            }
        }

        /// <summary>
        /// Gets exception during running.
        /// </summary>
        public Exception InnerException { get; private set; }

        public Task(IEnumerator routine)
        {
            innerRoutine = routine;
            currentState = RunningState.Init;
        }

        /// <summary>
        /// Cancel the task till next iteration;
        /// </summary>
        public void Cancel()
        {
            if (State == TaskState.Running)
            {
                GotoState(RunningState.CancellationRequested);
            }
        }

        /// <summary>
        /// A co-routine that waits the task.
        /// </summary>
        public IEnumerator Wait()
        {
            while (State == TaskState.Running)
                yield return null;
        }

        private void GotoState(RunningState state)
        {
            if (currentState == state) return;

            lock (this)
            {
                previousState = currentState;
                currentState = state;
            }
        }

        private void SetPendingCurrentObject(object current)
        {
            lock (this)
            {
                pendingCurrent = current;
            }
        }

        private bool OnMoveNext()
        {
            // No running for null
            if (innerRoutine == null)
                return false;

            // Set current to null so that Unity not get same yield value twice
            Current = null;

            // Loops until the inner routine yield something to Unity
            while (true)
            {
                // A simple state machine
                switch (currentState)
                {
                    // First, goto background
                    case RunningState.Init:
                        GotoState(RunningState.ToBackground);
                        break;
                    // Running in background, wait a frame
                    case RunningState.RunningAsync:
                        return true;

                    // Runs on main thread
                    case RunningState.RunningSync:
                        MoveNextUnity();
                        break;

                    // Need switch to background
                    case RunningState.ToBackground:
                        GotoState(RunningState.RunningAsync);
                        // Call the thread launcher
                        MoveNextAsync();
                        return true;

                    // Something was yield returned
                    case RunningState.PendingYield:
                        if (pendingCurrent == ThreadNinja.JumpBack)
                        {
                            // Do not break the loop, switch to background
                            GotoState(RunningState.ToBackground);
                        }
                        else if (pendingCurrent == ThreadNinja.JumpToUnity)
                        {
                            // Do not break the loop, switch to main thread
                            GotoState(RunningState.RunningSync);
                        }
                        else
                        {
                            // Not from the Ninja, then Unity should get noticed, set to Current property to achieve this
                            Current = pendingCurrent;

                            // Yield from background thread, or main thread?
                            if (previousState == RunningState.RunningAsync)
                            {
                                // If from background thread, go back into background in the next loop
                                pendingCurrent = ThreadNinja.JumpBack;
                            }
                            else
                            {
                                // Otherwise go back to main thread the next loop;
                                pendingCurrent = ThreadNinja.JumpToUnity;
                            }

                            // End this iteration and Unity get noticed...
                            return true;
                        }
                        break;

                    // Done running, pass false to Unity;
                    case RunningState.Error:
                    case RunningState.Done:
                    case RunningState.CancellationRequested:
                    default:
                        return false;
                }
            }
        }

        private void MoveNextAsync()
        {
            ThreadPool.QueueUserWorkItem(BackgroundRunner);
        }

        private void BackgroundRunner(object state)
        {
            MoveNextUnity();
        }

        private void MoveNextUnity()
        {
            try
            {
                // Run next part of the user routine
                var result = innerRoutine.MoveNext();

                if (result)
                {
                    // Something has been yield returned, handle it...
                    SetPendingCurrentObject(innerRoutine.Current);
                    GotoState(RunningState.PendingYield);
                }
                else
                {
                    // User routine simple done!
                    GotoState(RunningState.Done);
                }
            }
            catch (Exception ex)
            {
                InnerException = ex;

                Debug.LogError($"Task: {ex.Message}\n{ex.StackTrace}");

                GotoState(RunningState.Error);
            }
        }
    }
}