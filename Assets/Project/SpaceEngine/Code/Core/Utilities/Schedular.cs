using SpaceEngine.Types.Containers;

using UnityEngine;

namespace SpaceEngine.Core.Utilities
{
    public class Schedular : MonoBehaviour
    {
        SetQueue<Task> TaskQueue;
        Task.EqualityComparer Comparer;

        public Schedular()
        {
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