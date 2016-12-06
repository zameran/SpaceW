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
// Modified by Denis Ovchinnikov 2015-2016
#endregion

using System.Collections.Generic;

namespace SpaceEngine.Types.Containers
{
    public class DictionaryQueue<KEY, VALUE>
    {
        private readonly Dictionary<KEY, LinkedListNode<KeyValuePair<KEY, VALUE>>> dictionary;
        private readonly LinkedList<KeyValuePair<KEY, VALUE>> list;

        public DictionaryQueue()
        {
            dictionary = new Dictionary<KEY, LinkedListNode<KeyValuePair<KEY, VALUE>>>();
            list = new LinkedList<KeyValuePair<KEY, VALUE>>();
        }

        public DictionaryQueue(IEqualityComparer<KEY> comparer)
        {
            dictionary = new Dictionary<KEY, LinkedListNode<KeyValuePair<KEY, VALUE>>>(comparer);
            list = new LinkedList<KeyValuePair<KEY, VALUE>>();
        }

        public bool ContainsKey(KEY key)
        {
            return dictionary.ContainsKey(key);
        }

        public void Replace(KEY key, VALUE val)
        {
            var node = dictionary[key];
            node.Value = new KeyValuePair<KEY, VALUE>(key, val);
        }

        public void AddFirst(KEY key, VALUE val)
        {
            dictionary.Add(key, list.AddFirst(new KeyValuePair<KEY, VALUE>(key, val)));
        }

        public void AddLast(KEY key, VALUE val)
        {
            dictionary.Add(key, list.AddLast(new KeyValuePair<KEY, VALUE>(key, val)));
        }

        public int Count()
        {
            return dictionary.Count;
        }

        public bool Empty()
        {
            return (dictionary.Count == 0);
        }

        public VALUE Get(KEY key)
        {
            var node = dictionary[key];
            return node.Value.Value;
        }

        public KeyValuePair<KEY, VALUE> First()
        {
            return list.First.Value;
        }

        public KeyValuePair<KEY, VALUE> Last()
        {
            return list.Last.Value;
        }

        public VALUE RemoveFirst()
        {
            var node = list.First;

            list.RemoveFirst();
            dictionary.Remove(node.Value.Key);

            return node.Value.Value;
        }

        public VALUE RemoveLast()
        {
            var node = list.Last;

            list.RemoveLast();
            dictionary.Remove(node.Value.Key);

            return node.Value.Value;
        }

        public VALUE Remove(KEY key)
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