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
// Creation Date: 2017.09.16
// Creation Time: 1:27 AM
// Creator: zameran
#endregion

using System;
using System.Collections.Generic;

namespace SpaceEngine.Core.Patterns.Pool
{
    public class GenericPool<TKey, TValue> where TValue : IGenericPoolObject<TKey>
    {
        public int MaxInstances { get; protected set; }

        public virtual int InctanceCount => Vault.Count;
        public virtual int CacheCount => Cache.Count;

        public delegate bool Compare<in T>(T value) where T : TValue;

        protected Dictionary<TKey, List<TValue>> Vault;
        protected Dictionary<Type, List<TValue>> Cache;

        public GenericPool(int maxInstance)
        {
            MaxInstances = maxInstance;
            Vault = new Dictionary<TKey, List<TValue>>();
            Cache = new Dictionary<Type, List<TValue>>();
        }

        public virtual bool CanPush()
        {
            return InctanceCount + 1 < MaxInstances;
        }

        public virtual bool Push(TKey groupKey, TValue value)
        {
            var result = false;

            if (CanPush())
            {
                value.OnPush();

                if (!Vault.ContainsKey(groupKey)) { Vault.Add(groupKey, new List<TValue>()); }

                Vault[groupKey].Add(value);

                var type = value.GetType();

                if (!Cache.ContainsKey(type)) { Cache.Add(type, new List<TValue>()); }

                Cache[type].Add(value);

                result = true;
            }
            else
            {
                value.FailedPush();
            }

            return result;
        }

        public virtual T Pop<T>(TKey groupKey) where T : TValue
        {
            var result = default(T);

            if (Contains(groupKey) && Vault[groupKey].Count > 0)
            {
                for (var i = 0; i < Vault[groupKey].Count; i++)
                {
                    if (Vault[groupKey][i] is T)
                    {
                        result = (T)Vault[groupKey][i];

                        var type = result.GetType();

                        RemoveObject(groupKey, i);
                        RemoveFromCache(result, type);

                        result.Create();

                        break;
                    }
                }
            }

            return result;
        }

        public virtual T Pop<T>() where T : TValue
        {
            var result = default(T);
            var type = typeof(T);

            if (ValidateForPop(type))
            {
                for (var i = 0; i < Cache[type].Count; i++)
                {
                    result = (T)Cache[type][i];

                    if (result != null && Vault.ContainsKey(result.Group))
                    {
                        Vault[result.Group].Remove(result);

                        RemoveFromCache(result, type);

                        result.Create();

                        break;
                    }
                }
            }

            return result;
        }

        public virtual T Pop<T>(Compare<T> comparer) where T : TValue
        {
            var result = default(T);
            var type = typeof(T);

            if (ValidateForPop(type))
            {
                for (var i = 0; i < Cache[type].Count; i++)
                {
                    var value = (T)Cache[type][i];

                    if (comparer(value))
                    {
                        Vault[value.Group].Remove(value);

                        RemoveFromCache(result, type);

                        result = value;
                        result.Create();

                        break;
                    }
                }
            }

            return result;
        }


        public virtual bool Contains(TKey groupKey)
        {
            return Vault.ContainsKey(groupKey);
        }

        public virtual void Clear()
        {
            Vault.Clear();
        }

        protected virtual bool ValidateForPop(Type type)
        {
            return Cache.ContainsKey(type) && Cache[type].Count > 0;
        }

        protected virtual void RemoveObject(TKey groupKey, int idx)
        {
            if (idx >= 0 && idx < Vault[groupKey].Count)
            {
                Vault[groupKey].RemoveAt(idx);

                if (Vault[groupKey].Count == 0) { Vault.Remove(groupKey); }
            }
        }

        protected void RemoveFromCache(TValue value, Type type)
        {
            if (Cache.ContainsKey(type))
            {
                Cache[type].Remove(value);

                if (Cache[type].Count == 0) { Cache.Remove(type); }
            }
        }
    }
}