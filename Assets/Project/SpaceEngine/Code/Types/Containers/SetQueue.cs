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
    public class SetQueue<TValue>
    {
        private readonly Dictionary<TValue, LinkedListNode<KeyValuePair<TValue, TValue>>> dictionary;
        private readonly LinkedList<KeyValuePair<TValue, TValue>> list;

        public SetQueue()
        {
            dictionary = new Dictionary<TValue, LinkedListNode<KeyValuePair<TValue, TValue>>>();
            list = new LinkedList<KeyValuePair<TValue, TValue>>();
        }

        public SetQueue(IEqualityComparer<TValue> comparer)
        {
            dictionary = new Dictionary<TValue, LinkedListNode<KeyValuePair<TValue, TValue>>>(comparer);
            list = new LinkedList<KeyValuePair<TValue, TValue>>();
        }

        public bool Contains(TValue val)
        {
            return dictionary.ContainsKey(val);
        }

        public void AddFirst(TValue val)
        {
            dictionary.Add(val, list.AddFirst(new KeyValuePair<TValue, TValue>(val, val)));
        }

        public void AddLast(TValue val)
        {
            dictionary.Add(val, list.AddLast(new KeyValuePair<TValue, TValue>(val, val)));
        }

        public int Count()
        {
            return dictionary.Count;
        }

        public bool Empty()
        {
            return (dictionary.Count == 0);
        }

        public TValue First()
        {
            return list.First.Value.Value;
        }

        public TValue Last()
        {
            return list.Last.Value.Value;
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

        public void Remove(TValue val)
        {
            var node = dictionary[val];

            dictionary.Remove(val);
            list.Remove(node);
        }

        public void Clear()
        {
            dictionary.Clear();
            list.Clear();
        }
    }
}