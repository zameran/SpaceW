using SpaceEngine.Types.Containers;

using UnityEngine;

using TASK = SpaceEngine.Core.Utilities.Task;

namespace SpaceEngine.Core.Utilities
{
    public class Schedular : MonoBehaviour
    {
        SetQueue<TASK> TaskQueue;
        TASK.EqualityComparer Comparer;

        public Schedular()
        {
            Comparer = new TASK.EqualityComparer();
            TaskQueue = new SetQueue<TASK>(Comparer);
        }

        public void Add(TASK task)
        {
            if (task.IsDone) return;

            if (!TaskQueue.Contains(task))
                TaskQueue.AddLast(task);
        }

        public bool HasTask(TASK task)
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