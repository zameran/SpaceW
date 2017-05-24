#region License
//
// Procedural planet renderer.
// Copyright (c) 2008-2011 INRIA
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// Proland is distributed under a dual-license scheme.
// You can obtain a specific license from Inria: proland-licensing@inria.fr.
//
// Authors: Justin Hawkins 2014.
// Modified by Denis Ovchinnikov 2015-2017
#endregion

using System.Collections.Generic;

namespace SpaceEngine.Types.Containers
{
    public class DictionaryQueue<TKey, TValue>
    {
        private readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> dictionary;
        private readonly LinkedList<KeyValuePair<TKey, TValue>> list;

        public DictionaryQueue()
        {
            dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
            list = new LinkedList<KeyValuePair<TKey, TValue>>();
        }

        public DictionaryQueue(IEqualityComparer<TKey> comparer)
        {
            dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(comparer);
            list = new LinkedList<KeyValuePair<TKey, TValue>>();
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public void Replace(TKey key, TValue val)
        {
            var node = dictionary[key];
            node.Value = new KeyValuePair<TKey, TValue>(key, val);
        }

        public void AddFirst(TKey key, TValue val)
        {
            dictionary.Add(key, list.AddFirst(new KeyValuePair<TKey, TValue>(key, val)));
        }

        public void AddLast(TKey key, TValue val)
        {
            dictionary.Add(key, list.AddLast(new KeyValuePair<TKey, TValue>(key, val)));
        }

        public int Count()
        {
            return dictionary.Count;
        }

        public bool Empty()
        {
            return (dictionary.Count == 0);
        }

        public TValue Get(TKey key)
        {
            var node = dictionary[key];
            return node.Value.Value;
        }

        public KeyValuePair<TKey, TValue> First()
        {
            return list.First.Value;
        }

        public KeyValuePair<TKey, TValue> Last()
        {
            return list.Last.Value;
        }

        public TValue RemoveFirst()
        {
            var node = list.First;

            list.RemoveFirst();
            dictionary.Remove(node.Value.Key);

            return node.Value.Value;
        }

        public TValue RemoveLast()
        {
            var node = list.Last;

            list.RemoveLast();
            dictionary.Remove(node.Value.Key);

            return node.Value.Value;
        }

        public TValue Remove(TKey key)
        {
            var node = dictionary[key];

            dictionary.Remove(key);
            list.Remove(node);

            return node.Value.Value;
        }

        public void Clear()
        {
            dictionary.Clear();
            list.Clear();
        }
    }
}