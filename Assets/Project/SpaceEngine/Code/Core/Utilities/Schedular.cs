using SpaceEngine.Types.Containers;

using System;
using System.Collections.Generic;

namespace SpaceEngine.Core.Utilities
{
    public class Schedular : MonoSingleton<Schedular>
    {
        public abstract class Task
        {
            public class EqualityComparer : IEqualityComparer<Task>
            {
                public bool Equals(Task t1, Task t2)
                {
                    return Object.ReferenceEquals(t1, t2);
                }

                public int GetHashCode(Task t)
                {
                    return t.GetHashCode();
                }
            }

            public bool IsDone { get; protected set; }

            public Task()
            {

            }

            public abstract void Run();
        }

        Task.EqualityComparer Comparer;
        SetQueue<Task> TaskQueue;

        private void Awake()
        {
            Instance = this;

            Comparer = new Task.EqualityComparer();
            TaskQueue = new SetQueue<Task>(Comparer);
        }

        public void Add(Task task)
        {
            if (task.IsDone) return;

            if (!TaskQueue.Contains(task))
                TaskQueue.AddLast(task);
        }

        public bool HasTask(Task task)
        {
            return TaskQueue.Contains(task);
        }

        public void Run()
        {
            var task = (TaskQueue.Empty()) ? null : TaskQueue.First();

            while (task != null)
            {
                task.Run();

                TaskQueue.Remove(task);

                if (TaskQueue.Empty())
                    task = null;
                else
                    task = TaskQueue.First();
            }
        }
    }
}