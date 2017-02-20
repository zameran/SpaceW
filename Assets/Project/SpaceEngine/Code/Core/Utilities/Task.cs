using System;
using System.Collections.Generic;

using Object = UnityEngine.Object;

namespace SpaceEngine.Core.Utilities
{
    [Serializable]
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
}