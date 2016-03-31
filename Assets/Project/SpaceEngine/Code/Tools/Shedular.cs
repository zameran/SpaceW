using UnityEngine;

using System.Collections.Generic;

public class ProlandShedular : MonoBehaviour
{
    SetQueue<ProlandShedularTask> m_taskQueue;
    ProlandShedularTask.EqualityComparer m_comparer;

    public ProlandShedular()
    {
        m_comparer = new ProlandShedularTask.EqualityComparer();
        m_taskQueue = new SetQueue<ProlandShedularTask>(m_comparer);
    }

    public void Add(ProlandShedularTask task)
    {
        if (task.IsDone()) return;

        if (!m_taskQueue.Contains(task))
            m_taskQueue.AddLast(task);
    }

    public bool HasTask(ProlandShedularTask task)
    {
        return m_taskQueue.Contains(task);
    }

    public void Run()
    {
        ProlandShedularTask task = (m_taskQueue.Empty()) ? null : m_taskQueue.First();

        while (task != null)
        {
            task.Run();

            m_taskQueue.Remove(task);

            if (m_taskQueue.Empty())
                task = null;
            else
                task = m_taskQueue.First();
        }
    }
}

public abstract class ProlandShedularTask
{
    public class EqualityComparer : IEqualityComparer<ProlandShedularTask>
    {
        public bool Equals(ProlandShedularTask t1, ProlandShedularTask t2)
        {
            return Object.ReferenceEquals(t1, t2);
        }

        public int GetHashCode(ProlandShedularTask t)
        {
            return t.GetHashCode();
        }
    }

    bool m_done = false;

    public ProlandShedularTask()
    {

    }

    public abstract void Run();

    public bool IsDone()
    {
        return m_done;
    }

    public void SetDone(bool b)
    {
        m_done = b;
    }
}