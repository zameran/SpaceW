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
    public class SetQueue<VALUE>
    {
        private readonly Dictionary<VALUE, LinkedListNode<KeyValuePair<VALUE, VALUE>>> dictionary;
        private readonly LinkedList<KeyValuePair<VALUE, VALUE>> list;

        public SetQueue()
        {
            dictionary = new Dictionary<VALUE, LinkedListNode<KeyValuePair<VALUE, VALUE>>>();
            list = new LinkedList<KeyValuePair<VALUE, VALUE>>();
        }

        public SetQueue(IEqualityComparer<VALUE> comparer)
        {
            dictionary = new Dictionary<VALUE, LinkedListNode<KeyValuePair<VALUE, VALUE>>>(comparer);
            list = new LinkedList<KeyValuePair<VALUE, VALUE>>();
        }

        public bool Contains(VALUE val)
        {
            return dictionary.ContainsKey(val);
        }

        public void AddFirst(VALUE val)
        {
            dictionary.Add(val, list.AddFirst(new KeyValuePair<VALUE, VALUE>(val, val)));
        }

        public void AddLast(VALUE val)
        {
            dictionary.Add(val, list.AddLast(new KeyValuePair<VALUE, VALUE>(val, val)));
        }

        public int Count()
        {
            return dictionary.Count;
        }

        public bool Empty()
        {
            return (dictionary.Count == 0);
        }

        public VALUE First()
        {
            return list.First.Value.Value;
        }

        public VALUE Last()
        {
            return list.Last.Value.Value;
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

        public void Remove(VALUE val)
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