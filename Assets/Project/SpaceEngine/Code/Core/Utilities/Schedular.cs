#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
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
// Creation Date: 2017.03.28
// Creation Time: 2:18 PM
// Creator: zameran
#endregion

using System;
using System.Collections.Generic;
using SpaceEngine.Core.Containers;
using SpaceEngine.Core.Patterns.Singleton;

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
                    return ReferenceEquals(t1, t2);
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

            public virtual void Finish()
            {
                IsDone = true;
            }
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